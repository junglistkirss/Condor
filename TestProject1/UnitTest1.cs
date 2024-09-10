using Condor.Visitor.Generator;
using Condor.Visitor.Generator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;


namespace Testproject1;


public class VisitorGeneratorTests
{
    [Fact]
    public void TestBasicVisitorGeneration_interface()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType {}

    [Visitor]
    [Acceptor<MyType>]
    public partial interface TestVisitor {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface TestVisitor", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType element);", result.ToString());
    }

    [Fact]
    public void TestBasicVisitorGeneration_class()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType {}

    [Visitor]
    [Acceptor<MyType>]
    public partial class TestVisitor {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial class TestVisitor", result.ToString());
        Assert.Contains("public partial void Visit(TestNamespace.MyType element);", result.ToString());
    }

    [Fact]
    public void TestBasicVisitorGeneration_auto_class()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor]
    [AutoAcceptor<MyBaseType>]
    public partial class TestVisitor {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial class TestVisitor", result.ToString());
        Assert.Contains("public partial void Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("public partial void Visit(TestNamespace.MyType2 element);", result.ToString());
    }

    [Fact]
    public void TestBasicVisitorGeneration_auto_interface()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor]
    [AutoAcceptor<MyBaseType>]
    public partial interface TestVisitor {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface TestVisitor", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", result.ToString());
    }

    private static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(VisitorAttribute).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}