using System;

namespace Condor.Visitor.Generator.Abstractions
{
    [Flags]
    public enum VisitorOptions : byte
    {
        None = 0,
        AddVisitFallBack = 1,
        AddVisitRedirect = 1 << 1,
        All = AddVisitFallBack | AddVisitRedirect,

    }

    public abstract class BaseVisitorAttribute : Attribute
    {
        public bool IsAsync { get; set; }
        public VisitorOptions Options { get; set; } = VisitorOptions.AddVisitFallBack;
    }
}
