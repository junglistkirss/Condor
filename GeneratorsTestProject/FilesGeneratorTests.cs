using Condor.Constants.Generator;
using Condor.Constants.Generator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;


namespace GeneratorsTestProject;


public class FilesGeneratorTests
{
    [Fact(Skip = "Should be reveiwed")]
    public void Constants_auto_interface_filter()
    {
        // Arrange
        var source = @"
using Condor.Files.Generator.Abstractions;

namespace TestNamespace
{
    [Files(""template"", "".*"")]
    public class TestConstants {}
}";

        var compilation = CreateCompilation(source);
        var generator = new TemplatedFilesGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts([
                new StringAdditionalText("template.mustache","{{#each Files}}{{./FileName}}{{/each}}"),
                new StringAdditionalText("icon.svg","SVG")
            ]);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("template", result.ToString());
        Assert.Contains("icon", result.ToString());

    }

    private static Compilation CreateCompilation(string source)
    {
        var o = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        //Assembly assembly = Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
        CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);
        Compilation compilation = CSharpCompilation.Create(
            "ConstantsGeneratorTests",
            [
                CSharpSyntaxTree.ParseText(source, options: new CSharpParseOptions(LanguageVersion.Latest))
            ],
            [
               MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location, MetadataReferenceProperties.Assembly),
                MetadataReference.CreateFromFile(typeof(ConstantsAttribute).GetTypeInfo().Assembly.Location, MetadataReferenceProperties.Assembly),
                .. o!.ToString()!.Split(";").Select(x => MetadataReference.CreateFromFile(x))
                //MetadataReference.CreateFromFile(assembly.Location),
            ],
            options);

        return compilation;
    }
    public class StringAdditionalText : AdditionalText
    {
        private readonly string path;
        private readonly string inline;

        public override string Path => path;

        public StringAdditionalText(string path, string inline)
        {
            this.path = path;
            this.inline = inline;
        }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(inline);
        }
    }
}