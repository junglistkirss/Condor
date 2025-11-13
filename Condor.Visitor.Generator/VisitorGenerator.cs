using Condor.Generator.Utils;
using Condor.Generator.Utils.Templating;
using Condor.Generator.Utils.Visitors;
using Condor.Visitor.Generator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Condor.Visitor.Generator;

[Generator]
public class VisitorGenerator : IIncrementalGenerator
{
    private const string VisitorTemplateName = "Visitor";
    private const string DefaultVisitMethodName = "Visit";
    private const string DefaultAcceptMethodName = "Accept";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        IncrementalValueProvider<TypesProvider> types = context.GetTypesProvider();

        IncrementalValuesProvider<VisitorInfo> visitors = GetVisitorsInfo(context);
        IncrementalValuesProvider<AcceptorInfo> acceptors = GetAcceptorsInfo(context);
        IncrementalValuesProvider<OutputInfo> output = GetOutputInfo(context);
        IncrementalValuesProvider<AcceptorInfo> autoAcceptor = GetAutoAcceptorsInfo(context, types);
        IncrementalValuesProvider<VisitParamInfo> visitParams = GetVisitParamdInfo(context);
        IncrementalValuesProvider<GenerateDefaultInfo> @default = GetGenerateDefaultInfo(context);
        IncrementalValuesProvider<VisitableInfo> visitable = GetVisitableInfo(context);
        IncrementalValuesProvider<AcceptParamInfo> acceptParams = GetAcceptParamInfo(context);
        IncrementalValuesProvider<OutputVisitorInfo> combine = CombineData(visitors, acceptors, autoAcceptor, output, @default, visitable, acceptParams, visitParams);

        context.RegisterSourceOutput(combine, (ctx, data) =>
        {
            string sourceName = string.Join(".", data.ClassName.SanitizeToHintName(), VisitorTemplateName, "generated");
            try
            {
                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithAccessors(x => x
                        .AddDefaultsAccessors()
                        .CreateMemberObjectAccessor<NamedParamInfo>(NamedParamInfoAccessor.GetNamedProperty)
                        .CreateMemberObjectAccessor<OutputVisitableInfo>(OutputVisitableInfoAccessor.GetNamedProperty)
                        .CreateMemberObjectAccessor<OutputVisitorDefaultInfo>(OutputVisitorDefaultInfoAccessor.GetNamedProperty)
                        .CreateMemberObjectAccessor<OutputVisitorInfo>(OutputVisitorInfoAccessor.GetNamedProperty)
                        .CreateMemberObjectAccessor<ImplGroup>(ImplGroupAccessor.GetNamedProperty)
                    )
                    .WithTemplates([new KeyedTemplate(DefaultTemplates.DefaultVisitorTemplateKey, DefaultTemplates.VisitorTemplate)])
                    .Build();

                string result = templateProcessor.Render(DefaultTemplates.DefaultVisitorTemplateKey, data);
                ctx.AddSource(sourceName, result);
            }
            catch (Exception ex)
            {
                ctx.AddSource(sourceName + ".error", $"/*{ex}*/");
            }
        });

    }
    private static IncrementalValuesProvider<AcceptParamInfo> GetAcceptParamInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
               typeof(AcceptParamAttribute<>).FullName,
               (node, _) => node is TypeDeclarationSyntax,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   return sc.Attributes.Select(attr => (
                        Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                        AcceptParamType: ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance),
                        AcceptParamName: attr.TryGetNamedArgument(nameof(AcceptParamAttribute<object>.ParamName), out string n) ? n : null
                   ));
               }).SelectMany((x, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   return x.GroupBy(e => e.Correlation).Select(e =>
                   {
                       return new AcceptParamInfo(e.Key, e.Select(i => (i.AcceptParamType, i.AcceptParamName)));
                   });
               });
    }

    private static IncrementalValuesProvider<VisitableInfo> GetVisitableInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
               typeof(GenerateVisitableAttribute).FullName,
               (node, _) => node is TypeDeclarationSyntax,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   AttributeData attr = sc.Attributes.Single();
                   return new VisitableInfo(sc.TargetSymbol.Accept(StrongNameVisitor.Instance), attr.TryGetNamedArgument(nameof(GenerateVisitableAttribute.AcceptMethodName), out string name) ? name : DefaultAcceptMethodName);
               });
    }

    private static IncrementalValuesProvider<GenerateDefaultInfo> GetGenerateDefaultInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
               typeof(GenerateDefaultAttribute).FullName,
               (node, _) => node is TypeDeclarationSyntax,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   AttributeData attr = sc.Attributes.Single();
                   VisitOptions vo = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.VisitOptions), out VisitOptions f) ? f : VisitOptions.AbstractVisit;
                   OptionsDefault o = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.Options), out OptionsDefault od) ? od : OptionsDefault.None;
                   return new GenerateDefaultInfo(
                        sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                        GenerateDefault: true,
                        vo == VisitOptions.UseVisitFallBack,
                        o.HasFlag(OptionsDefault.ForcePublic),
                        o.HasFlag(OptionsDefault.IsPartial),
                        o.HasFlag(OptionsDefault.IsAbstract),
                        vo == VisitOptions.AbstractVisit
                   );
               });
    }

    private static IncrementalValuesProvider<VisitParamInfo> GetVisitParamdInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
               typeof(VisitParamAttribute<>).FullName,
               (node, _) => node is TypeDeclarationSyntax,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   return sc.Attributes.Select(attr => (
                        Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                        VisitParamType: ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance),
                        VisitParamName: attr.TryGetNamedArgument(nameof(VisitParamAttribute<object>.ParamName), out string n) ? n : null
                   ));
               }).SelectMany((x, _) =>
               {
                   return x.GroupBy(e => e.Correlation).Select(e =>
                   {
                       return new VisitParamInfo(e.Key, e.Select(i => (i.VisitParamType, i.VisitParamName)));
                   });
               });
    }

    private static IncrementalValuesProvider<VisitorInfo> GetVisitorsInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
           .ForAttributeWithMetadataName(
               typeof(VisitorAttribute).FullName,
               (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   AttributeData attr = sc.Attributes.Single();
                   return new VisitorInfo(
                       sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                       sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                       ((TypeDeclarationSyntax)sc.TargetNode).Keyword.Text,
                       sc.TargetSymbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                       attr.TryGetNamedArgument(nameof(VisitorAttribute.IsAsync), out bool w) && w,
                       attr.TryGetNamedArgument(nameof(VisitorAttribute.VisitMethodName), out string name) ? name : DefaultVisitMethodName
                    );
               });
    }

    private static IncrementalValuesProvider<AcceptorInfo> GetAcceptorsInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
               typeof(AcceptorAttribute<>).FullName,
               (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   return sc.Attributes.Select(attr => (
                        Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                        ImplementationType: ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance)
                   ));
               }).SelectMany((x, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   return x.GroupBy(e => new { e.Correlation, e.ImplementationType }).Select(e =>
                   {
                       return new AcceptorInfo(e.Key.Correlation, e.Key.ImplementationType, AddVisitFallback: false, AddVisitRedirect: false, ImplementationTypes: e.Select(i => i.ImplementationType));
                   });
               });
    }

    private static IncrementalValuesProvider<OutputInfo> GetOutputInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
           .ForAttributeWithMetadataName(
               typeof(OutputAttribute<>).FullName,
               (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   AttributeData attr = sc.Attributes.Single();
                   return new OutputInfo(
                       sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                       ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance)
                    );
               });
    }
    private static IEnumerable<AcceptedKind> GetFlags(AcceptedKind value)
    {
        foreach (AcceptedKind flag in Enum.GetValues(typeof(AcceptedKind)))
        {
            if (value.HasFlag(flag) && flag != 0)
            {
                yield return flag;
            }
        }
    }
    private static bool CheckTypeAccept(AcceptedKind kind, INamedTypeSymbol type)
    {
        return kind switch
        {
            AcceptedKind.Class => type.TypeKind == TypeKind.Class,
            AcceptedKind.Interface => type.TypeKind == TypeKind.Interface,
            AcceptedKind.Struct => type.TypeKind == TypeKind.Struct,
            AcceptedKind.Record => type.IsRecord,
            AcceptedKind.Generic => type.IsGenericType,
            AcceptedKind.Abstract => type.IsAbstract,
            AcceptedKind.Concrete => !type.IsAbstract,
            AcceptedKind.Sealed => type.IsSealed,
            _ => true,
        };
    }
    private static IncrementalValuesProvider<AcceptorInfo> GetAutoAcceptorsInfo(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<TypesProvider> types)
    {
        return context.SyntaxProvider
                    .ForAttributeWithMetadataName(
                    typeof(AutoAcceptorAttribute<>).FullName,
                    (node, _) => node is TypeDeclarationSyntax,
                    (sc, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return sc.Attributes.Select(attr => (
                             Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                             VisitedType: ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance),
                             AssemblyPattern: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AssemblyPattern), out string ap) ? ap : sc.TargetSymbol.ContainingAssembly.Name,
                             TypePattern: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.TypePattern), out string tp) ? tp : null,
                             Accept: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.Accept), out AcceptedKind abs) ? abs : AcceptedKind.Class,
                             AcceptAll: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AcceptRequireAll), out bool a) && a,
                             AddVisitFallback: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AddVisitFallback), out bool f) && f,
                             AddVisitRedirect: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AddVisitRedirect), out bool d) && d
                         ));
                    }).Combine(types).SelectMany((x, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        AllTypeFinder discoveredTypes = x.Right.Combined;
                        return x.Left
                             .GroupBy(e => e.Correlation)
                             .SelectMany(g =>
                             {
                                 return g.Select(e =>
                                 {
                                     IEnumerable<TargetTypeInfo> ImplementationTypes = discoveredTypes(
                                         x => string.IsNullOrWhiteSpace(e.AssemblyPattern) || Regex.IsMatch(x.Name, e.AssemblyPattern),
                                         x =>
                                         {
                                             bool typePatternMatch = string.IsNullOrWhiteSpace(e.TypePattern)
                                                || Regex.IsMatch(x.Accept(StrongNameVisitor.Instance), e.TypePattern);
                                             bool subTypeMatch =
                                                e.VisitedType.TypeFullName != x.Accept(StrongNameVisitor.Instance)
                                                && x.AllInterfaces.Any(i => i.Accept(StrongNameVisitor.Instance) == e.VisitedType.TypeFullName)
                                                    || x.Accept(BaseTypesVisitor.Instance).Any(b => b.Accept(StrongNameVisitor.Instance) == e.VisitedType.TypeFullName);
                                             if (typePatternMatch && subTypeMatch)
                                             {
                                                 if (e.Accept != AcceptedKind.None)
                                                 {
                                                     if (e.AcceptAll)
                                                         return GetFlags(e.Accept).All(v => CheckTypeAccept(v, x));
                                                     else
                                                         return GetFlags(e.Accept).Any(v => CheckTypeAccept(v, x));
                                                 }
                                                 return true;
                                             }
                                             return false;
                                         });

                                     return new AcceptorInfo(e.Correlation, e.VisitedType, e.AddVisitFallback, e.AddVisitRedirect, ImplementationTypes);
                                 });
                             });
                    });
    }

    private static IncrementalValuesProvider<OutputVisitorInfo> CombineData(
            IncrementalValuesProvider<VisitorInfo> visitors,
            IncrementalValuesProvider<AcceptorInfo> acceptors,
            IncrementalValuesProvider<AcceptorInfo> autoAcceptor,
            IncrementalValuesProvider<OutputInfo> output,
            IncrementalValuesProvider<GenerateDefaultInfo> defaultGen,
            IncrementalValuesProvider<VisitableInfo> visitable,
            IncrementalValuesProvider<AcceptParamInfo> acceptParams,
            IncrementalValuesProvider<VisitParamInfo> visitParams)
    {
        return visitors
            .Combine(output.Collect())
            .Combine(defaultGen.Collect())
            .Combine(visitable.Collect())
            .Combine(acceptors.Collect())
            .Combine(autoAcceptor.Collect())
            .Combine(acceptParams.Collect())
            .Combine(visitParams.Collect())
            .Select((data, _) =>
            {
                (((((((VisitorInfo Visitor, ImmutableArray<OutputInfo> Output) Left, ImmutableArray<GenerateDefaultInfo> GenerateDefault) Left, ImmutableArray<VisitableInfo> Visitable) Left, ImmutableArray<AcceptorInfo> Acceptors) Left, ImmutableArray<AcceptorInfo> AutoAcceptors) Left, ImmutableArray<AcceptParamInfo> AcceptParam) Left, ImmutableArray<VisitParamInfo> VisitParams) = data;

                VisitorInfo Visitor = Left.Left.Left.Left.Left.Left.Visitor;
                OutputInfo Output = Left.Left.Left.Left.Left.Left.Output.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                GenerateDefaultInfo GenerateDefault = Left.Left.Left.Left.Left.GenerateDefault.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                VisitableInfo Visitable = Left.Left.Left.Left.Visitable.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                IEnumerable<AcceptorInfo> Acceptors = Left.Left.Left.Acceptors.Where(x => x.Correlation == Visitor.Correlation);
                IEnumerable<AcceptorInfo> AutoAcceptors = Left.Left.AutoAcceptors.Where(x => x.Correlation == Visitor.Correlation);
                AcceptParamInfo AcceptParam = Left.AcceptParam.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                VisitParamInfo VisitParam = VisitParams.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                //string Template = Templates.FirstOrDefault(x => x.Key == VisitorTemplateName)?.Template ?? DefaultTemplates.VisitorTemplate;
                string returnType = default;
                bool hasReturnType = false;
                if (Output != default)
                {
                    hasReturnType = true;
                    returnType = Output.Output.TypeFullName;
                }
                else
                {
                    hasReturnType = Visitor.Owner.IsGeneric && (Visitor.Owner.GenericTypes.Any(x => x.IsOut) || Visitor.Owner.GenericTypes.Where(x => x.IsVarianceUnspecified).Count() == 1);
                    returnType = Visitor.Owner.GenericTypes.FirstOrDefault(x => x.IsOut)?.Name ?? Visitor.Owner.GenericTypes.FirstOrDefault(x => x.IsVarianceUnspecified)?.Name;
                }
                List<NamedParamInfo> typedArgs = [];
                if (Visitor.Owner.IsGeneric && Visitor.Owner.GenericTypes.Any(x => x.IsIn))
                {
                    typedArgs.AddRange(Visitor.Owner.GenericTypes.Where(x => x.IsIn).Select(x => new NamedParamInfo
                    {
                        ParamTypeFullName = x.Name,
                        SanitizedParamName = x.Name.StartsWith("T") ? x.Name.Substring(1).ToLower() : x.Name.ToLower()

                    }));
                }
                if (VisitParam.VisitParamTypes != null && VisitParam.VisitParamTypes.Count() > 0)
                    typedArgs.AddRange(VisitParam.VisitParamTypes.Select(x =>
                    {
                        return new NamedParamInfo
                        {
                            ParamTypeFullName = x.VisitParamType.TypeFullName,
                            SanitizedParamName = x.VisitParamName ?? x.VisitParamType.SanitizeTypeNameAsArg,
                        };
                    }));
                List<NamedParamInfo> accept = [];
                if (Visitor.Owner.IsGeneric && Visitor.Owner.GenericTypes.Any(x => x.IsIn))
                {
                    accept.AddRange(Visitor.Owner.GenericTypes.Where(x => x.IsIn).Select(x => new NamedParamInfo
                    {
                        ParamTypeFullName = x.Name,
                        SanitizedParamName = x.Name.StartsWith("T") ? x.Name.Substring(1).ToLower() : x.Name.ToLower()

                    }));
                }
                if (AcceptParam.AcceptParamTypes != null && AcceptParam.AcceptParamTypes.Count() > 0)
                    accept.AddRange(AcceptParam.AcceptParamTypes.Select(x => new NamedParamInfo
                    {
                        SanitizedParamName = x.AcceptParamName ?? x.AcceptParamType.SanitizeTypeNameAsArg,
                        ParamTypeFullName = x.AcceptParamType.TypeFullName,
                    }));
                return new OutputVisitorInfo
                {
                    VisitMethodName = string.IsNullOrWhiteSpace(Visitor.VisitMethodName) ? DefaultVisitMethodName : Visitor.VisitMethodName.Trim(),
                    AccessibilityModifier = Visitor.AccessibilityModifier,
                    ClassName = Visitor.Owner.TypeName,
                    OutputNamespace = Visitor.Owner.ContainingNamespace,
                    KeywordTypeDefinition = Visitor.Keyword,
                    OriginalTypeDefinition = Visitor.Owner.TypeDefinition,
                    GenericTypesDefinition = Visitor.Owner.IsGeneric ? Visitor.Owner.TypeName.Replace(Visitor.Owner.GenericBaseTypeName, "") : null,
                    BaseTypeDefinition = Visitor.Owner.IsGeneric ? Visitor.Owner.TypeName.Substring(0, Visitor.Owner.TypeName.IndexOf("<")) : Visitor.Owner.TypeName,
                    HasReturnType = hasReturnType,
                    ReturnType = returnType,
                    HasArgs = typedArgs.Count > 0,
                    IsAsync = Visitor.IsAsync,
                    TypedArgs = [.. typedArgs],
                    ImplementationGroup = [.. Acceptors.Union(AutoAcceptors).Select(x => new ImplGroup
                    {
                        VisitedType = x.VisitedType,
                        AddVisitFallback = x.AddVisitFallback,
                        AddVisitRedirect = x.AddVisitRedirect,
                        ImplementationTypes = [.. x.ImplementationTypes],
                    })],
                    Default = new OutputVisitorDefaultInfo
                    {
                        DefaultTypeName = (Visitor.Owner.IsGeneric ? Visitor.Owner.TypeName.Substring(0, Visitor.Owner.TypeName.IndexOf("<")) : Visitor.Owner.TypeName).SanitizeBaseOrInterfaceName(),
                        GenerateDefault = GenerateDefault.GenerateDefault,
                        ForcePublic = GenerateDefault.ForcePublic,
                        IsAbstract = GenerateDefault.IsAbstract,
                        IsPartial = GenerateDefault.IsPartial,
                        IsVisitAbstract = GenerateDefault.IsVisitAbstract && GenerateDefault.IsAbstract,
                    },
                    Visitable = new OutputVisitableInfo
                    {
                        VisitableTypeName = Visitor.Owner.GenericBaseTypeName.Replace("Visitor", "") + "Visitable",
                        VisitableParameters = [.. accept],
                        GenerateVisitable = Visitable != default,
                        AcceptMethodName = string.IsNullOrWhiteSpace(Visitable.AcceptMethodName) ? DefaultAcceptMethodName : Visitable.AcceptMethodName.Trim(),
                    }
                };
            });
    }

    private record struct VisitorInfo(string Correlation, TargetTypeInfo Owner, string Keyword, string AccessibilityModifier, bool IsAsync, string VisitMethodName) { }
    private record struct AcceptorInfo(string Correlation, TargetTypeInfo VisitedType
        , bool AddVisitFallback, bool AddVisitRedirect, IEnumerable<TargetTypeInfo> ImplementationTypes)
    { }
    private record struct OutputInfo(string Correlation, TargetTypeInfo Output) { }
    private record struct VisitParamInfo(string Correlation, IEnumerable<(TargetTypeInfo VisitParamType, string VisitParamName)> VisitParamTypes) { }
    private record struct GenerateDefaultInfo(string Correlation, bool GenerateDefault, bool UseVisitFallBack, bool ForcePublic, bool IsPartial, bool IsAbstract, bool IsVisitAbstract) { }
    private record struct VisitableInfo(string Correlation, string AcceptMethodName) { }
    private record struct AcceptParamInfo(string Correlation, IEnumerable<(TargetTypeInfo AcceptParamType, string AcceptParamName)> AcceptParamTypes) { }
}
