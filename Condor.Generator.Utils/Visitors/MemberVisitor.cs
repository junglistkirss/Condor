using Condor.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors
{
    public sealed class MemberVisitor : SymbolVisitor<MemberInfo>
    {
        public static readonly MemberVisitor Instance = new();

        public override MemberInfo VisitField(IFieldSymbol symbol)
        {
            return new MemberInfo
            {
                IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
                MemberName = symbol.Name,
                MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance),
                Attributes = symbol.Accept(AttributesVisitor.Instance),
            };
        }
        public override MemberInfo VisitProperty(IPropertySymbol symbol)
        {
            return new MemberInfo
            {
                IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
                MemberName = symbol.Name,
                MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance),
                Attributes = symbol.Accept(AttributesVisitor.Instance),
            };
        }
    }
}
