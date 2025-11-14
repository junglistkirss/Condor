using RobinMustache.Generators.Accessor;

namespace Condor.Visitor.Generator;

[GenerateAccessor]
internal record class NamedParamInfo
{
    public string ParamTypeFullName { get; internal set; } = default!;
    public string SanitizedParamName { get; internal set; } = default!;
}
