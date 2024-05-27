using Condor.Generator.Utils;

namespace Condor.Visitor.Generator
{
    internal record class NamedParamInfo
    {
        public TargetTypeInfo ParamType { get; internal set; }
        public string ParamName { get; internal set; }
        public string SanitizedParamName => ParamName ?? ParamType.SanitizeTypeNameAsArg;
    }
}
