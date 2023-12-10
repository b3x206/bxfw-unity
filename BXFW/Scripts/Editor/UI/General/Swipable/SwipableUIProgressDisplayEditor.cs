using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BXFW.UI;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(SwipableUIProgressDisplay)), CanEditMultipleObjects]
    internal class SwipableUIProgressDisplayEditor : Editor
    {
        private static bool TargetFadeTypeIsColorFade(SwipableUIProgressDisplay p)
        {
            return p.ChildImageFadeType == FadeType.None || p.ChildImageFadeType == FadeType.ColorFade || p.ChildImageFadeType == FadeType.CustomUnityEvent;
        }

        public override void OnInspectorGUI()
        {
            var targets = base.targets.Cast<SwipableUIProgressDisplay>().ToArray();

            foreach (var target in targets)
            {
                target.GenerateChildImage();
            }

            var dictDraw = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(SwipableUIProgressDisplay.TargetSwipableUI), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
                    {
                        if (targets.Any(supd => supd.TargetSwipableUI != null))
                        {
                            // Add a button to go to the target swipable ui
                            if (GUILayout.Button(new GUIContent(targets.Length > 1 ? "Show target displays" : "Go to target display", "Makes the focus TargetSwipableUI.")))
                            {
                                if (targets.Length > 1)
                                {
                                    SwipableUIProgressDisplay[] nonNullUI = targets.Where(t => t.TargetSwipableUI != null).ToArray();
                                    Selection.objects = nonNullUI;
                                }
                                else
                                {
                                    Selection.activeTransform = targets[0].TargetSwipableUI.transform;
                                    SceneView.FrameLastActiveSceneView(); // Frames the scene view to active transform
                                }
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
                        if (targets.Any(t => t.ChildImageFadeType == FadeType.None || t.ChildImageFadeType == FadeType.CustomUnityEvent))
                        {
                            EditorGUILayout.HelpBox("FadeType : None & CustomUnityEvent is not supported.", MessageType.Warning);
                        }
                    })
                }
            };

            if (targets.All(t => t.ChildImageFadeType == FadeType.SpriteSwap))
            {
                dictDraw.Add(nameof(SwipableUIProgressDisplay.ChildImageColorFadeTween), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dictDraw.Add(nameof(SwipableUIProgressDisplay.ActiveColor), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dictDraw.Add(nameof(SwipableUIProgressDisplay.DisabledColor), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }
            else if (targets.All(t => TargetFadeTypeIsColorFade(t)))
            {
                dictDraw.Add(nameof(SwipableUIProgressDisplay.ActiveSprite), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dictDraw.Add(nameof(SwipableUIProgressDisplay.DisabledSprite), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                if (targets.Any(t => !t.ChangeColorWithTween))
                {
                    dictDraw.Add(nameof(SwipableUIProgressDisplay.ChildImageColorFadeTween), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                }
            }

            serializedObject.DrawCustomDefaultInspector(dictDraw);
        }
    }
}