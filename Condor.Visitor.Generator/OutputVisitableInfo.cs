using RobinMustache.Generators.Accessor;

namespace Condor.Visitor.Generator;

[GenerateAccessor]
internal record class OutputVisitableInfo
{
    public string VisitableTypeName { get; internal set; } = default!;
    public string AcceptMethodName { get; internal set; } = default!;
    public NamedParamInfo[] VisitableParameters { get; internal set; } = [];
}
