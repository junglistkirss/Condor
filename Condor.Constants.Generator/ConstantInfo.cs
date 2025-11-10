using Condor.Generator.Utils;

namespace Condor.Constants.Generator
{
    internal record ConstantInfo 
    {
        public MemberInfo Member { get; internal set; }
        public string[] Partials { get; internal set; }
    }
}
