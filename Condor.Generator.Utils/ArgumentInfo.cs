using RobinMustache.Generators.Accessor;
using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{ArgumentName} =({ArgumentType}){ArgumentValue}")]
    [GenerateAccessor]
    public sealed record class ArgumentInfo
    {
        public object ArgumentValue { get; internal set; }
        public bool IsNull { get; internal set; }
        public TargetTypeInfo ArgumentType { get; internal set; }
        public string ArgumentName { get; internal set; }
    }
}
