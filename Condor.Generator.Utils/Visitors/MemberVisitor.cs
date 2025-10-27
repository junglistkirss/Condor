﻿using Microsoft.CodeAnalysis;

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
            MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("TargetType required"),
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new NullReferenceException("Attributes required"),
        };
    }
    public override MemberInfo VisitProperty(IPropertySymbol symbol)
    {
        return new MemberInfo
        {
            IsConstant = false,
            IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
            MemberName = symbol.Name,
            MemberType = symbol.Type.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("TargetType required"),
            Attributes = symbol.Accept(AttributesVisitor.Instance) ?? throw new NullReferenceException("Attributes required"),
        };
    }
}
