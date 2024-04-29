using Microsoft.CodeAnalysis.CSharp.Syntax;
using Condor.Generator.Utils;

namespace Condor.Visitor.Generator
{
    internal record class OutputVisitorInfo : GeneratedTypeInfo
    {
        public TargetTypeInfo[] ImplementationTypes { get; internal set; }
        public TargetTypeInfo VisitedType { get; internal set; }
        public bool AddVisitFallBack { get; internal set; }
        public string AccessibilityModifier { get; internal set; }
        public TargetTypeInfo Owner { get; internal set; }
        public string OriginalTypeDefinition { get; internal set; }
        public string GenericTypesDefinition { get; internal set; }
        public string BaseTypeDefinition { get; internal set; }
        public OutputVisitorDefaultInfo Default { get; internal set; }
        public OutputVisitableInfo Visitable { get; internal set; }

        public NamedParamInfo[] TypedArgs { get; internal set; }
        public bool IsAsync { get; internal set; }

        public bool HasReturnType => Owner.IsGeneric && (Owner.GenericTypes.Any(x => x.IsOut) || Owner.GenericTypes.Where(x => x.IsVarianceUnspecified).Count() == 1);
        public string ReturnType => Owner.GenericTypes.FirstOrDefault(x => x.IsOut)?.Name ?? Owner.GenericTypes.FirstOrDefault(x => x.IsVarianceUnspecified)?.Name;
        public bool HasArgs => Owner.IsGeneric && Owner.GenericTypes.Any(x => x.IsIn);

        private static string GetArgName(string type)
        {
            if (type.StartsWith("T"))
                return type.Substring(1).ToLower();
            return type.ToLower();
        }

        public string[] Args => HasArgs ? Owner.GenericTypes.Where(x => x.IsIn).Select(x => x.Name + ' ' + GetArgName(x.Name)).ToArray() : [];
        public string[] ArgsName => HasArgs ? Owner.GenericTypes.Where(x => x.IsIn).Select(x => GetArgName(x.Name)).ToArray() : [];

        public bool AddVisitRedirect { get; internal set; }
    }
}
