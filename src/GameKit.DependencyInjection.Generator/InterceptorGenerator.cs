using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameKit.DependencyInjection.Generator;

enum InterceptionKind
{
    AddSingleton,
    AddSingletonWithAlias,
    AddSingletonFactory,
    OnStart
}

readonly record struct ExtractionResult(InterceptionInfo? Interception, DiagnosticInfo? Diagnostic);

readonly record struct DiagnosticInfo(string Id, string Message, string FilePath, int Line, int Column, int EndLine, int EndColumn);

readonly record struct InterceptionInfo(
    InterceptionKind Kind,
    string InterceptsLocationAttribute,
    string? ImplementationTypeFullName,
    EquatableArray<string> ConstructorParameterTypes,
    string? ServiceTypeFullName,
    string? DelegateTypeFullName,
    EquatableArray<string> DelegateParameterTypes,
    bool DelegateReturnsVoid
);

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    private const string ServiceCollectionFullName = "GameKit.DependencyInjection.ServiceCollection";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ExtractionResult?> results = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsCandidateInvocation(node),
            transform: static (ctx, ct) => ExtractInterception(ctx, ct)
        ).Where(static r => r is not null);

        IncrementalValueProvider<ImmutableArray<ExtractionResult?>> collected = results.Collect();

        context.RegisterSourceOutput(collected, static (spc, items) =>
        {
            ImmutableArray<InterceptionInfo>.Builder interceptions = ImmutableArray.CreateBuilder<InterceptionInfo>();

            foreach (ExtractionResult? item in items)
            {
                if (item is not { } result)
                {
                    continue;
                }

                if (result.Diagnostic is { } diag)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            diag.Id,
                            "GameKit.DependencyInjection",
                            diag.Message,
                            "GameKit.DependencyInjection",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.Create(
                            diag.FilePath,
                            default,
                            new LinePositionSpan(
                                new LinePosition(diag.Line, diag.Column),
                                new LinePosition(diag.EndLine, diag.EndColumn)))));
                }

                if (result.Interception is { } interception)
                {
                    interceptions.Add(interception);
                }
            }

            if (interceptions.Count == 0)
            {
                return;
            }

            string source = GenerateInterceptors(interceptions.ToImmutable());
            spc.AddSource("ServiceCollectionInterceptors.g.cs", source);
        });
    }

    private static bool IsCandidateInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        string? methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name switch
            {
                GenericNameSyntax g => g.Identifier.Text,
                IdentifierNameSyntax id => id.Identifier.Text,
                _ => null
            },
            GenericNameSyntax g => g.Identifier.Text,
            IdentifierNameSyntax id => id.Identifier.Text,
            _ => null
        };

        return methodName is "AddSingleton" or "OnStart";
    }

    private static ExtractionResult? ExtractInterception(GeneratorSyntaxContext context, CancellationToken ct)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        string? containingType = methodSymbol.ContainingType?.ToDisplayString();
        if (containingType != ServiceCollectionFullName)
        {
            return null;
        }

        InterceptableLocation? interceptableLocation = context.SemanticModel.GetInterceptableLocation(invocation, ct);
        if (interceptableLocation == null)
        {
            return null;
        }

        string methodName = methodSymbol.Name;

        if (methodName == "AddSingleton")
        {
            return ExtractAddSingleton(methodSymbol, invocation, context, interceptableLocation, ct);
        }

        if (methodName == "OnStart")
        {
            InterceptionInfo? onStart = ExtractOnStart(methodSymbol, invocation, context, interceptableLocation, ct);
            return onStart is { } info ? new ExtractionResult(info, null) : null;
        }

        return null;
    }

    private static ExtractionResult? ExtractAddSingleton(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        GeneratorSyntaxContext context,
        InterceptableLocation interceptableLocation,
        CancellationToken ct)
    {
        // AddSingleton<T>(Delegate) — factory overload
        // Must check parameter type to distinguish from AddSingleton<T>(T instance)
        if (methodSymbol.TypeArguments.Length == 1 && methodSymbol.Parameters.Length == 1
            && methodSymbol.Parameters[0].Type.ToDisplayString() == "System.Delegate")
        {
            string serviceTypeFullName = methodSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            InterceptionInfo? result = ExtractDelegateInterception(
                InterceptionKind.AddSingletonFactory,
                invocation,
                context,
                interceptableLocation,
                ct,
                serviceTypeFullName);
            return result is { } info ? new ExtractionResult(info, null) : null;
        }

        // AddSingleton<T>() — single type param, no args
        if (methodSymbol.TypeArguments.Length == 1 && methodSymbol.Parameters.Length == 0)
        {
            return ExtractAddSingletonType(methodSymbol, invocation, interceptableLocation);
        }

        // AddSingleton<TService, TImpl>() — two type params, no args
        if (methodSymbol.TypeArguments.Length == 2 && methodSymbol.Parameters.Length == 0)
        {
            return ExtractAddSingletonWithAlias(methodSymbol, invocation, interceptableLocation);
        }

        return null;
    }

    private static ExtractionResult? ExtractAddSingletonType(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        InterceptableLocation interceptableLocation)
    {
        ITypeSymbol implType = methodSymbol.TypeArguments[0];

        if (implType is not INamedTypeSymbol implNamedType)
        {
            return new ExtractionResult(null, CreateDiagnostic(invocation, "GK0001",
                $"AddSingleton<{implType.Name}>() cannot be used with an open generic type parameter. Use AddSingleton<{implType.Name}>(Func<ServiceProvider, {implType.Name}>) instead."));
        }

        IMethodSymbol? constructor = GetSinglePublicConstructor(implNamedType);
        if (constructor == null)
        {
            return new ExtractionResult(null, CreateDiagnostic(invocation, "GK0002",
                $"AddSingleton<{implType.Name}>() requires exactly one public constructor."));
        }

        ImmutableArray<string> paramTypes = constructor.Parameters
            .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToImmutableArray();

        return new ExtractionResult(new InterceptionInfo(
            InterceptionKind.AddSingleton,
            interceptableLocation.GetInterceptsLocationAttributeSyntax(),
            implType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            new EquatableArray<string>(paramTypes),
            null,
            null,
            new EquatableArray<string>(ImmutableArray<string>.Empty),
            false
        ), null);
    }

    private static ExtractionResult? ExtractAddSingletonWithAlias(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        InterceptableLocation interceptableLocation)
    {
        ITypeSymbol serviceType = methodSymbol.TypeArguments[0];
        ITypeSymbol implType = methodSymbol.TypeArguments[1];

        if (implType is not INamedTypeSymbol implNamedType)
        {
            return new ExtractionResult(null, CreateDiagnostic(invocation, "GK0001",
                $"AddSingleton<{serviceType.Name}, {implType.Name}>() cannot be used with open generic type parameters. Use AddSingleton<{serviceType.Name}>(Func<ServiceProvider, {serviceType.Name}>) instead."));
        }

        IMethodSymbol? constructor = GetSinglePublicConstructor(implNamedType);
        if (constructor == null)
        {
            return new ExtractionResult(null, CreateDiagnostic(invocation, "GK0002",
                $"AddSingleton<{serviceType.Name}, {implType.Name}>() requires {implType.Name} to have exactly one public constructor."));
        }

        ImmutableArray<string> paramTypes = constructor.Parameters
            .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToImmutableArray();

        return new ExtractionResult(new InterceptionInfo(
            InterceptionKind.AddSingletonWithAlias,
            interceptableLocation.GetInterceptsLocationAttributeSyntax(),
            implType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            new EquatableArray<string>(paramTypes),
            serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            null,
            new EquatableArray<string>(ImmutableArray<string>.Empty),
            false
        ), null);
    }

    private static InterceptionInfo? ExtractOnStart(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        GeneratorSyntaxContext context,
        InterceptableLocation interceptableLocation,
        CancellationToken ct)
    {
        if (methodSymbol.Parameters.Length != 1)
        {
            return null;
        }

        return ExtractDelegateInterception(
            InterceptionKind.OnStart,
            invocation,
            context,
            interceptableLocation,
            ct,
            null);
    }

    private static InterceptionInfo? ExtractDelegateInterception(
        InterceptionKind kind,
        InvocationExpressionSyntax invocation,
        GeneratorSyntaxContext context,
        InterceptableLocation interceptableLocation,
        CancellationToken ct,
        string? serviceTypeFullName)
    {
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        ExpressionSyntax argExpression = invocation.ArgumentList.Arguments[0].Expression;

        // Get the type info of the delegate argument
        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(argExpression, ct);
        ITypeSymbol? delegateType = typeInfo.Type ?? typeInfo.ConvertedType;

        if (delegateType is INamedTypeSymbol namedDelegateType && namedDelegateType.DelegateInvokeMethod != null)
        {
            IMethodSymbol invokeMethod = namedDelegateType.DelegateInvokeMethod;

            ImmutableArray<string> paramTypes = invokeMethod.Parameters
                .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .ToImmutableArray();

            string delegateTypeStr = namedDelegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool returnsVoid = invokeMethod.ReturnsVoid;

            return new InterceptionInfo(
                kind,
                interceptableLocation.GetInterceptsLocationAttributeSyntax(),
                null,
                new EquatableArray<string>(ImmutableArray<string>.Empty),
                serviceTypeFullName,
                delegateTypeStr,
                new EquatableArray<string>(paramTypes),
                returnsVoid
            );
        }

        // Method group: the argument has no concrete delegate type, but GetSymbolInfo
        // resolves the target method directly.
        SymbolInfo argSymbolInfo = context.SemanticModel.GetSymbolInfo(argExpression, ct);
        IMethodSymbol? targetMethod = argSymbolInfo.Symbol as IMethodSymbol
            ?? argSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (targetMethod == null)
        {
            return null;
        }

        ImmutableArray<string> methodParamTypes = targetMethod.Parameters
            .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToImmutableArray();

        // Build the appropriate Func<> or Action<> delegate type string
        string methodDelegateTypeStr;
        bool methodReturnsVoid = targetMethod.ReturnsVoid;

        if (methodReturnsVoid)
        {
            if (methodParamTypes.Length == 0)
            {
                methodDelegateTypeStr = "global::System.Action";
            }
            else
            {
                methodDelegateTypeStr = $"global::System.Action<{string.Join(", ", methodParamTypes)}>";
            }
        }
        else
        {
            string returnType = targetMethod.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (methodParamTypes.Length == 0)
            {
                methodDelegateTypeStr = $"global::System.Func<{returnType}>";
            }
            else
            {
                methodDelegateTypeStr = $"global::System.Func<{string.Join(", ", methodParamTypes)}, {returnType}>";
            }
        }

        return new InterceptionInfo(
            kind,
            interceptableLocation.GetInterceptsLocationAttributeSyntax(),
            null,
            new EquatableArray<string>(ImmutableArray<string>.Empty),
            serviceTypeFullName,
            methodDelegateTypeStr,
            new EquatableArray<string>(methodParamTypes),
            methodReturnsVoid
        );
    }

    private static IMethodSymbol? GetSinglePublicConstructor(INamedTypeSymbol type)
    {
        IMethodSymbol[] publicConstructors = type.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
            .ToArray();

        if (publicConstructors.Length == 1)
        {
            return publicConstructors[0];
        }

        // Allow implicit parameterless constructor
        if (publicConstructors.Length == 0)
        {
            IMethodSymbol? implicitCtor = type.InstanceConstructors
                .FirstOrDefault(c => c.IsImplicitlyDeclared && c.Parameters.Length == 0);
            return implicitCtor;
        }

        // Multiple constructors — callers emit a GK0002 diagnostic
        return null;
    }

    private static DiagnosticInfo CreateDiagnostic(InvocationExpressionSyntax invocation, string id, string message)
    {
        FileLinePositionSpan span = invocation.SyntaxTree.GetLineSpan(invocation.Span);
        return new DiagnosticInfo(
            id,
            message,
            span.Path,
            span.StartLinePosition.Line,
            span.StartLinePosition.Character,
            span.EndLinePosition.Line,
            span.EndLinePosition.Character);
    }

    private static string GenerateInterceptors(ImmutableArray<InterceptionInfo> interceptions)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine();
        sb.AppendLine("namespace System.Runtime.CompilerServices");
        sb.AppendLine("{");
        sb.AppendLine("#pragma warning disable CS9113");
        sb.AppendLine("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]");
        sb.AppendLine("    file sealed class InterceptsLocationAttribute(int version, string data) : global::System.Attribute;");
        sb.AppendLine("#pragma warning restore CS9113");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("namespace GameKit.DependencyInjection.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    file static class ServiceCollectionInterceptors");
        sb.AppendLine("    {");

        for (int i = 0; i < interceptions.Length; i++)
        {
            InterceptionInfo info = interceptions[i];

            if (i > 0)
            {
                sb.AppendLine();
            }

            switch (info.Kind)
            {
                case InterceptionKind.AddSingleton:
                    GenerateAddSingletonInterceptor(sb, info, i);
                    break;
                case InterceptionKind.AddSingletonWithAlias:
                    GenerateAddSingletonWithAliasInterceptor(sb, info, i);
                    break;
                case InterceptionKind.AddSingletonFactory:
                    GenerateAddSingletonFactoryInterceptor(sb, info, i);
                    break;
                case InterceptionKind.OnStart:
                    GenerateOnStartInterceptor(sb, info, i);
                    break;
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateAddSingletonInterceptor(StringBuilder sb, InterceptionInfo info, int index)
    {
        sb.AppendLine($"        {info.InterceptsLocationAttribute}");
        sb.AppendLine($"        public static void AddSingleton_{index}<T>(");
        sb.AppendLine($"            this global::GameKit.DependencyInjection.ServiceCollection collection)");
        sb.AppendLine($"            where T : class");
        sb.AppendLine($"        {{");
        sb.Append($"            collection.AddSingleton<{info.ImplementationTypeFullName}>(static sp => new {info.ImplementationTypeFullName}(");

        for (int j = 0; j < info.ConstructorParameterTypes.Length; j++)
        {
            if (j > 0)
            {
                sb.Append(", ");
            }
            sb.Append($"sp.GetService<{info.ConstructorParameterTypes[j]}>()");
        }

        sb.AppendLine("));");
        sb.AppendLine($"        }}");
    }

    private static void GenerateAddSingletonWithAliasInterceptor(StringBuilder sb, InterceptionInfo info, int index)
    {
        sb.AppendLine($"        {info.InterceptsLocationAttribute}");
        sb.AppendLine($"        public static void AddSingleton_{index}<TService, TImplementation>(");
        sb.AppendLine($"            this global::GameKit.DependencyInjection.ServiceCollection collection)");
        sb.AppendLine($"            where TService : class");
        sb.AppendLine($"            where TImplementation : class, TService");
        sb.AppendLine($"        {{");
        sb.Append($"            collection.AddSingleton<{info.ServiceTypeFullName}>(static sp => new {info.ImplementationTypeFullName}(");

        for (int j = 0; j < info.ConstructorParameterTypes.Length; j++)
        {
            if (j > 0)
            {
                sb.Append(", ");
            }
            sb.Append($"sp.GetService<{info.ConstructorParameterTypes[j]}>()");
        }

        sb.AppendLine("));");
        sb.AppendLine($"        }}");
    }

    private static void GenerateAddSingletonFactoryInterceptor(StringBuilder sb, InterceptionInfo info, int index)
    {
        sb.AppendLine($"        {info.InterceptsLocationAttribute}");
        sb.AppendLine($"        public static void AddSingleton_{index}<T>(");
        sb.AppendLine($"            this global::GameKit.DependencyInjection.ServiceCollection collection,");
        sb.AppendLine($"            global::System.Delegate factory)");
        sb.AppendLine($"            where T : class");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            {info.DelegateTypeFullName} typedFactory = ({info.DelegateTypeFullName})factory;");
        sb.Append($"            collection.AddSingleton<{info.ServiceTypeFullName}>(sp => typedFactory(");

        for (int j = 0; j < info.DelegateParameterTypes.Length; j++)
        {
            if (j > 0)
            {
                sb.Append(", ");
            }
            sb.Append($"sp.GetService<{info.DelegateParameterTypes[j]}>()");
        }

        sb.AppendLine("));");
        sb.AppendLine($"        }}");
    }

    private static void GenerateOnStartInterceptor(StringBuilder sb, InterceptionInfo info, int index)
    {
        sb.AppendLine($"        {info.InterceptsLocationAttribute}");
        sb.AppendLine($"        public static void OnStart_{index}(");
        sb.AppendLine($"            this global::GameKit.DependencyInjection.ServiceCollection collection,");
        sb.AppendLine($"            global::System.Delegate action)");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            {info.DelegateTypeFullName} typedAction = ({info.DelegateTypeFullName})action;");
        sb.AppendLine($"            collection.OnStart(sp => typedAction(");

        for (int j = 0; j < info.DelegateParameterTypes.Length; j++)
        {
            if (j > 0)
            {
                sb.Append(", ");
            }
            sb.Append($"sp.GetService<{info.DelegateParameterTypes[j]}>()");
        }

        sb.AppendLine("));");
        sb.AppendLine($"        }}");
    }
}
