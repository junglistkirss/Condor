using Condor.Constants.Generator.Abstractions;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Templating;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Condor.Constants.Generator;

[Generator]
public class ConstantsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<KeyedTemplate> additionalFiles = context.GetTemplates();

        IncrementalValuesProvider<ConstsOwnerInfo> visitors = GetConstantsInfo(context);
        IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, ConstantsInfo)> combine = CombineData(visitors, additionalFiles);

        context.RegisterSourceOutput(combine, (ctx, data) =>
        {
            ImmutableArray<KeyedTemplate> templates = data.Item1;
            ConstantsInfo template_datas = data.Item2;

            string sourceName = string.Join(".", template_datas.ClassName.SanitizeToHintName(), template_datas.TemplateName, "generated");
            try
            {
                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithTemplates(templates).Build();
                string? template = templates.FirstOrDefault(x => x.Key == template_datas.TemplateName)?.Template;
                if (!string.IsNullOrEmpty(template))
                {
                    string result = templateProcessor.Render(template!, template_datas);
                    ctx.AddSource(sourceName, result);
                }
            }
            catch (Exception ex)
            {
                ctx.AddSource(sourceName + ".error", $"/*{ex}*/");
            }
        });

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
                       var members = sc.TargetSymbol
                            .Accept(MembersVisitor<IFieldSymbol>.Instance)
                            .Where(x => x.IsConstant)
                            .Select(x =>
                            {
                                string[] partials = [.. x.Attributes
                                    .Where(a => a.AttributeType.TypeFullName == typeof(ConstantAttribute).FullName)
                                    .Select(x => x.ConstructorArguments[0].ArgumentValue?.ToString() ?? throw new NullReferenceException("Missing argument value"))];
                                return new ConstInfo(x, partials ?? []);
                            }).ToArray();

                       consts.Add(new ConstsOwnerInfo(
                           sc.TargetSymbol.Accept(StrongNameVisitor.Instance) ?? throw new NullReferenceException("TargetSymbol name required"),
                           sc.TargetSymbol.Accept(TargetTypeVisitor.Instance) ?? throw new NullReferenceException("TargetSymbol type required"),
                           attr.ConstructorArguments[0].Value?.ToString() ?? throw new NullReferenceException("Missing argument value"),
                           members
                        ));
                   }
                   return consts;
               }).SelectMany((x, _) => x);
    }


    private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, ConstantsInfo)> CombineData(
            IncrementalValuesProvider<ConstsOwnerInfo> consts,
            IncrementalValuesProvider<KeyedTemplate> additionalFiles)
    {
        return consts
            .Combine(additionalFiles.Collect())
            .Select((data, _) =>
            {
                return (data.Right, new ConstantsInfo
                {
                    ClassName = data.Left.Owner.TypeName,
                    ConstantType = data.Left.Owner,
                    TemplateName = data.Left.Template,
                    OutputNamespace = data.Left.Owner.ContainingNamespace,
                    Map = [.. data.Left.Consts.Select(x => new ConstantInfo(x.Member, x.Partials))]
                });
            });
    }
}

internal record struct ConstsOwnerInfo(string Correlation, TargetTypeInfo Owner, string Template, ConstInfo[] Consts);
internal record struct ConstInfo(MemberInfo Member, string[] Partials);
