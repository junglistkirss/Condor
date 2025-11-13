using Condor.Generator.Utils;

namespace Condor.Contracts.Generator;

internal record class OutputTypeInfo 
{
    public string OutputNamespace { get; set; }
    public string ClassName { get; set; }
    public TargetTypeInfo BaseType { get; set; }
    public TargetTypeInfo[] Map { get; set; }
}
