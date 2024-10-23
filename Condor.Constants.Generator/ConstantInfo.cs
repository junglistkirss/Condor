using Condor.Generator.Utils;
using System;

namespace Condor.Constants.Generator
{
    internal record ConstantInfo : GeneratedTypeInfo
    {
        public MemberInfo Member { get; internal set; }
        public string[] Options { get; internal set; }
    }
}
