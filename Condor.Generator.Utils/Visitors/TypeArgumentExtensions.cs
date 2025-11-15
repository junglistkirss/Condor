using Microsoft.CodeAnalysis;
using System.Net.NetworkInformation;

namespace Condor.Generator.Utils.Visitors;

public static class TypeArgumentExtensions
{
    public static TypeArgumentInfo? GetTypeArgument(this ITypeParameterSymbol symbol)
    {
        return symbol.Accept(TypeArgumentVisitor.Instance);
    }
    public static TypeArgumentInfo RequireTypeArgument(this ITypeParameterSymbol symbol)
    {
        return symbol.Accept(TypeArgumentVisitor.Instance) ?? throw new Exception($"Unable to resolve type argument info on [ITypeParameterSymbol]=\"{symbol}\"");
    }

    private sealed class TypeArgumentVisitor : SymbolVisitor<TypeArgumentInfo>
    {
        public static readonly TypeArgumentVisitor Instance = new();
        public override TypeArgumentInfo VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            return new TypeArgumentInfo
            {
                Name = symbol.Name,
                HasConstraint = symbol.HasValueTypeConstraint,
                Contraints = symbol.HasValueTypeConstraint ? [.. symbol.ConstraintTypes.Select(x => x.RequireTargetTypeInfo())] : [],
                IsNullable = symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated,
                IsIn = symbol.Variance == VarianceKind.In,
                IsOut = symbol.Variance == VarianceKind.Out,
                IsVarianceUnspecified = symbol.Variance == VarianceKind.None,
            };
        }
    }
}
