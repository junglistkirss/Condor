using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Condor.Generator.Utils.Visitors;

public sealed class AttributesVisitor : SymbolVisitor<AttributeInfo[]>
{
    public static readonly AttributesVisitor Instance = new();

    private static AttributeInfo[] GetAttributeInfos(ImmutableArray<AttributeData> datas)
    {
        List<AttributeInfo> all = new();
        for (int i = 0; i < datas.Length; i++)
        {
            all.Add(new AttributeInfo
            {
                AttributeType = datas[i].AttributeClass.Accept(TargetTypeVisitor.Instance),
                Constructor = datas[i].AttributeConstructor?.Accept(ActionVisitor.Instance),
                ConstructorArguments = [.. datas[i].ConstructorArguments.Select(x => new ArgumentInfo
                {
                    ArgumentName = null,
                    ArgumentValue = x.Kind == TypedConstantKind.Array ? x.Values : x.Value,
                    IsNull = x.IsNull,
                    ArgumentType = x.Type?.Accept(TargetTypeVisitor.Instance),
                })],
                NamedArguments = [.. datas[i].NamedArguments.Select(x => new ArgumentInfo
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
