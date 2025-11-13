using RobinMustache.Generators.Accessor;

namespace Condor.Visitor.Generator;

[GenerateAccessor]
internal record class OutputVisitableInfo
{
    public string VisitableTypeName { get; internal set; }
    public bool GenerateVisitable { get; internal set; }
    public string AcceptMethodName { get; internal set; }
    public NamedParamInfo[] VisitableParameters { get; internal set; }
}
