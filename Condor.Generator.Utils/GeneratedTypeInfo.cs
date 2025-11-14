namespace Condor.Generator.Utils
{
    public abstract record GeneratedTypeInfo
    {
        public string OutputNamespace { get; set; }
        public string ClassName { get; set; }

    }
}
