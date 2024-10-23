using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{MemberName}")]
    public record class MemberInfo
    {
        public string MemberName { get; internal set; }
        public bool IsConstant{ get; internal set; }
        public bool IsNullable { get; internal set; }
        public TargetTypeInfo MemberType { get; internal set; }
        public AttributeInfo[] Attributes { get; internal set; }

    }
}
