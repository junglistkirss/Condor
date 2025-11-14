using RobinMustache.Generators.Accessor;

namespace Condor.Files.Generator;

[GenerateAccessor]
internal record TemplatedFileInfo
{
    public string FileName { get; internal set; } = default!;
    public string FileContent { get; internal set; } = default!;
}
