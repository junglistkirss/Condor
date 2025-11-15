using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class FriendlyDefinitionExtensions
{

    public static string? GetFriendlyDefinition(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetFriendlyDefinition() : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    }
    public static string RequireFriendlyDefinition(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequireFriendlyDefinition() : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    }
    public static string? GetFriendlyDefinition(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(FriendlyDefinitionVisitor.Instance);
    }

    public static string RequireFriendlyDefinition(this INamedTypeSymbol symbol)
    {
        return symbol.GetFriendlyDefinition() ?? throw new Exception($"Unable to resolve friendly definition on [INamedTypeSymbol]=\"{symbol}\"");
    }

    private sealed class FriendlyDefinitionVisitor : SymbolVisitor<string>
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
