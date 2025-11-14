using System;
using System.Diagnostics.CodeAnalysis;

namespace Condor.Constants.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
public sealed class ConstantsAttribute(string template) : Attribute
{
    public string Template { get; } = template;

}
