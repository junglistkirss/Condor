using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Condor.Generator.Utils.Visitors;

public sealed class AttributesVisitor : SymbolVisitor<AttributeInfo[]>
{
    public static readonly AttributesVisitor Instance = new();

    private static AttributeInfo[] GetAttributeInfos(ImmutableArray<AttributeData> datas)
    {
        List<AttributeInfo> all = [];
        for (int i = 0; i < datas.Length; i++)
        {
            var attr = datas[i];
            if (attr.AttributeClass is not null)
                all.Add(new AttributeInfo
                {
                    AttributeType = attr.AttributeClass.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("TargetType required"),
                    Constructor = attr.AttributeConstructor?.Accept(ActionVisitor.Instance),
                    ConstructorArguments = [.. attr.ConstructorArguments.Select(x => new ArgumentInfo
                    {
                        ArgumentName = null,
                        ArgumentValue = x.Kind == TypedConstantKind.Array ? x.Values : x.Value,
                        IsNull = x.IsNull,
                        ArgumentType = x.Type?.Accept(TargetTypeVisitor.Instance),
                    })],
                    NamedArguments = [.. attr.NamedArguments.Select(x => new ArgumentInfo
                    {
                        ArgumentName = x.Key,
                        ArgumentValue = x.Value,
                        IsNull = false,
                        ArgumentType = null,
                    })],
                });
        }

        return [.. all];
    }

    public override AttributeInfo[] VisitField(IFieldSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitProperty(IPropertySymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitAssembly(IAssemblySymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitMethod(IMethodSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitParameter(IParameterSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitNamedType(INamedTypeSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitModule(IModuleSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitNamespace(INamespaceSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
    public override AttributeInfo[] VisitArrayType(IArrayTypeSymbol symbol) => GetAttributeInfos(symbol.GetAttributes());
}
