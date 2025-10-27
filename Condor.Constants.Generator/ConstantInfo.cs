using Condor.Generator.Utils;

namespace Condor.Constants.Generator;

internal record ConstantInfo
{
    public ConstantInfo(MemberInfo member, string[] partials)
    {
        Member = member;
        Partials = partials;
    }

    public MemberInfo Member { get; }
    public string[] Partials { get; }
}
