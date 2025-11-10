using System;

namespace Condor.Constants.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class ConstantsAttribute(string template) : Attribute
    {
        public string Template { get; } = template;

    }
}
