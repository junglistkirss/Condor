# Condor.Constants.Generator

This generator is used to genrate a custom template given as additionnal file

```csharp
namespace Test.Grants;

[Constants("AllKeys")]
public static partial class CrudSample
{
    public const string CanCreate = "can__create";
    public const string CanRead = "can__read";
    public const string CanUpdate = "can__update";
    public const string CanDelete = "can__delete";
}
```

Additional file : AllKeys.mustache

```handlebars
/* auto-generated */

namespace {{OutputNamespace}}
{
    public static partial class {{ClassName}}
    {
        public static IEnumerable<string> GetAll()
        {
            {{#each Map}}
            yield return {{{Member.MemberName}}};
            {{/each}}
        }
    }
}
```

Generated file is *CrudSample.AllKeys.generated.cs*

```csharp
/* auto-generated */

namespace Test.Grants
{
    public static partial class CrudSample
    {
        public static IEnumerable<string> GetAll()
        {
            yield return CanCreate;
            yield return CanRead;
            yield return CanUpdate;
            yield return CanDelete;
        }
    }
}
```

Partial template keys defined on fields can enpower conditions in templates to make variance on generated output.

## Generator data

The decorated class does not need to be partial, it depends about the template purpose. The decorated class with `ConstantsAttribute` is generated using the follow informations :

**ContantsInfo**
| Property | Description |
| -------- | ----------- |
| OutputNamespace | Same as decorated class |
| ClassName | Same as decorated class |
| Map | `MemberInfo` collection containing all the constants fields found in decorated class |


**MemberInfo**

| Property | Description |
| -------- | ----------- |
| MemberName | Name of the field |
| IsNullable | boolean inidcating if field type is nullable type |
| MemberType | type of this field |
| Attributes | coolection of AttributeInfo decorating the target field |
| Partials | array of key defined by the |

## Limitations

There is no type check on constants fields.