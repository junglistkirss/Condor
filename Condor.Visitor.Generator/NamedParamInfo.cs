namespace Condor.Visitor.Generator;

internal record class NamedParamInfo
{
    public NamedParamInfo(string paramTypeFullName, string sanitizedParamName)
    {
        ParamTypeFullName = paramTypeFullName;
        SanitizedParamName = sanitizedParamName;
    }

    public string ParamTypeFullName { get; }
    public string SanitizedParamName { get; }
}
