using Condor.Generator.Utils;

namespace Condor.With.Generator
{
    internal struct WithInfo
    {
        public TargetTypeInfo Owner { get; set; }
        public MemberInfo[] Properties { get; set; }
        public string Accessibility { get; internal set; }
    }
}
