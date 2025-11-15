using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class ActionsInfoExtensions
{
    public static IEnumerable<ActionInfo?> GetActionsInfo(this IMethodSymbol symbol)
    {
        return symbol.Accept(new ActionsVisitor<ActionInfo?>(s => s.GetActionInfo())) ?? [];
    }

    public static IEnumerable<ActionInfo> RequireActionsInfo(this IMethodSymbol symbol)
    {
        return symbol.Accept(new ActionsVisitor<ActionInfo>(s => s.RequireActionInfo())) ?? [];
    }
    private sealed class ActionsVisitor<TOut>(Func<IMethodSymbol, TOut> map) : SymbolVisitor<IEnumerable<TOut>>
    {
        public override IEnumerable<TOut> VisitNamedType(INamedTypeSymbol symbol)
        {
            return symbol.GetMembers().OfType<IMethodSymbol>().Select(map);
        }
    }
}
