using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Files.Generator;

[GenerateAccessor]
internal record class TemplatedFileInfoCollection 
{
    public string OutputNamespace { get; internal set; }
    public string ClassName { get; internal set; }
    public string TemplateName { get; internal set; }
    public TemplatedFileInfo[] Files { get; internal set; }
    public TargetTypeInfo Type { get; internal set; }
}
