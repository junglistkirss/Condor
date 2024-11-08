using HandlebarsDotNet;
using HandlebarsDotNet.Helpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            processor.RegisterHelper("Strip", (ctx, args) =>
            {
                string obj = args.First().ToString();
                string replace = args.Last().ToString();
                return obj.Replace(replace, "");
            });
            processor.RegisterHelper("Replace", (ctx, args) =>
            {
                string obj = args.First().ToString();
                if (args.Length == 3)
                {
                    string replace = args[1].ToString();
                    string replacement = args[2].ToString();
                    return obj.Replace(replace, replacement);
                }
                return obj;
            });
            processor.RegisterHelper("Trim", (ctx, args) =>
            {
                string obj = args.First().ToString();
                string _char = args.Last().ToString();
                return obj.Trim(_char.ToCharArray());
            });
            processor.RegisterHelper("TrimStart", (ctx, args) =>
            {
                string obj = args.First().ToString();
                string _char = args.Last().ToString();
                return obj.TrimStart(_char.ToCharArray());
            });
            processor.RegisterHelper("TrimEnd", (ctx, args) =>
            {
                string obj = args.First().ToString();
                string _char = args.Last().ToString();
                return obj.TrimEnd(_char.ToCharArray());
            });
            processor.RegisterHelper("Uppercase", (ctx, args) =>
            {
                string obj = args.First().ToString();
                return obj.ToUpper();
            });
            processor.RegisterHelper("Lowercase", (ctx, args) =>
            {
                string obj = args.First().ToString();
                return obj.ToLower();
            });
            processor.RegisterHelper("Capitalize", (ctx, args) =>
            {
                string obj = args.First().ToString();
                return obj.ToUpper();
            });
            processor.RegisterHelper("Substring", (ctx, args) =>
            {
                string obj = args.First().ToString();
                if (args.Length == 2)
                {
                    int start = (int)args[1];
                    return obj.Substring(start);
                }
                else if (args.Length == 3)
                {
                    int start = (int)args[1];
                    int end = (int)args[2];
                    return obj.Substring(start, end);
                }
                else
                    return obj;
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
