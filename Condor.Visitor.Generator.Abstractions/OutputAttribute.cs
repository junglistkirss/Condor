using System;
using System.Diagnostics.CodeAnalysis;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
public sealed class OutputAttribute<T> : Attribute { }
