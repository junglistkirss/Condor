namespace Condor.Constants.Generator;

internal record TemplatedFileInfo
{
    public string FileName { get; internal set; }
    public string FileContent { get; internal set; }
}
