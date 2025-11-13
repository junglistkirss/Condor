using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Visitor.Generator;

[GenerateAccessor]
internal record class ImplGroup
{

    public TargetTypeInfo VisitedType { get; internal set; }
    public bool AddVisitFallback { get; internal set; }
    public bool AddVisitRedirect { get; internal set; }
    public TargetTypeInfo[] ImplementationTypes { get; internal set; }
}
