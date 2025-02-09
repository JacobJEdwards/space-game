using Microlight.MicroEditor;
using UnityEditor;
using UnityEngine;

namespace Microlight.MicroBar
{
#if UNITY_EDITOR
    // ****************************************************************************************************
    // Stores instructions on how to draw the AnimCommand drawer
    // ****************************************************************************************************
    internal class AnimCommand_Fields
    {
        internal bool animAxis;
        internal string boolLabel = "";
        internal string boolTooltip = "";
        internal bool boolValue;
        internal string colorLabel = "";
        internal string colorTooltip = "";
        internal bool colorValue;
        internal bool delay;
        internal bool duration;
        internal bool durationAndDelay;
        internal bool ease;
        internal bool effect;
        internal bool execAndEffect;
        internal bool execution;
        internal bool extraFadeLine;
        internal string floatLabel = "";
        internal string floatTooltip = "";
        internal bool floatValue;
        internal string frequencyLabel = "";
        internal string frequencyTooltip = "";

        // Additional settings
        internal bool frequencyValue;
        internal bool header;
        internal string intLabel = "";
        internal string intTooltip = "";
        internal bool intValue = false;
        internal string percentLabel = "";
        internal string percentTooltip = "";
        internal bool percentValue;
        internal bool transformProperty;

        // Values
        internal bool valueMode;
        internal bool valuesFadeLine;
        internal string vector2Label = "";
        internal string vector2Tooltip = "";
        internal bool vector2Value;
        internal bool vector2Value2Row;
        internal string vector3Label = "";
        internal string vector3Tooltip = "";
        internal bool vector3Value;
        internal bool vector3Value2Row;

        internal AnimCommand_Fields(SerializedProperty property)
        {
            header = true;
            execution = true;
            duration = true;

            #region Execution

            if (execution)
            {
                var executionProperty = property.FindPropertyRelative("execution");
                switch ((AnimExecution)executionProperty.enumValueIndex)
                {
                    case AnimExecution.Sequence:
                        effect = true;
                        delay = true;
                        valuesFadeLine = true;
                        extraFadeLine = true;
                        break;
                    case AnimExecution.Parallel:
                        effect = true;
                        delay = true;
                        valuesFadeLine = true;
                        extraFadeLine = true;
                        break;
                    case AnimExecution.Wait:
                        return;
                    default:
#if UNITY_EDITOR
                        Debug.LogWarning(
                            $"MicroBar: Unknown execution value: {(AnimExecution)executionProperty.enumValueIndex}");
#endif
                        return;
                }
            }

            #endregion

            #region Effects

            if (effect)
            {
                delay = true;
                ease = true;
                valueMode = true;

                var effectProperty = property.FindPropertyRelative("effect");
                var animAxisProperty = property.FindPropertyRelative("animAxis");
                var transformPropertyProperty = property.FindPropertyRelative("transformProperty");
                var boolValueProperty = property.FindPropertyRelative("boolValue");
                var valueModeProperty = property.FindPropertyRelative("valueMode");

                var startingOrDefault = (ValueMode)valueModeProperty.enumValueIndex == ValueMode.StartingValue ||
                                        (ValueMode)valueModeProperty.enumValueIndex == ValueMode.DefaultValue;

                switch ((AnimEffect)effectProperty.enumValueIndex)
                {
                    case AnimEffect.Color:
                        if (!startingOrDefault)
                        {
                            colorValue = true;
                            colorLabel = "Color";
                        }

                        break;
                    case AnimEffect.Fade:
                        if ((ValueMode)valueModeProperty.enumValueIndex == ValueMode.Absolute)
                        {
                            percentValue = true;
                            percentLabel = "Fade";
                        }
                        else if (!startingOrDefault)
                        {
                            floatValue = true;
                            floatLabel = "Fade";
                        }

                        break;
                    case AnimEffect.Fill:
                        boolValue = true;
                        boolLabel = "Custom";
                        boolTooltip =
                            "If turned off, image will animate to the current health value\nTurning custom on, let's user choose fill amount manually";
                        valueMode = false;
                        if (boolValueProperty.boolValue)
                        {
                            valueMode = true;
                            if ((ValueMode)valueModeProperty.enumValueIndex == ValueMode.Absolute)
                            {
                                percentValue = true;
                                percentLabel = "Fill";
                            }
                            else if (!startingOrDefault)
                            {
                                floatValue = true;
                                floatLabel = "Fill";
                            }
                        }

                        break;
                    case AnimEffect.Move:
                        animAxis = true;
                        if (!startingOrDefault)
                        {
                            if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.XY)
                            {
                                vector2Value = true;
                                vector2Label = "Axis";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.X)
                            {
                                floatValue = true;
                                floatLabel = "X";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.Y)
                            {
                                floatValue = true;
                                floatLabel = "Y";
                            }
                            else
                            {
                                floatValue = true;
                                floatLabel = "XY";
                            }
                        }

                        break;
                    case AnimEffect.Rotate:
                        if (!startingOrDefault)
                        {
                            floatValue = true;
                            floatLabel = "Angle";
                            floatTooltip = "Animate to the desired angle on Z axis (degrees)";
                        }

                        break;
                    case AnimEffect.Scale:
                        animAxis = true;
                        if (!startingOrDefault)
                        {
                            if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.XY)
                            {
                                vector2Value = true;
                                vector2Label = "Axis";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.X)
                            {
                                floatValue = true;
                                floatLabel = "X";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.Y)
                            {
                                floatValue = true;
                                floatLabel = "Y";
                            }
                            else
                            {
                                floatValue = true;
                                floatLabel = "XY";
                            }
                        }

                        break;
                    case AnimEffect.Punch:
                        transformProperty = true;
                        valueMode = false;
                        if ((TransformProperties)transformPropertyProperty.enumValueIndex ==
                            TransformProperties.Rotation)
                        {
                            floatValue = true;
                            floatLabel = "Strength";
                        }
                        else
                        {
                            vector2Value = true;
                            vector2Label = "Strength";
                        }

                        frequencyValue = true;
                        frequencyLabel = "Freq.";
                        frequencyTooltip =
                            "Frequency of animation, bigger number means object will be faster (default = 10)";
                        percentValue = true;
                        percentLabel = "Elasticity";
                        percentTooltip = "How much inertia has effect on object (0-1)";
                        break;
                    case AnimEffect.Shake:
                        valueMode = false;
                        transformProperty = true;
                        floatValue = true;
                        floatLabel = "Strength";
                        floatTooltip = "Strength of the shake (default = 1, 100 for anchor transform)";
                        frequencyValue = true;
                        frequencyLabel = "Freq.";
                        frequencyTooltip =
                            "Frequency of animation, bigger number means object will be faster (default = 10)";
                        break;
                    case AnimEffect.AnchorMove:
                        animAxis = true;
                        if (!startingOrDefault)
                        {
                            if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.XY)
                            {
                                vector2Value = true;
                                vector2Label = "Axis";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.X)
                            {
                                floatValue = true;
                                floatLabel = "X";
                            }
                            else if ((AnimAxis)animAxisProperty.enumValueIndex == AnimAxis.Y)
                            {
                                floatValue = true;
                                floatLabel = "Y";
                            }
                            else
                            {
                                floatValue = true;
                                floatLabel = "XY";
                            }
                        }

                        break;
                    default:
                        Debug.LogWarning(
                            $"MicroBar: Unknown effect value: {(AnimEffect)effectProperty.enumValueIndex}");
                        return;
                }
            }

            #endregion

            #region Combine fields

            if (EditorGUIUtility.currentViewWidth > MicroEditor_Utility.SingleRowThreshold && execution && effect)
            {
                execution = false;
                effect = false;
                execAndEffect = true;
            }

            if (EditorGUIUtility.currentViewWidth > MicroEditor_Utility.SingleRowThreshold && duration && delay)
            {
                duration = false;
                delay = false;
                durationAndDelay = true;
            }

            if (EditorGUIUtility.currentViewWidth <= MicroEditor_Utility.UnityTwoRowsThreshold && vector2Value)
            {
                vector2Value = false;
                vector2Value2Row = true;
            }

            if (EditorGUIUtility.currentViewWidth <= MicroEditor_Utility.UnityTwoRowsThreshold && vector3Value)
            {
                vector3Value = false;
                vector3Value2Row = true;
            }

            #endregion
        }
    }
#endif
}