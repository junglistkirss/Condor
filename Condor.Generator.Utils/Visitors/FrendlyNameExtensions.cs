using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public static class FrendlyNameExtensions
{
    public static string? GetFriendlyname(this ITypeSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance);
    public static string? GetFriendlyname(this INamespaceSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance);
    public static string? GetFriendlyname(this IParameterSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance);
    public static string? GetFriendlyname(this IMethodSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance);
    public static string? GetFriendlyname(this INamedTypeSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance);

    public static string RequireFriendlyname(this ITypeSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception($"Unable to resolve friendly name on [ITypeSymbol]=\"{symbol}\"");
    public static string RequireFriendlyname(this INamespaceSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception($"Unable to resolve friendly name on [INamespaceSymbol]=\"{symbol}\"");
    public static string RequireFriendlyname(this IParameterSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception($"Unable to resolve friendly name on [IParameterSymbol]=\"{symbol}\"");
    public static string RequireFriendlyname(this IMethodSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception($"Unable to resolve friendly name on [IMethodSymbol]=\"{symbol}\"");
    public static string RequireFriendlyname(this INamedTypeSymbol symbol) => symbol.Accept(FriendlyNameVisitor.Instance) ?? throw new Exception($"Unable to resolve friendly name on [INamedTypeSymbol]=\"{symbol}\"");

    private sealed class FriendlyNameVisitor : SymbolVisitor<string>
    {
        public static readonly FriendlyNameVisitor Instance = new();

        public override string VisitNamespace(INamespaceSymbol x)
        {
            return x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        }
        public override string VisitParameter(IParameterSymbol x)
        {
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        public override string VisitMethod(IMethodSymbol x)
        {
            return x.Name;
        }
        public override string VisitNamedType(INamedTypeSymbol x)
        {
            if (x.IsUnmanagedType)
                return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            return x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.ExpandNullable));
        }
    }
}
