using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Condor.Generator.Utils.Templating;
using Condor.Constants.Generator.Abstractions;

namespace Condor.Constants.Generator;

[Generator]
public class ConstantsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<KeyedTemplate> additionalFiles = context.GetTemplates();

        IncrementalValuesProvider<ConstsOwnerInfo> visitors = GetConstantsInfo(context);
        IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, ConstantInfoCollection)> combine = CombineData(visitors, additionalFiles);

        context.RegisterSourceOutput(combine, (Action<SourceProductionContext, (ImmutableArray<KeyedTemplate>, ConstantInfoCollection)>)((ctx, data) =>
        {
            ImmutableArray<KeyedTemplate> templates = data.Item1;
            ConstantInfoCollection template_datas = data.Item2;

            string sourceName = string.Join(".", template_datas.ClassName.SanitizeToHintName(), template_datas.TemplateName, "generated");
            try
            {
                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithAccessors(x => x
                        .AddDefaultsAccessors()
                        .CreateMemberObjectAccessor<ConstantInfo>(ConstantInfoAccessor.GetNamedProperty)
                        .CreateMemberObjectAccessor<ConstantInfoCollection>(ConstantInfoCollectionAccessor.GetNamedProperty)
                    )
                    .WithTemplates(templates)
                    .Build();
                string result = templateProcessor.Render(template_datas.TemplateName, template_datas);
                ctx.AddSource(sourceName, result);

            }
            catch (Exception ex)
            {
                ctx.AddSource(sourceName + ".error", $"/*{ex}*/");
            }
        }));

    }
    private static IncrementalValuesProvider<ConstsOwnerInfo> GetConstantsInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
           .ForAttributeWithMetadataName<IEnumerable<ConstsOwnerInfo>>(
               typeof(ConstantsAttribute).FullName,
               (node, _) => node is TypeDeclarationSyntax,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   List<ConstsOwnerInfo> consts = [];
                   foreach (AttributeData attr in sc.Attributes)
                   {
                       ConstInfo[] members = [..
                           sc.TargetSymbol
                            .Accept(MembersVisitor<IFieldSymbol>.Instance)
                            .Where(x => x.IsConstant)
                            .Select(x =>
                            {
                                string[] partials = [.. x.Attributes.Where(a => a.AttributeType.TypeFullName == typeof(ConstantAttribute).FullName).Select(x => x.ConstructorArguments[0].ArgumentValue?.ToString())];
                                return new ConstInfo(x, partials ?? []);
                            })
                       ];

                       consts.Add(new ConstsOwnerInfo(
                           sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                           sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                           attr.ConstructorArguments[0].Value?.ToString(),
                           members
                        ));
                   }
                   return consts;
               }).SelectMany((x, _) => x);
    }


    private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, ConstantInfoCollection)> CombineData(
            IncrementalValuesProvider<ConstsOwnerInfo> consts,
            IncrementalValuesProvider<KeyedTemplate> additionalFiles)
    {
        return consts
            .Combine(additionalFiles.Collect())
            .Select((data, _) =>
            {
                return (data.Right, new ConstantInfoCollection
                {
                    ClassName = data.Left.Owner.TypeName,
                    ConstantType = data.Left.Owner,
                    TemplateName = data.Left.Template,
                    OutputNamespace = data.Left.Owner.ContainingNamespace,
                    Map = [.. data.Left.Consts.Select(x => new ConstantInfo
                    {
                        Member = x.Member,
                        Partials = x.Partials,
                    })]
                });
            });
    }
}

internal record struct ConstsOwnerInfo(string Correlation, TargetTypeInfo Owner, string Template, ConstInfo[] Consts);
internal record struct ConstInfo(MemberInfo Member, string[] Partials);
