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
    public void Visitor_auto_interface_filter()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public interface IBaseType {}
    public interface ISubType : IBaseType {}
    public class MyType1 : IBaseType{}
    public class MyType2 : IBaseType{}
    public class ExtraType {}

    [Visitor]
    [AutoAcceptor<IBaseType>()]
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
        Assert.DoesNotContain("void Visit(TestNamespace.IBaseType element);", result.ToString());
        Assert.DoesNotContain("void Visit(TestNamespace.ISubType element);", result.ToString());
        Assert.DoesNotContain("void Visit(TestNamespace.ExtraType element);", result.ToString());
    }

    [Fact]
    public void Visitor_auto_interface_filterClass()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class BaseType {}
    public class MyType1 : BaseType{}
    public class MyType2 : BaseType{}
    public class ExtraType {}

    [Visitor]
    [AutoAcceptor<BaseType>()]
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
        Assert.DoesNotContain("void Visit(TestNamespace.BaseType element);", result.ToString());
        Assert.DoesNotContain("void Visit(TestNamespace.ExtraType element);", result.ToString());
    }

    [Fact]
    public void Visitor_auto_interface()
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

    [Fact]
    public void Visitor_auto_interface__redirect()
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
    [AutoAcceptor<MyBaseType>(AddVisitRedirect = true)]
    public partial interface TestVisitorWithRedirect {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface TestVisitorWithRedirect", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", result.ToString());
        Assert.Contains("void VisitRedirect(TestNamespace.MyBaseType element);", result.ToString());
    }
    [Fact]
    public void Visitor_auto_interface_generic_in_out()
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
    public partial interface ITestVisitor<out T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<out T, in TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Visitor_auto_interface_generic_in()
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
    public partial interface ITestVisitor<T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<T, in TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Visitor_auto_interface_generic()
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
    public partial interface ITestVisitor<T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<T, TArgs>", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", result.ToString());
    }
    [Fact]
    public void Visitor_auto_interface_generic_out()
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
    public partial interface ITestVisitor<out T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<out T, TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element);", result.ToString());
    }

    [Fact]
    public void Async_Visitor_auto_interface()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor(IsAsync = true)]
    [AutoAcceptor<MyBaseType>]
    public partial interface ITestVisitorAsync {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create([GeneratorExtensions.AsSourceGenerator(generator)], parseOptions: new CSharpParseOptions(LanguageVersion.LatestMajor));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var compil, out var diag);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitorAsync", result.ToString());
        Assert.Contains("ValueTask Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("ValueTask Visit(TestNamespace.MyType2 element);", result.ToString());
    }
    [Fact]
    public void Async_Visitor_auto_interface_generic_in_out()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor(IsAsync = true)]
    [AutoAcceptor<MyBaseType>]
    public partial interface ITestVisitorAsync<out T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitorAsync<out T, in TArgs>", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Async_Visitor_auto_interface_generic_in()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor(IsAsync = true)]
    [AutoAcceptor<MyBaseType>]
    public partial interface ITestVisitorAsync<T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitorAsync<T, in TArgs>", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Async_Visitor_auto_interface_generic()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor(IsAsync = true)]
    [AutoAcceptor<MyBaseType>]
    public partial interface ITestVisitorAsync<T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitorAsync<T, TArgs>", result.ToString());
        Assert.Contains("ValueTask Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("ValueTask Visit(TestNamespace.MyType2 element);", result.ToString());
    }

    [Fact]
    public void Async_Visitor_auto_interface_generic_out()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyBaseType {}
    public class MyType1 : MyBaseType{}
    public class MyType2 : MyBaseType{}

    [Visitor(IsAsync = true)]
    [AutoAcceptor<MyBaseType>]
    public partial interface ITestVisitorAsync<out T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitorAsync<out T, TArgs>", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element);", result.ToString());
    }


    [Fact]
    public void Visitor_interfaceFallBack()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public abstract class MyType {}
    public class MyType1 : MyType {}
    public class MyType2 : MyType {}

    [Visitor]
    [AutoAcceptor<MyType>(Accept = AcceptedKind.Concrete, AddVisitRedirect = true, AddVisitFallBack = true)]
    [GenerateVisitable, GenerateDefault(Options = OptionsDefault.AsbtractPartial, VisitOptions = VisitOptions.AbstractVisit)]
    public partial interface TestVisitor {}
}";
        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        var compilationDiagnostics = compilation.GetDiagnostics();
            Assert.Empty(compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [GeneratorExtensions.AsSourceGenerator(generator)],
            parseOptions: new CSharpParseOptions(LanguageVersion.LatestMajor)
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        string code = result.ToString();
        Assert.Contains("public partial interface TestVisitor", code);
        Assert.DoesNotContain("void Visit(TestNamespace.MyType element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
        Assert.Contains("public abstract void VisitFallBack(TestNamespace.MyType element)", code);
    }
    [Fact]
    public void Visitor_interface()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType1 {}
    public class MyType2 {}

    [Visitor]
    [Acceptor<MyType1>]
    [Acceptor<MyType2>]
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
    [Fact]
    public void Visitor_interface_generic_in_out()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType1 {}
    public class MyType2 {}

    [Visitor]
    [Acceptor<MyType1>]
    [Acceptor<MyType2>]
    public partial interface ITestVisitor<out T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<out T, in TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Visitor_interface_generic_in()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType1 {}
    public class MyType2 {}

    [Visitor]
    [Acceptor<MyType1>]
    [Acceptor<MyType2>]
    public partial interface ITestVisitor<T, in TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<T, in TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", result.ToString());
    }
    [Fact]
    public void Visitor_interface_generic()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType1 {}
    public class MyType2 {}

    [Visitor]
    [Acceptor<MyType1>]
    [Acceptor<MyType2>]
    public partial interface ITestVisitor<T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<T, TArgs>", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", result.ToString());
    }
    [Fact]
    public void Visitor_interface_generic_out()
    {
        // Arrange
        var source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType1 {}
    public class MyType2 {}

    [Visitor]
    [Acceptor<MyType1>]
    [Acceptor<MyType2>]
    public partial interface ITestVisitor<out T, TArgs> {}
}";

        var compilation = CreateCompilation(source);
        var generator = new VisitorGenerator();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        Assert.Contains("public partial interface ITestVisitor<out T, TArgs>", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType1 element);", result.ToString());
        Assert.Contains("T Visit(TestNamespace.MyType2 element);", result.ToString());
    }

    [Fact]
    public void Visitor_class()
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
    public void Visitor_auto_class()
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


    private static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create(
            "TestNamespace",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, options: new CSharpParseOptions(LanguageVersion.Latest))],
            references: [
                .. Basic.Reference.Assemblies.NetStandard20.References.All,
                // MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                // MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                // MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(VisitorAttribute).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}