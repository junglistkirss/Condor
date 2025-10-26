# Condor.Constants.Generator

This generator is used to genrate a custom template given as additionnal file

```
csharp

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

```
mustache
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
```
csharp
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

## Limitations

There is no type check on constants fields.