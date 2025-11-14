using System;

namespace Condor.Visitor.Generator.Abstractions
{
    [Flags]
    public enum OptionsDefault
    {
        None = 0,
        IsPartial = 1,
        IsAbstract = 1 << 1,
        ForcePublic = 1 << 2,
        All = IsPartial | IsAbstract | ForcePublic,
        AsbtractPartial = IsPartial | IsAbstract,
    }

}
