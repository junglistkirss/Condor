using Condor.Generator.Utils;

namespace Condor.Constants.Generator;

internal record ConstantsInfo
{
    public string OutputNamespace { get; set; }
    public string ClassName { get; set; }

    public string TemplateName { get; internal set; }
    public TargetTypeInfo ConstantType { get; internal set; }
    public ConstantInfo[] Map { get; internal set; }
}
