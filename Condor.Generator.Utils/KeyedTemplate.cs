using Microsoft.CodeAnalysis.CSharp;

namespace Condor.Generator.Utils;

public record KeyedTemplate
{
    public string Key { get; set; }
    public string Template { get; set; }

}
