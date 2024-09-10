namespace Condor.Visitor.Generator
{
    internal static class DefaultTemplates
    {
        internal const string VisitorTemplate = @"// Visitor type for {{{VisitedType.TypeFullName}}}


{{#*inline ""Response""}}
{{~#if IsAsync}}{{~#if HasReturnType}}ValueTask<{{{ReturnType}}}>{{~else~}}ValueTask{{/if~}}{{else}}{{~#if HasReturnType}}{{{ReturnType}}}{{~else~}}void{{/if~}}{{/if~}}
{{/inline}}
{{#*inline ""ResponseNested""}}
{{~#if ../IsAsync}}{{~#if ../HasReturnType}}ValueTask<{{{../ReturnType}}}>{{~else~}}ValueTask{{/if~}}{{else}}{{~#if ../HasReturnType}}{{{../ReturnType}}}{{~else~}}void{{/if~}}{{/if~}}
{{/inline}}
{{#*inline ""VisitOptions""}}
    {{#each ImplementationGroup}}
        {{#if AddVisitFallBack}}
        public virtual {{>Response}} VisitFallBack(
            {{{VisitedType.TypeFullName}}} element{{#each TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each Args}}, 
            {{{.}}}{{/each}})
        {
            {{#if Default.ThrowOnFallBack}}
            throw new System.NotImplementedException();
            {{else}}
            {{#if IsAsync}}
            {{#if HasReturnType}}
            return ValueTask.FromResult<{{{ReturnType}}}>(default!);
            {{else}}
            return ValueTask.CompletedValueTask;
            {{/if}}
            {{else}}
            {{#if HasReturnType}}
            return default!;
            {{else}}
            return;
            {{/if}}
            {{/if}}
            {{/if}}
        }
        {{/if}}

        {{#if AddVisitRedirect}}
        public virtual {{>Response}} VisitRedirect(
            {{{VisitedType.TypeFullName}}} element{{#each TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each Args}}, 
            {{{.}}}{{/each}})
        {
            switch (element)
            {
                {{#each ImplementationTypes}}
                case {{{TypeFullName}}} x:
                    {{#if ../HasReturnType}} 
                    return Visit(x{{#each ../TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ../ArgsName}}, {{{.}}}{{/each}});
                    {{else}}
                    {{#if ../IsAsync}}
                    return Visit(x{{#each ../TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ../ArgsName}}, {{{.}}}{{/each}});
                    {{else}}
                    Visit(x{{#each ../TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ../ArgsName}}, {{{.}}}{{/each}});
                    break;
                    {{/if}}
                    {{/if}}
                {{/each}}
                default:
                    {{#if HasReturnType}}   
                    {{#if Default.UseVisitFallBack}}
                    return VisitFallBack(element{{#each TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ArgsName}}, {{{.}}}{{/each}});
                    {{else}}
                    {{#if IsAsync}}
                    return ValueTask.FromResult<{{{ReturnType}}}>(default!);
                    {{else}}
                    return default!;
                    {{/if}}
                    {{/if}}
                    {{else}}
                    break;
                    {{/if}}
            }
        }
        {{/if}}
        {{#each ImplementationTypes}}
        public partial {{>ResponseNested}} Visit({{{TypeFullName}}} element{{#each ../TypedArgs}}, {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each ../Args}}, {{{.}}}{{/each}});
        {{/each}}
    {{/each}}
{{/inline}}


namespace {{OutputNamespace}}
{
    {{#if Visitable.GenerateVisitable}}
    {{AccessibilityModifier}} partial interface {{{Visitable.VisitableTypeName}}} {
        {{>Response}} Accept{{{GenericTypesDefinition}}}(
            {{BaseTypeDefinition}}{{{GenericTypesDefinition}}} visitor{{#each Visitable.VisitableParameters}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each Args}}, 
            {{{.}}}{{/each}});
    }
    {{/if}}

    {{AccessibilityModifier}} partial {{{KeywordTypeDefinition}}} {{{OriginalTypeDefinition}}}
    {
        {{#if IsInterface}}
        {{#each ImplementationGroup}}
        {{#if AddVisitFallBack}}
        {{>Response}} VisitFallBack(
            {{{VisitedType.TypeFullName}}} element{{#each TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each Args}}, 
            {{{.}}}{{/each}});
        {{/if}}
        {{#if AddVisitRedirect}}
        {{>Response}} VisitRedirect(
            {{{VisitedType.TypeFullName}}} element{{#each TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each Args}}, 
            {{{.}}}{{/each}});
        {{/if}}
        {{#each ImplementationTypes}}
        {{>ResponseNested}} Visit({{{TypeFullName}}} element{{#each ../TypedArgs}}, {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each ../Args}}, {{{.}}}{{/each}});
        {{/each}}
        {{/each}}
        {{else}}
        {{>VisitOptions}}
        {{/if}}
    }

    {{#if Default.GenerateDefault}}
    {{#if Default.ForcePublic}}public {{else}}{{AccessibilityModifier}} {{/if}}{{#if Default.IsAbstract}}abstract {{/if}}{{#if Default.IsPartial}}partial {{/if}}class Default{{Default.DefaultTypeName}}{{{GenericTypesDefinition}}} : {{BaseTypeDefinition}}{{{GenericTypesDefinition}}}
    {
        {{>VisitOptions}}
        {{#each ImplementationTypes}}
        {{#if ../Default.IsVisitAbstract}}
        public abstract {{>ResponseNested}} Visit(
            {{{TypeFullName}}} element{{#each ../TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each ../Args}}, 
            {{{.}}}{{/each}});
        {{else}}
        public virtual {{>ResponseNested}} Visit(
            {{{TypeFullName}}} element{{#each ../TypedArgs}}, 
            {{{ParamType.TypeFullName}}} {{SanitizedParamName}}{{/each}}{{#each ../Args}}, 
            {{{.}}}{{/each}})
        {
            {{#if ../HasReturnType}}   
            {{#if ../Default.UseVisitFallBack}}
            return VisitFallBack(element{{#each ../TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ../ArgsName}}, {{{.}}}{{/each}});
            {{else}}
            return default!;
            {{/if}}
            {{else}}
            {{#if ../Default.UseVisitFallBack}}
            VisitFallBack(element{{#each ../TypedArgs}}, {{SanitizedParamName}}{{/each}}{{#each ../ArgsName}}, {{{.}}}{{/each}});
            {{else}}
            return;
            {{/if}}
            {{/if}}
        }
        {{/if}}
        {{/each}}
    }
    {{/if}}
}";
    }
}
