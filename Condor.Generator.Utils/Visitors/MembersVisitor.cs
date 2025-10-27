using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class MembersVisitor<T> : SymbolVisitor<MemberInfo[]>
    where T : ISymbol
{
    public static readonly MembersVisitor<T> Instance = new();

    public override MemberInfo[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [.. symbol.GetMembers().OfType<T>().Select(x => x.Accept(MemberVisitor.Instance) ?? throw new NullReferenceException("MemberInfo required"))];
    }
}
