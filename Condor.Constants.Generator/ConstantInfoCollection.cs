using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Constants.Generator;

[GenerateAccessor]
internal record ConstantInfoCollection
{
    public string OutputNamespace { get; internal set; } = default!;
    public string ClassName { get; internal set; } = default!;

    public string TemplateName { get; internal set; } = default!;
    public TargetTypeInfo ConstantType { get; internal set; } = default!;
    public ConstantInfo[] Map { get; internal set; } = [];
}
