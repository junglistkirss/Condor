using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Constants.Generator;

[GenerateAccessor]
internal record ConstantInfoCollection
{
    public string OutputNamespace { get; set; }
    public string ClassName { get; set; }

    public string TemplateName { get; internal set; }
    public TargetTypeInfo ConstantType { get; internal set; }
    public ConstantInfo[] Map { get; internal set; }
}
