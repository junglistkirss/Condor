using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Toucan.Generator.Utils;
using Toucan.Generator.Utils.Visitors;
using Toucan.Generator.Utils.Templating;
using Toucan.Aggregate.Generator.Abstractions;

namespace Toucan.Aggregate.Generator
{
    [Generator]
    public class AggregateGenerator : IIncrementalGenerator
    {

        private const string AggregateTemplateName = "Aggregate";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            static (TargetTypeInfo PropertyType, string PropertyName) MapAggregateProperties(IPropertySymbol p)
            {
                return (
                    PropertyType: p.Type.Accept(TargetTypeVisitor.Instance),
                    PropertyName: p.Name
                );
            }

            IncrementalValueProvider<TypesProvider> types = context.GetTypesProvider();
            IncrementalValueProvider<ImmutableArray<KeyedTemplate>> additionalFiles = context.GetTemplates().Collect();

            var aggregate = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(AggregateAttribute).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       return (
                           Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           AggregateType: sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                           AggregateProperties: sc.TargetSymbol.Accept(MapMembers.Properties(MapAggregateProperties))
                        );
                   }).Combine(context.SyntaxProvider
             .ForAttributeWithMetadataName(
                 typeof(TemplatedAttribute).FullName,
                 (node, _) => node is InterfaceDeclarationSyntax,
                 (sc, cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     var attr = sc.Attributes.Single();
                     return (
                         Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                         AggregateTemplate: attr.ConstructorArguments.Single().Value.ToString()
                      );
                 }).Collect()).Select((x, cancellationToken) =>
                 {
                     var (Correlation, AggregateType, AggregateProperties) = x.Left;
                     ImmutableArray<(string Correlation, string AggregateTemplate)> tmpl = x.Right;
                     return (Correlation, AggregateType, AggregateProperties, AggregateTemplate: tmpl.FirstOrDefault(t => t.Correlation == Correlation).AggregateTemplate ?? AggregateTemplateName);
                 });
            var identity = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(Identity<>).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       return (
                           Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           IdentityType: attr.AttributeClass.GetTypedArguments().Single()
                        );
                   });

            var snapshot = context.SyntaxProvider
               .ForAttributeWithMetadataName(
                   typeof(Snapshot<>).FullName,
                   (node, _) => node is InterfaceDeclarationSyntax,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       return (
                           Correlation: sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           SnapshotType: attr.AttributeClass.GetTypedArguments().Single()
                        );
                   });

            var required = context.SyntaxProvider.ForAttributeWithMetadataName(
                   typeof(RequiredOnCreateAttribute).FullName,
                   (node, _) => node is PropertyDeclarationSyntax p,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return (
                            Correlation: sc.TargetSymbol.ContainingSymbol.Accept(StrongNameVisitor.Instance),
                            PropertyName: sc.TargetSymbol.Name,
                            IsUnique: sc.Attributes.Single().TryGetNamedArgument(nameof(RequiredOnCreateAttribute.IsUnique), out bool u) ? u : false
                       );
                   });
            var updatables = context.SyntaxProvider.ForAttributeWithMetadataName(
                   typeof(UpdatableAttribute).FullName,
                   (node, _) => node is PropertyDeclarationSyntax p,
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       return (
                            Correlation: sc.TargetSymbol.ContainingSymbol.Accept(StrongNameVisitor.Instance),
                            PropertyName: sc.TargetSymbol.Name
                       );
                   });

            var aggregatesDefinition = aggregate.Collect().Combine(identity.Collect()).Combine(snapshot.Collect()).SelectMany((dat, _) =>
            {
                return dat.Left.Left.Select(x =>
                {
                    var HasSnapshot = dat.Right.Any(s => s.Correlation == x.Correlation);
                    return (
                        x.Correlation,
                        x.AggregateType,
                        x.AggregateProperties,
                        x.AggregateTemplate,
                        dat.Left.Right.Single(i => i.Correlation == x.Correlation).IdentityType,
                        HasSnapshot,
                        SnapshotType: HasSnapshot ? dat.Right.FirstOrDefault(s => s.Correlation == x.Correlation).SnapshotType : null
                    );
                });
            }).Collect()
            .Combine(required.Collect())
            .Combine(updatables.Collect())
            .SelectMany((dat, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ImmutableArray<(string Correlation, string PropertyName, bool IsUnique)> create = dat.Left.Right;
                ImmutableArray<(string Correlation, string PropertyName)> update = dat.Right;

                return dat.Left.Left.Select(x =>
                {
                    var (Correlation, AggregateType, AggregateProperties, AggregateTemplate, IdentityType, HasSnapshot, SnapshotType) = x;
                    return (
                        Correlation,
                        AggregateType,
                        AggregateTemplate,
                        IdentityType,
                        HasSnapshot,
                        SnapshotType,
                        AggregateProperties: AggregateProperties.Select(p =>
                        {
                            var (PropertyType, PropertyName) = p;
                            return (
                                PropertyType,
                                PropertyName,
                                RequiredOnCreate: create.Any(c => c.Correlation == Correlation && c.PropertyName == PropertyName),
                                IsUnique: create.Any(c => c.Correlation == Correlation && c.PropertyName == PropertyName && c.IsUnique),
                                IsUpdate: update.Any(up => up.Correlation == Correlation && up.PropertyName == PropertyName)
                            );
                        })
                    ); ;
                });
            });

            var gen = aggregatesDefinition.Combine(additionalFiles);

            context.RegisterImplementationSourceOutput(gen, (ctx, data) =>
            {
                ImmutableArray<KeyedTemplate> templates = data.Right;
                TemplateProcessor templateProcessor = new TemplateProcessorBuilder().WithTemplates(templates).Build();
                var (Correlation, AggregateType, AggregateTemplate, IdentityType, HasSnapshot, SnapshotType, AggregateProperties) = data.Left;

                string template = templates
                     .FirstOrDefault(x => x.Key == AggregateTemplate)?.Template ?? throw new InvalidOperationException($"Missing template : {AggregateTemplate}");

                string className = AggregateType.TypeName + AggregateTemplate;
                var template_datas = new OutputAggregateInfo
                {
                    ClassName = className,
                    OutputNamespace = AggregateType.ContainingNamespace,
                    AggregateType = AggregateType,
                    HasSnapshot = HasSnapshot,
                    SnapshotType = SnapshotType,
                    IdentityType = IdentityType,
                    Datas = AggregateProperties.Select(p => new AggregateDataInfo
                    {
                        IsCreate = p.RequiredOnCreate,
                        IsUnique = p.IsUnique,
                        IsUpdate = p.IsUpdate,
                        PropertyType = p.PropertyType,
                        PropertyName = p.PropertyName,
                    }).ToArray(),
                };
                var result = templateProcessor.Render(template, template_datas);//, new RendererSettings())
                ctx.AddSource(string.Join(".", className.SanitizeToHintName(), AggregateTemplate, "Generated"), result);

            });

        }
    }
}
