using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Condor.Generator.Utils.Templating;
using Condor.Files.Generator.Abstractions;
using System.Text.RegularExpressions;

namespace Condor.Files.Generator;

[Generator]
public class TemplatedFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<KeyedTemplate> additionalFiles = context.GetTemplates();

        IncrementalValuesProvider<IntermediateInfo> visitors = GetFilesInfo(context);
        IncrementalValuesProvider<AddonsIntermediateInfo> addons = GetAddonsFilesInfo(context);
        IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, TemplatedFileInfoCollection)> combine = CombineData(visitors, addons, additionalFiles);

        context.RegisterSourceOutput(combine, (ctx, data) =>
        {
            ImmutableArray<KeyedTemplate> templates = data.Item1;
            TemplatedFileInfoCollection template_datas = data.Item2;

            string sourceName = string.Join(".", template_datas.ClassName.SanitizeToHintName(), template_datas.TemplateName, "generated");
            try
            {
                TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithAccessors(x => x
                    .AddDefaultsAccessors()
                    .CreateMemberObjectAccessor<TemplatedFileInfo>(TemplatedFileInfoAccessor.GetNamedProperty)
                    .CreateMemberObjectAccessor<TemplatedFileInfoCollection>(TemplatedFileInfoCollectionAccessor.GetNamedProperty)
                )
                .WithTemplates(templates).Build();
                string result = templateProcessor.Render(template_datas.TemplateName, template_datas);
                ctx.AddSource(sourceName, result);
            }
            catch (Exception ex)
            {
                ctx.AddSource(sourceName + ".error", $"/*{ex}*/");
            }
        });

    }

    private static IncrementalValuesProvider<AddonsIntermediateInfo> GetAddonsFilesInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.AdditionalTextsProvider
                    .Select((text, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new AddonsIntermediateInfo(text.Path, new TemplatedFileInfo
                        {
                            FileName = Path.GetFileNameWithoutExtension(text.Path),
                            FileContent = text.GetText(cancellationToken)?.ToString()
                        });
                    });
    }

    private static IncrementalValuesProvider<IntermediateInfo> GetFilesInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
           .ForAttributeWithMetadataName<IEnumerable<IntermediateInfo>>(
               typeof(FilesAttribute).FullName,
               (node, _) => true,
               (sc, cancellationToken) =>
               {
                   cancellationToken.ThrowIfCancellationRequested();
                   List<IntermediateInfo> files = [];
                   foreach (AttributeData attr in sc.Attributes)
                   {
                       files.Add(new IntermediateInfo(
                           sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                           attr.ConstructorArguments[0].Value?.ToString(),
                           attr.ConstructorArguments[1].Value?.ToString()
                        ));
                   }
                   return files;
               }).SelectMany((x, _) => x);
    }


    private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, TemplatedFileInfoCollection)> CombineData(
            IncrementalValuesProvider<IntermediateInfo> visitors,
            IncrementalValuesProvider<AddonsIntermediateInfo> addons,
            IncrementalValuesProvider<KeyedTemplate> additionalFiles)
    {
        return visitors
            .Combine(additionalFiles.Collect())
            .Combine(addons.Collect())
            .Select((x, _) =>
            {
                ImmutableArray<KeyedTemplate> templates = x.Left.Right;
                IntermediateInfo intermediate = x.Left.Left;
                ImmutableArray<AddonsIntermediateInfo> addons = x.Right;
                return (templates, new TemplatedFileInfoCollection
                {
                    ClassName = intermediate.Owner.TypeName,
                    OutputNamespace = intermediate.Owner.ContainingNamespace,
                    TemplateName = intermediate.TemplateName,
                    Type = intermediate.Owner,
                    Files = [.. addons.Where(x => Regex.IsMatch(x.Path, intermediate.Pattern)).Select(x => x.File)],
                });
            });
    }

    internal record struct IntermediateInfo(TargetTypeInfo Owner, string TemplateName, string Pattern);
    internal record struct AddonsIntermediateInfo(string Path, TemplatedFileInfo File);

}
