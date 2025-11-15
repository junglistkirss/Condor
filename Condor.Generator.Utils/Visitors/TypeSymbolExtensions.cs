using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class TypeSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetSubTypes(this INamespaceSymbol typeSymbol)
    {
        return typeSymbol.Accept(SubTypesVisitor.Instance) ?? [];
    }
    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.Accept(BaseTypesVisitor.Instance) ?? [];
    }

    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ISymbol typeSymbol)
    {
        return typeSymbol.Accept(BaseTypesVisitor.Instance) ?? [];
    }
    private class SubTypesVisitor : SymbolVisitor<IEnumerable<INamedTypeSymbol>>
    {
        public readonly static SubTypesVisitor Instance = new();

        public override IEnumerable<INamedTypeSymbol> DefaultVisit(ISymbol symbol) => [];
        public override IEnumerable<INamedTypeSymbol> VisitNamespace(INamespaceSymbol symbol) => [.. symbol.GetMembers().Where(x => x.IsNamespace || x.IsType).SelectMany(x => x.Accept(Instance))];
        public override IEnumerable<INamedTypeSymbol> VisitNamedType(INamedTypeSymbol symbol)
        {
            return [symbol, .. symbol.GetMembers().OfType<ITypeSymbol>().SelectMany(x => x.Accept(Instance)).ToArray()];
        }
    }

    private class BaseTypesVisitor : SymbolVisitor<IEnumerable<INamedTypeSymbol>>
    {
        public readonly static BaseTypesVisitor Instance = new();

        public override IEnumerable<INamedTypeSymbol> DefaultVisit(ISymbol symbol) => [];
        public override IEnumerable<INamedTypeSymbol> VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.BaseType is not null)
                return [symbol.BaseType, .. symbol.BaseType.Accept(Instance) ?? throw new Exception("Unable to resolve base type info")];
            return [];
        }
    }
}
