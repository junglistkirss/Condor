using Microsoft.CodeAnalysis;
using System.Linq;

namespace Condor.Generator.Utils.Visitors;

//public sealed class ActionsVisitor<TOut> : SymbolVisitor<(ActionInfo, TOut)[]>
//{
//    public static ActionsVisitor<TOut> Create(Func<IMethodSymbol, TOut> func) => new ActionsVisitor<TOut>(func);
//    private Func<IMethodSymbol, TOut> func;
//    private ActionsVisitor(Func<IMethodSymbol, TOut> func)
//    {
//        this.func = func;
//    }
//    public override (ActionInfo, TOut)[] VisitNamedType(INamedTypeSymbol symbol)
//    {
//        return symbol.GetMembers().OfType<IMethodSymbol>()
//            .Select(x => (x.Accept(ActionVisitor.Instance), func(x))).ToArray();
//    }
//}

public sealed class ParameterVisitor : SymbolVisitor<ParameterInfo>
{
    public static readonly ParameterVisitor Instance = new();
    public override ParameterInfo VisitParameter(IParameterSymbol symbol)
    {
        return new ParameterInfo
        {
            ParameterName = symbol.Name,
            HasDefaultExpression = symbol.HasExplicitDefaultValue,
            DefaultExpression = symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue : null,
            ParameterType = symbol.Type.Accept(TargetTypeVisitor.Instance),
            IsOptional = symbol.IsOptional,
            IsParams = symbol.IsParams,
            IsExtension = symbol.IsThis,
            IsRef = symbol.RefKind == RefKind.None,
            IsOut = symbol.RefKind == RefKind.Out,
            IsIn = symbol.RefKind == RefKind.In,
            IsRefReadOnly = symbol.RefKind == RefKind.RefReadOnly,
            Attributes = symbol.Accept(AttributesVisitor.Instance),
        };
    }
}

public sealed class TypeArgumentVisitor : SymbolVisitor<TypeArgumentInfo>
{
    public static readonly TypeArgumentVisitor Instance = new();
    public override TypeArgumentInfo VisitTypeParameter(ITypeParameterSymbol symbol)
    {
        return new TypeArgumentInfo
        {
            Name = symbol.Name,
            HasConstraint = symbol.HasValueTypeConstraint,
            Contraints = symbol.HasValueTypeConstraint ? symbol.ConstraintTypes.Select(x => x.Accept(TargetTypeVisitor.Instance)).ToArray() : [],
            IsNullable = symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated,
            IsIn = symbol.Variance == VarianceKind.In,
            IsOut = symbol.Variance == VarianceKind.Out,
            IsVarianceUnspecified = symbol.Variance == VarianceKind.None,
        };
    }
}
