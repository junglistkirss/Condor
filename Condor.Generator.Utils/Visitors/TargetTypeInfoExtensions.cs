using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class TargetTypeInfoExtensions
{
    public static TargetTypeInfo? GetTargetTypeInfo(this ISymbol symbol)
    {
        return symbol is ITypeSymbol type ? type.GetTargetTypeInfo() : null;
    }
    public static TargetTypeInfo? GetTargetTypeInfo(this ITypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance);
    }
    public static TargetTypeInfo? GetTargetTypeInfo(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance);
    }
    public static TargetTypeInfo RequireTargetTypeInfo(this ISymbol symbol)
    {
        return symbol is ITypeSymbol type ? type.RequireTargetTypeInfo() : throw new Exception($"Symbol is not an [ITypeSymbol], [ISymbol]=\"{symbol}\"");
    }
    public static TargetTypeInfo RequireTargetTypeInfo(this ITypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance) ?? throw new Exception($"Unable to resolve type info on [ITypeSymbol]=\"{symbol}\"");
    }
    public static TargetTypeInfo RequireTargetTypeInfo(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance) ?? throw new Exception($"Unable to resolve type info on [INamedTypeSymbol]=\"{symbol}\"");
    }

    public static TargetTypeInfo? GetTargetTypeInfo(this IArrayTypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance);
    }

    public static TargetTypeInfo RequireTargetTypeInfo(this IArrayTypeSymbol symbol)
    {
        return symbol.Accept(TargetTypeVisitor.Instance) ?? throw new Exception($"Unable to resolve array type info on [IArrayTypeSymbol]=\"{symbol}\"");
    }

    private sealed class TargetTypeVisitor : SymbolVisitor<TargetTypeInfo>
    {
        public static readonly TargetTypeVisitor Instance = new();

        public override TargetTypeInfo VisitNamedType(INamedTypeSymbol x)
        {
            return new TargetTypeInfo
            {
                ContainingAssembly = x.ContainingAssembly.Name,
                ContainingNamespace = x.ContainingNamespace.RequireFriendlyname(),
                TypeFullName = x.RequireStrongName(),
                TypeName = x.RequireFriendlyname(),
                GenericBaseTypeName = x.RequireGenericBaseTypeName(),
                TypeDefinition = x.RequireFriendlyDefinition(),
                Interfaces = [.. x.Interfaces.Select(x => x.RequireTargetTypeInfo())],
                AllInterfaces = [.. x.AllInterfaces.Select(x => x.RequireTargetTypeInfo())],
                IsNullable = x.NullableAnnotation == NullableAnnotation.Annotated,
                IsAbstract = x.IsAbstract,
                IsRecord = x.IsRecord,
                IsArray = false,
                ElementType = null,
                IsGeneric = x.IsGenericType,
                GenericTypes = x.IsGenericType ? [.. x.TypeParameters.Select(x => x.RequireTypeArgument())] : [],
            };
        }

        public override TargetTypeInfo VisitArrayType(IArrayTypeSymbol x)
        {
            return new TargetTypeInfo
            {
                ContainingAssembly = x.ElementType.ContainingAssembly.Name,
                ContainingNamespace = x.ElementType.ContainingNamespace.RequireFriendlyname(),
                GenericBaseTypeName = x.ElementType.RequireGenericBaseTypeName(),
                TypeName = x.ElementType.RequireFriendlyname() + "[]",
                TypeFullName = x.ElementType.RequireStrongName() + "[]",
                TypeDefinition = x.ElementType.RequireFriendlyDefinition() + "[]",
                IsRecord = x.ElementType.IsRecord,
                IsAbstract = x.ElementType.IsAbstract,
                Interfaces = [.. x.ElementType.Interfaces.Select(x => x.RequireTargetTypeInfo())],
                AllInterfaces = [.. x.ElementType.AllInterfaces.Select(x => x.RequireTargetTypeInfo())],
                IsNullable = x.ElementType.NullableAnnotation == NullableAnnotation.Annotated,
                IsArray = true,
                ElementType = x.ElementType.GetTargetTypeInfo(),
                IsGeneric = x.ElementType is INamedTypeSymbol named && named.IsGenericType,
                //GenericTypes = x.ElementType is INamedTypeSymbol n ? n.TypeArguments.IsGenericType : [],
            };
        }
    }
}
