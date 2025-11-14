using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class ActionVisitor : SymbolVisitor<ActionInfo>
{
    public static readonly ActionVisitor Instance = new();

    public override ActionInfo VisitMethod(IMethodSymbol symbol)
    {
        return new ActionInfo
        {
            AccessibilityModifier = symbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
            IsStatic = symbol.IsStatic,
            Name = symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name"),
            Definition = symbol.Accept(FriendlyDefinitionVisitor.Instance) ?? throw new Exception("Unable to resolve friendly definition"),
            ReturnType = symbol.ReturnType.Accept(TargetTypeVisitor.Instance) ?? throw new Exception("Unable to resolve return type info"),
            IsVoid = symbol.ReturnType.SpecialType == SpecialType.System_Void,
            TypeArguments = [.. symbol.TypeArguments.Select(x => x.Accept(TargetTypeVisitor.Instance) ?? throw new Exception("Unable to resolve argument type info"))],
            Parameters = [.. symbol.Parameters.Select(x => x.Accept(ParameterVisitor.Instance) ?? throw new Exception("Unable to resolve parameter info"))],
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new Exception("Unable to resolve attributes info")
        };
    }
}
