using Condor.Constants.Generator;
using Condor.Constants.Generator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace GeneratorsTestProject;


public class ConstantsGeneratorTests
{
    [Fact]
    public void Constants_auto_interface_filter()
    {
        // Arrange
        string source = @"
using Condor.Constants.Generator.Abstractions;

namespace TestNamespace
{
    [Constants(""template"")]
    public partial class TestConstants
    {    
        public const string test = ""test"";
        public const string test2 = ""test"";
        public const string test3 = ""test"";
    }
}";
        StringAdditionalText template = new("template.mustache", @"namespace {{OutputNamespace}}
{
    public static partial class {{ClassName}}
    {
        public static IEnumerable<string> GetAll()
        {
            {{#Map}}
            yield return {{{Member.MemberName}}};
            {{/Map}}
        }
    }
}");
        string code = GenerateCode(source, out Diagnostic[] diagnostics, template);
        Assert.Empty(diagnostics);
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public static partial class TestConstants", code);
        Assert.Contains("yield return test;", code);
        Assert.Contains("yield return test2;", code);
        Assert.Contains("yield return test3;", code);

    }


    private static string GenerateCode(string source, out Diagnostic[] diagnostics, params StringAdditionalText[] additionalTexts)
    {
        Compilation compilation = CSharpCompilation.Create(
            "TestNamespace",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, options: new CSharpParseOptions(LanguageVersion.Latest))],
            references: [
                .. Basic.Reference.Assemblies.NetStandard20.References.All,
                MetadataReference.CreateFromFile(typeof(ConstantAttribute).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        ConstantsGenerator generator = new();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [GeneratorExtensions.AsSourceGenerator(generator)],
            parseOptions: new CSharpParseOptions(LanguageVersion.LatestMajor)
        );
        if (additionalTexts.Length > 0)
            driver = driver.AddAdditionalTexts([.. additionalTexts]);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation? _, out ImmutableArray<Diagnostic> compilationDiagnostics);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        diagnostics = [.. compilationDiagnostics];
        return result.ToString();
    }

    public class StringAdditionalText(string path, string inline) : AdditionalText
    {
        public override string Path => path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(inline);
        }
    }
}