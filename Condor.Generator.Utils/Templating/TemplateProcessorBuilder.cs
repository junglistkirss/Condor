using HandlebarsDotNet;
using HandlebarsDotNet.Helpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Condor.Generator.Utils.Templating
{

    public class TemplateProcessorBuilder
    {
        private Action<HandlebarsConfiguration> configure = null;
        private Action<IHandlebars> extend = null;

        private KeyedTemplate[] templates;
        public TemplateProcessorBuilder WithConfigure(Action<HandlebarsConfiguration> configure)
        {
            this.configure = configure;
            return this;
        }
        public TemplateProcessorBuilder WithExtend(Action<IHandlebars> extend)
        {
            this.extend = extend;
            return this;
        }
        public TemplateProcessorBuilder WithTemplates(IEnumerable<KeyedTemplate> templates)
        {
            this.templates = templates.ToArray();
            return this;
        }
        public TemplateProcessor Build()
        {
            HandlebarsConfiguration conf = new HandlebarsConfiguration()
            .Configure(builder =>
            {
                configure?.Invoke(builder);
            });
            IHandlebars processor = Handlebars.Create(conf);

            extend?.Invoke(processor);
            processor.RegisterHelper("SanitizeBaseOrInterfaceName", (ctx, args) =>
            {
                string obj = args.First().ToString();
                return obj.SanitizeBaseOrInterfaceName();

            });

            if (templates is not null)
                foreach (KeyedTemplate tmpl in templates)
                {
                    processor.RegisterTemplate(tmpl.Key, tmpl.Template);
                }
            //HandlebarsHelpers.Register(processor, o =>
            //{

            //    o.Categories = [
            //        //Category.Url,
            //        //Category.Enumerable,
            //        Category.DynamicLinq, 
            //        //Category.DateTime, 
            //        //Category.Math, 
            //        //Category.Boolean, 
            //        //Category.Random, 
            //    ];
            //    o.UseCategoryPrefix = true;

            //} );

            return new TemplateProcessor(processor);
        }


    }

    internal class FilterHelper : IHelpers
    {

    }

}
