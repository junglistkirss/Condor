using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Constants.Generator;

[GenerateAccessor]
internal record ConstantInfo
{
    public MemberInfo Member { get; internal set; } = default!;
    public string[] Partials { get; internal set; } = [];
}
