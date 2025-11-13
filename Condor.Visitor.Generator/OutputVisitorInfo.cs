using Condor.Generator.Utils;
using RobinMustache.Generators.Accessor;

namespace Condor.Visitor.Generator;

[GenerateAccessor]
internal record class OutputVisitorInfo 
{
    public string OutputNamespace { get; set; }
    public string ClassName { get; set; }

    public string VisitMethodName { get; internal set; }
    public string AccessibilityModifier { get; internal set; }
    public string KeywordTypeDefinition { get; internal set; }
    public bool IsInterface => KeywordTypeDefinition == "interface";
    public string OriginalTypeDefinition { get; internal set; }

    //public TargetTypeInfo Owner { get; internal set; }
    public ImplGroup[] ImplementationGroup { get; internal set; }
    public string GenericTypesDefinition { get; internal set; }
    public string BaseTypeDefinition { get; internal set; }
    public OutputVisitorDefaultInfo Default { get; internal set; }
    public OutputVisitableInfo Visitable { get; internal set; }
    public NamedParamInfo[] TypedArgs { get; internal set; }
    public bool IsAsync { get; internal set; }

    public bool HasReturnType { get; internal set; }
    public string ReturnType { get; internal set; }
    public bool HasArgs { get; internal set; }

    //private static string GetArgName(string type)
    //{
    //    if (type.StartsWith("T"))
    //        return type.Substring(1).ToLower();
    //    return type.ToLower();
    //}

    //public NamedParamInfo[] Args => HasArgs ? Owner.GenericTypes.Where(x => x.IsIn).Select(x => x.Name + ' ' + GetArgName(x.Name)).ToArray() : [];
    //public string[] ArgsName => HasArgs ? Owner.GenericTypes.Where(x => x.IsIn).Select(x => GetArgName(x.Name)).ToArray() : [];

}
