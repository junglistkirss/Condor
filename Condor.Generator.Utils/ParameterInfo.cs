using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{Name}")]
    public record class ParameterInfo
    {
        public string ParameterName { get; internal set; }
        public object DefaultExpression { get; internal set; }
        public TargetTypeInfo ParameterType { get; internal set; }
        public bool HasDefaultExpression { get; internal set; }
        public bool IsOptional { get; internal set; }
        public bool IsParams { get; internal set; }
        public bool IsExtension { get; internal set; }
        public bool IsRefReadOnly { get; internal set; }
        public bool IsIn { get; internal set; }
        public bool IsOut { get; internal set; }
        public bool IsRef { get; internal set; }
        public AttributeInfo[] Attributes { get; internal set; }
    }
}
