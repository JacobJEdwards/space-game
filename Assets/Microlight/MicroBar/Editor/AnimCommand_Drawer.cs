using System;
using Microlight.MicroEditor;
using UnityEditor;
using UnityEngine;

namespace Microlight.MicroBar
{
#if UNITY_EDITOR
    // ****************************************************************************************************
    // Property drawer for the AnimCommand class
    // ****************************************************************************************************
    [CustomPropertyDrawer(typeof(AnimCommand))]
    public class AnimCommand_Drawer : PropertyDrawer
    {
        public static float GetHeight(SerializedProperty property)
        {
            float totalHeight = 0;

            var fields = new AnimCommand_Fields(property);
            // Header
            if (fields.header)
                totalHeight += HeaderHeight();
            // Execution and effect
            if (fields.execution)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.effect)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.execAndEffect)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            // Duration and delay
            if (fields.duration)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.delay)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.durationAndDelay)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;

            // Fade line separator
            if (fields.valuesFadeLine)
                totalHeight += FadeLineHeight();

            if (fields.transformProperty)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;

            // Values
            if (fields.boolValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.valueMode)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.animAxis)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.intValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.frequencyValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.floatValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.vector2Value)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.vector2Value2Row)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight * 2;
            if (fields.vector3Value)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.vector3Value2Row)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight * 2;
            if (fields.colorValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;
            if (fields.percentValue)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;

            // Fade line separator
            if (fields.extraFadeLine)
                totalHeight += FadeLineHeight();

            // Additional settings
            if (fields.ease)
                totalHeight += MicroEditor_Utility.DefaultFieldHeight;

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var fields = new AnimCommand_Fields(property);
            // Header
            if (fields.header)
                position = DrawHeader(position, property);
            // Execution and effect
            if (fields.execution)
                position = DrawExecution(position, property);
            if (fields.effect)
                position = DrawEffect(position, property);
            if (fields.execAndEffect)
                position = DrawExecutionAndEffect(position, property);
            // Duration and delay
            if (fields.duration)
                position = DrawDuration(position, property);
            if (fields.delay)
                position = DrawDelay(position, property);
            if (fields.durationAndDelay)
                position = DrawDurationAndDelay(position, property);

            // Fade line separator
            if (fields.valuesFadeLine)
                position = DrawFadeLine(position);

            if (fields.transformProperty)
                position = DrawTransformProperty(position, property);

            if (fields.boolValue)
                position = DrawBool(position, property, fields.boolLabel, fields.boolTooltip);
            if (fields.valueMode)
                position = DrawValueMode(position, property);
            if (fields.animAxis)
                position = DrawAnimAxis(position, property);
            if (fields.intValue)
                position = DrawInt(position, property, fields.intLabel, fields.intTooltip);
            if (fields.frequencyValue)
                position = DrawFrequency(position, property, fields.frequencyLabel, fields.frequencyTooltip);
            if (fields.floatValue)
                position = DrawFloat(position, property, fields.floatLabel, fields.floatTooltip);
            if (fields.vector2Value)
                position = DrawVector2(position, property, fields.vector2Label, fields.vector2Tooltip);
            if (fields.vector2Value2Row)
                position = DrawVector22Row(position, property, fields.vector2Label, fields.vector2Tooltip);
            if (fields.vector3Value)
                position = DrawVector3(position, property, fields.vector3Label, fields.vector3Tooltip);
            if (fields.vector3Value2Row)
                position = DrawVector32Row(position, property, fields.vector3Label, fields.vector3Tooltip);
            if (fields.colorValue)
                position = DrawColor(position, property, fields.colorLabel, fields.colorTooltip);
            if (fields.percentValue)
                position = DrawPercent(position, property, fields.percentLabel, fields.percentTooltip);

            // Fade line separator
            if (fields.extraFadeLine)
                position = DrawFadeLine(position);

            if (fields.ease)
                position = DrawEase(position, property);
            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     Defines text for the header element
        /// </summary>
        private static string HeaderText(SerializedProperty property)
        {
            var executionProperty = property.FindPropertyRelative("execution");
            var effectProperty = property.FindPropertyRelative("effect");
            var prefix = Enum.GetName(typeof(AnimExecution), executionProperty.enumValueIndex);
            var suffix = Enum.GetName(typeof(AnimEffect), effectProperty.enumValueIndex);

            if ((AnimExecution)executionProperty.enumValueIndex == AnimExecution.Wait) return prefix;

            return prefix + " - " + suffix;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight(property);
        }

        #region Building blocks

        #region Button rects

        // Used by MicroBarAnimation_Drawer to get rect for drawing the remove button
        public static Rect RemoveButtonRect(Rect position)
        {
            return new Rect(
                position.xMax - MicroEditor_Utility.HeaderLineHeight,
                position.y,
                MicroEditor_Utility.HeaderLineHeight,
                MicroEditor_Utility.HeaderLineHeight);
        }

        // Used by MicroBarAnimation_Drawer to get rect for drawing the down button
        public static Rect DownButtonRect(Rect position)
        {
            return new Rect(
                position.x + MicroEditor_Utility.HeaderLineHeight,
                position.y,
                MicroEditor_Utility.HeaderLineHeight,
                MicroEditor_Utility.HeaderLineHeight);
        }

        // Used by MicroBarAnimation_Drawer to get rect for drawing the up button
        public static Rect UpButtonRect(Rect position)
        {
            return new Rect(
                position.x,
                position.y,
                MicroEditor_Utility.HeaderLineHeight,
                MicroEditor_Utility.HeaderLineHeight);
        }

        #endregion

        #region Command settings

        private Rect DrawHeader(Rect position, SerializedProperty property)
        {
            // Prepare
            var headerRect = new Rect(
                position.x,
                position.y,
                position.width,
                MicroEditor_Utility.HeaderLineHeight);
            var centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            // Draw
            MicroEditor_DrawUtility.DrawContainer(position);
            MicroEditor_DrawUtility.DrawContainerGlow(headerRect);
            MicroEditor_DrawUtility.DrawContainerBottomOutline(headerRect);
            EditorGUI.LabelField(headerRect, HeaderText(property), centeredStyle);

            // Finish
            return new Rect(
                position.x + 5,
                position.y + headerRect.height + MicroEditor_Utility.VerticalSpacing,
                position.width - 10,
                position.height);
        }

        private Rect DrawExecution(Rect position, SerializedProperty property)
        {
            var executionProperty = property.FindPropertyRelative("execution");
            return MicroEditor_DrawUtility.DrawProperty(position, executionProperty,
                new GUIContent("Execution", MicroBar_Theme.ExecutionTooltip));
        }

        private Rect DrawEffect(Rect position, SerializedProperty property)
        {
            var effectProperty = property.FindPropertyRelative("effect");
            return MicroEditor_DrawUtility.DrawProperty(position, effectProperty,
                new GUIContent("Effect", MicroBar_Theme.EffectTooltip));
        }

        private Rect DrawExecutionAndEffect(Rect position, SerializedProperty property)
        {
            var executionProperty = property.FindPropertyRelative("execution");
            var effectProperty = property.FindPropertyRelative("effect");
            return MicroEditor_DrawUtility.DrawTwoPropertiesConstantField(
                position,
                executionProperty, new GUIContent("Exec", MicroBar_Theme.ExecutionTooltip),
                effectProperty, new GUIContent("Effect", MicroBar_Theme.EffectTooltip),
                80);
        }

        private Rect DrawDuration(Rect position, SerializedProperty property)
        {
            var durationProperty = property.FindPropertyRelative("duration");
            return MicroEditor_DrawUtility.DrawProperty(position, durationProperty,
                new GUIContent("Duration", MicroBar_Theme.DurationTooltip));
        }

        private Rect DrawDelay(Rect position, SerializedProperty property)
        {
            var delayProperty = property.FindPropertyRelative("delay");
            return MicroEditor_DrawUtility.DrawProperty(position, delayProperty,
                new GUIContent("Delay", MicroBar_Theme.DelayTooltip));
        }

        private Rect DrawDurationAndDelay(Rect position, SerializedProperty property)
        {
            var durationProperty = property.FindPropertyRelative("duration");
            var delayProperty = property.FindPropertyRelative("delay");
            return MicroEditor_DrawUtility.DrawTwoPropertiesConstantField(
                position,
                durationProperty, new GUIContent("Duration", MicroBar_Theme.DurationTooltip),
                delayProperty, new GUIContent("Delay", MicroBar_Theme.DelayTooltip),
                50);
        }

        #endregion

        #region Values

        private Rect DrawValueMode(Rect position, SerializedProperty property)
        {
            var valueModeProperty = property.FindPropertyRelative("valueMode");
            return MicroEditor_DrawUtility.DrawProperty(position, valueModeProperty,
                new GUIContent("Value Mode", MicroBar_Theme.ValueModeTooltip));
        }

        private Rect DrawFloat(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var floatProperty = property.FindPropertyRelative("floatValue");
            return MicroEditor_DrawUtility.DrawProperty(position, floatProperty,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawInt(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var intProperty = property.FindPropertyRelative("intValue");
            return MicroEditor_DrawUtility.DrawProperty(position, intProperty, new GUIContent(labelText, tooltipText));
        }

        private Rect DrawFrequency(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var frequencyProperty = property.FindPropertyRelative("frequency");
            return MicroEditor_DrawUtility.DrawProperty(position, frequencyProperty,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawBool(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var boolValueProperty = property.FindPropertyRelative("boolValue");
            return MicroEditor_DrawUtility.DrawProperty(position, boolValueProperty,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawVector2(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var vector2Property = property.FindPropertyRelative("vector2Value");
            return MicroEditor_DrawUtility.DrawProperty(position, vector2Property,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawVector22Row(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var tmpRect = DrawVector2(position, property, labelText, tooltipText);
            tmpRect.y += MicroEditor_Utility.DefaultFieldHeight;
            return tmpRect;
        }

        private Rect DrawVector3(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var vector3Property = property.FindPropertyRelative("vector3Value");
            return MicroEditor_DrawUtility.DrawProperty(position, vector3Property,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawVector32Row(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var tmpRect = DrawVector3(position, property, labelText, tooltipText);
            tmpRect.y += MicroEditor_Utility.DefaultFieldHeight;
            return tmpRect;
        }

        private Rect DrawColor(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var colorProperty = property.FindPropertyRelative("colorValue");
            return MicroEditor_DrawUtility.DrawProperty(position, colorProperty,
                new GUIContent(labelText, tooltipText));
        }

        private Rect DrawPercent(Rect position, SerializedProperty property, string labelText, string tooltipText)
        {
            var percentValueProperty = property.FindPropertyRelative("percentValue");
            return MicroEditor_DrawUtility.DrawProperty(position, percentValueProperty,
                new GUIContent(labelText, tooltipText));
        }

        #endregion

        #region Additional settings

        private Rect DrawEase(Rect position, SerializedProperty property)
        {
            var easeProperty = property.FindPropertyRelative("ease");
            return MicroEditor_DrawUtility.DrawProperty(position, easeProperty,
                new GUIContent("Ease", MicroBar_Theme.EaseTooltip));
        }

        private Rect DrawAnimAxis(Rect position, SerializedProperty property)
        {
            var animAxisProperty = property.FindPropertyRelative("animAxis");
            return MicroEditor_DrawUtility.DrawProperty(position, animAxisProperty,
                new GUIContent("Axis", MicroBar_Theme.AxisTooltip));
        }

        private Rect DrawTransformProperty(Rect position, SerializedProperty property)
        {
            var transformPropertyProperty = property.FindPropertyRelative("transformProperty");
            return MicroEditor_DrawUtility.DrawProperty(position, transformPropertyProperty,
                new GUIContent("Transform", MicroBar_Theme.TransformTooltip));
        }

        private Rect DrawFadeLine(Rect position)
        {
            var fadeRect = new Rect(position.x, position.y + 5, position.width, position.height);
            MicroEditor_DrawUtility.DrawFadeLine(fadeRect);
            return new Rect(position.x, position.y + 13, position.width, position.height);
        }

        #endregion

        #endregion

        #region Height

        private static float HeaderHeight()
        {
            return MicroEditor_Utility.HeaderLineHeight + MicroEditor_Utility.VerticalSpacing +
                   1; // 1 is added because of the bottom border which is inside container
        }

        private static float FadeLineHeight()
        {
            return 15;
        }

        #endregion
    }
#endif
}