using RobinMustache;
using RobinMustache.Abstractions.Helpers;
using System;
using System.Collections.Generic;

namespace Condor.Generator.Utils.Templating;


public class TemplateProcessorBuilder
{
    private Action<StaticDataFacadeResolverBuilder> facades = null;
    private Action<StaticAccessorBuilder> accessors = null;
    private Action<Helper> helpers = null;

    private readonly List<KeyedTemplate> templates = [];
    public TemplateProcessorBuilder WithFacades(Action<StaticDataFacadeResolverBuilder> facades)
    {
        this.facades = facades;
        return this;
    }
    public TemplateProcessorBuilder WithAccessors(Action<StaticAccessorBuilder> accessors)
    {
        this.accessors = accessors;
        return this;
    }
    public TemplateProcessorBuilder WithHelpers(Action<Helper> helpers)
    {
        this.helpers = helpers;
        return this;
    }
    public TemplateProcessorBuilder WithTemplates(IEnumerable<KeyedTemplate> templates)
    {
        this.templates.AddRange(templates);
        return this;
    }
    public TemplateProcessor Build()
    {
        IStringRenderer renderer = Renderer.CreateStringRenderer(facades, accessors, helpers);

        TemplateProcessor processor = new(renderer);
        if (templates is not null)
            foreach (KeyedTemplate tmpl in templates)
            {
                processor.RegisterTemplate(tmpl.Key, tmpl.Template);
            }
        return processor;
    }


}
