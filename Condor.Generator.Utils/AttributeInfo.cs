using System.Diagnostics;

namespace Condor.Generator.Utils;

[DebuggerDisplay("{AttributeType}")]
public record class AttributeInfo
{
    public TargetTypeInfo AttributeType { get; internal set; } = default!;
    public ActionInfo? Constructor { get; internal set; }
    public ArgumentInfo[] ConstructorArguments { get; internal set; } = [];
    public ArgumentInfo[] NamedArguments { get; internal set; } = [];
}
