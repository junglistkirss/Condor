using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class MapMembersVisitor<T, TOut> : SymbolVisitor<TOut[]>
   where T : ISymbol
{
    private readonly Func<T, TOut> map;

    public MapMembersVisitor(Func<T, TOut> map)
    {
        this.map = map;
    }

    public override TOut[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [.. symbol.GetMembers().OfType<T>().Select(map)];
    }
}
