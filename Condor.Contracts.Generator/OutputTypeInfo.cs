using Condor.Generator.Utils;

namespace Condor.Contracts.Generator;

internal record class OutputTypeInfo : GeneratedTypeInfo
{
    public TargetTypeInfo BaseType { get; internal set; } = default!;
    public TargetTypeInfo[] Map { get; internal set; } = [];
}
