using Condor.Generator.Utils;

namespace Condor.Constants.Generator
{
    internal record ConstantsInfo : GeneratedTypeInfo
    {
        public string TemplateName { get; internal set; } = default!;
        public TargetTypeInfo ConstantType { get; internal set; } = default!;
        public ConstantInfo[] Map { get; internal set; } = [];
    }
}
