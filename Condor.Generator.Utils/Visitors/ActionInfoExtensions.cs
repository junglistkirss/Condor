using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class ActionInfoExtensions
{
    public static ActionInfo? GetActionInfo(this IMethodSymbol symbol)
    {
        return symbol?.Accept(ActionVisitor.Instance);
    }

    public static ActionInfo RequireActionInfo(this IMethodSymbol symbol)
    {
        return symbol?.Accept(ActionVisitor.Instance) ?? throw new Exception($"Unable to resolve action info on [IMethodSymbol]=\"{symbol}\"");
    }

    private sealed class ActionVisitor : SymbolVisitor<ActionInfo>
    {
        public static readonly ActionVisitor Instance = new();

        public override ActionInfo VisitMethod(IMethodSymbol symbol)
        {
            return new ActionInfo
            {
                AccessibilityModifier = symbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                IsStatic = symbol.IsStatic,
                Name = symbol.RequireFriendlyname(),
                Definition = symbol.RequireFriendlyDefinition(),
                ReturnType = symbol.ReturnType.RequireTargetTypeInfo(),
                IsVoid = symbol.ReturnType.SpecialType == SpecialType.System_Void,
                TypeArguments = [.. symbol.TypeArguments.Select(x => x.RequireTargetTypeInfo())],
                Parameters = [.. symbol.Parameters.Select(x => x.RequireParameterInfo())],
                Attributes = symbol.RequireAttributesInfo()
            };
        }
    }
}
