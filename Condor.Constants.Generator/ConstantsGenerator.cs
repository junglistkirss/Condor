using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis.CSharp;
using Condor.Generator.Utils.Templating;
using Condor.Constants.Generator.Abstractions;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Condor.Constants.Generator
{
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
        private static IncrementalValuesProvider<ConstsOwnerInfo> GetConstantsInfo(IncrementalGeneratorInitializationContext context)
        {
            return context.SyntaxProvider
               .ForAttributeWithMetadataName<IEnumerable<ConstsOwnerInfo>>(
                   typeof(ConstantsAttribute).FullName,
                   (node, _) => node is TypeDeclarationSyntax i && i.Modifiers.Any(SyntaxKind.PartialKeyword),
                   (sc, cancellationToken) =>
                   {
                       cancellationToken.ThrowIfCancellationRequested();
                       var attr = sc.Attributes.Single();
                       List<ConstsOwnerInfo> consts = [];
                       foreach (var item in sc.Attributes)
                       {
                           var members = sc.TargetSymbol
                                .Accept(MembersVisitor<IFieldSymbol>.Instance)
                                .Where(x => x.IsConstant)
                                .Select(x =>
                                {
                                    string[] partials = x.Attributes.Where(a => a.AttributeType.TypeFullName == typeof(ConstantAttribute).FullName).Select(x => x.NamedArguments[0].ArgumentValue?.ToString()).ToArray();
                                    return new ConstInfo(x, partials ?? []);
                                }).ToArray();

                           consts.Add(new ConstsOwnerInfo(
                               sc.TargetSymbol.Accept(StrongNameVisitor.Instance),
                               sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                               attr.TryGetNamedArgument(nameof(ConstantsAttribute.Template), out string w) ? w : string.Empty,
                               members
                            ));
                       }
                       return consts;
                   }).SelectMany((x,_)=> x);
        }


        private static IncrementalValuesProvider<(ImmutableArray<KeyedTemplate>, ConstantsInfo)> CombineData(
                IncrementalValuesProvider<ConstsOwnerInfo> consts,
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
                    ((((((((ConstsOwnerInfo Visitor, ImmutableArray<OutputInfo> Output) Left, ImmutableArray<GenerateDefaultInfo> GenerateDefault) Left, ImmutableArray<VisitableInfo> Visitable) Left, ImmutableArray<AcceptorInfo> Acceptors) Left, ImmutableArray<AcceptorInfo> AutoAcceptors) Left, ImmutableArray<AcceptParamInfo> AcceptParam) Left, ImmutableArray<VisitParamInfo> VisitParam) Left, ImmutableArray<KeyedTemplate> Templates) = data;

                    ConstsOwnerInfo Visitor = Left.Left.Left.Left.Left.Left.Left.Visitor;
                    OutputInfo Output = Left.Left.Left.Left.Left.Left.Left.Output.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    GenerateDefaultInfo GenerateDefault = Left.Left.Left.Left.Left.Left.GenerateDefault.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
                    VisitableInfo Visitable = Left.Left.Left.Left.Left.Visitable.FirstOrDefault(x => x.Correlation == Visitor.Correlation);
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
                    List<NamedParamInfo> typedArgs = [];
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
                    List<NamedParamInfo> accept = [];
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
                        VisitMethodName = string.IsNullOrWhiteSpace(Visitor.VisitMethodName) ? DefaultVisitMethodName : Visitor.VisitMethodName.Trim(),
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
                            AcceptMethodName = string.IsNullOrWhiteSpace(Visitable.AcceptMethodName) ? DefaultAcceptMethodName : Visitable.AcceptMethodName.Trim(),
                        }
                    });
                });
        }
    }

    internal record struct ConstsOwnerInfo(string Correlation, TargetTypeInfo Owner, string Template, ConstInfo[] Consts) { }
    internal record struct ConstInfo(MemberInfo Member, string[] Partials) { }
    { }
}
