using System;

namespace Condor.Files.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class FilesAttribute(string template, string filePattern) : Attribute
{
    public string Template { get; } = template;
    public string FilePattern { get; } = filePattern;
}
