using RobinMustache.Generators.Accessor;
using System.Diagnostics;

namespace Condor.Generator.Utils
{
    [DebuggerDisplay("{AttributeType}")]
    [GenerateAccessor]
    public sealed record class AttributeInfo
    {
        public TargetTypeInfo AttributeType { get; internal set; }
        public ActionInfo Constructor { get; internal set; }
        public ArgumentInfo[] ConstructorArguments { get; internal set; }
        public ArgumentInfo[] NamedArguments { get; internal set; }
    }
}
