using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using BXFW.UI;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(SwipableUIProgressDisplay)), CanEditMultipleObjects]
    public class SwipableUIProgressDisplayEditor : MultiUIManagerBaseEditor
    {
        protected override void GetCustomPropertyDrawerDictionary(Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, MultiUIManagerBase[] targets)
        {
            base.GetCustomPropertyDrawerDictionary(dict, targets);

            // Omit these built-in UI's
            dict["m_ElementCount"] = OMIT_ACTION;

            // Get targets to be casted IEnumerable
            var castTargets = targets.Cast<SwipableUIProgressDisplay>();

            foreach (var target in castTargets)
            {
                target.UpdateElementsAppearance();
                if (target.targetSwipableUI != null)
                    target.ElementCount = target.targetSwipableUI.MenuCount;
            }

            // Button + Warning for the 'TargetSwipableUI'
            dict.Add(nameof(SwipableUIProgressDisplay.targetSwipableUI), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
            {
                if (castTargets.Any(supd => supd.targetSwipableUI != null))
                {
                    // Add a button to go to the target swipable ui
                    if (GUILayout.Button(new GUIContent(targets.Length > 1 ? "Select target displays" : "Go to target display", "Makes the focus TargetSwipableUI.")))
                    {
                        if (targets.Length > 1)
                        {
                            SwipableUIProgressDisplay[] nonNullUI = castTargets.Where(t => t.targetSwipableUI != null).ToArray();
                            Selection.objects = nonNullUI;
                        }
                        else
                        {
                            Selection.activeTransform = castTargets.First().targetSwipableUI.transform;
                            SceneView.FrameLastActiveSceneView(); // Frames the scene view to active transform
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please assign a TargetSwipableUI.", MessageType.Warning);
                }
            }));
            // Unsupported image fade type helpbox
            dict.Add(nameof(SwipableUIProgressDisplay.childImageFadeType), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
            {
                if (castTargets.Any(t => t.childImageFadeType == FadeType.None || t.childImageFadeType == FadeType.CustomUnityEvent))
                {
                    EditorGUILayout.HelpBox("FadeType : None & CustomUnityEvent is not supported. Will use ColorFade instead.", MessageType.Warning);
                }
            }));

            // Hide/Show Fields
            if (castTargets.All(t => t.childImageFadeType == FadeType.SpriteSwap))
            {
                dict.Add(nameof(SwipableUIProgressDisplay.colorFadeUseTween), OMIT_ACTION);
                dict.Add(nameof(SwipableUIProgressDisplay.colorFadeTween), OMIT_ACTION);
                dict.Add(nameof(SwipableUIProgressDisplay.activeColor), OMIT_ACTION);
                dict.Add(nameof(SwipableUIProgressDisplay.disabledColor), OMIT_ACTION);
            }
            else // Remove sprite swap to display the color fields
            {
                if (castTargets.Any(t => !t.colorFadeUseTween))
                {
                    dict.Add(nameof(SwipableUIProgressDisplay.colorFadeUseTween), OMIT_ACTION);
                }

                dict.Add(nameof(SwipableUIProgressDisplay.activeSprite), OMIT_ACTION);
                dict.Add(nameof(SwipableUIProgressDisplay.disabledSprite), OMIT_ACTION);
                if (castTargets.Any(t => !t.colorFadeUseTween))
                {
                    dict.Add(nameof(SwipableUIProgressDisplay.colorFadeTween), OMIT_ACTION);
                }
            }
        }
    }
}
