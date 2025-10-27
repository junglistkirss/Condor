namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class AcceptParamAttribute<TParamType> : BaseParamAttribute<TParamType> { }
