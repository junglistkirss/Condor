using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class GenericBaseTypeExtensions
{
    public static string? GetGenericBaseTypeName(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol? namedTypeSymbol.GetGenericBaseTypeName() : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    }
    public static string RequireGenericBaseTypeName(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequireGenericBaseTypeName() : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    }
    public static string? GetGenericBaseTypeName(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(GenericBaseTypeNameVisitor.Instance);
    }

    public static string RequireGenericBaseTypeName(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(GenericBaseTypeNameVisitor.Instance) ?? throw new Exception($"Unable to resolve generic base type name [INamedTypeSymbol]=\"{symbol}\"");
    }

    private sealed class GenericBaseTypeNameVisitor : SymbolVisitor<string>
    {
        public static readonly GenericBaseTypeNameVisitor Instance = new();

        public override string VisitNamedType(INamedTypeSymbol x)
        {
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                    .RemoveGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters));
        }
    }
}
