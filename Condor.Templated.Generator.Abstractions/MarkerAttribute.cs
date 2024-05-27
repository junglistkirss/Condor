using System;

namespace Condor.Templated.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class MarkerAttribute(string key) : Attribute
    {
        public string Key { get; } = key;
    }
}