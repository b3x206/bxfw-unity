using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using BXFW.Collections;
using System.Linq;
using System.Reflection;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(CameraCapture)), CanEditMultipleObjects]
    public class CameraCaptureEditor : Editor
    {
        // SerializableDictionary ordering is NOT undefined behaviour, so it's the way that the elements are added.
        /// <summary>
        /// Defines a list of capturing presets.
        /// </summary>
        private static readonly SerializableDictionary<string, Vector2Int> m_Presets = new SerializableDictionary<string, Vector2Int>
        {
            { "720p 16:9 Landspace", new Vector2Int(1280, 720) },
            { "720p 16:9 Portrait", new Vector2Int(720, 1280) },
            { "1080p 16:9 Landspace", new Vector2Int(1920, 1080) },
            { "1080p 16:9 Portrait", new Vector2Int(1080, 1920) },
            { "1440p 16:9 Landspace", new Vector2Int(2560, 1440) },
            { "1440p 16:9 Portrait", new Vector2Int(1440, 2560) },
            { "2160p 16:9 Landspace", new Vector2Int(3840, 2160) },
            { "2160p 16:9 Portrait", new Vector2Int(2160, 3840) },
            { "4320p 16:9 Landspace", new Vector2Int(7680, 4320) },
            { "4320p 16:9 Portrait", new Vector2Int(4320, 7680) },
        };

        private Rect lastDropdownRepaintRect = Rect.zero;
        public override void OnInspectorGUI()
        {
            var target = base.target as CameraCapture;
            var targets = base.targets.Cast<CameraCapture>().ToArray();

            serializedObject.DrawCustomDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(CameraCapture.screenshotResolution), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After,
                    () =>
                    {
                        EditorGUI.indentLevel++;

                        // Draw the preset selector
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Select Resoulution Preset", GUILayout.Width(EditorGUIUtility.labelWidth));

                        bool prevShowMixed = EditorGUI.showMixedValue;
                        EditorGUI.showMixedValue = targets.Any(t => t.screenshotResolution != target.screenshotResolution);
                        KeyValuePair<string, Vector2Int> currentPickedPreset = m_Presets.FirstOrDefault(p => p.Value == target.screenshotResolution);

                        if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(currentPickedPreset.Key) ? "Custom" : currentPickedPreset.Key), FocusType.Keyboard))
                        {
                            GenericMenu presetsSelectionMenu = new GenericMenu();
                            foreach (KeyValuePair<string, Vector2Int> preset in m_Presets)
                            {
                                presetsSelectionMenu.AddItem(new GUIContent(preset.Key), !EditorGUI.showMixedValue && preset.Value == currentPickedPreset.Value, () =>
                                {
                                    Undo.IncrementCurrentGroup();
                                    Undo.SetCurrentGroupName("set CameraCapture preset");
                                    int undoGroup = Undo.GetCurrentGroup();

                                    foreach (CameraCapture capture in targets)
                                    {
                                        Undo.RecordObject(capture, string.Empty);
                                        capture.screenshotResolution = preset.Value;
                                    }

                                    Undo.CollapseUndoOperations(undoGroup);
                                });
                            }

                            if (lastDropdownRepaintRect != Rect.zero)
                            {
                                presetsSelectionMenu.DropDown(lastDropdownRepaintRect);
                            }
                            else
                            {
                                presetsSelectionMenu.ShowAsContext();
                            }
                        }
                        if (Event.current.type == EventType.Repaint)
                        {
                            lastDropdownRepaintRect = GUILayoutUtility.GetLastRect();
                        }
                        EditorGUI.showMixedValue = prevShowMixed;

                        GUILayout.EndHorizontal();

                        EditorGUI.indentLevel--;
                    })
                },
                { nameof(CameraCapture.captureKey), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After,
                    () =>
                    {
                        if (targets.Length <= 1)
                        {
                            if (GUILayout.Button("Capture"))
                            {
                                target.TakeCameraShot();
                            }
                        }
                    })
                },
                { $"m_{nameof(CameraCapture.CaptureSuperSamplingFactor)}", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After,
                    () =>
                    {
                        if (targets.Length <= 1)
                        {
                            float screenWidth = Screen.width;
                            float screenHeight = Screen.height;

                            Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView", false);
                            if (gameViewType != null)
                            {
                                EditorWindow gameViewObject = EditorWindow.GetWindow(gameViewType);
                                
                                // internal Rect targetRenderSize { get; }
                                PropertyInfo targetRenderSizeProperty = gameViewType.GetProperty("targetRenderSize", BindingFlags.NonPublic | BindingFlags.Instance);
                                Vector2 targetRenderSizeRect = (Vector2)targetRenderSizeProperty.GetValue(gameViewObject);

                                screenWidth = targetRenderSizeRect.x;
                                screenHeight = targetRenderSizeRect.y;
                            }

                            if (GUILayout.Button(new GUIContent($"Screenshot ({screenWidth * target.CaptureSuperSamplingFactor}x{screenHeight * target.CaptureSuperSamplingFactor})", "Takes a screenshot using ScreenCapture.CaptureScreenshot.")))
                            {
                                target.TakeScreenShot();
                            }
                        }
                    })
                }
            });
        }
    }
}