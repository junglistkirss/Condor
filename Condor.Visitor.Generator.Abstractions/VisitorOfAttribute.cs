using System;

namespace Condor.Visitor.Generator.Abstractions
{
    public abstract class VisitorOfAttribute(Type type) : BaseVisitorAttribute
    {
        public Type Type { get; } = type;
    }

}
