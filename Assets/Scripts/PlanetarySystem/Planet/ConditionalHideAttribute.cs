using System;
using UnityEngine;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Sebastian Lague

namespace PlanetarySystem.Planet
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct)]
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
            EnumIndex = enumIndex;
        }
    }
}