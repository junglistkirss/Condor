using System;

namespace Condor.Visitor.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class VisitParamAttribute<TArg> : BaseParamAttribute<TArg> { }

}
