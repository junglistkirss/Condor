using System;

namespace Condor.Visitor.Generator.Abstractions;

public abstract class BaseParamAttribute<TParamType> : Attribute
{
    public string ParamName { get; set; } = null;
}
