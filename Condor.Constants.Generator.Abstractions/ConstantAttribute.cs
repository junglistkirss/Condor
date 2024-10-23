using System;

namespace Condor.Constants.Generator.Abstractions
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class ConstantAttribute(string partialTemplate) : Attribute
    {
        public string PartialTemplate { get; } = partialTemplate;
    }
}
