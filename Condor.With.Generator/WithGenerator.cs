using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Condor.Generator.Utils.Visitors;
using Microsoft.CodeAnalysis.CSharp;
using Condor.Generator.Utils;
using Condor.Generator.Utils.Templating;
using Condor.With.Generator.Abstractions;

namespace Condor.With.Generator
{

    [Generator]
    public class WithGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var with = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    typeof(GenerateWithAttribute).FullName,
                    (node, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return node is RecordDeclarationSyntax r && r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) && r.Modifiers.Any(SyntaxKind.PartialKeyword);
                    },
                    (sc, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new WithInfo
                        {
                            Accessibility = sc.TargetSymbol.DeclaredAccessibility.GetAccessibilityKeyWord(),
                            Owner = sc.TargetSymbol.Accept(TargetTypeVisitor.Instance),
                            Properties = sc.TargetSymbol.Accept(MembersVisitor<IPropertySymbol>.Instance)
                                                    .Where(x => x.Attributes.Any(x => x.AttributeType.TypeFullName == typeof(WithAssignAttribute).FullName)).ToArray()
                        };
                    });

            context.RegisterSourceOutput(with, Execute);
        }
        private void Execute(SourceProductionContext ctx, WithInfo info)
        {
            TemplateProcessor templateProcessor = new TemplateProcessorBuilder().Build();


            string template = @"using System.Diagnostics.Contracts;
namespace {{Owner.ContainingNamespace}}
{
    {{Accessibility}} partial record class {{Owner.TypeName}}
    {
    {{#each Properties}}
        [Pure]
        public {{../Owner.TypeName}} With{{MemberName}}({{{MemberType.TypeFullName}}} input)
        {
            return this with
            {
                {{MemberName}} = input,
            };
        }
        
    {{/each}}
    }
}
";
            string className = info.Owner.TypeFullName.Replace(".", "");
            var result = templateProcessor.Render(template, info);//, new RendererSettings())
            ctx.AddSource(className + ".With.Generated", result);

        }
    }
}
