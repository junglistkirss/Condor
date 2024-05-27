using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Condor.Contracts.Generator.Abstractions;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Templating;
using Condor.Generator.Utils.Visitors;

namespace Condor.Contracts.Generator
{

    [Generator]
    public class FindTypesGenerator : IIncrementalGenerator
    {
        static bool symbolPredicate(INamedTypeSymbol t) => t.IsType && t.SpecialType == SpecialType.None;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {


            var byAttributes = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    typeof(FindTypesAttribute<>).FullName,
                    (node, _) => true,
                    (sc, _) => sc.Attributes.Select(x =>
                    {
                        return new TypeFinderInfo
                        {
                            TypeContraint = x.AttributeClass.TypeArguments.Single().Accept(TargetTypeVisitor.Instance),
                            AssemblyContraint = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.AssemblyContraint), out string s) ? s : null,
                            AssemblyName = sc.TargetSymbol.Name,
                            Template = x.ConstructorArguments.Single().Value.ToString(),
                            IsRecord = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsRecord), out bool r) ? r : null,
                            IsGeneric = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsGeneric), out bool g) ? g : null,
                            IsAbstract = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.IsAbstract), out bool a) ? a : null,
                            GroupByHostAssembly = x.TryGetNamedArgument(nameof(FindTypesAttribute<object>.GroupByHostAssembly), out bool b) ? b : false,
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
            var (Info, TypesProvider, Templates) = data;

            TemplateProcessor templateProcessor = new TemplateProcessorBuilder()
                    .WithTemplates(Templates).Build();

            string outputNamespace = Info.AssemblyName;
            TargetTypeInfo[] types = TypesProvider
                .Combined(a => a.Name.StartsWith(Info.AssemblyContraint, StringComparison.OrdinalIgnoreCase),
                    x => x.SpecialType == SpecialType.None
                        && (x.IsType || (!Info.IsRecord.HasValue || (x.IsRecord == Info.IsRecord.Value)))
                        && (!Info.IsAbstract.HasValue || (x.IsAbstract == Info.IsAbstract.Value))
                        && (!Info.IsGeneric.HasValue || (x.IsGenericType == Info.IsGeneric.Value))
                        && (
                            x.AllInterfaces.Any(i => i.Accept(StrongNameVisitor.Instance) == Info.TypeContraint.TypeFullName) || x.Accept(BaseTypesVisitor.Instance).Any(i => i.Accept(StrongNameVisitor.Instance) == Info.TypeContraint.TypeFullName))
                            ).ToArray();

            string template = Templates
                .FirstOrDefault(x => x.Key == Info.Template)
                .Template ?? throw new InvalidDataException($"Missing template {Info.Template}");

            if (Info.GroupByHostAssembly)
            {
                foreach (var group in types.GroupBy(x => x.ContainingAssembly))
                {
                    string className = group.Key.Replace(".", "").Replace(Info.AssemblyContraint, "");
                    OutputTypeInfo template_datas = new()
                    {
                        OutputNamespace = outputNamespace,
                        ClassName = className,
                        BaseType = Info.TypeContraint,
                        Map = [.. group],
                    };
                    var result = templateProcessor.Render(template, template_datas);
                    ctx.AddSource(Info.Template + "-" + className + "_" + Info.TypeContraint.TypeName.SanitizeToHintName() +  ".Generated", result);
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
                var result = templateProcessor.Render(template, template_datas);
                ctx.AddSource(Info.Template + "-" + className + "_" + Info.TypeContraint.TypeName.SanitizeToHintName() +  ".Generated", result);
            }

        }
    }
}
