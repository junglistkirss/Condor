using Microsoft.CodeAnalysis;
using System.Linq;

namespace Condor.Generator.Utils.Visitors;

public sealed class TargetTypeVisitor : SymbolVisitor<TargetTypeInfo>
{
    public static readonly TargetTypeVisitor Instance = new();

    public override TargetTypeInfo VisitNamedType(INamedTypeSymbol x)
    {
        return new TargetTypeInfo
        {
            ContainingAssembly = x.ContainingAssembly.Name,
            ContainingNamespace = x.ContainingNamespace.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name"),
            TypeFullName = x.Accept(StrongNameVisitor.Instance) ?? throw new Exception("Unable to resolve strong name"),
            TypeName = x.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name"),
            GenericBaseTypeName = x.Accept(GenericBaseTypeNameVisitor.Instance) ?? throw new Exception("Unable to resolve generic base type"),
            TypeDefinition = x.Accept(FriendlyDefinitionVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name"),
            Interfaces = [.. x.Interfaces.Select(x => x.Accept(Instance) ?? throw new Exception("Unable to resolve interface type info"))],
            AllInterfaces = [.. x.AllInterfaces.Select(x => x.Accept(Instance) ?? throw new Exception("Unable to resolve interface type info"))],
            IsNullable = x.NullableAnnotation == NullableAnnotation.Annotated,
            IsAbstract = x.IsAbstract,
            IsRecord = x.IsRecord,
            IsArray = false,
            ElementType = null,
            IsGeneric = x.IsGenericType,
            GenericTypes = x.IsGenericType ? [.. x.TypeParameters.Select(x => x.Accept(TypeArgumentVisitor.Instance) ?? throw new Exception("Unable to resolve type argument"))] : [],
        };
    }

    public override TargetTypeInfo VisitArrayType(IArrayTypeSymbol x)
    {
        return new TargetTypeInfo
        {
            ContainingAssembly = x.ElementType.ContainingAssembly.Name,
            ContainingNamespace = x.ElementType.ContainingNamespace.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name"),
            GenericBaseTypeName = x.ElementType.Accept(GenericBaseTypeNameVisitor.Instance) ?? throw new Exception("Unable to resolve generic base type"),
            TypeName = (x.ElementType.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception("Unable to resolve friendly name")) + "[]",
            TypeFullName = (x.ElementType.Accept(StrongNameVisitor.Instance) ?? throw new Exception("Unable to resolve strong name")) + "[]",
            TypeDefinition = (x.ElementType.Accept(FriendlyDefinitionVisitor.Instance) ?? throw new Exception("Unable to resolve friendly definition")) + "[]",
            IsRecord = x.ElementType.IsRecord,
            IsAbstract = x.ElementType.IsAbstract,
            Interfaces = [.. x.ElementType.Interfaces.Select(x => x.Accept(Instance) ?? throw new Exception("Unable to resolve interface type info"))],
            AllInterfaces = [.. x.ElementType.AllInterfaces.Select(x => x.Accept(Instance) ?? throw new Exception("Unable to resolve interface type info"))],
            IsNullable = x.ElementType.NullableAnnotation == NullableAnnotation.Annotated,
            IsArray = true,
            ElementType = x.ElementType.Accept(Instance),
            IsGeneric = x.ElementType is INamedTypeSymbol named && named.IsGenericType,
            //GenericTypes = x.ElementType is INamedTypeSymbol n ? n.TypeArguments.IsGenericType : [],
        };
    }
}
