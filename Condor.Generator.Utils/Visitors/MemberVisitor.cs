using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class MemberVisitor : SymbolVisitor<MemberInfo>
{
    public static readonly MemberVisitor Instance = new();

    public override MemberInfo VisitField(IFieldSymbol symbol)
    {
        return new MemberInfo
        {
            IsConstant = symbol.HasConstantValue,
            IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
            MemberName = symbol.Name,
            MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance) ?? throw new Exception("Unable to resolve member type info"),
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new Exception("Unable to resolve attributes info"),
        };
    }
    public override MemberInfo VisitProperty(IPropertySymbol symbol)
    {
        return new MemberInfo
        {
            IsConstant = false,
            IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
            MemberName = symbol.Name,
            MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance) ?? throw new Exception("Unable to resolve member type info"),
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new Exception("Unable to resolve attributes info"),
        };
    }
}
