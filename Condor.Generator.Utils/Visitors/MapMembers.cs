using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class MapMembers
{
    public static MapMembersVisitor<IPropertySymbol, TOut> Properties<TOut>(Func<IPropertySymbol, TOut> map) => new(map);
}
