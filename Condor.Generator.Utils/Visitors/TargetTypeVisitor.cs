using Microsoft.CodeAnalysis;
using System.Linq;

namespace Condor.Generator.Utils.Visitors
{
    public sealed class TargetTypeVisitor : SymbolVisitor<TargetTypeInfo>
    {
        public static readonly TargetTypeVisitor Instance = new();

        public override TargetTypeInfo VisitNamedType(INamedTypeSymbol x)
        {
            return new TargetTypeInfo
            {
                ContainingAssembly = x.ContainingAssembly.Name,
                ContainingNamespace = x.ContainingNamespace.Accept(FriendlyNameVisitor.Instance),
                TypeFullName = x.Accept(StrongNameVisitor.Instance),
                TypeName = x.Accept(FriendlyNameVisitor.Instance),
                GenericBaseTypeName = x.Accept(GenericBaseTypeNameVisitor.Instance),
                TypeDefinition = x.Accept(FriendlyDefinitionVisitor.Instance),
                Interfaces = x.Interfaces.Select(x => x.Accept(Instance)).ToArray(),
                AllInterfaces = x.AllInterfaces.Select(x => x.Accept(Instance)).ToArray(),
                IsNullable = x.NullableAnnotation == NullableAnnotation.Annotated,
                IsAbstract = x.IsAbstract,
                IsRecord = x.IsRecord,
                IsArray = false,
                ElementType = null,
                IsGeneric = x.IsGenericType,
                GenericTypes = x.IsGenericType ? x.TypeParameters.Select(x => x.Accept(TypeArgumentVisitor.Instance)).ToArray() : [],
            };
        }

        public override TargetTypeInfo VisitArrayType(IArrayTypeSymbol x)
        {
            return new TargetTypeInfo
            {
                ContainingAssembly = x.ElementType.ContainingAssembly.Name,
                ContainingNamespace = x.ElementType.ContainingNamespace.Accept(FriendlyNameVisitor.Instance),
                GenericBaseTypeName = x.ElementType.Accept(GenericBaseTypeNameVisitor.Instance),
                TypeName = x.ElementType.Accept(FriendlyNameVisitor.Instance) + "[]",
                TypeFullName = x.ElementType.Accept(StrongNameVisitor.Instance) + "[]",
                TypeDefinition = x.ElementType.Accept(FriendlyDefinitionVisitor.Instance) + "[]",
                IsRecord = x.ElementType.IsRecord,
                IsAbstract = x.ElementType.IsAbstract,
                Interfaces = x.ElementType.Interfaces.Select(x => x.Accept(Instance)).ToArray(),
                AllInterfaces = x.ElementType.AllInterfaces.Select(x => x.Accept(Instance)).ToArray(),
                IsNullable = x.ElementType.NullableAnnotation == NullableAnnotation.Annotated,
                IsArray = true,
                ElementType = x.ElementType.Accept(Instance),
                IsGeneric = x.ElementType is INamedTypeSymbol named && named.IsGenericType,
                //GenericTypes = x.ElementType is INamedTypeSymbol n ? n.TypeArguments.IsGenericType : [],
            };
        }
    }
}
