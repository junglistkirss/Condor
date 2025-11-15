using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Condor.Generator.Utils.Visitors;

public static class AttributeExtensions
{
    public static INamedTypeSymbol RequireAttributeClass(this AttributeData attributeData) => attributeData.AttributeClass ?? throw new Exception("Attribute class is required");
    public static AttributeInfo[]? GetAttributesInfo(this IFieldSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IPropertySymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IAssemblySymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IMethodSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IParameterSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this INamedTypeSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IModuleSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this INamespaceSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this IArrayTypeSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);
    public static AttributeInfo[]? GetAttributesInfo(this ITypeSymbol symbol) => symbol.Accept(AttributesVisitor.Instance);

    public static AttributeInfo[] RequireAttributesInfo(this IFieldSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IFieldSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IPropertySymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IPropertySymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IAssemblySymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IAssemblySymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IMethodSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IMethodSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IParameterSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IParameterSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this INamedTypeSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [INamedTypeSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IModuleSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IModuleSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this INamespaceSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [INamespaceSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this IArrayTypeSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [IArrayTypeSymbol]=\"{symbol}\"");
    public static AttributeInfo[] RequireAttributesInfo(this ITypeSymbol symbol) => symbol.GetAttributesInfo() ?? throw new Exception($"Unable to resolve attributes info on [ITypeSymbol]=\"{symbol}\"");
    
    private sealed class AttributesVisitor : SymbolVisitor<AttributeInfo[]>
    {
        public static readonly AttributesVisitor Instance = new();

        private static AttributeInfo[] GetAttributeInfos(ImmutableArray<AttributeData> datas)
        {
            List<AttributeInfo> all = [];
            for (int i = 0; i < datas.Length; i++)
            {
                all.Add(new AttributeInfo
                {
                    AttributeType = datas[i].RequireAttributeClass().RequireTargetTypeInfo(),
                    Constructor = datas[i].AttributeConstructor?.RequireActionInfo() ?? throw new Exception("Constructor is missing on attribute"),
                    ConstructorArguments = [.. datas[i].ConstructorArguments.Select(x => new ArgumentInfo
                {
                    ArgumentName = null,
                    ArgumentValue = x.Kind == TypedConstantKind.Array ? x.Values : x.Value,
                    IsNull = x.IsNull,
                    ArgumentType = x.Type?.RequireTargetTypeInfo(),
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
}
