using System.Diagnostics.CodeAnalysis;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
public sealed class AcceptorAttribute<T> : Attribute { }
