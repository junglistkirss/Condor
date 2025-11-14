using System;

namespace Condor.Visitor.Generator.Abstractions
{
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
        Concrete = 64,
        Sealed = 128,
    }

}
