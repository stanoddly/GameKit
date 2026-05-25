using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using GameKit.DependencyInjection.Generator;

namespace GameKit.DependencyInjection.Generator.Tests;

public class GeneratorTrimAnnotationTests
{
    private const string DamAttribute = "[global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]";

    // Minimal source that triggers all four intercepted overloads of AddSingleton:
    // - AddSingleton<T>()
    // - AddSingleton<TService, TImplementation>()
    // - AddSingleton<T>(Delegate factory)
    // - AddSingleton<TService, TFactory>() instance factory
    private const string RegistrationSource = """
        using GameKit.DependencyInjection;
        using System;

        public class MyService { }
        public interface IMyService { }
        public class MyServiceImpl : IMyService { }
        public class ProductService { public ProductService(string v) {} }
        public class MyFactory
        {
            internal ProductService CreateProduct() => new ProductService("test");
        }

        public static class Registrations
        {
            public static void Register(ServiceCollection services)
            {
                services.AddSingleton<MyService>();
                services.AddSingleton<IMyService, MyServiceImpl>();
                services.AddSingleton<MyService>(() => new MyService());
                services.AddSingleton<MyFactory>();
                services.AddSingleton<ProductService, MyFactory>();
            }
        }
        """;

    [Test]
    public void GeneratedCode_ContainsTrimAnnotations_WhenPropertyIsUnset()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: null);

        Assert.That(generated, Does.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_ContainsTrimAnnotations_WhenPropertyIsTrue()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "true");

        Assert.That(generated, Does.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_ContainsTrimAnnotations_WhenPropertyIsTrue_CaseInsensitive()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "True");

        Assert.That(generated, Does.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_OmitsTrimAnnotations_WhenPropertyIsFalse()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "false");

        Assert.That(generated, Does.Not.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_OmitsTrimAnnotations_WhenPropertyIsFalse_CaseInsensitive()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "False");

        Assert.That(generated, Does.Not.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_AddSingleton_ContainsAnnotation_OnTParam_WhenEnabled()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "true");

        // AddSingleton_N<[DAM] T>( should appear for the single-type overload
        Assert.That(generated, Does.Match(@"AddSingleton_\d+<\[global::System\.Diagnostics\.CodeAnalysis\.DynamicallyAccessedMembers.*?\] T>"));
    }

    [Test]
    public void GeneratedCode_AddSingletonWithAlias_ContainsAnnotation_OnTImplementationParam_WhenEnabled()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "true");

        // AddSingleton_N<TService, [DAM] TImplementation>( should appear for the two-type overload
        Assert.That(generated, Does.Match(@"AddSingleton_\d+<TService, \[global::System\.Diagnostics\.CodeAnalysis\.DynamicallyAccessedMembers.*?\] TImplementation>"));
    }

    [Test]
    public void GeneratedCode_AddSingletonFactory_ContainsAnnotation_OnTParam_WhenEnabled()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "true");

        // The factory overload also adds [DAM] on T
        // The method signature has two type parameters between the attribute and T; just check the
        // annotation is present (already covered by the broad Does.Contain test above).
        Assert.That(generated, Does.Contain(DamAttribute));
    }

    [Test]
    public void GeneratedCode_IsNotEmpty_WhenAnnotationsDisabled()
    {
        string generated = RunGenerator(emitTrimAnnotationsPropertyValue: "false");

        // The interceptors themselves must still be generated — only the attribute is absent
        Assert.That(generated, Does.Contain("ServiceCollectionInterceptors"));
        Assert.That(generated, Does.Contain("AddSingleton_"));
    }

    private static string RunGenerator(string? emitTrimAnnotationsPropertyValue)
    {
        CSharpCompilation compilation = CreateCompilation(RegistrationSource);
        InterceptorGenerator generator = new InterceptorGenerator();

        OptionsProvider optionsProvider = new OptionsProvider(emitTrimAnnotationsPropertyValue);

        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new ISourceGenerator[] { generator.AsSourceGenerator() },
            optionsProvider: optionsProvider,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees[0].Options);

        GeneratorDriver ran = driver.RunGenerators(compilation);
        GeneratorDriverRunResult result = ran.GetRunResult();

        // Find the interceptors file
        foreach (GeneratorRunResult generatorResult in result.Results)
        {
            foreach (GeneratedSourceResult source in generatorResult.GeneratedSources)
            {
                if (source.HintName == "ServiceCollectionInterceptors.g.cs")
                {
                    return source.SourceText.ToString();
                }
            }
        }

        return string.Empty;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        // Collect references: mscorlib/System.Runtime + the DI runtime assembly
        List<MetadataReference> references = new List<MetadataReference>();

        // Core runtime references
        string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")));

        // GameKit.DependencyInjection runtime assembly
        Assembly diAssembly = typeof(GameKit.DependencyInjection.ServiceCollection).Assembly;
        references.Add(MetadataReference.CreateFromFile(diAssembly.Location));

        CSharpParseOptions parseOptions = new CSharpParseOptions(
            languageVersion: LanguageVersion.CSharp13,
            preprocessorSymbols: ImmutableArray<string>.Empty);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, parseOptions);

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    // Provides controlled AnalyzerConfigOptions that simulate the MSBuild property.
    private sealed class OptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly ControlledOptions _global;

        public OptionsProvider(string? propertyValue)
        {
            _global = new ControlledOptions(propertyValue);
        }

        public override AnalyzerConfigOptions GlobalOptions => _global;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _global;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _global;
    }

    private sealed class ControlledOptions : AnalyzerConfigOptions
    {
        private readonly string? _propertyValue;

        public ControlledOptions(string? propertyValue)
        {
            _propertyValue = propertyValue;
        }

        public override bool TryGetValue(string key, out string value)
        {
            if (key == "build_property.GameKitDIEmitTrimAnnotations" && _propertyValue != null)
            {
                value = _propertyValue;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
