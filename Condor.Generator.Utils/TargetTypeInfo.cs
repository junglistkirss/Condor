using RobinMustache.Generators.Accessor;
using System.Diagnostics;

namespace Condor.Generator.Utils;

[DebuggerDisplay("{TypeFullName}")]
[GenerateAccessor]
public sealed record class TargetTypeInfo
{
    public string ContainingAssembly { get; internal set; } = default!;
    public string ContainingNamespace { get; internal set; } = default!;
    public string SanitizeTypeNameAsArg => SanitizeTypeName.ToLower();
    public string SanitizeTypeName => TypeName.SanitizeBaseOrInterfaceName();

    public string GenericBaseTypeName { get; internal set; } = default!;
    public string TypeName { get; internal set; } = default!;
    public string TypeFullName { get; internal set; } = default!;
    public string TypeDefinition { get; internal set; } = default!;
    public TargetTypeInfo[] Interfaces { get; internal set; } = [];
    public TargetTypeInfo[] AllInterfaces { get; internal set; } = [];

    public bool IsNullable { get; internal set; }
    public bool IsArray { get; internal set; }
    public TargetTypeInfo? ElementType { get; internal set; }
    public bool IsGeneric { get; internal set; }
    public bool IsAbstract { get; internal set; }
    public bool IsRecord { get; internal set; }
    public TypeArgumentInfo[] GenericTypes { get; internal set; } = [];
}
