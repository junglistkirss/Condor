using System;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class VisitorAttribute : Attribute
{
    public bool IsAsync { get; set; } = false;
    public string? VisitMethodName { get; set; } = null;
}
