namespace Condor.Visitor.Generator
{
    internal record class OutputVisitorDefaultInfo
    {
        public bool ForcePublic { get; internal set; }
        public bool IsAbstract { get; internal set; }
        public bool IsPartial { get; internal set; }
        public bool GenerateDefault { get; internal set; }
        public bool IsVisitAbstract { get; internal set; }
        public string DefaultTypeName { get; internal set; }
    }
}
