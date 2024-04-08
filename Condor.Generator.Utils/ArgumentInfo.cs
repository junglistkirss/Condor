using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{ArgumentName} =({ArgumentType}){ArgumentValue}")]
    public record class ArgumentInfo
    {
        public object ArgumentValue { get; internal set; }
        public bool IsNull { get; internal set; }
        public TargetTypeInfo ArgumentType { get; internal set; }
        public string ArgumentName { get; internal set; }
    }
}
