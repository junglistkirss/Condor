using System.Diagnostics;

namespace Condor.Generator.Utils;

[DebuggerDisplay("{Definition}")]
public record class ActionInfo
{
    public bool IsStatic { get; internal set; }
    public string Name { get; internal set; }
    public string Definition { get; internal set; }
    public TargetTypeInfo ReturnType { get; internal set; }
    public TargetTypeInfo[] TypeArguments { get; internal set; }
    public ParameterInfo[] Parameters { get; internal set; }
    public AttributeInfo[] Attributes { get; internal set; }
    public string AccessibilityModifier { get; internal set; }
    public bool IsVoid { get; internal set; }
}
