﻿using UnityEditor;
using UnityEngine;

using BXFW.UI;
using BXFW.Tools.Editor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(UICustomResizer), true)]
    public class UICustomResizerEditor : Editor
    {
        private static string scriptPath;
        // These files are contained with this editor script.
        // unity moment : Unity doesn't allow us to put custom gui to each 'Toolbar' cell
        // If we could have done that we would use positioning only with a circle & cube outline
        private string TexNameCubeUpperLeft    => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeUpperLeft.png");
        private string TexNameCubeUpperCenter  => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeUpperCenter.png");
        private string TexNameCubeUpperRight   => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeUpperRight.png");

        private string TexNameCubeMiddleLeft   => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeMiddleLeft.png");
        private string TexNameCubeMiddleCenter => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeMiddleCenter.png");
        private string TexNameCubeMiddleRight  => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeMiddleRight.png");
        
        private string TexNameCubeLowerLeft    => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeLowerLeft.png");
        private string TexNameCubeLowerCenter  => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeLowerCenter.png");
        private string TexNameCubeLowerRight   => Path.Combine(scriptPath, "EditorRes/AnchorPrevCubeLowerRight.png");

        private static readonly Texture2D[] texPreviews = new Texture2D[9];
        /// <summary>
        /// Rect transform tracker, used to constraint size delta.
        /// </summary>
        private DrivenRectTransformTracker tracker;

        private void OnEnable()
        {
            var target = base.target as UICustomResizer;

            // The resources file 'EditorRes' is stored in the same path as this script.
            scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            scriptPath = Path.GetDirectoryName(scriptPath); // omit the file name

            if (texPreviews.Any(t => t == null))
            {
                // Load if texPreviews contains null.
                texPreviews[0] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperLeft);
                texPreviews[1] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperCenter);
                texPreviews[2] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeUpperRight);

                texPreviews[3] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleLeft);
                texPreviews[4] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleCenter);
                texPreviews[5] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeMiddleRight);

                texPreviews[6] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerLeft);
                texPreviews[7] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerCenter);
                texPreviews[8] = AssetDatabase.LoadAssetAtPath<Texture2D>(TexNameCubeLowerRight);
            }

            EditorApplication.playModeStateChanged += SetRectTransformTrackerState;

            DrivenTransformProperties flags = DrivenTransformProperties.None;
            if (target.applyX)
            {
                flags |= DrivenTransformProperties.SizeDeltaX;
            }

            if (target.applyY)
            {
                flags |= DrivenTransformProperties.SizeDeltaY;
            }

            tracker.Add(target, target.RectTransform, flags);
        }
        // Disable rect transform tracker (rect transform tracker is buggy and likes to resize the rect transform)
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= SetRectTransformTrackerState;

            tracker.Clear();
        }
        // This method handles an editor bug where the tracker causes the rect to be returned as 0.
        // (this happens because the tracker does something with the size delta when play mode)
        private void SetRectTransformTrackerState(PlayModeStateChange obj)
        {
            var target = base.target as UICustomResizer;

            switch (obj)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    tracker.Clear();
                    DrivenTransformProperties flags = DrivenTransformProperties.None;
                    if (target.applyX)
                    {
                        flags |= DrivenTransformProperties.SizeDeltaX;
                    }

                    if (target.applyY)
                    {
                        flags |= DrivenTransformProperties.SizeDeltaY;
                    }

                    tracker.Add(target, target.RectTransform, flags);
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    tracker.Clear();
                    break;

                default:
                    break;
            }
        }

        private readonly Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> m_drawDict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>(16);
        /// <summary>
        /// Whether if the custom Inspector dictionary needs to be regathered. Setting this flag <see langword="true"/> 
        /// will make the <see cref="GetCustomInspectorDictionary(in Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}}, UICustomResizer)"/> be called.
        /// </summary>
        protected bool DictionaryNeedsRefresh = true;

        protected virtual void GetCustomInspectorDictionary(in Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, UICustomResizer target)
        {
            m_drawDict.Add(nameof(UICustomResizer.alignPivot), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
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
            }));
        }

        public override void OnInspectorGUI()
        {
            var target = base.target as UICustomResizer;

            // Dirty the dict only when needed, for the custom resizer editor there is no need to dirty constantly.
            if (DictionaryNeedsRefresh)
            {
                m_drawDict.Clear();
                GetCustomInspectorDictionary(m_drawDict, target);
                DictionaryNeedsRefresh = false;
            }

            serializedObject.DrawCustomDefaultInspector(m_drawDict);
        }
    }
}
