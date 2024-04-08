using Microsoft.CodeAnalysis;
using System.Linq;

namespace Condor.Generator.Utils.Visitors
{
    public sealed class ActionVisitor : SymbolVisitor<ActionInfo>
    {
        public static readonly ActionVisitor Instance = new();

        public override ActionInfo VisitMethod(IMethodSymbol symbol)
        {
            return new ActionInfo
            {
                AccessibilityModifier = symbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                IsStatic = symbol.IsStatic,
                Name = symbol.Accept(FriendlyNameVisitor.Instance),
                Definition = symbol.Accept(FriendlyDefinitionVisitor.Instance),
                ReturnType = symbol.ReturnType.Accept(TargetTypeVisitor.Instance),
                IsVoid = symbol.ReturnType.SpecialType == SpecialType.System_Void,
                TypeArguments = symbol.TypeArguments.Select(x => x.Accept(TargetTypeVisitor.Instance)).ToArray(),
                Parameters = symbol.Parameters.Select(x => x.Accept(ParameterVisitor.Instance)).ToArray(),
                Attributes = symbol.Accept(AttributesVisitor.Instance)
            };
        }
    }
}
