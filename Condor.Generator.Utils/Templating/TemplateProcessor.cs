using RobinMustache;
using RobinMustache.Abstractions.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Condor.Generator.Utils.Templating
{

    public class TemplateProcessor
    {
        private readonly IStringRenderer renderer;
        private readonly Dictionary<string, ImmutableArray<INode>> templates = [];

        internal TemplateProcessor(IStringRenderer renderer)
        {
            this.renderer = renderer;
        }
        public string Render<T>(string templateKey, T datas)
        {
            if (templates.TryGetValue(templateKey, out ImmutableArray<INode> template))
                throw new InvalidOperationException($"Template {templateKey} is missing");
            return renderer.Render(template, datas);

        }
        internal void RegisterTemplate(string key, string template)
        {
            ImmutableArray<INode> compiledTemplate = template.AsSpan().Parse();
            templates.Add(key, compiledTemplate);
        }
    }
}
