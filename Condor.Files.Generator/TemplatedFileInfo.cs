using RobinMustache.Generators.Accessor;

namespace Condor.Constants.Generator;

[GenerateAccessor]
internal record TemplatedFileInfo
{
    public string FileName { get; internal set; }
    public string FileContent { get; internal set; }
}
