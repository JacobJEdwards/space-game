using Microlight.MicroEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Microlight.MicroBar
{
#if UNITY_EDITOR
    // ****************************************************************************************************
    // Property drawer for the SimpleMicroBar class
    // ****************************************************************************************************
    [CustomPropertyDrawer(typeof(SimpleMicroBar))]
    public class SimpleMicroBarDrawer : PropertyDrawer
    {
        public static float GetHeight(SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;

            float totalHeight = 0;

            totalHeight += 20; // Container alignament
            totalHeight += RenderingHeight(property); // Rendering
            totalHeight += SpriteMaskWarningHeight(property);
            totalHeight += CanvasWarningHeight(property);
            totalHeight += FadeLineHeight();
            totalHeight += ColorsHeight(property);
            if (isAnimated)
            {
                totalHeight += FadeLineHeight();
                totalHeight += GhostBarHeight(property); // Ghost bar
                totalHeight += FadeLineHeight();
                totalHeight += DamageAnimationHeight(property); // Bar damage animation
                totalHeight += FadeLineHeight();
                totalHeight += HealAnimationHeight(property); // Bar heal animation
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = new Rect(position.x - 5, position.y + 10, position.width,
                position.height - 10); // Prepare position for the drawing of the property
            EditorGUI.BeginProperty(position, label, property);

            MicroEditor_DrawUtility.DrawContainer(position);
            position = new Rect(position.x + 10, position.y + 5, position.width - 20,
                position.height + 10); // Accomodate the container

            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;

            position = DrawRendering(position, property);
            position = DrawSpriteMaskWarning(position, property);
            position = DrawCanvasWarning(position, property);
            position = DrawFadeLine(position);
            position = DrawColors(position, property);
            if (isAnimated)
            {
                position = DrawFadeLine(position);
                position = DrawGhostBar(position, property);
                position = DrawFadeLine(position);
                position = DrawDamageAnimation(position, property);
                position = DrawFadeLine(position);
                position = DrawHealAnimation(position, property);
            }

            EditorGUI.EndProperty();
        }

        #region Building blocks

        private Rect DrawRendering(Rect position, SerializedProperty property)
        {
            var renderTypeProperty = property.FindPropertyRelative("_renderType");
            var isAnimatedProperty = property.FindPropertyRelative("_isAnimated");
            SerializedProperty backgroundProperty;
            SerializedProperty primaryBarProperty;
            if (renderTypeProperty.enumValueIndex == (int)RenderType.Image)
            {
                backgroundProperty = property.FindPropertyRelative("_uiBackground");
                primaryBarProperty = property.FindPropertyRelative("_uiPrimaryBar");
            }
            else
            {
                backgroundProperty = property.FindPropertyRelative("_srBackground");
                primaryBarProperty = property.FindPropertyRelative("_srPrimaryBar");
            }

            position = MicroEditor_DrawUtility.DrawBoldLabel(position, "Rendering", true);
            position = MicroEditor_DrawUtility.DrawProperty(position, renderTypeProperty,
                new GUIContent("Rendering type"));
            position = MicroEditor_DrawUtility.DrawProperty(position, backgroundProperty, new GUIContent("Background"));
            position = MicroEditor_DrawUtility.DrawProperty(position, primaryBarProperty, new GUIContent("Bar"));
            position = MicroEditor_DrawUtility.DrawProperty(position, isAnimatedProperty, new GUIContent("Animated"));
            return position;
        }

        private Rect DrawColors(Rect position, SerializedProperty property)
        {
            var adaptiveColorProperty = property.FindPropertyRelative("_adaptiveColor");
            var barPrimaryColorProperty = property.FindPropertyRelative("_barPrimaryColor");
            var adaptiveColor = adaptiveColorProperty.boolValue;

            position = MicroEditor_DrawUtility.DrawBoldLabel(position, "Colors", true);
            position = MicroEditor_DrawUtility.DrawProperty(position, adaptiveColorProperty,
                new GUIContent("Adaptive color", "When on, health bar changes color based on % of fill"));
            if (adaptiveColor)
            {
                var barAdaptiveColorProperty = property.FindPropertyRelative("_barAdaptiveColor");

                position = MicroEditor_DrawUtility.DrawProperty(position, barPrimaryColorProperty,
                    new GUIContent("Full color", "Color of the bar when HP is at 100%"));
                position = MicroEditor_DrawUtility.DrawProperty(position, barAdaptiveColorProperty,
                    new GUIContent("Empty color", "Color of the bar when HP is at 0%"));
            }
            else
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, barPrimaryColorProperty,
                    new GUIContent("Bar color"));
            }

            return position;
        }

        private Rect DrawGhostBar(Rect position, SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return position;

            var useGhostBarProperty = property.FindPropertyRelative("_useGhostBar");
            var useGhostBar = useGhostBarProperty.boolValue;

            position = MicroEditor_DrawUtility.DrawBoldLabel(position, "Ghost Bar", true);
            position = MicroEditor_DrawUtility.DrawProperty(position, useGhostBarProperty, new GUIContent("Ghost bar"));
            if (useGhostBar)
            {
                var renderTypeProperty = property.FindPropertyRelative("_renderType");
                var dualGhostBarsProperty = property.FindPropertyRelative("_dualGhostBars");
                var ghostBarDamageColorProperty = property.FindPropertyRelative("_ghostBarDamageColor");
                var ghostBarHealColorProperty = property.FindPropertyRelative("_ghostBarHealColor");
                SerializedProperty ghostBarProperty;
                var dualGhostBars = dualGhostBarsProperty.boolValue;

                // Dual mode
                position = MicroEditor_DrawUtility.DrawProperty(
                    position,
                    dualGhostBarsProperty,
                    new GUIContent("Dual ghost bars",
                        "Differently colored ghost bars based on healing or damaging effect"));

                // Renderer
                if (renderTypeProperty.enumValueIndex == (int)RenderType.Image)
                    ghostBarProperty = property.FindPropertyRelative("_uiGhostBar");
                else
                    ghostBarProperty = property.FindPropertyRelative("_srGhostBar");
                position = MicroEditor_DrawUtility.DrawProperty(position, ghostBarProperty, new GUIContent("Bar"));

                // Colors
                if (dualGhostBars)
                {
                    position = MicroEditor_DrawUtility.DrawProperty(position, ghostBarDamageColorProperty,
                        new GUIContent("Hurt color", "Ghost bar color when getting hurt"));
                    position = MicroEditor_DrawUtility.DrawProperty(position, ghostBarHealColorProperty,
                        new GUIContent("Heal color", "Ghost bar color when getting healed"));
                }
                else
                {
                    position = MicroEditor_DrawUtility.DrawProperty(position, ghostBarDamageColorProperty,
                        new GUIContent("Color"));
                }
            }

            return position;
        }

        private Rect DrawDamageAnimation(Rect position, SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return position;

            var damageAnimProperty = property.FindPropertyRelative("_damageAnim");
            var damageAnimDurationProperty = property.FindPropertyRelative("_damageAnimDuration");
            var damageAnimDelayProperty = property.FindPropertyRelative("_damageAnimDelay");
            var damageFlashColorProperty = property.FindPropertyRelative("_damageFlashColor");
            var damageAnimStrengthProperty = property.FindPropertyRelative("_damageAnimStrength");

            position = MicroEditor_DrawUtility.DrawBoldLabel(position, "Damage animation", true);
            position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimProperty, new GUIContent("Animation"));

            // None doesnt have any of the variables
            if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Fill)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimDelayProperty,
                    new GUIContent("Delay"));
            }
            else if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Flash)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimDelayProperty,
                    new GUIContent("Delay"));
                position = MicroEditor_DrawUtility.DrawProperty(position, damageFlashColorProperty,
                    new GUIContent("Flash color"));
            }
            else if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Shake)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, damageAnimStrengthProperty,
                    new GUIContent("Shake strength"));
            }

            return position;
        }

        private Rect DrawHealAnimation(Rect position, SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return position;

            var healAnimProperty = property.FindPropertyRelative("_healAnim");
            var healAnimDurationProperty = property.FindPropertyRelative("_healAnimDuration");
            var healAnimDelayProperty = property.FindPropertyRelative("_healAnimDelay");
            var healFlashColorProperty = property.FindPropertyRelative("_healFlashColor");
            var healAnimStrengthProperty = property.FindPropertyRelative("_healAnimStrength");

            position = MicroEditor_DrawUtility.DrawBoldLabel(position, "Heal animation", true);
            position = MicroEditor_DrawUtility.DrawProperty(position, healAnimProperty, new GUIContent("Animation"));

            // None doesnt have any of the variables
            if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Fill)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimDelayProperty,
                    new GUIContent("Delay"));
            }
            else if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Flash)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimDelayProperty,
                    new GUIContent("Delay"));
                position = MicroEditor_DrawUtility.DrawProperty(position, healFlashColorProperty,
                    new GUIContent("Flash color"));
            }
            else if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Shake)
            {
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimDurationProperty,
                    new GUIContent("Duration"));
                position = MicroEditor_DrawUtility.DrawProperty(position, healAnimStrengthProperty,
                    new GUIContent("Shake strength"));
            }

            return position;
        }

        private Rect DrawFadeLine(Rect position)
        {
            var fadeRect = new Rect(position.x, position.y + 5, position.width, position.height);
            MicroEditor_DrawUtility.DrawFadeLine(fadeRect);
            return new Rect(position.x, position.y + 13, position.width, position.height);
        }

        private Rect DrawCanvasWarning(Rect position, SerializedProperty property)
        {
            var hasCanvas = HasCanvasParent(property);
            var needsCanvas = property.FindPropertyRelative("_renderType").enumValueIndex == (int)RenderType.Image;

            var message = "";
            if (!hasCanvas && needsCanvas)
                message = "Rendering is set to 'Image' but there is no 'Canvas' parent.";
            else if (hasCanvas && !needsCanvas) message = "Rendering is set to 'Sprite' but there is 'Canvas' parent.";

            if (message == "") return position;

            // Draw warning
            var elementRect = new Rect(
                position.x,
                position.y + MicroEditor_Utility.VerticalSpacing,
                position.width,
                MicroEditor_Utility.LineHeight * 2);
            EditorGUI.HelpBox(elementRect, message, MessageType.Warning);
            return new Rect(
                position.x,
                position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                position.width,
                position.height);
        }

        private Rect DrawSpriteMaskWarning(Rect position, SerializedProperty property)
        {
            var renderTypeProperty = property.FindPropertyRelative("_renderType");
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            var useGhostBar = property.FindPropertyRelative("_useGhostBar").boolValue;

            if ((RenderType)renderTypeProperty.enumValueIndex ==
                RenderType.Image) return position; // If its image, we don't need masks

            // Primary bar
            var primaryBarProperty = property.FindPropertyRelative("_srPrimaryBar");
            if (primaryBarProperty.objectReferenceValue == null) return position;

            var spriteRenderer = (SpriteRenderer)primaryBarProperty.objectReferenceValue;
            if (spriteRenderer == null) return position; // This should never happen but okay

            var spriteGroup = spriteRenderer.GetComponent<SortingGroup>();
            if (spriteGroup == null) position = DrawSortingGroupWarning(spriteRenderer.gameObject.name);

            var spriteMask = spriteRenderer.GetComponentInChildren<SpriteMask>();
            if (spriteMask == null) position = DrawSpriteMaskWarning(spriteRenderer.gameObject.name);

            // Ghost bar
            if (isAnimated && useGhostBar)
            {
                var ghostBarProperty = property.FindPropertyRelative("_srGhostBar");
                if (ghostBarProperty.objectReferenceValue == null) return position;

                var ghostSpriteRenderer = (SpriteRenderer)ghostBarProperty.objectReferenceValue;
                if (ghostSpriteRenderer == null) return position; // This should never happen but okay

                var ghostSpriteGroup = ghostSpriteRenderer.GetComponent<SortingGroup>();
                if (ghostSpriteGroup == null) position = DrawSortingGroupWarning(ghostSpriteRenderer.gameObject.name);

                var ghostSpriteMask = ghostSpriteRenderer.GetComponentInChildren<SpriteMask>();
                if (ghostSpriteMask == null) position = DrawSpriteMaskWarning(ghostSpriteRenderer.gameObject.name);
            }

            return position;

            Rect DrawSortingGroupWarning(string gameObjectName)
            {
                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 3);
                EditorGUI.HelpBox(elementRect,
                    $"'Sprite Renderer' bar should have 'SortingGroup' component, '{gameObjectName}' doesn't have it." +
                    "Check documentation under 'Render type' for more info",
                    MessageType.Warning);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }

            Rect DrawSpriteMaskWarning(string gameObjectName)
            {
                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 3);
                EditorGUI.HelpBox(elementRect,
                    $"'Sprite Renderer' bar should have 'SpriteMask' child, '{gameObjectName}' doesn't have it." +
                    "Check documentation under 'Render type' for more info",
                    MessageType.Warning);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }
        }

        #endregion

        #region Heights

        private static float RenderingHeight(SerializedProperty property)
        {
            return MicroEditor_Utility.DefaultFieldHeight * 5;
        }

        private static float ColorsHeight(SerializedProperty property)
        {
            var adaptiveColor = property.FindPropertyRelative("_adaptiveColor").boolValue;

            float height =
                MicroEditor_Utility.LineHeight * 3 +
                MicroEditor_Utility.VerticalSpacing * 3; // Header label, adaptive color and primary color
            if (adaptiveColor)
                height += MicroEditor_Utility.LineHeight + MicroEditor_Utility.VerticalSpacing; // Secondary color
            return height;
        }

        private static float GhostBarHeight(SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return 0f;

            var useGhostBar = property.FindPropertyRelative("_useGhostBar").boolValue;

            var height = MicroEditor_Utility.DefaultFieldHeight; // Ghost bar label
            height += MicroEditor_Utility.DefaultFieldHeight; // Use ghost bar
            if (useGhostBar)
            {
                var dualGhostBars = property.FindPropertyRelative("_dualGhostBars").boolValue;
                height += MicroEditor_Utility.DefaultFieldHeight; // Dual mode
                height += MicroEditor_Utility.DefaultFieldHeight; // Renderer

                if (dualGhostBars)
                    height += MicroEditor_Utility.DefaultFieldHeight * 2; // Dual mode, dual line
                else
                    height += MicroEditor_Utility.DefaultFieldHeight; // Single color
            }

            return height;
        }

        private static float DamageAnimationHeight(SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return 0f;

            var damageAnimProperty = property.FindPropertyRelative("_damageAnim");

            var height = MicroEditor_Utility.DefaultFieldHeight * 2; // Label and animation

            if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Fill)
                height += MicroEditor_Utility.DefaultFieldHeight * 2; // Duration and delay
            else if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Flash)
                height += MicroEditor_Utility.DefaultFieldHeight * 3; // Duration, delay and flash color
            else if (damageAnimProperty.enumValueIndex == (int)SimpleAnim.Shake)
                height += MicroEditor_Utility.DefaultFieldHeight * 2; // Duration and shake strength
            return height;
        }

        private static float HealAnimationHeight(SerializedProperty property)
        {
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            if (!isAnimated) return 0f;

            var healAnimProperty = property.FindPropertyRelative("_healAnim");

            var height = MicroEditor_Utility.DefaultFieldHeight * 2; // Label and animation

            if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Fill)
                height += MicroEditor_Utility.DefaultFieldHeight * 2; // Duration and delay
            else if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Flash)
                height += MicroEditor_Utility.DefaultFieldHeight * 3; // Duration, delay and flash color
            else if (healAnimProperty.enumValueIndex == (int)SimpleAnim.Shake)
                height += MicroEditor_Utility.DefaultFieldHeight * 2; // Duration and shake strength
            return height;
        }

        private static float FadeLineHeight()
        {
            return 13;
        }

        private static float CanvasWarningHeight(SerializedProperty property)
        {
            var hasCanvas = HasCanvasParent(property);
            var needsCanvas = property.FindPropertyRelative("_renderType").enumValueIndex == (int)RenderType.Image;

            if (!hasCanvas && needsCanvas)
                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;

            if (hasCanvas && !needsCanvas)
                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;

            return 0f;
        }

        private static float SpriteMaskWarningHeight(SerializedProperty property)
        {
            var renderTypeProperty = property.FindPropertyRelative("_renderType");
            var isAnimated = property.FindPropertyRelative("_isAnimated").boolValue;
            var useGhostBar = property.FindPropertyRelative("_useGhostBar").boolValue;
            float height = 0;

            if ((RenderType)renderTypeProperty.enumValueIndex ==
                RenderType.Image) return height; // If its image, we don't need masks

            // Primary bar
            var primaryBarProperty = property.FindPropertyRelative("_srPrimaryBar");
            if (primaryBarProperty.objectReferenceValue == null) return height;

            var spriteRenderer = (SpriteRenderer)primaryBarProperty.objectReferenceValue;
            if (spriteRenderer == null) return height; // This should never happen but okay

            var spriteGroup = spriteRenderer.GetComponent<SortingGroup>();
            if (spriteGroup == null) height += ReturnSpace();

            var spriteMask = spriteRenderer.GetComponentInChildren<SpriteMask>();
            if (spriteMask == null) height += ReturnSpace();

            // Ghost bar
            if (isAnimated && useGhostBar)
            {
                var ghostBarProperty = property.FindPropertyRelative("_srGhostBar");
                if (ghostBarProperty.objectReferenceValue == null) return height;

                var ghostSpriteRenderer = (SpriteRenderer)ghostBarProperty.objectReferenceValue;
                if (ghostSpriteRenderer == null) return height; // This should never happen but okay

                var ghostSpriteGroup = ghostSpriteRenderer.GetComponent<SortingGroup>();
                if (ghostSpriteGroup == null) height += ReturnSpace();

                var ghostSpriteMask = ghostSpriteRenderer.GetComponentInChildren<SpriteMask>();
                if (ghostSpriteMask == null) height += ReturnSpace();
            }

            return height;

            float ReturnSpace()
            {
                return MicroEditor_Utility.LineHeight * 3 + MicroEditor_Utility.VerticalSpacing * 3;
            }
        }

        #endregion

        #region Utilities

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight(property);
        }

        // Checks if transform has a parent with canvas component
        private static bool HasCanvasParent(SerializedProperty property)
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<Canvas>();
        }

        #endregion
    }
#endif
}