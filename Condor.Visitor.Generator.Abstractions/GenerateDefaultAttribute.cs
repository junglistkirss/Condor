﻿using System;

namespace Condor.Visitor.Generator.Abstractions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class GenerateDefaultAttribute : Attribute
{
    public OptionsDefault Options { get; set; } = OptionsDefault.None;
    public VisitOptions VisitOptions { get; set; } = VisitOptions.AbstractVisit;
}
