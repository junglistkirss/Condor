# Condor.Visitor.Generator.Abstractions

**VisitorAttribute** are placed on `Class` Or `Interface` Or `Struct`. The decorated element must be partial, it could have TypeArguments with constraints, in case of In or Out type arguement please provide hints on the type arguments.

| Named argument | Description |
| -------------- | ----------- |
| IsAsync | instruct the generator to use `ValueTask`  `ValueTask<>` methods result |
| VisitMethodName | By default method is named `Visit`, this default behavior can be changed, please give a valid MethodName to avoid compilation errors |

**VisitParamAttribute<>** This attribute should be placed on the same Node decorating with the `VisitorAttribute` class. This instruct the generator to define the type argument as argument on all Visit method of the generator. This attribute can be used along with generic type arguments on decorated node.

| Named argument | Description |
| -------------- | ----------- |
| ParamName | Specify the name to use for the argument of all methods, if not specifed, the argument name is auto generated based on Type name |

**AcceptorAttribute<>** This attribute define the type to create visit methods of given type argument passed on this attribute.

**AutoAcceptorAttribute<>** This attribute define the base type to create visit methods of derived types of given type argument passed on this attribute, derived types are discovered in all knowns assemblies used by the project

| Named argument | Description |
| -------------- | ----------- |
| AssemblyPattern | String Regex expression matching the assembly FullyQualifiedTypeName |
| TypePattern | String Regex expression matching the type FullyQualifiedTypeName |
| Accept | Flag enum to specify the type Kind: None, Class, Interface, Struct, Record, Generic, Abstract, Concrete, Sealed |
| AcceptRequireAll | The discovered type must match all the constraint decribed by the Accept parameter |
| AddVisitRedirect | Voolean to instruct generator to create a visit redirect that receive base element type (ref. type argument's attribute). The body method use a switch statement to redirect to matche type visit method |
| AddVisitFallback | boolean to instruct the generator to create a VisitFallback method use in VisitRedirect method when element type does not match |

**OutputAttribute<>** This attribute should be placed on the same Node decorating with the `VisitorAttribute` class. This instruct the generator to define output  type on all Visit method of the generator

**GenerateVisitableAttribute<>** This attribute should be placed on the same Node decorating with the `VisitorAttribute` class. This instruct the generator to define an interface that define an Accept method, with same type arguments as the generator.

| Named argument | Description |
| -------------- | ----------- |
| MethodName | Specify the name to use for the methodof the genrated visitable interface |

**AcceptParamAttribute<>** This attribute should be placed on the same Node decorating with the `VisitorAttribute` and `GenerateVisitableAttribute`. This instruct the generator to define the type argument as argument on the accept method of the visitbale interface generated. This attribute can be used along with generic type arguments on decorated node.

| Named argument | Description |
| -------------- | ----------- |
| ParamName | Specify the name to use for the argument of all methods, if not specifed, the argument name is auto generated based on Type name |

**GenerateDefaultAttribute** This attribute should be placed on the same Node decorating with the `VisitorAttribute`. This instruct the generator to define a class defining visit methods implementing the generated visitor attribute interface, the.

| Named argument | Description |
| -------------- | ----------- |
| Options | Specify the accessibility modifier and if class should be generated as partial and/or abstract |
| VisitOptions | Specify how the visit methods should be defined in the default geenrated class |

