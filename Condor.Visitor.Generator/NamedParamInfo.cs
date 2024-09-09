using Condor.Generator.Utils;

namespace Condor.Visitor.Generator
{
    internal record class NamedParamInfo
    {
        public string ParamTypeFullName { get; internal set; }
        public string SanitizedParamName { get; internal set; }
    }
}
