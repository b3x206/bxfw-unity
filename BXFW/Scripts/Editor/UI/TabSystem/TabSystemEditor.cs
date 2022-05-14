using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using TMPro;
using BXFW.UI;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TabSystem))]
    internal class TabSystemEditor : Editor
    {
        //////////// Object Creation
        [MenuItem("GameObject/UI/Tab System")]
        public static void CreateTabSystem(MenuCommand Command)
        {
            // Create primary gameobject.
            GameObject TSystem = new GameObject("Tab System");

            // Align stuff
            GameObjectUtility.SetParentAndAlign(TSystem, (GameObject)Command.context);

            #region Creation
            // Add components here... (Also create tab button)

            // TabSystem on empty object.
            TabSystem CreatedTabSystem = TSystem.AddComponent<TabSystem>();
            // Layout group
            HorizontalLayoutGroup TabSystemLayoutGroup = TSystem.AddComponent<HorizontalLayoutGroup>();
            TabSystemLayoutGroup.childControlHeight = true;
            TabSystemLayoutGroup.childControlWidth = true;
            TabSystemLayoutGroup.spacing = 10f;
            // Tab Button
            _ = CreatedTabSystem.CreateTab();

            // Resize stuff accordingly.
            // Width -- Height
            RectTransform TSystemTransform = TSystem.GetComponent<RectTransform>();
            TSystemTransform.sizeDelta = new Vector2(200, 100);
            #endregion

            // Set Unity Stuff
            Undo.RegisterCreatedObjectUndo(TSystem, string.Format("Create {0}", TSystem.name));
            Selection.activeObject = TSystem;
        }

        public override void OnInspectorGUI()
        {
            // Standard
            var Target = (TabSystem)target;
            var tabSO = serializedObject;
            tabSO.Update();

            // Draw the 'm_Script' field that monobehaviour makes (with disabled gui)
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(tabSO.FindProperty("m_Script"));
            GUI.enabled = gEnabled;

            EditorGUI.BeginChangeCheck();

            // Setup variables
            EditorGUILayout.LabelField("Standard Settings", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal(); // TabButtonAmount
            var TBtnAmount = EditorGUILayout.IntField(nameof(Target.TabButtonAmount), Target.TabButtonAmount);
            if (GUILayout.Button("+", GUILayout.Width(20f))) { TBtnAmount++; }
            if (GUILayout.Button("-", GUILayout.Width(20f))) { TBtnAmount--; }
            GUILayout.EndHorizontal();
            // Show warning if TabButtonAmount is 0 or lower.
            if (TBtnAmount <= 0)
                EditorGUILayout.HelpBox("Warning : TabSystem is disabled. To enable it again set TabButtonAmount to 1 or more.", MessageType.Warning);

            EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.ButtonFadeType)));
            var CRefTB = EditorGUILayout.IntField(nameof(Target.CurrentReferenceTabButton), Target.CurrentReferenceTabButton);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);
            // Button fade
            switch (Target.ButtonFadeType)
            {
                case FadeType.ColorFade:
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonFadeSpeed)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonFadeColorTargetHover)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonFadeColorTargetClick)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonSubtractFromCurrentColor)));
                    break;
                case FadeType.SpriteSwap:
                    EditorGUILayout.LabelField(
                        "Note : Default sprite is the image that is currently inside the Image component.",
                        EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.HoverSpriteToSwap)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TargetSpriteToSwap)));
                    break;
                case FadeType.CustomUnityEvent:
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonCustomEventOnReset)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonCustomEventHover)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TabButtonCustomEventClick)));
                    break;

                default:
                case FadeType.None:
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tab Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.OnTabButtonsClicked)));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target, string.Format("Change variable on TabSystem {0}", Target.name));

                // Apply properties
                if (Target.TabButtonAmount != TBtnAmount)
                {
                    Target.TabButtonAmount = TBtnAmount;
                }

                Target.CurrentReferenceTabButton = CRefTB;

                // Apply serializedObject
                tabSO.ApplyModifiedProperties();
            }

            // -- Tab List Actions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tab List", EditorStyles.boldLabel);

            EditorGUI.indentLevel++; // indentLevel = normal + 1
            GUI.enabled = false;
            EditorGUILayout.PropertyField(tabSO.FindProperty("TabButtons"));
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Tabs"))
            {
                Target.ClearTabs();
            }
            if (GUILayout.Button("Generate Tabs"))
            {
                Target.GenerateTabs();
            }
            if (GUILayout.Button("Reset Tabs"))
            {
                Target.ResetTabs();
            }
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--; // indentLevel = normal
        }
    }
}