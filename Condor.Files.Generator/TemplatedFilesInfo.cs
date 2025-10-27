using Condor.Generator.Utils;

namespace Condor.Constants.Generator;

internal record TemplatedFilesInfo : GeneratedTypeInfo
{
    public string TemplateName { get; internal set; }
    public TemplatedFileInfo[] Files { get; internal set; }
    public TargetTypeInfo Type { get; internal set; }
}
