using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class FriendlyNameVisitor : SymbolVisitor<string>
{
    public static readonly FriendlyNameVisitor Instance = new();

    public override string VisitNamespace(INamespaceSymbol x)
    {
        return x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
    }
    public override string VisitParameter(IParameterSymbol x)
    {
        return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    public override string VisitMethod(IMethodSymbol x)
    {
        return x.Name;
    }
    public override string VisitNamedType(INamedTypeSymbol x)
    {
        //if (x.IsGenericType)
        //{
        //    return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None));
        //}
        if (x.IsUnmanagedType)
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.ExpandNullable));
    }
}

public sealed class GenericBaseTypeNameVisitor : SymbolVisitor<string>
{
    public static readonly GenericBaseTypeNameVisitor Instance = new();

    public override string VisitNamedType(INamedTypeSymbol x)
    {
        return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                .RemoveGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters));
    }
}
