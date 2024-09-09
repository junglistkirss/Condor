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
            IncrementalValueProvider<ImmutableArray<KeyedTemplate>> additionalFiles = context.GetTemplates().Collect();

            var visitors = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(VisitorAttribute).FullName,
                   (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       return (
                           Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           Owner: sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                           Keyword: ((TypeDeclarationSyntax)sc.TargetNode).Keyword.Text,
                           AccessibilityModifier: sc.TargetSymbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                           IsAsync: attr.TryGetNamedArgument(nameof(VisitorAttribute.IsAsync), out bool w) ? w : false
                        );
                   });


            var acceptors = context.SyntaxProvider.ForAttributeWithMetadataName(
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
                       return x.GroupBy(e => e.Correlation).Select(e =>
                       {
                           return (Correlation: e.Key, AddVisitFallBack: false, AddVisitRedirect: false, ThrowOnFallBack: false, ImplementationTypes: e.Select(i => i.ImplementationType));
                       });
                   });
            var output = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(OutputAttribute<>).FullName,
                   (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       return (
                           Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           Output: ((INamedTypeSymbol)attr.AttributeClass.TypeArguments.Single()).Accept(TargetTypeVisitor.Instance)
                        );
                   });
            var autoAcceptor = context.SyntaxProvider
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
                     AddVisitRedirect: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.AddVisitRedirect), out bool d) ? d : false,
                     ThrowOnFallBack: attr.TryGetNamedArgument(nameof(AutoAcceptorAttribute<object>.ThrowOnFallBack), out bool t) ? t : false
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
                             return (e.Correlation, e.AddVisitFallBack, e.AddVisitRedirect, e.ThrowOnFallBack, ImplementationTypes);
                         });
                     });
            });

            var visitParams = context.SyntaxProvider.ForAttributeWithMetadataName(
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
                           return (Correlation: e.Key, VisitParamTypes: e.Select(i => (i.VisitParamType, i.VisitParamName)));
                       });
                   });

            var @default = context.SyntaxProvider.ForAttributeWithMetadataName(
                   typeof(GenerateDefaultAttribute).FullName,
                   (node, _) => node is TypeDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       var vo = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.VisitOptions), out VisitOptions f) ? f : VisitOptions.AbstractVisit;
                       var o = attr.TryGetNamedArgument(nameof(GenerateDefaultAttribute.Options), out OptionsDefault od) ? od : OptionsDefault.None;
                       return (
                            Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                            GenerateDefault: true,
                            UseVisitFallBack: vo == VisitOptions.UseVisitFallBack,
                            ForcePublic: o.HasFlag(OptionsDefault.ForcePublic),
                            IsPartial: o.HasFlag(OptionsDefault.IsPartial),
                            IsAbstract: o.HasFlag(OptionsDefault.IsAbstract),
                            IsVisitAbstract: vo == VisitOptions.AbstractVisit
                       );
                   });

            var visitable = context.SyntaxProvider.ForAttributeWithMetadataName(
                   typeof(GenerateVisitableAttribute).FullName,
                   (node, _) => node is TypeDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return sc.TargetSymbol.Accept(StrongNameVisitor.Instance);
                   });

            var acceptParams = context.SyntaxProvider.ForAttributeWithMetadataName(
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
                           return (Correlation: e.Key, AcceptParamTypes: e.Select(i => (i.AcceptParamType, i.AcceptParamName)));
                       });
                   });


            var gen = visitors.Collect()
                .Combine(acceptors.Collect())
                .SelectMany((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return x.Left.Select(e =>
                    {
                        var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync) = e;
                        var ImplementationTypes = x.Right.Where(r => r.Correlation == Correlation).Select(z => (z.AddVisitFallBack, z.AddVisitRedirect, z.ThrowOnFallBack, z.ImplementationTypes));
                        return (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes);
                    });
                })
                .Combine(autoAcceptor.Collect())
                .Select((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes) = x.Left;
                    var AutoImplementationTypes = x.Right.Where(r => r.Correlation == Correlation).Select(z => (z.AddVisitFallBack, z.AddVisitRedirect, z.ThrowOnFallBack, z.ImplementationTypes));
                    return (
                        Correlation,
                        Owner,
                        Keyword,
                        AccessibilityModifier,
                        IsAsync,
                        ImplementationTypes: ImplementationTypes.Union(AutoImplementationTypes)
                    );
                })
                .Combine(output.Collect())
                .Select((x, cancellationToken) =>
                {
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes) = x.Left;
                    TargetTypeInfo? Output = null;
                    if(x.Right.Any(x => x.Correlation == Correlation))
                        Output = x.Right.SingleOrDefault(x => x.Correlation == Correlation).Output;
                    return (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output);
                })
                .Combine(@default.Collect())
                .Select((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output) = x.Left;
                    var GenerateDefault = x.Right.FirstOrDefault(x => x.Correlation == Correlation);
                    return (
                        Correlation,
                        Owner,
                        Keyword,
                        AccessibilityModifier,
                        IsAsync,
                        ImplementationTypes, 
                        Output,
                        GenerateDefault
                    );
                })
                .Combine(visitable.Collect())
                .Select((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault) = x.Left;
                    bool GenerateVisitable = x.Right.Any(x => x == Correlation);
                    return (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable);
                })
                .Combine(acceptParams.Collect())
                .Select((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable) = x.Left;
                    IEnumerable<(TargetTypeInfo AcceptParamType, string AcceptParamName)> AcceptParams = x.Right.Where(x => x.Correlation == Correlation).SelectMany(x => x.AcceptParamTypes);
                    return (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable, AcceptParams);
                })
                .Combine(visitParams.Collect())
                .Select((x, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable, AcceptParams) = x.Left;
                    IEnumerable<(TargetTypeInfo VisitParamType, string VisitParamName)> VisitParams = x.Right.Where(x => x.Correlation == Correlation).SelectMany(x => x.VisitParamTypes);
                    return (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable, AcceptParams, VisitParams);
                })
                /*.Select((x,_)=>
                {
                    (string Correlation, TargetTypeInfo Owner, string AccessibilityModifier, TargetTypeInfo VisitedType, TargetTypeInfo[] ImplementationTypes, (bool GenerateDefault, bool ThrowOnFallBack, bool UseVisitFallback) GenerateDefault, bool GenerateVisitable, TargetTypeInfo[] AcceptParams, TargetTypeInfo[] VisitParams) t = x;
                    return (t.Owner, t.AccessibilityModifier, t.VisitedType, t.ImplementationTypes, t.GenerateDefault, t.GenerateVisitable, t.AcceptParams, t.VisitParams);
                })*/
                .Combine(additionalFiles);


            context.RegisterSourceOutput(gen, (ctx, data) =>
            {
                ImmutableArray<KeyedTemplate> templates = data.Right;

                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithTemplates(templates).Build();

                string template = templates
                     .FirstOrDefault(x => x.Key == VisitorTemplateName)?.Template ?? DefaultTemplates.VisitorTemplate;

                var (Correlation, Owner, Keyword, AccessibilityModifier, IsAsync, ImplementationTypes, Output, GenerateDefault, GenerateVisitable, AcceptParams, VisitParams) = data.Left;
                string className = Owner.TypeName;

                List<NamedParamInfo> typedArgs = new List<NamedParamInfo>();
                if (Owner.IsGeneric && Owner.GenericTypes.Any(x => x.IsIn))
                {
                    typedArgs.AddRange(Owner.GenericTypes.Where(x => x.IsIn).Select(x => new NamedParamInfo
                    {
                        ParamTypeFullName = x.Name,
                        SanitizedParamName = x.Name.StartsWith("T") ? x.Name.Substring(1).ToLower() : x.Name.ToLower()

                    }));
                }
                typedArgs.AddRange(VisitParams.Select(x => new NamedParamInfo
                {
                    ParamTypeFullName = x.VisitParamType.TypeFullName,
                    SanitizedParamName = x.VisitParamName ?? x.VisitParamType.SanitizeTypeNameAsArg,
                }));

                string returnType = default;
                bool hasReturnType = false;
                if (Output != null) {
                    hasReturnType = true;
                    returnType = Output.TypeFullName;
                }
                else
                {
                    hasReturnType = Owner.IsGeneric && (Owner.GenericTypes.Any(x => x.IsOut) || Owner.GenericTypes.Where(x => x.IsVarianceUnspecified).Count() == 1);
                    returnType = Owner.GenericTypes.FirstOrDefault(x => x.IsOut)?.Name ?? Owner.GenericTypes.FirstOrDefault(x => x.IsVarianceUnspecified)?.Name;
                }


                var template_datas = new OutputVisitorInfo
                {
                    ClassName = className,
                    OutputNamespace = Owner.ContainingNamespace,
                    KeywordTypeDefinition = Keyword,
                    OriginalTypeDefinition = Owner.TypeDefinition,
                    GenericTypesDefinition = Owner.IsGeneric ? Owner.TypeName.Replace(Owner.GenericBaseTypeName, "") : null,
                    BaseTypeDefinition = Owner.IsGeneric ? Owner.TypeName.Substring(0, Owner.TypeName.IndexOf("<")) : Owner.TypeName,
                    AccessibilityModifier = AccessibilityModifier,
                    HasReturnType = hasReturnType,
                    ReturnType = returnType ,
                    HasArgs = typedArgs.Count > 0,
                    IsAsync = IsAsync,
                    TypedArgs = typedArgs.ToArray(),
                    ImplementationGroup = ImplementationTypes.Select(x => new ImplGroup
                    {
                        AddVisitFallBack = x.AddVisitFallBack,
                        AddVisitRedirect = x.AddVisitRedirect,
                        ImplementationTypes = x.ImplementationTypes.ToArray(),
                        ThrowOnFallBack = x.ThrowOnFallBack,
                    }).ToArray(),
                    Default = new OutputVisitorDefaultInfo
                    {
                        DefaultTypeName = (Owner.IsGeneric ? Owner.TypeName.Substring(0, Owner.TypeName.IndexOf("<")) : Owner.TypeName).SanitizeBaseOrInterfaceName(),
                        GenerateDefault = GenerateDefault.GenerateDefault,
                        ForcePublic = GenerateDefault.ForcePublic,
                        IsAbstract = GenerateDefault.IsAbstract,
                        IsPartial = GenerateDefault.IsPartial,
                        IsVisitAbstract = GenerateDefault.IsVisitAbstract && GenerateDefault.IsAbstract,
                    },
                    Visitable = new OutputVisitableInfo
                    {
                        VisitableTypeName = Owner.GenericBaseTypeName.Replace("Visitor", "") + "Visitable",
                        VisitableParameters = AcceptParams.Select(x => new NamedParamInfo
                        {
                            SanitizedParamName = x.AcceptParamName ?? x.AcceptParamType.SanitizeTypeNameAsArg,
                            ParamTypeFullName = x.AcceptParamType.TypeFullName,
                        }).ToArray(),
                        GenerateVisitable = GenerateVisitable,

                    }
                };
                var result = templateProcessor.Render(template, template_datas);//, new RendererSettings())
                ctx.AddSource(string.Join(".", className.SanitizeToHintName(), VisitorTemplateName, "g"), result);
            });

        }
    }
}
