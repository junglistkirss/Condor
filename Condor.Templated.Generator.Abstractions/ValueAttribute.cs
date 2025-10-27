using System;

namespace Condor.Templated.Generator.Abstractions;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class ValueAttribute(string key, string value) : Attribute
{
    public string Key { get; } = key;
    public string Value { get; } = value;
}