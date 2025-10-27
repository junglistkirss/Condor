using Condor.Generator.Utils;

namespace Condor.Templated.Generator;

internal record class TemplatedInfo : GeneratedTypeInfo
{
    public TargetTypeInfo TemplatedType { get; internal set; }
    public EnhanceInfo[] Enhancements { get; internal set; } = [];
    public ExtendInfo[] Extends { get; internal set; } = [];

    public MemberDataInfo[] Properties { get; internal set; }
    public IEnumerable<string> PropertiesKeys => Properties.SelectMany(p => p.Enhancements.Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => x.Key)).Distinct();

    public ActionDataInfo[] Actions { get; internal set; }
    public IEnumerable<string> ActionsKeys => Actions.SelectMany(p => p.Enhancements.Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => x.Key)).Distinct();
}

internal record class ExtendInfo
{
    public string Key { get; internal set; } = null;
    public string Value { get; internal set; } = null;
}
internal record class EnhanceInfo
{
    public string Key { get; internal set; } = null;
    public TargetTypeInfo EnchancedType { get; internal set; } = null;
}

internal record class MemberDataInfo
{
    public MemberInfo Member { get; internal set; }
    public EnhanceInfo[] Enhancements { get; internal set; } = [];
    public ExtendInfo[] Extends { get; internal set; } = [];
}

internal record class ActionDataInfo
{
    public ActionInfo Action { get; internal set; }
    public EnhanceInfo[] Enhancements { get; internal set; } = [];
    public ExtendInfo[] Extends { get; internal set; } = [];
}