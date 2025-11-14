using Condor.Visitor.Generator;
using Condor.Visitor.Generator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;


namespace GeneratorsTestProject;

public class VisitorGeneratorTests
{
    [Fact]
    public void Visitor_auto_interface_filter()
    {
        // Arrange
        string source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public interface IBaseType {}
    public interface ISubType : IBaseType {}
    public class MyType1 : IBaseType{}
    public class MyType2 : IBaseType{}
    public class MyType3 : IBaseType{}
    public class MyType4 : IBaseType{}
    public class MyType5 : IBaseType{}
    public class MyType6 : IBaseType{}
    public class MyType7 : IBaseType{}
    public class ExtraType {}

    [Visitor]
    [AutoAcceptor<IBaseType>()]
    public partial interface TestVisitor {}
}";
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface TestVisitor", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
        Assert.DoesNotContain("void Visit(TestNamespace.IBaseType element);", code);
        Assert.DoesNotContain("void Visit(TestNamespace.ISubType element);", code);
        Assert.DoesNotContain("void Visit(TestNamespace.ExtraType element);", code);
    }

    [Fact]
    public void Visitor_auto_interface_filterClass()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface TestVisitor", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
        Assert.DoesNotContain("void Visit(TestNamespace.BaseType element);", code);
        Assert.DoesNotContain("void Visit(TestNamespace.ExtraType element);", code);
    }

    [Fact]
    public void Visitor_auto_interface()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface TestVisitor", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_auto_interface__redirect()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface TestVisitorWithRedirect", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
        Assert.Contains("void VisitRedirect(TestNamespace.MyBaseType element);", code);
    }

    [Fact]
    public void Visitor_auto_interface_generic_in_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<out T, in TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Visitor_auto_interface_generic_in()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<T, in TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Visitor_auto_interface_generic()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<T, TArgs>", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_auto_interface_generic_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<out T, TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Async_Visitor_auto_interface()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);
        Assert.Contains("public partial interface ITestVisitorAsync", code);
        Assert.Contains("ValueTask Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("ValueTask Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Async_Visitor_auto_interface_generic_in_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitorAsync<out T, in TArgs>", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Async_Visitor_auto_interface_generic_in()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitorAsync<T, in TArgs>", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Async_Visitor_auto_interface_generic()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitorAsync<T, TArgs>", code);
        Assert.Contains("ValueTask Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("ValueTask Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Async_Visitor_auto_interface_generic_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitorAsync<out T, TArgs>", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("ValueTask<T> Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_interfaceFallBack()
    {
        // Arrange
        string source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public abstract class MyType {}
    public class MyType1 : MyType {}
    public class MyType2 : MyType {}

    [Visitor]
    [AutoAcceptor<MyType>(Accept = AcceptedKind.Concrete, AddVisitRedirect = true, AddVisitFallback = true)]
    [GenerateVisitable, GenerateDefault(Options = OptionsDefault.AsbtractPartial, VisitOptions = VisitOptions.AbstractVisit)]
    public partial interface TestVisitor : ITestVisitable {}
}";
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);
        Assert.Contains("public partial interface TestVisitor", code);
        Assert.DoesNotContain("void Visit(TestNamespace.MyType element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
        Assert.Contains("public abstract void VisitFallback(TestNamespace.MyType element)", code);
    }

    [Fact]
    public void Visitor_interface()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface TestVisitor", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_interface_generic_in_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<out T, in TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Visitor_interface_generic_in()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<T, in TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element, TArgs args);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element, TArgs args);", code);
    }

    [Fact]
    public void Visitor_interface_generic()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<T, TArgs>", code);
        Assert.Contains("void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("void Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_interface_generic_out()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial interface ITestVisitor<out T, TArgs>", code);
        Assert.Contains("T Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("T Visit(TestNamespace.MyType2 element);", code);
    }

    [Fact]
    public void Visitor_class()
    {
        // Arrange
        string source = @"
using Condor.Visitor.Generator.Abstractions;

namespace TestNamespace
{
    public class MyType {}

    [Visitor]
    [Acceptor<MyType>]
    public partial class TestVisitor {}
}";
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial class TestVisitor", code);
        Assert.Contains("public partial void Visit(TestNamespace.MyType element);", code);
    }

    [Fact]
    public void Visitor_auto_class()
    {
        // Arrange
        string source = @"
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
        string code = GenerateCode(source, out Diagnostic[] diagnostics);
        Assert.Empty(diagnostics);

        Assert.Contains("public partial class TestVisitor", code);
        Assert.Contains("public partial void Visit(TestNamespace.MyType1 element);", code);
        Assert.Contains("public partial void Visit(TestNamespace.MyType2 element);", code);
    }

    private static string GenerateCode(string source, out Diagnostic[] diagnostics)
    {
        Compilation compilation = CSharpCompilation.Create(
            "TestNamespace",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, options: new CSharpParseOptions(LanguageVersion.Latest))],
            references: [
                .. Basic.Reference.Assemblies.NetStandard20.References.All,
                MetadataReference.CreateFromFile(typeof(VisitorAttribute).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        VisitorGenerator generator = new();
        // Act
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [GeneratorExtensions.AsSourceGenerator(generator)],
            parseOptions: new CSharpParseOptions(LanguageVersion.LatestMajor)
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation? outputCompilation, out ImmutableArray<Diagnostic> compilationDiagnostics);
        // Assert
        SyntaxTree result = Assert.Single(driver.GetRunResult().GeneratedTrees);
        diagnostics = [.. compilationDiagnostics];
        return result.ToString();
    }
}