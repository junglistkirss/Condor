using Condor.Contracts.Generator.Abstractions;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Templating;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Condor.Contracts.Generator;


[Generator]
public class FindTypesGenerator : IIncrementalGenerator
{
    //static bool symbolPredicate(INamedTypeSymbol t) => t.IsType && t.SpecialType == SpecialType.None;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {


        IncrementalValuesProvider<(TypeFinderInfo Info, TypesProvider TypesProvider, ImmutableArray<KeyedTemplate> Templates)> byAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(FindTypesAttribute<>).FullName,
                (node, _) => true,
                (sc, _) => sc.Attributes.Select(x =>
                {
                    return new TypeFinderInfo
                    {
                        TypeContraint = x.RequireAttributeClass().TypeArguments.Single().RequireTargetTypeInfo(),
                        AssemblyContraint = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.AssemblyContraint), out string? s) ? s : null,
                        AssemblyName = sc.TargetSymbol.Name,
                        TemplateKey = x.ConstructorArguments.Single().Value?.ToString() ?? throw new Exception("Template key is required"),
                        IsRecord = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsRecord), out bool r) ? r : null,
                        IsGeneric = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsGeneric), out bool g) ? g : null,
                        IsAbstract = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsAbstract), out bool a) ? a : null,
                        GroupByHostAssembly = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.GroupByHostAssembly), out bool b) && b,
                    };
                }).ToArray()).SelectMany((x, _) => x)
            .Combine(context.GetTypesProvider())
            .Combine(context.GetTemplates().Collect())
            .Select((dat, _) =>
            {
                return (
                    Info: dat.Left.Left,
                    TypesProvider: dat.Left.Right,
                    Templates: dat.Right
                );
            });

        context.RegisterSourceOutput(byAttributes, Execute);
    }
    private void Execute(SourceProductionContext ctx, (TypeFinderInfo Info, TypesProvider TypesProvider, ImmutableArray<KeyedTemplate> Templates) data)
    {
        (TypeFinderInfo Info, TypesProvider TypesProvider, ImmutableArray<KeyedTemplate> Templates) = data;

        TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
            .WithAccessors(x => x
                .AddDefaultsAccessors()
                .CreateMemberObjectAccessor<OutputTypeInfo>(OutputTypeInfoAccessor.GetNamedProperty)
            )
            .WithTemplates(Templates).Build();

        string outputNamespace = Info.AssemblyName;
        TargetTypeInfo[] types = [.. TypesProvider
            .Combined(a => a.Name.StartsWith(Info.AssemblyContraint, StringComparison.OrdinalIgnoreCase),
                x => x.SpecialType == SpecialType.None
                    && (x.IsType || (!Info.IsRecord.HasValue || (x.IsRecord == Info.IsRecord.Value)))
                    && (!Info.IsAbstract.HasValue || (x.IsAbstract == Info.IsAbstract.Value))
                    && (!Info.IsGeneric.HasValue || (x.IsGenericType == Info.IsGeneric.Value))
                    && (
                        x.AllInterfaces.Any(i => i.GetStrongName() == Info.TypeContraint.TypeFullName) || x.GetBaseTypes().Any(i => i.GetStrongName() == Info.TypeContraint.TypeFullName))
                        )];

        if (Info.GroupByHostAssembly)
        {
            foreach (IGrouping<string, TargetTypeInfo> group in types.GroupBy(x => x.ContainingAssembly))
            {
                string className = group.Key.Replace(".", "").Replace(Info.AssemblyContraint, "");
                OutputTypeInfo template_datas = new()
                {
                    OutputNamespace = outputNamespace,
                    ClassName = className,
                    BaseType = Info.TypeContraint,
                    Map = [.. group],
                };
                string result = templateProcessor.Render(Info.TemplateKey, template_datas);
                ctx.AddSource(Info.TemplateKey + "-" + className + "_" + Info.TypeContraint.TypeName.SanitizeToHintName() + ".Generated", result);
            }
        }
        else
        {
            string className = Info.TypeContraint.TypeName.Replace(".", "").Replace(Info.AssemblyContraint, "");
            OutputTypeInfo template_datas = new()
            {
                OutputNamespace = outputNamespace,
                ClassName = className,
                BaseType = Info.TypeContraint,
                Map = types,
            };
            string result = templateProcessor.Render(Info.TemplateKey, template_datas);
            ctx.AddSource(Info.TemplateKey + "-" + className + "_" + Info.TypeContraint.TypeName.SanitizeToHintName() + ".Generated", result);
        }

    }
}
