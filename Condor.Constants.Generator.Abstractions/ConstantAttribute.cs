using System;
using System.Diagnostics.CodeAnalysis;

namespace Condor.Constants.Generator.Abstractions;


[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
public class ConstantAttribute(string partialTemplate) : Attribute
{
    public string PartialTemplate { get; } = partialTemplate;
}
