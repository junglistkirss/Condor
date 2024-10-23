using Condor.Generator.Utils;

namespace Condor.Constants.Generator
{
    internal record ConstantsInfo : GeneratedTypeInfo
    {
        public string TemplateName { get; internal set; }
        public TargetTypeInfo ConstantType { get; internal set; }
        public ConstantInfo[] Map { get; internal set; }
    }
}
