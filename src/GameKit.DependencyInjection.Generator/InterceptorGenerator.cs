using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
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
        IncrementalValuesProvider<InterceptionInfo> interceptions = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsCandidateInvocation(node),
            transform: static (ctx, ct) => ExtractInterception(ctx, ct)
        ).Where(static r => r is not null)
         .Select(static (r, _) => r!.Value);

        IncrementalValueProvider<ImmutableArray<InterceptionInfo>> collected = interceptions.Collect();

        context.RegisterSourceOutput(collected, static (spc, interceptions) =>
        {
            if (interceptions.IsEmpty)
            {
                return;
            }

            string source = GenerateInterceptors(interceptions);
            spc.AddSource("ServiceCollectionInterceptors.g.cs", source);
        });
    }

    private static bool IsCandidateInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        string methodName;
        if (memberAccess.Name is GenericNameSyntax genericName)
        {
            methodName = genericName.Identifier.Text;
        }
        else if (memberAccess.Name is IdentifierNameSyntax identifierName)
        {
            methodName = identifierName.Identifier.Text;
        }
        else
        {
            return false;
        }

        return methodName is "AddSingleton" or "OnStart";
    }

    private static InterceptionInfo? ExtractInterception(GeneratorSyntaxContext context, CancellationToken ct)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
        MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

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
            return ExtractOnStart(methodSymbol, invocation, context, interceptableLocation, ct);
        }

        return null;
    }

    private static InterceptionInfo? ExtractAddSingleton(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        GeneratorSyntaxContext context,
        InterceptableLocation interceptableLocation,
        CancellationToken ct)
    {
        // AddSingleton<T>(Delegate) — factory overload
        if (methodSymbol.TypeArguments.Length == 1 && methodSymbol.Parameters.Length == 1)
        {
            return ExtractDelegateInterception(
                InterceptionKind.AddSingletonFactory,
                invocation,
                context,
                interceptableLocation,
                ct);
        }

        // AddSingleton<T>() — single type param, no args
        if (methodSymbol.TypeArguments.Length == 1 && methodSymbol.Parameters.Length == 0)
        {
            return ExtractAddSingletonType(methodSymbol, interceptableLocation);
        }

        // AddSingleton<TService, TImpl>() — two type params, no args
        if (methodSymbol.TypeArguments.Length == 2 && methodSymbol.Parameters.Length == 0)
        {
            return ExtractAddSingletonWithAlias(methodSymbol, interceptableLocation);
        }

        return null;
    }

    private static InterceptionInfo? ExtractAddSingletonType(
        IMethodSymbol methodSymbol,
        InterceptableLocation interceptableLocation)
    {
        ITypeSymbol implType = methodSymbol.TypeArguments[0];

        if (implType is not INamedTypeSymbol implNamedType)
        {
            return null;
        }

        IMethodSymbol? constructor = GetSinglePublicConstructor(implNamedType);
        if (constructor == null)
        {
            return null;
        }

        ImmutableArray<string> paramTypes = constructor.Parameters
            .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToImmutableArray();

        return new InterceptionInfo(
            InterceptionKind.AddSingleton,
            interceptableLocation.GetInterceptsLocationAttributeSyntax(),
            implType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            new EquatableArray<string>(paramTypes),
            null,
            null,
            new EquatableArray<string>(ImmutableArray<string>.Empty),
            false
        );
    }

    private static InterceptionInfo? ExtractAddSingletonWithAlias(
        IMethodSymbol methodSymbol,
        InterceptableLocation interceptableLocation)
    {
        ITypeSymbol serviceType = methodSymbol.TypeArguments[0];
        ITypeSymbol implType = methodSymbol.TypeArguments[1];

        if (implType is not INamedTypeSymbol implNamedType)
        {
            return null;
        }

        IMethodSymbol? constructor = GetSinglePublicConstructor(implNamedType);
        if (constructor == null)
        {
            return null;
        }

        ImmutableArray<string> paramTypes = constructor.Parameters
            .Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .ToImmutableArray();

        return new InterceptionInfo(
            InterceptionKind.AddSingletonWithAlias,
            interceptableLocation.GetInterceptsLocationAttributeSyntax(),
            implType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            new EquatableArray<string>(paramTypes),
            serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            null,
            new EquatableArray<string>(ImmutableArray<string>.Empty),
            false
        );
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
            ct);
    }

    private static InterceptionInfo? ExtractDelegateInterception(
        InterceptionKind kind,
        InvocationExpressionSyntax invocation,
        GeneratorSyntaxContext context,
        InterceptableLocation interceptableLocation,
        CancellationToken ct)
    {
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        ExpressionSyntax argExpression = invocation.ArgumentList.Arguments[0].Expression;

        // Get the type info of the delegate argument
        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(argExpression, ct);
        ITypeSymbol? delegateType = typeInfo.Type ?? typeInfo.ConvertedType;

        if (delegateType is not INamedTypeSymbol namedDelegateType)
        {
            return null;
        }

        IMethodSymbol? invokeMethod = namedDelegateType.DelegateInvokeMethod;
        if (invokeMethod == null)
        {
            return null;
        }

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
            null,
            delegateTypeStr,
            new EquatableArray<string>(paramTypes),
            returnsVoid
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

        // Multiple constructors — don't intercept, let runtime throw
        return null;
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
        sb.Append($"            collection.AddSingletonGenerated<T>(static sp => new {info.ImplementationTypeFullName}(");

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
        sb.AppendLine($"            where TImplementation : class");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (!collection.IsRegistered<{info.ImplementationTypeFullName}>())");
        sb.AppendLine($"            {{");
        sb.Append($"                collection.AddSingletonGenerated<{info.ImplementationTypeFullName}>(static sp => new {info.ImplementationTypeFullName}(");

        for (int j = 0; j < info.ConstructorParameterTypes.Length; j++)
        {
            if (j > 0)
            {
                sb.Append(", ");
            }
            sb.Append($"sp.GetService<{info.ConstructorParameterTypes[j]}>()");
        }

        sb.AppendLine("));");
        sb.AppendLine($"            }}");
        sb.AppendLine($"            collection.AddAlias<{info.ServiceTypeFullName}, {info.ImplementationTypeFullName}>();");
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
        sb.Append($"            collection.AddSingletonGenerated<T>(sp => (object)typedFactory(");

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
        sb.AppendLine($"            collection.OnStartGenerated(sp => typedAction(");

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
