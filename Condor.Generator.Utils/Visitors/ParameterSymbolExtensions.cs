using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class ParameterSymbolExtensions
{
    public static ParameterInfo? GetParameterInfo(this IParameterSymbol symbol)
    {
        return symbol.Accept(ParameterVisitor.Instance);
    }

    public static ParameterInfo RequireParameterInfo(this IParameterSymbol symbol)
    {
        return symbol.Accept(ParameterVisitor.Instance) ?? throw new Exception($"Unable to resolve parameter info on [IParameterInfo]=\"{symbol}\"");
    }

    private sealed class ParameterVisitor : SymbolVisitor<ParameterInfo>
    {
        public static readonly ParameterVisitor Instance = new();
        public override ParameterInfo VisitParameter(IParameterSymbol symbol)
        {
            return new ParameterInfo
            {
                ParameterName = symbol.Name,
                HasDefaultExpression = symbol.HasExplicitDefaultValue,
                DefaultExpression = symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue : null,
                ParameterType = symbol.Type.RequireTargetTypeInfo(),
                IsOptional = symbol.IsOptional,
                IsParams = symbol.IsParams,
                IsExtension = symbol.IsThis,
                IsRef = symbol.RefKind == RefKind.None,
                IsOut = symbol.RefKind == RefKind.Out,
                IsIn = symbol.RefKind == RefKind.In,
                IsRefReadOnly = symbol.RefKind == RefKind.RefReadOnly,
                Attributes = symbol.RequireAttributesInfo(),
            };
        }
    }
}
