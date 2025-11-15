using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class StrongNameExtensions
{
    public static string? GetStrongName(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.GetStrongName() : null;
    }

    public static string RequireStrongName(this ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.RequiredStrongName() : throw new Exception($"Symbol is not an [INamedTypeSymbol], [ISymbol]=\"{symbol}\"");
    }

    public static string? GetStrongName(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(StrongNameVisitor.Instance);
    }

    public static string RequiredStrongName(this INamedTypeSymbol symbol)
    {
        return symbol.Accept(StrongNameVisitor.Instance) ?? throw new Exception($"Unable to resolve strong name on [INamedTypeSymbol]=\"{symbol}\"");
    }

    private sealed class StrongNameVisitor : SymbolVisitor<string>
    {
        public static readonly StrongNameVisitor Instance = new();

        public override string VisitNamedType(INamedTypeSymbol x)
        {
            return x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.ExpandNullable)
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        }
    }
}
