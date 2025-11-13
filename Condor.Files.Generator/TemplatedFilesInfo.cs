using Condor.Generator.Utils;

namespace Condor.Constants.Generator;

internal record TemplatedFilesInfo 
{
    public string OutputNamespace { get; set; }
    public string ClassName { get; set; }
    public string TemplateName { get; internal set; }
    public TemplatedFileInfo[] Files { get; internal set; }
    public TargetTypeInfo Type { get; internal set; }
}
