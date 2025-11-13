using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Contracts.Generator;

[GenerateAccessor]
internal record class OutputTypeInfo
{
    public string OutputNamespace { get; internal set; }
    public string ClassName { get; internal set; }
    public TargetTypeInfo BaseType { get; internal set; }
    public TargetTypeInfo[] Map { get; internal set; }
}
