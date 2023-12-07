using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;

namespace BXFW.Tweening.Editor
{
    /// <summary>
    /// Draws a fancy selector for the <see cref="EaseType"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(EaseType))]
    public class EaseTypePropertyDrawer : PropertyDrawer
    {
        public class EaseTypeSelectorDropdown : SearchDropdown
        {
            public class Item : SearchDropdownElement
            {
                private const float PlotFieldHeight = 40f;
                private const float PlotFieldWidth = 70f;
                private const float EasePreviewDuration = 0.5f;

                private ElementGUIDrawingState previousGUIState;
                private float currentPreviewElapsed = 0f;
                public readonly EaseType ease;

                public Item(EaseType ease, string label) : base(label)
                {
                    this.ease = ease;
                }
                public Item(EaseType ease, GUIContent content) : base(content)
                {
                    this.ease = ease;
                }
                public Item(EaseType ease, string label, string tooltip) : base(label, tooltip)
                {
                    this.ease = ease;
                }
                public Item(EaseType ease, string label, int childrenCapacity) : base(label, childrenCapacity)
                {
                    this.ease = ease;
                }
                public Item(EaseType ease, GUIContent content, int childrenCapacity) : base(content, childrenCapacity)
                {
                    this.ease = ease;
                }
                public Item(EaseType ease, string label, string tooltip, int childrenCapacity) : base(label, tooltip, childrenCapacity)
                {
                    this.ease = ease;
                }

                public override float GetHeight(float viewWidth)
                {
                    return Mathf.Max(base.GetHeight(viewWidth), PlotFieldHeight);
                }

                /// <summary>
                /// Draws the tween name on the left, a graph preview on the right and when hovered, the background eases in with the same effect.
                /// </summary>
                /// <inheritdoc cref="SearchDropdownElement.OnGUI(Rect, ElementGUIDrawingState)"/>
                public override void OnGUI(Rect position, ElementGUIDrawingState drawingState)
                {
                    // Draw base labels
                    drawingContext.Reset();
                    Rect contextRect = drawingContext.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);

                    Rect iconRect = new Rect(contextRect)
                    {
                        width = EditorGUIUtility.singleLineHeight
                    };
                    Rect textRect = new Rect(contextRect)
                    {
                        x = contextRect.x + EditorGUIUtility.singleLineHeight + 5f,
                        width = contextRect.width - (iconRect.width + EditorGUIUtility.singleLineHeight + PlotFieldWidth)
                    };
                    Rect plottingAreaRect = new Rect(contextRect)
                    {
                        x = textRect.x + textRect.width,
                        height = PlotFieldHeight,
                        width = PlotFieldWidth,
                    };

                    // Elements | Background
                    // Background box tint
                    // Initial background rect
                    EditorGUI.DrawRect(position, new Color(0.2f, 0.2f, 0.2f));

                    Color stateColor = new Color(0.15f, 0.15f, 0.15f);
                    // Update 'currentPreviewElapsed' only in Repaint
                    bool isRepaintEvent = Event.current.type == EventType.Repaint;
                    switch (drawingState)
                    {
                        case ElementGUIDrawingState.Selected:
                            stateColor = new Color(0.15f, 0.35f, 0.39f);
                            if (isRepaintEvent)
                            {
                                currentPreviewElapsed = 1f;
                            }
                            break;

                        case ElementGUIDrawingState.Hover:
                            RequestsRepaint = !Mathf.Approximately(currentPreviewElapsed, 1f);
                            // Reset the hover rect to be zero sized
                            if (isRepaintEvent)
                            {
                                if (previousGUIState == ElementGUIDrawingState.Selected)
                                {
                                    currentPreviewElapsed = 0f;
                                }

                                if (RequestsRepaint)
                                {
                                    currentPreviewElapsed = Mathf.Clamp01(currentPreviewElapsed + (EditorTime.DeltaTime / EasePreviewDuration));
                                }
                            }
                            break;

                        default:
                            RequestsRepaint = !Mathf.Approximately(currentPreviewElapsed, 0f);
                            if (isRepaintEvent && RequestsRepaint)
                            {
                                currentPreviewElapsed = Mathf.Clamp01(currentPreviewElapsed - (EditorTime.DeltaTime / EasePreviewDuration));
                            }
                            break;
                    }

                    // Draw the partial animated rect 
                    EditorGUI.DrawRect(new Rect(position)
                    {
                        width = position.width * Mathf.Clamp01(BXTweenEase.EasedValue(currentPreviewElapsed, ease))
                    }, stateColor);

                    if (content.image != null)
                    {
                        // Icon
                        GUI.DrawTexture(iconRect, content.image, ScaleMode.ScaleToFit);
                    }
                    // Label
                    GUI.Label(textRect, content, SearchDropdownWindow.StyleList.LabelStyle);
                    // Plotting
                    Color gColor = GUI.color;
                    GUI.color = Color.green;
                    GUIAdditionals.PlotLine(plottingAreaRect, (float t) => BXTweenEase.EasedValue(t, ease), true, false, 0f, 1f, 1.5f, 28);
                    GUI.color = gColor;

                    if (isRepaintEvent)
                    {
                        previousGUIState = drawingState;
                    }
                }
            }

            public readonly EaseType selectedEase = EaseType.Linear;
            protected internal override StringComparison SearchComparison => StringComparison.OrdinalIgnoreCase;

            protected override SearchDropdownElement BuildRoot()
            {
                SearchDropdownElement rootElement = new SearchDropdownElement("Ease List");

                foreach (EaseType ease in Enum.GetValues(typeof(EaseType)).Cast<EaseType>())
                {
                    Item easeItem = new Item(ease, ease.ToString())
                    {
                        Selected = selectedEase == ease
                    };

                    rootElement.Add(easeItem);
                }

                return rootElement;
            }

            public EaseTypeSelectorDropdown(EaseType selected)
            {
                selectedEase = selected;
            }
        }

        private readonly PropertyRectContext mainCtx = new PropertyRectContext();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + mainCtx.Padding;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();

            label = EditorGUI.BeginProperty(position, label, property);

            Rect paddedPosition = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            Rect labelPosition = new Rect(paddedPosition)
            {
                width = EditorGUIUtility.labelWidth,
            };
            Rect dropdownSelectorPosition = new Rect(paddedPosition)
            {
                x = paddedPosition.x + labelPosition.width,
                width = Mathf.Max(paddedPosition.width - labelPosition.width, EditorGUIUtility.fieldWidth)
            };

            EaseType selectedValue = (EaseType)property.longValue;
            bool prevShowMixed = EditorGUI.showMixedValue;

            EditorGUI.LabelField(labelPosition, label);
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            if (EditorGUI.DropdownButton(dropdownSelectorPosition, new GUIContent(ObjectNames.NicifyVariableName(selectedValue.ToString()), label.tooltip), FocusType.Keyboard))
            {
                EaseTypeSelectorDropdown selectorDropdown = new EaseTypeSelectorDropdown(selectedValue);
                selectorDropdown.Show(dropdownSelectorPosition);

                SerializedObject copySo = new SerializedObject(property.serializedObject.targetObjects);
                SerializedProperty copySetProperty = copySo.FindProperty(property.propertyPath);

                selectorDropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
                {
                    if (!(element is EaseTypeSelectorDropdown.Item item))
                    {
                        return;
                    }

                    copySetProperty.longValue = (long)item.ease;

                    copySo.ApplyModifiedProperties();
                    // --
                    copySo.Dispose();
                    copySetProperty.Dispose();
                };
                selectorDropdown.OnDiscardEvent += () =>
                {
                    copySo.Dispose();
                    copySetProperty.Dispose();
                };
            }
            EditorGUI.showMixedValue = prevShowMixed;

            EditorGUI.EndProperty();
        }
    }
}
