using System;
using System.Diagnostics.CodeAnalysis;

namespace Condor.Visitor.Generator.Abstractions;

[ExcludeFromCodeCoverage]
public abstract class BaseParamAttribute<TParamType> : Attribute
{
    public string? ParamName { get; set; } = null;
}
