namespace Condor.Generator.Utils;

public abstract record GeneratedTypeInfo
{
    public string OutputNamespace { get; set; } = default!;
    public string ClassName { get; set; } = default!;

}
