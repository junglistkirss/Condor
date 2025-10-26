# Condor.Constants.Generator.Abstractions

Attibutes for ConstantGenerator

**ConstantsAttribute** are placed on *Class* Or *Struct*

**ConstantAttribute** are placed on *Field*

Both attributres must defined a string parameter, that correspond to file name with extension *.mustache* see Condor.Generator.Utils documentation.

The decorated class with **ConstantsAttribute** is generated using the follow arguments :

OutputNamespace : same as decorated class
ClassName : same as decorated class
Map : an array containing all the constants fields found in decorated class

Each item of that array is structured like : 

Member : details of member
	Member.MemberName : Name of the field
	Member.IsNullable : boolean inidcating if field type is nullable type
	Member.MemberType : type of this field
	Member.Attributes : coolection of attribute info decorating the target field

Partials : array of key defined by the **ConstantAttribute**


