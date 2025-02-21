using UnityEngine;
using System;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Sebastian Lague

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    public readonly string ConditionalSourceField;
    public readonly int EnumIndex;

    public ConditionalHideAttribute(string boolVariableName)
    {
        ConditionalSourceField = boolVariableName;
    }

    public ConditionalHideAttribute(string enumVariableName, int enumIndex)
    {
        ConditionalSourceField = enumVariableName;
        this.EnumIndex = enumIndex;
    }

}



