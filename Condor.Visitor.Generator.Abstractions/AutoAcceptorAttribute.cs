using System.Diagnostics.CodeAnalysis;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
public sealed class AutoAcceptorAttribute<T> : Attribute
{
    public string AssemblyPattern { get; set; } = default!;
    public string TypePattern { get; set; } = default!;
    public AcceptedKind Accept { get; set; } = AcceptedKind.Class;
    public bool AcceptRequireAll { get; set; } = false;

    public bool AddVisitRedirect { get; set; } = false;
    public bool AddVisitFallback { get; set; } = false;

}
