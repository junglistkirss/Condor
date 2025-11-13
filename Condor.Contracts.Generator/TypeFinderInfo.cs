using Condor.Generator.Utils;

namespace Condor.Contracts.Generator;

internal record struct TypeFinderInfo
{
    public string AssemblyName { get; internal set; }
    public string TypeNamespaceName { get; internal set; }


    public string AssemblyContraint { get; internal set; }
    public TargetTypeInfo TypeContraint { get; internal set; }
    public bool? IsRecord { get; internal set; }
    public bool? IsGeneric { get; internal set; }
    public bool? IsAbstract { get; internal set; }
    public string TemplateKey { get; internal set; }
    public bool GroupByHostAssembly { get; internal set; }
}
