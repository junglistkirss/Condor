namespace Condor.Contracts.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class FindTypesAttribute<T>(string template) : Attribute
{
    public string Template { get; } = template;
    public string? AssemblyContraint { get; set; }
    public bool IsRecord { get; set; }
    public bool IsGeneric { get; set; }
    public bool IsAbstract { get; set; }
    public bool GroupByHostAssembly { get; set; }
}
