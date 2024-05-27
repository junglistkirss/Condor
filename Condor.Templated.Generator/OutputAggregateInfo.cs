using Toucan.Generator.Utils;

namespace Toucan.Aggregate.Generator
{
    internal record class AggregateDataInfo
    {
        public TargetTypeInfo PropertyType { get; internal set; }
        public string PropertyName { get; internal set; }
        public bool IsCreate { get; internal set; }
        public bool IsUnique { get; internal set; }
        public bool IsUpdate { get; internal set; }
    }

    internal record class OutputAggregateInfo : GeneratedTypeInfo
    {
        public TargetTypeInfo AggregateType { get; internal set; }
        public TargetTypeInfo IdentityType { get; internal set; }
        public bool HasSnapshot { get; internal set; }
        public TargetTypeInfo SnapshotType { get; internal set; }

        public AggregateDataInfo[] Datas { get; internal set; }
        public IEnumerable<AggregateDataInfo> UniqueDatas => Datas.Where(x => x.IsUnique);
        public IEnumerable<AggregateDataInfo> CreateDatas => Datas.Where(x => x.IsCreate);
        public IEnumerable<AggregateDataInfo> UpdateDatas => Datas.Where(x => x.IsUpdate);
        public bool HasUniqueConstraint => Datas.Any(x => x.IsUnique);
        public string UniqueConstraintName => HasUniqueConstraint ? AggregateType.TypeName + string.Concat(UniqueDatas.Select(x => x.PropertyName)) : null;
        public bool IsUpdatable => Datas.Any(x => x.IsUpdate);
    }
}
