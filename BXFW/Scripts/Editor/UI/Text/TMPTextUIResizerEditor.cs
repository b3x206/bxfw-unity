using UnityEditor;
using UnityEngine;

using BXFW.UI;
using BXFW.Tools.Editor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TMPTextUIResizer))]
    internal class TMPTextUIResizerEditor : Editor
    {
        private static string scriptPath;
        // These files are contained with this editor script.
        // unity moment : Unity doesn't allow us to put custom gui to each 'Toolbar' cell
        // If we could have done that we would use positioning only with a circle & cube outline
        private string TexNameCubeUpperLeft    => Path.Combine(scriptPath, "AnchorPrevCubeUpperLeft.png");
        private string TexNameCubeUpperCenter  => Path.Combine(scriptPath, "AnchorPrevCubeUpperCenter.png");
        private string TexNameCubeUpperRight   => Path.Combine(scriptPath, "AnchorPrevCubeUpperRight.png");

        private string TexNameCubeMiddleLeft   => Path.Combine(scriptPath, "AnchorPrevCubeMiddleLeft.png");
        private string TexNameCubeMiddleCenter => Path.Combine(scriptPath, "AnchorPrevCubeMiddleCenter.png");
        private string TexNameCubeMiddleRight  => Path.Combine(scriptPath, "AnchorPrevCubeMiddleRight.png");
        
        private string TexNameCubeLowerLeft    => Path.Combine(scriptPath, "AnchorPrevCubeLowerLeft.png");
        private string TexNameCubeLowerCenter  => Path.Combine(scriptPath, "AnchorPrevCubeLowerCenter.png");
        private string TexNameCubeLowerRight   => Path.Combine(scriptPath, "AnchorPrevCubeLowerRight.png");

        private readonly Texture2D[] texPreviews = new Texture2D[9];
        /// <summary>
        /// Rect transform tracker, used to constraint size delta.
        /// </summary>
        private DrivenRectTransformTracker tracker;

        private void OnEnable()
        {
            var target = base.target as TMPTextUIResizer;

            scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            scriptPath = Path.GetDirectoryName(scriptPath);

            texPreviews[0] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperLeft);
            texPreviews[1] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperCenter);
            texPreviews[2] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperRight);

            texPreviews[3] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleLeft);
            texPreviews[4] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleCenter);
            texPreviews[5] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleRight);

            texPreviews[6] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerLeft);
            texPreviews[7] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerCenter);
            texPreviews[8] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerRight);

            EditorApplication.playModeStateChanged += SetRectTransformTrackerState;
            tracker.Add(target, target.RectTransform, DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);
        }
        // Disable tracker.
        private void OnDisable()
        {
            tracker.Clear();
        }
        // This method handles an editor bug where the tracker causes the rect to be returned as 0.
        // (this happens because the tracker does something with the size delta when play mode)
        private void SetRectTransformTrackerState(PlayModeStateChange obj)
        {
            var target = base.target as TMPTextUIResizer;

            switch (obj)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    tracker.Clear();
                    tracker.Add(target, target.RectTransform, DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    tracker.Clear();
                    break;
                default:
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            var target = base.target as TMPTextUIResizer;

            serializedObject.DrawCustomDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(TMPTextUIResizer.alignPivot), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        int scriptPivot = (int)target.alignPivot;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Align Pivot");
                        int currentSelectedPivot = GUILayout.Toolbar(scriptPivot, texPreviews, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f), GUILayout.MaxWidth(410f));
                        GUILayout.EndHorizontal();

                        if (currentSelectedPivot != scriptPivot)
                        {
                            Undo.RecordObject(target, "Inspector");
                            target.alignPivot = (TextAnchor)currentSelectedPivot;
                        }
                    })
                }
            });
        }
    }
}
