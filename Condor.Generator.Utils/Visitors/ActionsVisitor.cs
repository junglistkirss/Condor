using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

//public sealed class MembersVisitor<T, TOut> : SymbolVisitor<(MemberInfo, TOut)[]>
//    where T : ISymbol
//{
//    public static MembersVisitor<T, TOut> Create(Func<T, TOut> func) => new MembersVisitor<T, TOut>(func);

//    private Func<T, TOut> func;

//    private MembersVisitor(Func<T, TOut> func)
//    {
//        this.func = func;
//    }


//    public override (MemberInfo, TOut)[] VisitNamedType(INamedTypeSymbol symbol)
//    {
//        return symbol.GetMembers().OfType<T>()
//            .Select(x => (x.Accept(MemberVisitor.Instance), func(x))).ToArray();
//    }
//}

public sealed class ActionsVisitor : SymbolVisitor<ActionInfo[]>
{
    public static readonly ActionsVisitor Instance = new();

    public override ActionInfo[] VisitNamedType(INamedTypeSymbol symbol)
    {
        return [.. symbol.GetMembers().OfType<IMethodSymbol>().Select(x => x.Accept(ActionVisitor.Instance) ?? throw new Exception("Unable to resolve action info"))];
    }
}
