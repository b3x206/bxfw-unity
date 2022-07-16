using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BXFW.UI;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(SwipableUIProgressDisplay))]
    internal class SwipableUIProgressDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var target = base.target as SwipableUIProgressDisplay;
            target.GenerateChildImage();

            var dictDraw = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(SwipableUIProgressDisplay.TargetSwipableUI), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
                    {
                        if (target.TargetSwipableUI != null)
                        {
                            // Add a button to go to the target swipable ui
                            if (GUILayout.Button(new GUIContent("Go to target display", "Makes the focus TargetSwipableUI.")))
                            {
                                Selection.activeTransform = target.TargetSwipableUI.transform;
                                SceneView.FrameLastActiveSceneView(); // Frames the scene view to active transform
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Please assign a valid TargetSwipableUI.", MessageType.Warning);
                        }
                    })
                },
                { nameof(SwipableUIProgressDisplay.ChildImageFadeType), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
                    {
                        switch (target.ChildImageFadeType)
                        { 
                            case FadeType.None:
                            case FadeType.CustomUnityEvent:
                                EditorGUILayout.HelpBox("FadeType : None & CustomUnityEvent is not supported.", MessageType.Warning);
                                break;
                        }
                    })
                }
            };

            switch (target.ChildImageFadeType)
            {
                case FadeType.None:
                case FadeType.ColorFade:
                case FadeType.CustomUnityEvent:
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.ActiveSprite), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.DisabledSprite), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    if (!target.ChangeColorWithTween)
                    {
                        dictDraw.Add(nameof(SwipableUIProgressDisplay.ChildImageColorFadeTween), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    }
                    break;
                case FadeType.SpriteSwap:
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.ChildImageColorFadeTween), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.ActiveColor), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.DisabledColor), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    break;
            }

            serializedObject.DrawCustomDefaultInspector(dictDraw);
        }
    }
}