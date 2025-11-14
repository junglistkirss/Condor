using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis.CSharp;
using Condor.Generator.Utils.Templating;
using Condor.Files.Generator.Abstractions;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System;

namespace Condor.Constants.Generator
{
    [Generator]
    public class TemplatedFilesGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<KeyedTemplate> additionalFiles = context.GetTemplates();

            IncrementalValuesProvider<IntermediateInfo> visitors = GetFilesInfo(context);
            IncrementalValuesProvider<AddonsIntermediateInfo> addons = GetAddonsFilesInfo(context);
            IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, TemplatedFilesInfo)> combine = CombineData(visitors, addons, additionalFiles);

            context.RegisterSourceOutput(combine, (ctx, data) =>
            {
                ImmutableArray<KeyedTemplate> templates = data.Item1;
                TemplatedFilesInfo template_datas = data.Item2;

                string sourceName = string.Join(".", template_datas.ClassName.SanitizeToHintName(), template_datas.TemplateName, "generated");
                try
                {
                    TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                        .WithTemplates(templates).Build();
                    string template = templates.FirstOrDefault(x => x.Key == template_datas.TemplateName)?.Template;
                    if (!string.IsNullOrEmpty(template))
                    {
                        string result = templateProcessor.Render(template, template_datas);
                        ctx.AddSource(sourceName, result);
                    }
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


        private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, TemplatedFilesInfo)> CombineData(
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
                    return (templates, new TemplatedFilesInfo
                    {
                        ClassName = intermediate.Owner.TypeName,
                        OutputNamespace = intermediate.Owner.ContainingNamespace,
                        TemplateName = intermediate.TemplateName,
                        Type = intermediate.Owner,
                        Files = addons.Where(x => Regex.IsMatch(x.Path, intermediate.Pattern)).Select(x => x.File).ToArray(),
                    });
                });
        }

        internal record struct IntermediateInfo(TargetTypeInfo Owner, string TemplateName, string Pattern);
        internal record struct AddonsIntermediateInfo(string Path, TemplatedFileInfo File);

    }
}
