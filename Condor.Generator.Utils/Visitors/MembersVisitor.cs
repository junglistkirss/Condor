using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Condor.Generator.Utils.Visitors
{
    public sealed class MembersVisitor<T> : SymbolVisitor<MemberInfo[]>
        where T : ISymbol
    {
        public static readonly MembersVisitor<T> Instance = new();

        public override MemberInfo[] VisitNamedType(INamedTypeSymbol symbol)
        {
            return [.. symbol.GetMembers().OfType<T>().Select(x => x.Accept(MemberVisitor.Instance))];
        }
    }

    public static class MapMembers
    {
        public static MapMembersVisitor<IPropertySymbol, TOut> Properties<TOut>(Func<IPropertySymbol, TOut> map)
            => new MapMembersVisitor<IPropertySymbol, TOut>(map);


    }
    public sealed class MapMembersVisitor<T, TOut> : SymbolVisitor<TOut[]>
       where T : ISymbol
    {
        private readonly Func<T, TOut> map;

        public MapMembersVisitor(Func<T, TOut> map)
        {
            this.map = map;
        }

        public override TOut[] VisitNamedType(INamedTypeSymbol symbol)
        {
            return [.. symbol.GetMembers().OfType<T>().Select(map)];
        }
    }
}
