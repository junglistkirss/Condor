using System;

namespace Condor.Visitor.Generator.Abstractions
{
    public enum VisitOptions
    {
        AbstractVisit,
        UseVisitFallBack,
    }
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


    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class GenerateDefaultAttribute : Attribute
    {
        public OptionsDefault Options { get; set; } = OptionsDefault.None;
        public VisitOptions VisitOptions { get; set; } = VisitOptions.AbstractVisit;
    }

}
