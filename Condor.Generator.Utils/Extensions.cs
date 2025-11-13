using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Condor.Generator.Utils;

public delegate IEnumerable<TargetTypeInfo> ReferencedTypeFinder(Func<IAssemblySymbol, bool> assemblyPredicate, Func<INamedTypeSymbol, bool> symbolPredicate);
public delegate IEnumerable<TargetTypeInfo> TypeFinder(Func<INamedTypeSymbol, bool> symbolPredicate);

public delegate IEnumerable<TargetTypeInfo> AllTypeFinder(Func<IAssemblySymbol, bool> assemblyPredicate, Func<INamedTypeSymbol, bool> symbolPredicate);

public record struct TypesProvider(TypeFinder Current, ReferencedTypeFinder Referenced, AllTypeFinder Combined);
public static class Extensions
{
    private static AllTypeFinder CreateCombined(TypeFinder current, ReferencedTypeFinder referenced) => (a, s) =>
    {
        return current(s).Union(referenced(a, s));
    };

    public static IncrementalValuesProvider<KeyedTemplate> GetTemplates(this IncrementalGeneratorInitializationContext context, string extension = ".mustache")
    {
        return context.AdditionalTextsProvider
                    .Where(at => at.Path.EndsWith(extension))
                    .Select((text, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new KeyedTemplate(
                            Path.GetFileNameWithoutExtension(text.Path),
                            text.GetText(cancellationToken)?.ToString()
                        );
                    });
    }
    public static IncrementalValueProvider<TypesProvider> GetTypesProvider(this IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider
             .Select<Compilation, ReferencedTypeFinder>((cp, cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 return (assemblyPredicate, symbolPredicate) =>
                 {
                     return cp.SourceModule.ReferencedAssemblySymbols
                     .Where(assemblyPredicate) // CRACRA ça, trouver un autre moyen pour filrer les assemblys
                     .SelectMany(s =>
                     {
                         return s.GlobalNamespace.Accept(SubTypesVisitor.Instance).Where(symbolPredicate).Select(x => x.Accept(TargetTypeVisitor.Instance));
                     });
                 };
             })
             .Combine(context.CompilationProvider
             .Select<Compilation, TypeFinder>((cp, cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 return (symbolPredicate) =>
                 {
                     return cp.SourceModule.GlobalNamespace.Accept(SubTypesVisitor.Instance).Where(symbolPredicate).Select(x => x.Accept(TargetTypeVisitor.Instance));
                 };
             }))
             .Select((x, cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 return new TypesProvider(Current: x.Right, Referenced: x.Left, Combined: CreateCombined(x.Right, x.Left));
             });
    }
    public static IEnumerable<TargetTypeInfo> GetTypedArguments(this INamedTypeSymbol data)
        => data.TypeArguments.Select(x => x.Accept(TargetTypeVisitor.Instance));
    public static bool TryGetNamedArgument<T>(this AttributeData data, string name, out T value)
    {
        value = default;
        if (data != null && data.NamedArguments.Length > 0 && data.NamedArguments.Any(x => x.Key == name))
        {
            TypedConstant val = data.NamedArguments.Single(x => x.Key == name).Value;
            if (!val.IsNull)
            {
                if (val.Kind == TypedConstantKind.Array)
                    throw new InvalidOperationException($"Named argument {name} is an array");
                value = (T)val.Value;
                return true;
            }
        }
        return false;
    }
    public static bool TryGetNamedArgumentMany<T>(this AttributeData data, string name, out T[] value)
    {
        value = default;
        if (data != null && data.NamedArguments.Length > 0 && data.NamedArguments.Any(x => x.Key == name))
        {
            TypedConstant val = data.NamedArguments.Single(x => x.Key == name).Value;
            if (!val.IsNull)
            {
                if (val.Kind != TypedConstantKind.Array)
                    throw new InvalidOperationException($"Named argument {name} is not an array");
                value = [.. val.Values.Select(x => (T)x.Value)];
                return true;
            }
        }
        return false;
    }
    private static readonly string[] Removes = ["Base", "I"];

    public static string SanitizeBaseOrInterfaceName(this string name)
    {
        var sanitize = name;
        foreach (var item in Removes)
        {
            if (name.StartsWith(item))
                sanitize = name.Substring(item.Length);
        }
        return sanitize;
    }

    public static string SanitizeToHintName(this string name)
        => name.Replace(" ", "").Replace(",", ".").Replace("<", "-").Replace(">", "");
    public static string GetAccessibilityKeyWord(this Accessibility modifiers)
    {
        return modifiers switch
        {
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal or Accessibility.ProtectedOrInternal => "internal protected",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => string.Empty,
        };
    }
}
