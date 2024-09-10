using System;

namespace Condor.Visitor.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class AutoAcceptorAttribute<T> : Attribute
    {
        public string AssemblyPattern { get; set; } = default!;
        public string TypePattern { get; set; } = default!;
        public AcceptedKind Accept { get; set; } = AcceptedKind.Class;
        public bool AcceptRequireAll { get; set; } = false;

        public bool AddVisitRedirect { get; set; } = false;
        public bool AddVisitFallBack { get; set; } = false;

    }

    [Flags]
    public enum AcceptedKind : byte
    {
        None = 0,
        Class = 1,
        Interface = 2,
        Struct = 4,
        Record = 8,
        Generic = 16,
        Abstract = 32,
        Sealed = 64,
    }

}
