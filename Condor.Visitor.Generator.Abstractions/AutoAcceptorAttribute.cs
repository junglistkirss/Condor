using System;

namespace Condor.Visitor.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class AutoAcceptorAttribute<T> : Attribute
    {
        public string AssemblyPart { get; set; } = default!;
        public bool AllowAbstract { get; set; } = false;
        public bool AllowGeneric { get; set; } = false;
        public bool AllowRecord { get; set; } = true;
        
        public bool AddVisitRedirect { get; set; } = false;
        public bool AddVisitFallBack { get; set; } = false;

    }

}
