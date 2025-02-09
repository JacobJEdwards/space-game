using System;
using DG.Tweening;
using Microlight.MicroEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Microlight.MicroBar
{
#if UNITY_EDITOR
    // ****************************************************************************************************
    // Property drawer for the MicroBarAnimation class
    // ****************************************************************************************************
    [CustomPropertyDrawer(typeof(MicroBarAnimation))]
    public class MicroBarAnimation_Drawer : PropertyDrawer
    {
        public static float GetHeight(SerializedProperty property)
        {
            float totalHeight = 0;

            totalHeight += HeaderHeight(property); // Header
            if (property.isExpanded)
            {
                totalHeight += MicroEditor_Utility.DefaultFieldHeight; // Anim
                totalHeight += MicroEditor_Utility.DefaultFieldHeight; // Render type
                totalHeight += MicroEditor_Utility.DefaultFieldHeight; // Target
                totalHeight += NotBarHeight(property); // Not Bar
                totalHeight += CanvasWarningHeight(property); // Canvas warning
                totalHeight += SpriteMaskWarningHeight(property); // Sprite mask structure
                totalHeight += SelectionErrorHeight(property); // Error for using unsupported values
                totalHeight += FillWarningHeight(property); // Warning for no fill command
                totalHeight += CommandsHeight(property); // Commands
                totalHeight += AddButtonHeight(property); // Add button
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = new Rect(position.x + 10, position.y, position.width - 16,
                position.height); // Prepare position for the drawing of the property
            EditorGUI.BeginProperty(position, label, property);

            position = DrawHeader(position, property);
            position = DrawAnimField(position, property);
            position = DrawRenderField(position, property);
            position = DrawTargetField(position, property);
            position = DrawNotBarField(position, property);

            position = DrawSpriteMaskWarning(position, property); // Warning for not having SortingLayer and SpriteMask
            position = DrawCanvasWarning(position,
                property); // Warning for not having image inside canvas or having sprite inside canvas
            position = DrawSelectionError(position, property); // Displays error if uncompatible options are selected
            position = DrawFillWarning(position,
                property); // Displays warning if animation has not bar disabled and no fill animation command

            position = DrawCommands(position, property);
            position = DrawAddButton(position, property);

            EditorGUI.EndProperty();
        }

        #region Building blocks

        private Rect DrawHeader(Rect position, SerializedProperty property)
        {
            // Prepare rects to draw background container and foldout
            var headerRect = new Rect(
                position.x,
                position.y,
                position.width -
                MicroEditor_Utility
                    .HeaderLineHeight, // This is here so click action doesnt interfere with the remove button
                MicroEditor_Utility.HeaderLineHeight);
            // This rect is a bit to the left, because it wants to have foldout arrow inside
            var headerRectWithArrow = new Rect(
                position.x - EditorGUIUtility.singleLineHeight,
                position.y,
                position.width + EditorGUIUtility.singleLineHeight,
                MicroEditor_Utility.HeaderLineHeight);
            var wholeProperty = new Rect(
                position.x - EditorGUIUtility.singleLineHeight,
                position.y,
                position.width + EditorGUIUtility.singleLineHeight,
                position.height);

            if (property.isExpanded)
                // If property is expanded, we want to draw default color and leave coloring to the header drawing
                MicroEditor_DrawUtility.DrawContainer(wholeProperty);
            else
                MicroEditor_DrawUtility.DrawContainer(wholeProperty, DecideHeaderColor(property));

            // Add mouse interaction and draw hover glow 
            EditorGUIUtility.AddCursorRect(headerRectWithArrow, MouseCursor.Link);
            //if(Event.current.type == EventType.MouseMove && headerRectWithArrow.Contains(Event.current.mousePosition)) {
            if (headerRectWithArrow.Contains(Event.current.mousePosition) ||
                property.isExpanded)
            {
                MicroEditor_DrawUtility.DrawContainer(headerRectWithArrow,
                    DecideHeaderColor(property)); // Dont want glow effect
                if (property.isExpanded) MicroEditor_DrawUtility.DrawContainerBottomOutline(headerRectWithArrow);
            }

            property.isExpanded =
                EditorGUI.Foldout(headerRect, property.isExpanded, new GUIContent(HeaderName(property)), true);

            return new Rect(
                position.x - EditorGUIUtility.singleLineHeight + 5,
                position.y + headerRect.height + MicroEditor_Utility.VerticalSpacing,
                position.width + 18 - 10,
                position.height);
        }

        private Rect DrawAnimField(Rect position, SerializedProperty property)
        {
            if (property.isExpanded)
            {
                var animTypeProperty = property.FindPropertyRelative("animationType");
                return MicroEditor_DrawUtility.DrawProperty(position, animTypeProperty,
                    new GUIContent("Update animation"));
            }

            return position;
        }

        private Rect DrawRenderField(Rect position, SerializedProperty property)
        {
            if (property.isExpanded)
            {
                var renderTypeProperty = property.FindPropertyRelative("renderType");
                return MicroEditor_DrawUtility.DrawProperty(position, renderTypeProperty,
                    new GUIContent("Render type"));
            }

            return position;
        }

        private Rect DrawTargetField(Rect position, SerializedProperty property)
        {
            if (property.isExpanded)
            {
                var renderTypeProperty = property.FindPropertyRelative("renderType");
                if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
                {
                    var targetImageProperty = property.FindPropertyRelative("targetImage");
                    return MicroEditor_DrawUtility.DrawProperty(position, targetImageProperty,
                        new GUIContent("Target Image"));
                }

                if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Sprite)
                {
                    var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                    return MicroEditor_DrawUtility.DrawProperty(position, targetSpriteProperty,
                        new GUIContent("Target Sprite"));
                }
            }

            return position;
        }

        private Rect DrawNotBarField(Rect position, SerializedProperty property)
        {
            if (!property.isExpanded) return position;

            var renderTypeProperty = property.FindPropertyRelative("renderType");
            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
            {
                var targetImageProperty = property.FindPropertyRelative("targetImage");
                if (targetImageProperty.objectReferenceValue == null) return position;

                var image = (Image)targetImageProperty.objectReferenceValue;
                //SerializedProperty imageTypeProperty = targetImageProperty.FindPropertyRelative("type");
                if (image.type != Image.Type.Filled) return position;
            }
            else if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Sprite)
            {
                var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                if (targetSpriteProperty.objectReferenceValue == null) return position;
            }

            var notBarProperty = property.FindPropertyRelative("notBar");
            return MicroEditor_DrawUtility.DrawProperty(position, notBarProperty,
                new GUIContent("NOT Bar",
                    "Enable this when image DOES NOT represent health bar. For more info look into documentation on why this is important."));
        }

        private Rect DrawSelectionError(Rect position, SerializedProperty property)
        {
            if (!property.isExpanded) return position;

            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");
            var commandsProperty = property.FindPropertyRelative("commands");

            // Image render type doesn't have these problems
            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image) return position;

            // Commands
            var arraySize = commandsProperty.arraySize;
            if (arraySize > 0)
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    var effectProperty = elementProperty.FindPropertyRelative("effect");

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Fill)
                        if (notBarProperty.boolValue)
                            return DrawFillError();

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.AnchorMove) return DrawAnchorError();
                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Punch)
                    {
                        var transformProperty = elementProperty.FindPropertyRelative("transformProperty");
                        if ((TransformProperties)transformProperty.enumValueIndex == TransformProperties.AnchorPosition)
                            return DrawAnchorError();
                    }

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Scale)
                    {
                        var transformProperty = elementProperty.FindPropertyRelative("transformProperty");
                        if ((TransformProperties)transformProperty.enumValueIndex == TransformProperties.AnchorPosition)
                            return DrawAnchorError();
                    }
                }

            Rect DrawFillError()
            {
                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 2);
                EditorGUI.HelpBox(elementRect, "Sprites which are not bars, don't support Fill funcationalities",
                    MessageType.Error);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }

            Rect DrawAnchorError()
            {
                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 2);
                EditorGUI.HelpBox(elementRect, "Sprites don't support Anchored Position funcationalities",
                    MessageType.Error);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }

            return position;
        }

        private Rect DrawFillWarning(Rect position, SerializedProperty property)
        {
            if (!property.isExpanded) return position;

            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");
            var commandsProperty = property.FindPropertyRelative("commands");

            // Not Bar, when not bar is turned on, means it doesnt need to have fill, if it doesnt want to
            if (notBarProperty.boolValue)
                return position;

            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
            {
                var targetImageProperty = property.FindPropertyRelative("targetImage");
                if (targetImageProperty.objectReferenceValue == null) return position;
                var image = (Image)targetImageProperty.objectReferenceValue;
                if (image.type != Image.Type.Filled) return position;
            }
            else
            {
                var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                if (targetSpriteProperty.objectReferenceValue == null) return position;
            }

            // Commands
            var arraySize = commandsProperty.arraySize;
            if (arraySize > 0)
            {
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    var effectProperty = elementProperty.FindPropertyRelative("effect");

                    if ((AnimEffect)effectProperty.enumValueIndex ==
                        AnimEffect.Fill) return position; // If we have found fill command, leave
                }

                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 2);
                EditorGUI.HelpBox(elementRect,
                    "Your animation commands don't have a fill command. Is that intended behaviour?",
                    MessageType.Warning);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }

            return position;
        }

        private Rect DrawSpriteMaskWarning(Rect position, SerializedProperty property)
        {
            if (!property.isExpanded) return position;

            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");

            // Not Bar, when not bar is turned on, means it doesnt need to have fill, if it doesnt want to
            if (notBarProperty.boolValue) return position;

            if ((RenderType)renderTypeProperty.enumValueIndex ==
                RenderType.Image) return position; // If its image, we don't need masks

            var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
            if (targetSpriteProperty.objectReferenceValue == null) return position;

            var spriteRenderer = (SpriteRenderer)targetSpriteProperty.objectReferenceValue;
            if (spriteRenderer == null) return position; // This should never happen but okay

            var spriteGroup = spriteRenderer.GetComponent<SortingGroup>();
            if (spriteGroup == null) position = DrawSortingGroupWarning(spriteRenderer.gameObject.name);

            var spriteMask = spriteRenderer.GetComponentInChildren<SpriteMask>();
            if (spriteMask == null) position = DrawSpriteMaskWarning(spriteRenderer.gameObject.name);

            return position;

            Rect DrawSortingGroupWarning(string gameObjectName)
            {
                var elementRect = new Rect(
                    position.x,
                    position.y + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    MicroEditor_Utility.LineHeight * 3);
                EditorGUI.HelpBox(elementRect,
                    $"'Target Sprite' should have 'SortingGroup' component, '{gameObjectName}' doesn't have it." +
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
                    $"'Target Sprite' should have 'SpriteMask' child, '{gameObjectName}' doesn't have it." +
                    "Check documentation under 'Render type' for more info",
                    MessageType.Warning);

                return new Rect(
                    position.x,
                    position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing * 3,
                    position.width,
                    position.height);
            }
        }

        private Rect DrawCommands(Rect position, SerializedProperty property)
        {
            if (property.isExpanded)
            {
                var commandsProperty = property.FindPropertyRelative("commands");
                var arraySize = commandsProperty.arraySize;
                if (arraySize <= 0) return position; // Don't draw anything if empty

                // Draw sort label
                var labelRect = new Rect(position.x + 5, position.y, position.width, MicroEditor_Utility.LineHeight);
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = MicroEditor_Utility.DarkFontColor; // Change color to whatever you want
                EditorGUI.LabelField(labelRect, new GUIContent("Sort"), style);
                position.y += MicroEditor_Utility.LineHeight;

                // Draw
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    var elementRect = new Rect(
                        position.x + 5,
                        position.y,
                        position.width - 10,
                        AnimCommand_Drawer.GetHeight(elementProperty));
                    position = new Rect(
                        position.x,
                        position.y + elementRect.height + MicroEditor_Utility.VerticalSpacing,
                        position.width,
                        position.height);
                    EditorGUI.PropertyField(elementRect, elementProperty, GUIContent.none);

                    // Draw the remove button
                    GUI.backgroundColor = MicroBar_Theme.RemoveButtonColor;
                    if (GUI.Button(AnimCommand_Drawer.RemoveButtonRect(elementRect), "-"))
                    {
                        RemoveItem(commandsProperty, i);
                        arraySize = commandsProperty.arraySize;
                        i--;
                    }

                    GUI.backgroundColor = Color.white;

                    // Draw up and down buttons
                    if (i != 0)
                        if (GUI.Button(AnimCommand_Drawer.UpButtonRect(elementRect), "+"))
                            SwitchListOrder(commandsProperty, i, i - 1);

                    if (i != arraySize - 1)
                        if (GUI.Button(AnimCommand_Drawer.DownButtonRect(elementRect), "-"))
                            SwitchListOrder(commandsProperty, i, i + 1);
                }
            }

            return position;
        }

        private Rect DrawAddButton(Rect position, SerializedProperty property)
        {
            if (property.isExpanded)
            {
                var commandsProperty = property.FindPropertyRelative("commands");

                // Draw the button
                var buttonPosition = new Rect(
                    position.x + 5,
                    position.y,
                    position.width - 10,
                    MicroEditor_Utility.HeaderLineHeight);
                GUI.backgroundColor = MicroBar_Theme.AddButtonColor;
                if (GUI.Button(buttonPosition, "Add Command")) AddNewItem(commandsProperty);
                GUI.backgroundColor = Color.white;
                position = new Rect(
                    position.x,
                    position.y + buttonPosition.height + MicroEditor_Utility.VerticalSpacing,
                    position.width,
                    position.height);
            }

            return position;
        }

        private Rect DrawCanvasWarning(Rect position, SerializedProperty property)
        {
            var hasCanvas = HasCanvasParent(property);
            var needsCanvas = property.FindPropertyRelative("renderType").enumValueIndex == (int)RenderType.Image;

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

        #endregion

        #region Heights

        private static float HeaderHeight(SerializedProperty property)
        {
            float height = MicroEditor_Utility.HeaderLineHeight;
            if (property.isExpanded)
            {
                height += MicroEditor_Utility.VerticalSpacing;
                height += 1; // Because of the bottom border which is inside container
            }

            return height;
        }

        private static float NotBarHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;

            var renderTypeProperty = property.FindPropertyRelative("renderType");
            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
            {
                var targetImageProperty = property.FindPropertyRelative("targetImage");
                if (targetImageProperty.objectReferenceValue == null) return 0f;

                var image = (Image)targetImageProperty.objectReferenceValue;
                //SerializedProperty imageTypeProperty = targetImageProperty.FindPropertyRelative("type");
                if (image.type != Image.Type.Filled) return 0f;
            }
            else if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Sprite)
            {
                var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                if (targetSpriteProperty.objectReferenceValue == null) return 0f;
            }

            return MicroEditor_Utility.DefaultFieldHeight;
        }

        private static float SelectionErrorHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;

            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");
            var commandsProperty = property.FindPropertyRelative("commands");

            // Image render type doesn't have these problems
            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image) return 0f;

            // Commands
            var arraySize = commandsProperty.arraySize;
            if (arraySize > 0)
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    var effectProperty = elementProperty.FindPropertyRelative("effect");

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Fill)
                        if (notBarProperty.boolValue)
                            return ReturnSpace();

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.AnchorMove) return ReturnSpace();
                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Punch)
                    {
                        var transformProperty = elementProperty.FindPropertyRelative("transformProperty");
                        if ((TransformProperties)transformProperty.enumValueIndex == TransformProperties.AnchorPosition)
                            return ReturnSpace();
                    }

                    if ((AnimEffect)effectProperty.enumValueIndex == AnimEffect.Scale)
                    {
                        var transformProperty = elementProperty.FindPropertyRelative("transformProperty");
                        if ((TransformProperties)transformProperty.enumValueIndex == TransformProperties.AnchorPosition)
                            return ReturnSpace();
                    }
                }

            float ReturnSpace()
            {
                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;
            }

            return 0f;
        }

        private static float FillWarningHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;
            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");
            var commandsProperty = property.FindPropertyRelative("commands");

            // Not bar
            if (notBarProperty.boolValue)
                return 0f;

            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
            {
                var targetImageProperty = property.FindPropertyRelative("targetImage");
                if (targetImageProperty.objectReferenceValue == null) return 0f;
                var image = (Image)targetImageProperty.objectReferenceValue;
                if (image.type != Image.Type.Filled) return 0f;
            }
            else
            {
                var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                if (targetSpriteProperty.objectReferenceValue == null) return 0f;
            }

            // Commands
            var arraySize = commandsProperty.arraySize;
            if (arraySize > 0)
            {
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    var effectProperty = elementProperty.FindPropertyRelative("effect");

                    if ((AnimEffect)effectProperty.enumValueIndex ==
                        AnimEffect.Fill) return 0f; // If we have found fill command, leave
                }

                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;
            }

            return 0f;
        }

        private static float SpriteMaskWarningHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;

            var height = 0f;
            var notBarProperty = property.FindPropertyRelative("notBar");
            var renderTypeProperty = property.FindPropertyRelative("renderType");

            // Not Bar, when not bar is turned on, means it doesnt need to have fill, if it doesnt want to
            if (notBarProperty.boolValue) return 0f;

            if ((RenderType)renderTypeProperty.enumValueIndex ==
                RenderType.Image) return 0f; // If its image, we don't need masks

            var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
            if (targetSpriteProperty.objectReferenceValue == null) return 0f;

            var spriteRenderer = (SpriteRenderer)targetSpriteProperty.objectReferenceValue;
            if (spriteRenderer == null) return 0f; // This should never happen but okay

            var spriteGroup = spriteRenderer.GetComponent<SortingGroup>();
            if (spriteGroup == null) height += ReturnSpace();

            var spriteMask = spriteRenderer.GetComponentInChildren<SpriteMask>();
            if (spriteMask == null) height += ReturnSpace();

            return height;

            float ReturnSpace()
            {
                return MicroEditor_Utility.LineHeight * 3 + MicroEditor_Utility.VerticalSpacing * 3;
            }
        }

        private static float CommandsHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;

            float height = 0;
            var commandsProperty = property.FindPropertyRelative("commands");
            var arraySize = commandsProperty.arraySize;

            // Calculate
            if (arraySize > 0)
            {
                height = MicroEditor_Utility.LineHeight; // Sort label, displayed only when there are commands
                for (var i = 0; i < arraySize; i++)
                {
                    var elementProperty = commandsProperty.GetArrayElementAtIndex(i);
                    height += AnimCommand_Drawer.GetHeight(elementProperty) + MicroEditor_Utility.VerticalSpacing;
                }
            }

            return height;
        }

        private static float AddButtonHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return 0f;
            return MicroEditor_Utility.HeaderLineHeight + MicroEditor_Utility.VerticalSpacing;
        }

        private static float CanvasWarningHeight(SerializedProperty property)
        {
            var hasCanvas = HasCanvasParent(property);
            var needsCanvas = property.FindPropertyRelative("renderType").enumValueIndex == (int)RenderType.Image;

            if (!hasCanvas && needsCanvas)
                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;

            if (hasCanvas && !needsCanvas)
                return MicroEditor_Utility.LineHeight * 2 + MicroEditor_Utility.VerticalSpacing * 3;

            return 0f;
        }

        #endregion

        #region Utilities

        private static string HeaderName(SerializedProperty property)
        {
            var animationTypeProperty = property.FindPropertyRelative("animationType");
            var prefix = Enum.GetName(typeof(UpdateAnim), animationTypeProperty.enumValueIndex);
            var suffix = "'null'";

            var renderTypeProperty = property.FindPropertyRelative("renderType");
            if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Image)
            {
                var targetImageProperty = property.FindPropertyRelative("targetImage");
                if (targetImageProperty.objectReferenceValue != null)
                {
                    var image = (Image)targetImageProperty.objectReferenceValue;
                    suffix = image.name;
                }
            }
            else if ((RenderType)renderTypeProperty.enumValueIndex == RenderType.Sprite)
            {
                var targetSpriteProperty = property.FindPropertyRelative("targetSprite");
                if (targetSpriteProperty.objectReferenceValue != null)
                {
                    var sprite = (SpriteRenderer)targetSpriteProperty.objectReferenceValue;
                    suffix = sprite.name;
                }
            }

            return prefix + " animation for " + suffix;
        }

        // For adding new animation commands
        private static void AddNewItem(SerializedProperty commandsProperty)
        {
            commandsProperty.arraySize++;
            var newElement = commandsProperty.GetArrayElementAtIndex(commandsProperty.arraySize - 1);
            ResetCommandState(newElement);
        }

        private static void RemoveItem(SerializedProperty commandsProperty, int index)
        {
            commandsProperty.DeleteArrayElementAtIndex(index);
            commandsProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void ResetCommandState(SerializedProperty commandProperty)
        {
            commandProperty.FindPropertyRelative("execution").enumValueIndex = (int)AnimExecution.Sequence;
            commandProperty.FindPropertyRelative("effect").enumValueIndex = (int)AnimEffect.Fill;
            commandProperty.FindPropertyRelative("duration").floatValue = 0f;
            commandProperty.FindPropertyRelative("delay").floatValue = 0f;

            // Values
            commandProperty.FindPropertyRelative("valueMode").enumValueIndex = (int)ValueMode.Absolute;
            commandProperty.FindPropertyRelative("floatValue").floatValue = (int)AnimEffect.Scale;
            commandProperty.FindPropertyRelative("intValue").intValue = 0;
            commandProperty.FindPropertyRelative("frequency").intValue = 10;
            commandProperty.FindPropertyRelative("boolValue").boolValue = false;
            commandProperty.FindPropertyRelative("vector2Value").vector2Value = new Vector2(0f, 0f);
            commandProperty.FindPropertyRelative("vector3Value").vector3Value = new Vector3(0f, 0f, 0f);
            commandProperty.FindPropertyRelative("colorValue").colorValue = new Color(1f, 1f, 1f, 1f);
            commandProperty.FindPropertyRelative("percentValue").floatValue = 1f;

            // Additional settings
            commandProperty.FindPropertyRelative("ease").enumValueIndex = (int)Ease.Linear;
            commandProperty.FindPropertyRelative("animAxis").enumValueIndex = (int)AnimAxis.Uniform;
            commandProperty.FindPropertyRelative("transformProperty").enumValueIndex =
                (int)TransformProperties.Position;
        }

        private static void SwitchListOrder(SerializedProperty commandsProperty, int indexA, int indexB)
        {
            // Check if the indices are valid
            if (indexA < 0 || indexA >= commandsProperty.arraySize || indexB < 0 ||
                indexB >= commandsProperty.arraySize)
            {
                Debug.LogWarning("Invalid indices for switching order.");
                return;
            }

            // Move the element at indexA to indexB
            commandsProperty.MoveArrayElement(indexA, indexB);
        }

        private static Color DecideHeaderColor(SerializedProperty property)
        {
            var animTypeProperty = property.FindPropertyRelative("animationType");
            switch ((UpdateAnim)animTypeProperty.enumValueIndex)
            {
                case UpdateAnim.Damage:
                    return MicroBar_Theme.DamageAnimColorMultiplier;
                case UpdateAnim.Heal:
                    return MicroBar_Theme.HealAnimColorMultiplier;
                case UpdateAnim.CriticalDamage:
                    return MicroBar_Theme.DamageAnimColorMultiplier;
                case UpdateAnim.CriticalHeal:
                    return MicroBar_Theme.HealAnimColorMultiplier;
                case UpdateAnim.Armor:
                    return MicroBar_Theme.ArmorAnimColorMultiplier;
                case UpdateAnim.DOT:
                    return MicroBar_Theme.DOTAnimColorMultiplier;
                case UpdateAnim.HOT:
                    return MicroBar_Theme.HOTAnimColorMultiplier;
                case UpdateAnim.MaxHP:
                    return MicroBar_Theme.MaxHPAnimColorMultiplier;
                case UpdateAnim.Revive:
                    return MicroBar_Theme.ReviveAnimColorMultiplier;
                case UpdateAnim.Death:
                    return MicroBar_Theme.DeathAnimColorMultiplier;
                case UpdateAnim.Custom:
                    return MicroBar_Theme.CustomAnimColorMultiplier;
                default:
                    return new Color(1f, 1f, 1f);
            }
        }

        // Checks if transform has a parent with canvas component
        private static bool HasCanvasParent(SerializedProperty property)
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<Canvas>();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight(property);
        }

        #endregion
    }
#endif
}