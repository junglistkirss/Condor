# Condor.Constants.Generator.Abstractions

**ConstantsAttribute** are placed on `Class` Or `Struct`
This attribute must define a string parameter named template, that correspond to file name with extension  **.mustache* see Condor.Generator.Utils documentation for more informations, string argement named `template` should point to an additional template file (*[template].mustache*).

**ConstantAttribute** are placed on `Field`
This attribute is used to define a custom key on constant field, this key can be used to weave the output generation in the template




