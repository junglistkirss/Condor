using HandlebarsDotNet;

namespace Condor.Generator.Utils.Templating;


public class TemplateProcessor
{
    private readonly IHandlebars renderer;

    internal TemplateProcessor(IHandlebars renderer)
    {
        this.renderer = renderer;
    }
    public string Render<T>(string template, T datas)
    {
        return renderer.Compile(template)(datas);

    }
}
