using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class MembersVisitor<T> : SymbolVisitor<MemberInfo[]>
    where T : ISymbol
{
    public static readonly MembersVisitor<T> Instance = new();

    public override MemberInfo[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [.. symbol.GetMembers().OfType<T>().Select(x => x.Accept(MemberVisitor.Instance) ?? throw new Exception("Unable to resolve member info"))];
    }
}

public static class MapMembers
{
    public static MapMembersVisitor<IPropertySymbol, TOut> Properties<TOut>(Func<IPropertySymbol, TOut> map)
        => new(map);


}
public sealed class MapMembersVisitor<T, TOut>(Func<T, TOut> map) : SymbolVisitor<TOut[]>
   where T : ISymbol
{
    private readonly Func<T, TOut> map = map;

    public override TOut[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [.. symbol.GetMembers().OfType<T>().Select(map)];
    }
}
