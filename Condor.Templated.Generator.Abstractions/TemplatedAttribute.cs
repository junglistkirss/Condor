using System;

namespace Condor.Templated.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public class TemplatedAttribute(string templateName) : Attribute
{
    public string TemplateName { get; } = templateName;
}