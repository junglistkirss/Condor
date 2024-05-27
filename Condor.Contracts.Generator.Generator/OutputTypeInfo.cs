using Condor.Generator.Utils;

namespace Condor.Contracts.Generator
{
    internal record class OutputTypeInfo : GeneratedTypeInfo
    {
        public TargetTypeInfo BaseType { get; set; }
        public TargetTypeInfo[] Map { get; set; }
    }
}
