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
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class VisitorAttribute<TElement>() : VisitorOfAttribute(typeof(TElement)) { }

}
