using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class MemberInfoExtensions
{

    public static IEnumerable<MemberInfo?> GetPropertiesMemberInfo(this ISymbol symbol, Func<IPropertySymbol, bool>? filter = null) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetPropertiesMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static IEnumerable<MemberInfo?> GetFieldsMemberInfo(this ISymbol symbol, Func<IFieldSymbol, bool>? filter = null) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetFieldsMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static IEnumerable<MemberInfo> RequireFieldsMemberInfo(this ISymbol symbol, Func<IFieldSymbol, bool>? filter = null) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequireFieldsMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static IEnumerable<MemberInfo> RequirePropertiesMemberInfo(this ISymbol symbol, Func<IPropertySymbol, bool>? filter = null) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequirePropertiesMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static MemberInfo? GetPropertyMemberInfo(this ISymbol symbol, Func<IPropertySymbol, bool> filter) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetPropertyMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static MemberInfo? GetFieldMemberInfo(this ISymbol symbol, Func<IFieldSymbol, bool> filter) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetFieldMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static MemberInfo RequireFieldMemberInfo(this ISymbol symbol, Func<IFieldSymbol, bool> filter) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequireFieldMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    public static MemberInfo RequirePropertyMemberInfo(this ISymbol symbol, Func<IPropertySymbol, bool> filter) => symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequirePropertyMemberInfo(filter) : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");






    public static IEnumerable<MemberInfo?> GetPropertiesMemberInfo(this INamedTypeSymbol symbol, Func<IPropertySymbol, bool>? filter = null)
    {
        return symbol.Accept(PropertiesMemberInfo(filter)) ?? [];
    }

    public static IEnumerable<MemberInfo?> GetFieldsMemberInfo(this INamedTypeSymbol symbol, Func<IFieldSymbol, bool>? filter = null)
    {
        return symbol.Accept(FieldsMemberInfo(filter)) ?? [];
    }

    public static IEnumerable<MemberInfo> RequireFieldsMemberInfo(this INamedTypeSymbol symbol, Func<IFieldSymbol, bool>? filter = null)
    {
        return symbol.Accept(new MapMembersVisitor<IFieldSymbol, MemberInfo>(
            x => x.Accept(MemberVisitor.Instance) ?? throw new Exception($"Unable to resolve field info on \"{x.Name}\""),
            filter ?? (_ => true)
        )) ?? [];
    }

    public static IEnumerable<MemberInfo> RequirePropertiesMemberInfo(this INamedTypeSymbol symbol, Func<IPropertySymbol, bool>? filter = null)
    {
        return symbol.Accept(new MapMembersVisitor<IPropertySymbol, MemberInfo>(
            x => x.Accept(MemberVisitor.Instance) ?? throw new Exception($"Unable to resolve field info on \"{x.Name}\""),
            filter ?? (_ => true)
        )) ?? [];
    }


    public static MemberInfo? GetPropertyMemberInfo(this INamedTypeSymbol symbol, Func<IPropertySymbol, bool> filter)
    {
        return symbol.Accept(PropertyMemberInfo(filter));
    }

    public static MemberInfo? GetFieldMemberInfo(this INamedTypeSymbol symbol, Func<IFieldSymbol, bool> filter)
    {
        return symbol.Accept(FieldMemberInfo(filter));
    }

    public static MemberInfo RequireFieldMemberInfo(this INamedTypeSymbol symbol, Func<IFieldSymbol, bool> filter)
    {
        return symbol.Accept(new MapMemberVisitor<IFieldSymbol, MemberInfo>(
            x => x.Accept(MemberVisitor.Instance) ?? throw new Exception($"Unable to resolve field info on \"{x.Name}\""),
            filter
        )) ?? throw new Exception("Unable to find a property matching given filter");
    }

    public static MemberInfo RequirePropertyMemberInfo(this INamedTypeSymbol symbol, Func<IPropertySymbol, bool> filter)
    {
        return symbol.Accept(new MapMemberVisitor<IPropertySymbol, MemberInfo>(
            x => x.Accept(MemberVisitor.Instance) ?? throw new Exception($"Unable to resolve field info on \"{x.Name}\""),
            filter
        )) ?? throw new Exception("Unable to find a field matching given filter");
    }


    private sealed class MemberVisitor : SymbolVisitor<MemberInfo>
    {
        public static readonly MemberVisitor Instance = new();

        public override MemberInfo VisitField(IFieldSymbol symbol)
        {
            return new MemberInfo
            {
                IsConstant = symbol.HasConstantValue,
                IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
                MemberName = symbol.Name,
                MemberType = symbol.Type.RequireTargetTypeInfo(),
                Attributes = symbol.RequireAttributesInfo(),
            };
        }
        public override MemberInfo VisitProperty(IPropertySymbol symbol)
        {
            return new MemberInfo
            {
                IsConstant = false,
                IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated,
                MemberName = symbol.Name,
                MemberType = symbol.Type.RequireTargetTypeInfo(),
                Attributes = symbol.RequireAttributesInfo(),
            };
        }
    }
    public static SymbolVisitor<IEnumerable<TOut>> Properties<TOut>(Func<IPropertySymbol, TOut> map, Func<IPropertySymbol, bool>? filter = null)
        => new MapMembersVisitor<IPropertySymbol, TOut>(map, filter ?? (_ => true));

    public static SymbolVisitor<IEnumerable<MemberInfo?>> PropertiesMemberInfo(Func<IPropertySymbol, bool>? filter = null)
        => new MapMembersVisitor<IPropertySymbol, MemberInfo?>(x => x.Accept(MemberVisitor.Instance), filter ?? (_ => true));

    public static SymbolVisitor<IEnumerable<TOut>> Fields<TOut>(Func<IFieldSymbol, TOut> map, Func<IFieldSymbol, bool>? filter = null)
    => new MapMembersVisitor<IFieldSymbol, TOut>(map, filter ?? (_ => true));

    public static SymbolVisitor<IEnumerable<MemberInfo?>> FieldsMemberInfo(Func<IFieldSymbol, bool>? filter = null)
        => new MapMembersVisitor<IFieldSymbol, MemberInfo?>(x => x.Accept(MemberVisitor.Instance), filter ?? (_ => true));

    public static SymbolVisitor<TOut> Property<TOut>(Func<IPropertySymbol, TOut> map, Func<IPropertySymbol, bool> filter)
        => new MapMemberVisitor<IPropertySymbol, TOut>(map, filter);

    public static SymbolVisitor<MemberInfo> PropertyMemberInfo(Func<IPropertySymbol, bool> filter)
        => new MapMemberVisitor<IPropertySymbol, MemberInfo>(x => x.Accept(MemberVisitor.Instance) ?? throw new Exception("Unable to resolve member info"), filter);

    public static SymbolVisitor<TOut> Field<TOut>(Func<IFieldSymbol, TOut> map, Func<IFieldSymbol, bool> filter)
        => new MapMemberVisitor<IFieldSymbol, TOut>(map, filter);

    public static SymbolVisitor<MemberInfo?> FieldMemberInfo(Func<IFieldSymbol, bool> filter)
        => new MapMemberVisitor<IFieldSymbol, MemberInfo?>(x => x.Accept(MemberVisitor.Instance), filter);

    private sealed class MapMemberVisitor<T, TOut>(Func<T, TOut> map, Func<T, bool> filter) : SymbolVisitor<TOut>
        where T : ISymbol
    {
        private readonly Func<T, TOut> map = map;

        public override TOut VisitNamedType(INamedTypeSymbol symbol)
        {
            return symbol.GetMembers().OfType<T>().Where(filter).Select(map).SingleOrDefault();
        }
    }

    private sealed class MapMembersVisitor<T, TOut>(Func<T, TOut> map, Func<T, bool> filter) : SymbolVisitor<IEnumerable<TOut>>
        where T : ISymbol
    {
        private readonly Func<T, TOut> map = map;

        public override IEnumerable<TOut> VisitNamedType(INamedTypeSymbol symbol)
        {
            return symbol.GetMembers().OfType<T>().Where(filter).Select(map);
        }
    }

}
