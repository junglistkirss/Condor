# Condor.Contracts.Generator.Abstractions

**FindTypesAttribute<>** require a string template name (without `.mustache` extension), this attribute is only allowed at assembly level; The generic type parameter is used to find derived types.

| Named argument | Description |
| -------- | ----------- |
| AssemblyContraint | Regex to filter the containing assembly derived found type |
| IsRecord | boolean to find only record types |
| IsGeneric | boolean to find only generic types |
| IsAbstract | boolean to find only abstract types |
| GroupByHostAssembly | booelan to instruct generator by containing assembly (for the found types) |

**Project code**
```csharp
[assembly: FindTypes<Message>("TypeProvider", AssemblyContraint = "Sample.Assembly", IsGeneric = false, IsAbstract = false)]
```

**TypeProvider.mustache**
```handlebars
/* auto-generated */
using Sample.Assembly.Registry;

namespace {{OutputNamespace}}.TypeProviders
{
    internal sealed class {{ClassName}}TypeProvider : ITypeProvider
    {
        public static readonly {{ClassName}}TypeProvider Instance = new {{ClassName}}TypeProvider();

        public void Map(MyRegistryClass registry)
        {
            {{#Map}}
            registry.Map(typeof({{{ContainingNamespace}}}.{{{TypeDefinition}}}), "{{{TypeName}}}");
            {{/Map}}
        }
    }
}
```

**Generated code** : *[TemplateKey]*.*[ArgumentType]*.generated.cs
```csharp
/* auto-generated */
using Sample.Assembly.Registry

namespace Sample.Assembly.TypeProviders
{
    internal sealed class MessageTypeProvider : ITypeProvider
    {
        public static readonly MessageTypeProvider Instance = new MessageTypeProvider();

        public void Map(TypeNameRegistry registry)
        {
            registry.Map(typeof(Sample.Assembly.Events.MessageAdded), "MessageAdded");
            registry.Map(typeof(Sample.Assembly.Events.MessageDeleted), "MessageDeleted");
            registry.Map(typeof(Sample.Assembly.MessageBlocked), "MessageBlocked");
        }
    }
}


```