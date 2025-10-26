# Condor.Generator.Utils

Provide templating engine based on Handlebars.Net (https://github.com/Handlebars-Net)

Define strutures for carrying info about types, fields, properties, ... to pass to generators

## GeneratedTypeInfo

Abstract class that should be derived by generator project

OutputNamespace : Often based on the partial decorated class
ClassName : Often based on the partial decorated class, maybe suffixed by a keyword to describe the intention (ex: Visitor)

## TargetTypeInfo 

ContainingAssembly : full name of the containing assembly
ContainingNamespace : full name of the containing namespace  FullyQualifiedFormat without global name style
SanitizeTypeName: sanitized type name (trimming `Base` & `I`)
SanitizeTypeNameAsArg : SanitizeTypeName lowerized
GenericBaseTypeName : MinimallyQualifiedFormat without generic (remove `<T>` C# `(Of T)` VB.Net)
TypeName : MinimallyQualifiedFormat with ExpandNullable
TypeFullName : FullyQualifiedFormat with ExpandNullable
TypeDefinition : MinimallyQualifiedFormat with IncludeVariance, without IncludeContainingType

Interfaces : TargetTypeInfo collection of interfaces directly on the scoped type
AllInterfaces : TargetTypeInfo collection of all implemnented interfaces on the scoped type

IsNullable : boolean indicating the type nullabilty
IsArray : boolean indicating the type array
ElementType : if scoped type is array, get the TargetTypeInfo of the stored element type
IsGeneric : boolean indicating the type is generic or not
IsAbstract: boolean indicating the type is abstract definition
IsRecord: boolean indicating the type is record definition
GenericTypes : TypeArgumentInfo collection of generic type arguments of the scoped type

## AttributeInfo

AttributeType : TargetTypeInfo of the attribute
Constructor : Action info of the attribute's constructor
ConstructorArguments : ArgumentInfo collection of the scoped attribute
NamedArguments : ArgumentInfo collection of the scoped attribute


## MemberInfo

MemberName : name of the member
IsConstant : boolean indicating if member is defined as constant
IsNullable : boolean indicating if member type is nullable
MemberType : TargetTypeInfo of the member type
Attributes : AttributeInfo collection of the member's attributes 

## ActionInfo

IsStatic : boolean
Name : method string name
Definition : MinimallyQualifiedFormat
ReturnType : TargetTypeInfo
TypeArguments : TargetTypeInfo collection
Parameters : ParameterInfo collection
Attributes : AttributeInfo collection
AccessibilityModifier : string
IsVoid : boolean

## ArgumentInfo

ArgumentValue : value if not null
IsNull : Is null value
ArgumentType : TargetTypeInfo
ArgumentName : Argument name

## TypeArgumentInfo

IsVarianceUnspecified : boolean if variance is None
IsIn: boolean if variance is In
IsOut: boolean if variance is Out
Name : name of typeArgument
HasConstraint : boolean indicating if type has constraints
Contraints : TargetTypeInfo collection
IsNullable

## ParameterInfo

ParameterName : name of the parameter
DefaultExpression : default expression value or null
ParameterType : TargetTypeInfo
HasDefaultExpression : boolean
IsOptional : boolean
IsParams : boolean
IsExtension : boolean
IsRefReadOnly : boolean
IsIn : boolean if variance is In
IsOut : boolean if variance is Out
IsRef : booelan is ref type
Attributes : AttributeInfo collection