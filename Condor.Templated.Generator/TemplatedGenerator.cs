using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Condor.Generator.Utils.Templating;
using Condor.Templated.Generator.Abstractions;
using System.Data;
using HandlebarsDotNet;
using System.Collections;

namespace Condor.Templated.Generator
{

    [Generator]
    public class TemplatedGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<TypesProvider> types = context.GetTypesProvider();
            IncrementalValueProvider<ImmutableArray<KeyedTemplate>> additionalFiles = context.GetTemplates().Collect();

            var aggregate = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                 typeof(TemplatedAttribute).FullName,
                 (node, _) => node is InterfaceDeclarationSyntax,
                 (sc, cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     var attr = sc.Attributes.Single();
                     return (
                         Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                         TemplatedType: sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                         TemplateName: attr.ConstructorArguments.Single().Value?.ToString(),
                         Properties: sc.TargetSymbol.Accept(new MapMembersVisitor<IPropertySymbol, MemberDataInfo>(x =>
                         {
                             return new MemberDataInfo
                             {
                                 Enhancements = x.GetAttributes().Where(x => x.AttributeClass.MetadataName == typeof(EnhanceAttribute<>).Name).Select(attr =>
                                 {
                                     return new EnhanceInfo
                                     {
                                         Key = attr.ConstructorArguments.SingleOrDefault().Value?.ToString(),
                                         EnchancedType = attr.AttributeClass.GetTypedArguments().Single(),
                                     };
                                 }).Union(x.GetAttributes().Where(x => x.AttributeClass.MetadataName == typeof(MarkerAttribute).Name).Select(attr =>
                                 {
                                     return new EnhanceInfo
                                     {
                                         Key = attr.ConstructorArguments.SingleOrDefault().Value?.ToString(),
                                         EnchancedType = null,
                                     };
                                 })).ToArray(),
                                 Extends = x.GetAttributes().Where(x => x.AttributeClass.MetadataName == typeof(ValueAttribute).Name).Select(attr =>
                                 {
                                     return new ExtendInfo
                                     {
                                         Key = attr.ConstructorArguments.FirstOrDefault().Value?.ToString(),
                                         Value = attr.ConstructorArguments.LastOrDefault().Value?.ToString(),
                                     };
                                 }).ToArray(),
                                 Member = x.Accept(MemberVisitor.Instance),
                             };
                         })),
                         Actions: sc.TargetSymbol.Accept(new MapMembersVisitor<IMethodSymbol, ActionDataInfo>(x =>
                         {
                             return new ActionDataInfo
                             {
                                 Enhancements = x.GetAttributes().Where(x => x.AttributeClass.MetadataName == typeof(EnhanceAttribute<>).Name).Select(attr =>
                                 {
                                     return new EnhanceInfo
                                     {
                                         Key = attr.ConstructorArguments.SingleOrDefault().Value?.ToString(),
                                         EnchancedType = attr.AttributeClass.GetTypedArguments().Single(),
                                     };
                                 }).ToArray(),
                                 Extends = x.GetAttributes().Where(x => x.AttributeClass.MetadataName == typeof(ValueAttribute).Name).Select(attr =>
                                 {
                                     return new ExtendInfo
                                     {
                                         Key = attr.ConstructorArguments.FirstOrDefault().Value?.ToString(),
                                         Value = attr.ConstructorArguments.LastOrDefault().Value?.ToString(),
                                     };
                                 }).ToArray(),
                                 Action = x.Accept(ActionVisitor.Instance),
                             };
                         }))
                     );
                 });
            var enhances = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(EnhanceAttribute<>).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax || node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return (
                            Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                            Enhancement: sc.Attributes.Select(attr =>
                            {
                                return new EnhanceInfo
                                {
                                    Key = attr.ConstructorArguments.SingleOrDefault().Value?.ToString(),
                                    EnchancedType = attr.AttributeClass.GetTypedArguments().Single(),
                                };
                            })
                           );
                   });

            var markers = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(MarkerAttribute).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax || node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return (
                            Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                            Markers: sc.Attributes.Select(attr =>
                            {
                                return new EnhanceInfo
                                {
                                    Key = attr.ConstructorArguments.SingleOrDefault().Value?.ToString(),
                                    EnchancedType = null,
                                };
                            })
                       );
                   });

            var extends = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(ValueAttribute).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax || node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return (
                            Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                            Value: sc.Attributes.Select(attr =>
                            {
                                return new ExtendInfo
                                {
                                    Key = attr.ConstructorArguments.FirstOrDefault().Value?.ToString(),
                                    Value = attr.ConstructorArguments.LastOrDefault().Value?.ToString(),
                                };
                            })
                        );
                   });



            var definitions = aggregate.Combine(enhances.Collect()).Combine(markers.Collect()).Combine(extends.Collect())
                .Select((dat, _) =>
            {
                (string Correlation, TargetTypeInfo TemplatedType, string TemplateName, MemberDataInfo[] Properties, ActionDataInfo[] Actions) t = dat.Left.Left.Left;
                ImmutableArray<(string Correlation, IEnumerable<EnhanceInfo> Enhancement)> e = dat.Left.Left.Right;
                ImmutableArray<(string Correlation, IEnumerable<EnhanceInfo> Markers)> m = dat.Left.Right;
                ImmutableArray<(string Correlation, IEnumerable<ExtendInfo> Value)> x = dat.Right;
                return (
                    t.Correlation,
                    t.TemplatedType,
                    t.TemplateName,
                    t.Properties,
                    t.Actions,
                    Enchancements: e.Where(x => x.Correlation == t.Correlation).SelectMany(x => x.Enhancement),
                    Markers: m.Where(x => x.Correlation == t.Correlation).SelectMany(x => x.Markers).Distinct(),
                    Extends: x.Where(x => x.Correlation == t.Correlation).SelectMany(x => x.Value)
                );
            });

            var gen = definitions.Combine(additionalFiles);

            context.RegisterImplementationSourceOutput(gen, (ctx, data) =>
            {
                ImmutableArray<KeyedTemplate> templates = data.Right;
                TemplateProcessorBuilder templateProcessorBuilder = new TemplateProcessorBuilder()
                    .WithTemplates(templates)
                    .WithExtend(x =>
                    {
                        x.RegisterHelper("WithEnhancements", (ctx, args) =>
                        {
                            object obj = args.First();
                            IEnumerable<string> search = args.Skip(1).Select(x => x.ToString());
                            if (obj is EnhanceInfo[] enhances)
                            {
                                return enhances.Where(x => search.Any(s => x.Key == s));
                            }
                            if (obj is TemplatedInfo templated)
                            {
                                return templated.Enhancements.Where(x => search.Any(s => x.Key == s));
                            }
                            if (obj is ActionDataInfo[] actions)
                            {
                                return actions.Where(x => search.Any(s => x.Enhancements.Any(x => x.Key == s)));
                            }
                            if (obj is MemberDataInfo[] members)
                            {
                                return members.Where(x => search.Any(s => x.Enhancements.Any(x => x.Key == s)));
                            }
                            return null;
                        });
                        x.RegisterHelper("WithAllEnhancements", (ctx, args) =>
                        {
                            object obj = args.First();
                            IEnumerable<string> search = args.Skip(1).Select(x => x.ToString());
                            if (obj is EnhanceInfo[] enhances)
                            {
                                return enhances.Where(x => search.All(s => x.Key == s));
                            }
                            if (obj is TemplatedInfo templated)
                            {
                                return templated.Enhancements.Where(x => search.All(s => x.Key == s));
                            }
                            if (obj is ActionDataInfo[] actions)
                            {
                                return actions.Where(x => search.All(s => x.Enhancements.Any(x => x.Key == s)));
                            }
                            if (obj is MemberDataInfo[] members)
                            {
                                return members.Where(x => search.All(s => x.Enhancements.Any(x => x.Key == s)));
                            }
                            return null;
                        });
                        x.RegisterHelper("HasEnhancements", (ctx, args) =>
                        {
                            object obj = args.First();
                            IEnumerable<string> search = args.Skip(1).Select(x => x.ToString());
                            if (obj is EnhanceInfo[] enhances)
                            {
                                return enhances.Where(x => search.Any(s => x.Key == s)).Any();
                            }
                            if (obj is TemplatedInfo templated)
                            {
                                return templated.Enhancements.Where(x => search.Any(s => x.Key == s)).Any();
                            }
                            if (obj is ActionDataInfo[] actions)
                            {
                                return actions.Where(x => search.Any(s => x.Enhancements.Any(x => x.Key == s))).Any();
                            }
                            if (obj is MemberDataInfo[] members)
                            {
                                return members.Where(x => search.Any(s => x.Enhancements.Any(x => x.Key == s))).Any();
                            }
                            return false;
                        });
                        x.RegisterHelper("HasAllEnhancements", (ctx, args) =>
                        {
                            object obj = args.First();
                            IEnumerable<string> search = args.Skip(1).Select(x => x.ToString());
                            if (obj is EnhanceInfo[] enhances)
                            {
                                return enhances.Where(x => search.All(s => x.Key == s)).Any();
                            }
                            if (obj is TemplatedInfo templated)
                            {
                                return templated.Enhancements.Where(x => search.All(s => x.Key == s)).Any();
                            }
                            if (obj is ActionDataInfo[] actions)
                            {
                                return actions.Where(x => search.All(s => x.Enhancements.Any(x => x.Key == s))).Any();
                            }
                            else if (obj is MemberDataInfo[] members)
                            {
                                return members.Where(x => search.All(s => x.Enhancements.Any(x => x.Key == s))).Any();
                            }
                            return false;
                        });
                        x.RegisterHelper("SingleEnhancement", (ctx, args) =>
                        {
                            object obj = args.First();
                            string search = args.Last().ToString();
                            if (obj is EnhanceInfo[] enhances)
                            {
                                return enhances.SingleOrDefault(x => x.Key == search);
                            }
                            if (obj is TemplatedInfo templated)
                            {
                                return templated.Enhancements.SingleOrDefault(x => x.Key == search);
                            }
                            if (obj is ActionDataInfo[] actions)
                            {
                                return actions.SingleOrDefault(x => x.Enhancements.Any(x => x.Key == search));
                            }
                            if (obj is MemberDataInfo[] members)
                            {
                                return members.SingleOrDefault(x => x.Enhancements.Any(x => x.Key == search));
                            }
                            return null;
                        });
                        //x.RegisterHelper("HasKey", (ctx, args) =>
                        //{
                        //    var dic = args.At<IEnumerable>(0) as IDictionary;
                        //    return dic?.Contains(args.At<string>(1)) ?? false;
                        //});
                        //x.RegisterHelper("ByKey", (ctx, args) =>
                        //{
                        //    var dic = args.At<IEnumerable>(0) as IDictionary;
                        //    return dic?[args.At<string>(1)];
                        //});

                        //x.RegisterHelper("Keys", (ctx, args) =>
                        //{
                        //    var dic = args.At<IEnumerable>(0) as IDictionary;
                        //    return dic?.Keys;
                        //});
                        //x.RegisterHelper("Values", (ctx, args) =>
                        //{
                        //    var dic = args.At<IEnumerable>(0) as IDictionary;
                        //    return dic?.Values;
                        //});
                    });

                TemplateProcessor templateProcessor = templateProcessorBuilder.Build();

                var (Correlation, TemplatedType, TemplateName, Properties, Actions, Enchancements, Markers, Extends) = data.Left;

                string template = templates
                     .FirstOrDefault(x => x.Key == TemplateName)?.Template ?? throw new InvalidOperationException($"Missing template : {TemplateName}");

                string className = TemplatedType.TypeName;
                var template_datas = new TemplatedInfo
                {
                    ClassName = className,
                    OutputNamespace = TemplatedType.ContainingNamespace,
                    TemplatedType = TemplatedType,
                    Enhancements = Enchancements.Union(Markers).ToArray(),
                    Extends = Extends.ToArray(),
                    Properties = Properties.ToArray(),
                    Actions = Actions.ToArray(),
                };
                var result = templateProcessor.Render(template, template_datas);//, new RendererSettings())
                ctx.AddSource(string.Join(".", className.SanitizeToHintName(), TemplateName, "g"), result);

            });

        }
    }
}
