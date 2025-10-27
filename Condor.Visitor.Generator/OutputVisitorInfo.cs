using Microsoft.CodeAnalysis.CSharp.Syntax;
using Condor.Generator.Utils;

namespace Condor.Visitor.Generator;


internal record class OutputVisitorInfo : GeneratedTypeInfo
{
    public string VisitMethodName { get; internal set; } = default!;
    public string AccessibilityModifier { get; internal set; } = default!;
    public string KeywordTypeDefinition { get; internal set; } = default!;
    public bool IsInterface => KeywordTypeDefinition == "interface";
    public string OriginalTypeDefinition { get; internal set; } = default!;

    //public TargetTypeInfo Owner { get; internal set; }
    public ImplGroup[] ImplementationGroup { get; internal set; } = [];
    public string? GenericTypesDefinition { get; internal set; }
    public string BaseTypeDefinition { get; internal set; } = default!;
    public OutputVisitorDefaultInfo Default { get; internal set; } = default!;
    public OutputVisitableInfo Visitable { get; internal set; } = default!;
    public NamedParamInfo[] TypedArgs { get; internal set; } = [];
    public bool IsAsync { get; internal set; }

    public bool HasReturnType { get; internal set; }
    public string? ReturnType { get; internal set; }
    public bool HasArgs { get; internal set; }

}
