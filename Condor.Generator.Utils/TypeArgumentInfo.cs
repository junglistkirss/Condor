using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{Name}")]
    public record class TypeArgumentInfo
    {
        public bool IsVarianceUnspecified { get; internal set; }
        public bool IsIn { get; internal set; }
        public bool IsOut { get; internal set; }
        public string Name { get; internal set; }
        public bool HasConstraint { get; internal set; }
        public TargetTypeInfo[] Contraints { get; internal set; }
        public bool IsNullable { get; internal set; }
    }
}
