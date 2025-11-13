using RobinMustache;

namespace Condor.Generator.Utils;

public static class GenretorAccessorsExtensions
{
    public static StaticAccessorBuilder AddDefaultsAccessors(this StaticAccessorBuilder builder)
    {
        return builder
            .CreateMemberObjectAccessor<ActionInfo>(ActionInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<ArgumentInfo>(ArgumentInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<AttributeInfo>(AttributeInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<MemberInfo>(MemberInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<ParameterInfo>(ParameterInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<TargetTypeInfo>(TargetTypeInfoAccessor.GetNamedProperty)
            .CreateMemberObjectAccessor<TypeArgumentInfo>(TypeArgumentInfoAccessor.GetNamedProperty);
    }
}
