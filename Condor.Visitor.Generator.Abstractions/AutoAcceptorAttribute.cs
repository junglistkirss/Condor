using System;
/*
 * 
 * 

[Visitor<IEventMessage>, AutoAcceptor<IEventMessage>, VisitArg<RefToken>]
[GenerateDefault]
[GenerateVisitable, AcceptParam<IEventMessage>]
public partial interface IEventMessageVisitor<in TArgs> { }


[Visitor(typeof(ICommandMessage), GenerateDefault = true, CreateVisitable = true)]
public partial interface ICommandMessageVisitor<out T, in TArgs> { }


[Visitor(typeof(ISmartAggregate), GenerateDefault = true, CreateVisitable = true)]
public partial interface ISmartAggregateVisitor<out T, in TArgs> { }

  */

namespace Condor.Visitor.Generator.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public class AutoAcceptorAttribute<T> : Attribute
    {
        public string AssemblyPart { get; set; } = nameof(Toucan);
        public bool AllowAbstract { get; set; } = false;
        public bool AllowGeneric { get; set; } = false;
        public bool AllowRecord { get; set; } = true;
    }

}
