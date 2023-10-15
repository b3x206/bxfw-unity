using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using BXFW.UI;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TabSystem)), CanEditMultipleObjects]
    public sealed class TabSystemEditor : MultiUIManagerBaseEditor
    {
        [MenuItem("GameObject/UI/Tab System")]
        public static void CreateTabSystem(MenuCommand Command)
        {
            // Create primary gameobject.
            GameObject tabSystemObject = new GameObject("Tab System");

            // Align stuff
            GameObjectUtility.SetParentAndAlign(tabSystemObject, (GameObject)Command.context);

            // TabSystem on empty object.
            TabSystem tabSystem = tabSystemObject.AddComponent<TabSystem>();
            // Layout group
            HorizontalLayoutGroup tabSystemLayout = tabSystemObject.AddComponent<HorizontalLayoutGroup>();
            tabSystemLayout.childControlHeight = true;
            tabSystemLayout.childControlWidth = true;
            tabSystemLayout.spacing = 10f;
            // Tab Button
            // CreatedTabSystem.CreateTab();
            tabSystem.ElementCount = 1;

            // Resize stuff accordingly.
            // Width -- Height
            RectTransform tabTransform = tabSystemObject.GetComponent<RectTransform>();
            tabTransform.sizeDelta = new Vector2(200, 100);

            // Set Unity Stuff
            Undo.RegisterCreatedObjectUndo(tabSystemObject, string.Format("Create {0}", tabSystemObject.name));
            Selection.activeObject = tabSystemObject;
        }

        /// <summary>
        /// Adds omitting for the type field.
        /// </summary>
        private void AddFadeTypeOmits(Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, FadeType type)
        {
            switch (type)
            {
                case FadeType.None:
                    break;
                case FadeType.ColorFade:
                    dict.Add(nameof(TabSystem.FadeSpeed), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.FadeColorTargetDefault), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.FadeColorTargetDisabled), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.FadeColorTargetHover), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.FadeColorTargetClick), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.FadeSubtractFromCurrentColor), OMIT_ACTION);
                    break;
                case FadeType.SpriteSwap:
                    dict.Add(nameof(TabSystem.DefaultSpriteToSwap), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.DisabledSpriteToSwap), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.HoverSpriteToSwap), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.TargetSpriteToSwap), OMIT_ACTION);
                    break;
                case FadeType.CustomUnityEvent:
                    dict.Add(nameof(TabSystem.ButtonCustomEventOnReset), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.ButtonCustomEventOnClick), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.ButtonCustomEventOnDisable), OMIT_ACTION);
                    dict.Add(nameof(TabSystem.ButtonCustomEventOnHover), OMIT_ACTION);
                    break;
            }
        }

        protected override void GetCustomPropertyDrawerDictionary(Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, MultiUIManagerBase[] targets)
        {
            base.GetCustomPropertyDrawerDictionary(dict, targets);

            // Used to check the targets values
            IEnumerable<TabSystem> castedTargets = targets.Cast<TabSystem>();
            FadeType firstFadeType = castedTargets.First().ButtonFadeType;
            bool hasDifferentFadeTypes = castedTargets.Any(tabSystem => tabSystem.ButtonFadeType != firstFadeType);
            if (hasDifferentFadeTypes)
            {
                // Omit all
                foreach (FadeType type in Enum.GetValues(typeof(FadeType)).Cast<FadeType>())
                {
                    AddFadeTypeOmits(dict, type);
                }
            }
            else
            {
                // Omit depending on fade type
                foreach (FadeType type in Enum.GetValues(typeof(FadeType)).Cast<FadeType>().Where(type => type != firstFadeType))
                {
                    AddFadeTypeOmits(dict, type);
                }
            }

            dict.Add(nameof(TabSystem.OnTabButtonsClicked), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Before, () =>
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tab Event", EditorStyles.boldLabel);
            }));
        }
    }
}
