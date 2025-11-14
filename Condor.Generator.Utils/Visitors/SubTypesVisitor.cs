using Microsoft.CodeAnalysis;
using System.Linq;

namespace Condor.Generator.Utils.Visitors;


public class SubTypesVisitor : SymbolVisitor<INamedTypeSymbol[]>
{
    public readonly static SubTypesVisitor Instance = new();

    public override INamedTypeSymbol[] DefaultVisit(ISymbol symbol) => [];
    public override INamedTypeSymbol[] VisitNamespace(INamespaceSymbol symbol) => [.. symbol.GetMembers().Where(x => x.IsNamespace || x.IsType).SelectMany(x => x.Accept(Instance))];
    public override INamedTypeSymbol[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [symbol/*, .. symbol.GetMembers().SelectMany(x => x.Accept(Instance)).ToArray()*/];
    }
}

public class BaseTypesVisitor : SymbolVisitor<INamedTypeSymbol[]>
{
    public readonly static BaseTypesVisitor Instance = new();

    public override INamedTypeSymbol[] DefaultVisit(ISymbol symbol) => [];
    public override INamedTypeSymbol[] VisitNamedType(INamedTypeSymbol symbol)
    {
        if (symbol.BaseType is not null)
            return [symbol.BaseType, .. symbol.BaseType.Accept(Instance) ?? throw new Exception("Unable to resolve base type info")];
        return [];
    }
}
