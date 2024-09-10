using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis.CSharp;
using Condor.Generator.Utils.Templating;
using Condor.Visitor.Generator.Abstractions;

namespace Condor.Visitor.Generator
{
    [Generator]
    public class VisitorGenerator : IIncrementalGenerator
    {
        private const string VisitorTemplateName = "Visitor";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            IncrementalValueProvider<TypesProvider> types = context.GetTypesProvider();
            IncrementalValuesProvider<KeyedTemplate> additionalFiles = context.GetTemplates();

            IncrementalValuesProvider<VisitorInfo> visitors = GetVisitorsInfo(context);
            IncrementalValuesProvider<AcceptorInfo> acceptors = GetAcceptorsInfo(context);
            IncrementalValuesProvider<OutputInfo> output = GetOutputInfo(context);
            IncrementalValuesProvider<AcceptorInfo> autoAcceptor = GetAutoAcceptorsInfo(context, types);
            IncrementalValuesProvider<VisitParamInfo> visitParams = GetVisitParamdInfo(context);
            IncrementalValuesProvider<GenerateDefaultInfo> @default = GetGenerateDefaultInfo(context);
            IncrementalValuesProvider<VistableInfo> visitable = GetVisitableInfo(context);
            IncrementalValuesProvider<AcceptParamInfo> acceptParams = GetAcceptParamInfo(context);
            IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, OutputVisitorInfo)> combine = CombineData(visitors, acceptors, autoAcceptor, output, @default, visitable, acceptParams, visitParams, additionalFiles);

            context.RegisterSourceOutput(combine, (ctx, data) =>
            {
                ImmutableArray<KeyedTemplate> templates = data.Item1;
                OutputVisitorInfo template_datas = data.Item2;

                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithTemplates(templates).Build();

                string template = templates.FirstOrDefault(x => x.Key == VisitorTemplateName)?.Template ?? DefaultTemplates.VisitorTemplate;


                var result = templateProcessor.Render(template, template_datas);//, new RendererSettings())
                ctx.AddSource(string.Join(".", template_datas.ClassName.SanitizeToHintName(), VisitorTemplateName, "generated"), result);
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

        private static IncrementalValuesProvider<VistableInfo> GetVisitableInfo(IncrementalGeneratorInitializationContext context)
        {
            return context.SyntaxProvider.ForAttributeWithMetadataName(
                   typeof(GenerateVisitableAttribute).FullName,
                   (node, _) => node is TypeDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return new VistableInfo(sc.TargetSymbol.Accept(StrongNameVisitor.Instance));
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
                       var attr = sc.Attributes.Single();
                       var vo = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.VisitOptions), out VisitOptions f) ? f : VisitOptions.AbstractVisit;
                       var o = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.Options), out OptionsDefault od) ? od : OptionsDefault.None;
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
                       var attr = sc.Attributes.Single();
                       return new VisitorInfo(
                           sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                           ((TypeDeclarationSyntax)sc.TargetNode).Keyword.Text,
                           sc.TargetSymbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                           attr.TryGetNamedArgument(nameof(VisitorAttribute.IsAsync), out bool w) ? w : false
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
                       return x.GroupBy(e => new { e.Correlation , e.ImplementationType}).Select(e =>
                       {
                           return new AcceptorInfo(e.Key.Correlation, e.Key.ImplementationType, AddVisitFallBack: false, AddVisitRedirect: false, ImplementationTypes: e.Select(i => i.ImplementationType));
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
                       var attr = sc.Attributes.Single();
                       return new OutputInfo(
                           sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance)
                        );
                   });
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
                                 AssemblyPart: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AssemblyPart), out string ap) ? ap : nameof(Condor),
                                 AllowAbstract: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AllowAbstract), out bool abs) ? abs : false,
                                 AllowGeneric: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AllowGeneric), out bool g) ? g : false,
                                 AllowRecord: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AllowAbstract), out bool r) ? r : true,
                                 AddVisitFallBack: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AddVisitFallBack), out bool f) ? f : false,
                                 AddVisitRedirect: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AddVisitRedirect), out bool d) ? d : false
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
                                             x => x.Name.StartsWith(e.AssemblyPart, StringComparison.OrdinalIgnoreCase),
                                             x => (x.IsAbstract && e.AllowAbstract || !x.IsAbstract)
                                              && (x.IsRecord && e.AllowRecord || !x.IsRecord)
                                              && (x.IsGenericType && e.AllowGeneric || !x.IsGenericType)
                                              && (
                                                x.AllInterfaces.Any(i => i.Accept(StrongNameVisitor.Instance) == e.VisitedType.TypeFullName)
                                                || x.Accept(BaseTypesVisitor.Instance).Any(b => b.Accept(StrongNameVisitor.Instance) == e.VisitedType.TypeFullName)

                                                )
                                         );
                                         return new AcceptorInfo(e.Correlation, e.VisitedType, e.AddVisitFallBack, e.AddVisitRedirect, ImplementationTypes);
                                     });
                                 });
                        });
        }

        private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, OutputVisitorInfo)> CombineData(
            IncrementalValuesProvider<VisitorInfo> visitors,
            IncrementalValuesProvider<AcceptorInfo> acceptors,
            IncrementalValuesProvider<AcceptorInfo> autoAcceptor,
            IncrementalValuesProvider<OutputInfo> output,
            IncrementalValuesProvider<GenerateDefaultInfo> defaultGen,
            IncrementalValuesProvider<VistableInfo> visitable,
            IncrementalValuesProvider<AcceptParamInfo> acceptParams,
            IncrementalValuesProvider<VisitParamInfo> visitParams,
            IncrementalValuesProvider<KeyedTemplate> additionalFiles)
        {
            return visitors
                .Combine(output.Collect())
                .Combine(defaultGen.Collect())
                .Combine(visitable.Collect())
                .Combine(acceptors.Collect())
                .Combine(autoAcceptor.Collect())
                .Combine(acceptParams.Collect())
                .Combine(visitParams.Collect())
                .Combine(additionalFiles.Collect())
                .Select((data, _) =>
                {
                    ((((((((VisitorInfo Visitor, ImmutableArray<OutputInfo> Output) Left, ImmutableArray<GenerateDefaultInfo> GenerateDefault) Left, ImmutableArray<VistableInfo> Visitable) Left, ImmutableArray<AcceptorInfo> Acceptors) Left, ImmutableArray<AcceptorInfo> AutoAcceptors) Left, ImmutableArray<AcceptParamInfo> AcceptParam) Left, ImmutableArray<VisitParamInfo> VisitParam) Left, ImmutableArray<KeyedTemplate> Templates) = data;

                    VisitorInfo Visitor = Left.Left.Left.Left.Left.Left.Left.Visitor;
                    OutputInfo Output = Left.Left.Left.Left.Left.Left.Left.Output.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    GenerateDefaultInfo GenerateDefault = Left.Left.Left.Left.Left.Left.GenerateDefault.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    VistableInfo Visitable = Left.Left.Left.Left.Left.Visitable.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    IEnumerable<AcceptorInfo> Acceptors = Left.Left.Left.Left.Acceptors.Where(x => x.Correlation == Visitor.Correlation);
                    IEnumerable<AcceptorInfo> AutoAcceptors = Left.Left.Left.AutoAcceptors.Where(x => x.Correlation == Visitor.Correlation);
                    AcceptParamInfo AcceptParam = Left.Left.AcceptParam.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    VisitParamInfo VisitParam = Left.VisitParam.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
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
                    List<NamedParamInfo> typedArgs = new List<NamedParamInfo>();
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
                    List<NamedParamInfo> accept = new List<NamedParamInfo>();
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
                    return (Templates, new OutputVisitorInfo
                    {
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
                        TypedArgs = typedArgs.ToArray(),
                        ImplementationGroup = Acceptors.Union(AutoAcceptors).Select(x => new ImplGroup
                        {
                            VisitedType = x.VisitedType,
                            AddVisitFallBack = x.AddVisitFallBack,
                            AddVisitRedirect = x.AddVisitRedirect,
                            ImplementationTypes = x.ImplementationTypes.ToArray(),
                        }).ToArray(),
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
                            VisitableParameters = accept.ToArray(),
                            GenerateVisitable = Visitable != default,

                        }
                    });
                });
        }
    }

    internal record struct VisitorInfo(string Correlation, TargetTypeInfo Owner, string Keyword, string AccessibilityModifier, bool IsAsync) { }
    internal record struct AcceptorInfo(string Correlation, TargetTypeInfo VisitedType
        , bool AddVisitFallBack, bool AddVisitRedirect, IEnumerable<TargetTypeInfo> ImplementationTypes)
    { }
    internal record struct OutputInfo(string Correlation, TargetTypeInfo Output) { }
    internal record struct VisitParamInfo(string Correlation, IEnumerable<(TargetTypeInfo VisitParamType, string VisitParamName)> VisitParamTypes) { }
    internal record struct GenerateDefaultInfo(string Correlation, bool GenerateDefault, bool UseVisitFallBack, bool ForcePublic, bool IsPartial, bool IsAbstract, bool IsVisitAbstract) { }
    internal record struct VistableInfo(string Correlation) { }
    internal record struct AcceptParamInfo(string Correlation, IEnumerable<(TargetTypeInfo AcceptParamType, string AcceptParamName)> AcceptParamTypes) { }
}
