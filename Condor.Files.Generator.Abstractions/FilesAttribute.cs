using System.Diagnostics.CodeAnalysis;

namespace Condor.Files.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
public sealed class FilesAttribute(string template, string filePattern) : Attribute
{
    public string Template { get; } = template;
    public string FilePattern { get; } = filePattern;
}
