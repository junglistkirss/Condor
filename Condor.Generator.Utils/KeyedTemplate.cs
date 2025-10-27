namespace Condor.Generator.Utils;

public record KeyedTemplate
{
    public string Key { get; set; } = default!;
    public string Template { get; set; } = default!;

}
