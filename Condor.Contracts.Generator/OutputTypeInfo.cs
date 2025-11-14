using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Contracts.Generator;

[GenerateAccessor]
internal record class OutputTypeInfo
{
    public string OutputNamespace { get; internal set; } = default!;
    public string ClassName { get; internal set; } = default!;
    public TargetTypeInfo BaseType { get; internal set; } = default!;
    public TargetTypeInfo[] Map { get; internal set; } = [];
}
