using System;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class OutputAttribute<T> : Attribute { }
