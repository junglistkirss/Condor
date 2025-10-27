using Condor.Generator.Utils;

namespace Condor.Visitor.Generator;

internal record class ImplGroup
{

    public TargetTypeInfo VisitedType { get; internal set; } = default!;
    public bool AddVisitFallBack { get; internal set; }
    public bool AddVisitRedirect { get; internal set; }
    public TargetTypeInfo[] ImplementationTypes { get; internal set; } = [];
}
