using Microsoft.CodeAnalysis;

namespace Condor.Generator.Utils.Visitors;

public sealed class StrongNameVisitor : SymbolVisitor<string>
{
    public static readonly StrongNameVisitor Instance = new();

    public override string VisitNamedType(INamedTypeSymbol x)
    {
        return x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.ExpandNullable)
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
    }
}
