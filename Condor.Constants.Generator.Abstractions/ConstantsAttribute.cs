using System;
using System.Transactions;

namespace Condor.Constants.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConstantsAttribute(string template) : Attribute
    {
        public string Template { get; } = template;

    }
}
