using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors
{
    public sealed class FriendlyDefinitionVisitor : SymbolVisitor<string>
    {
        public static readonly FriendlyDefinitionVisitor Instance = new();

        public override string VisitNamedType(INamedTypeSymbol x)
        {
            //if (x.IsGenericType)
            //{
            //    return $"{x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            //        .WithGenericsOptions(SymbolDisplayGenericsOptions.None))}<{string.Concat(Enumerable.Repeat(',', x.TypeArguments.Length - 1))}>";
            //}
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                .AddGenericsOptions(SymbolDisplayGenericsOptions.IncludeVariance)
                .RemoveMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType));
        }
        public override string VisitMethod(IMethodSymbol x)
        {
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                .RemoveMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
                .RemoveMemberOptions(SymbolDisplayMemberOptions.IncludeExplicitInterface)
                .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers));
        }
    }
}
