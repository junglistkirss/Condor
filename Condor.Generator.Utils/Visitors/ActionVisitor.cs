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
            Name = symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new NullReferenceException("FriendlyName required"),
            Definition = symbol.Accept(FriendlyDefinitionVisitor.Instance) ?? throw new NullReferenceException("FriendlyDefinition required"),
            ReturnType = symbol.ReturnType.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("ReturnType required"),
            IsVoid = symbol.ReturnType.SpecialType == SpecialType.System_Void,
            TypeArguments = [.. symbol.TypeArguments.Select(x => x.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("TargetType required"))],
            Parameters = [.. symbol.Parameters.Select(x => x.Accept(ParameterVisitor.Instance) ?? throw new NullReferenceException("ParameterInfo required"))],
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new NullReferenceException("AttributeInfo required")
        };
    }
}
