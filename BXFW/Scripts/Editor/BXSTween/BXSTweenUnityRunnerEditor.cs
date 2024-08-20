using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BXFW.Tweening.Editor
{
    [CustomEditor(typeof(BXSTweenUnityRunner))]
    public class BXSTweenUnityRunnerEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            // Put repaint of this inspector to update to get realtime data viewed on inspector.
            EditorApplication.update += Repaint;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        /// <summary>
        /// Used to filter tweenables, but it is now used as view options.
        /// </summary>
        private struct EditorTweensViewOptions
        {
            public bool reverseTweensView;
            public int breakAtTweenCount;
        }
        private EditorTweensViewOptions viewOptions;

        private const float TweensScrollAreaHeight = 250;
        private GUIStyle boxStyle;
        private GUIStyle headerTextStyle;
        private GUIStyle miniTextStyle;
        private GUIStyle detailsLabelStyle;
        private GUIStyle buttonStyle;

        private Vector2 tweensListScroll;
        private bool expandFilterDebugTweens = false;
        /// <summary>
        /// A boolean array for the allocated tweens list.
        /// </summary>
        private readonly List<bool> m_expandedTweens = new List<bool>();
        public override void OnInspectorGUI()
        {
            // TODO : 
            // 1 : Clean up code + add filtering + searchbar filtering
            // 2 : Make sequences display it's children indented
            // 3 : After that add a raw tweens view
            // 4 : Optimize further (laggy on larger than >500 elements)
            // For now this is just a direct port of the BXTweenCoreInspector with monospace font and coloring, which was totally fine except for a lot of tweens.

            boxStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = false,
                //fixedWidth = EditorGUIUtility.currentViewWidth,
                fixedWidth = 500f,
                richText = true,
                fontSize = 14,
                font = GUIAdditionals.MonospaceFont
            };
            detailsLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 14,
                font = GUIAdditionals.MonospaceFont
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button);
            headerTextStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            miniTextStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.BoldAndItalic
            };

            // Draw a field for 'm_Script'
            using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            // Draw ReadOnly status properties 
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("BXSTween UnityRunner only works in runtime.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("[BXSTweenUnityRunner]", headerTextStyle, GUILayout.Height(32f));

            // Draw stats from the BXSTween class
            EditorGUILayout.LabelField(string.Format("Tween Amount = {0}", BXSTween.RunningTweens.Count));
            EditorGUILayout.LabelField(string.Format("Sequence Amount = {0}", BXSTween.RunningTweens.Where(t => t is BXSTweenSequence).Count()));
            EditorGUILayout.LabelField(string.Format("BXSTween Status = {0}", BXSTween.NeedsInitializeParameters ? "Error (Needs Initialize)" : "OK"));

            // Draw the list of running tweens
            GUIAdditionals.DrawUILineLayout(Color.gray);

            // Draw filter button/toggle + info text
            GUILayout.BeginHorizontal();
            expandFilterDebugTweens = GUILayout.Toggle(expandFilterDebugTweens, "Filter", buttonStyle, GUILayout.Width(70));
            GUILayout.Label("  -- Click on any box to view details about the tween --  ", miniTextStyle);
            GUILayout.EndHorizontal();
            if (expandFilterDebugTweens)
            {
                EditorGUI.indentLevel += 2;

                // Draw filter tweens area
                viewOptions.breakAtTweenCount = Mathf.Clamp(EditorGUILayout.IntField(
                    new GUIContent(
                        "Tween Amount To Pause (Break)",
                        "Pause editor after the amount of current tweens that is >= from this value.\nTo stop pausing set this value to 0 or lower."
                    ),
                    viewOptions.breakAtTweenCount), -1, int.MaxValue
                );
                viewOptions.reverseTweensView = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Reverse Tweens View",
                        "Reverses the current view, so the last tween run is the topmost."
                    ),
                    viewOptions.reverseTweensView
                );

                EditorGUI.indentLevel -= 2;
            }
            // Pause editor if the tween amount exceeded
            if (viewOptions.breakAtTweenCount > 0 && BXSTween.RunningTweens.Count >= viewOptions.breakAtTweenCount)
            {
                EditorApplication.isPaused = true;
            }

            // Draw the list of current running tweens (with name)
            // Get a monospace font style with rich text coloring as well
            tweensListScroll = GUILayout.BeginScrollView(tweensListScroll, GUILayout.Height(TweensScrollAreaHeight));
            for (int guiIndex = 0; guiIndex < BXSTween.RunningTweens.Count; guiIndex++)
            {
                int tweenIndex = viewOptions.reverseTweensView ? BXSTween.RunningTweens.Count - (guiIndex + 1) : guiIndex;
                BXSTweenable tween = BXSTween.RunningTweens[tweenIndex];

                // Allocate toggles (use 'i' parameter, as it's the only one that goes sequentially)
                // We just want to reverse the 'CurrentRunningTweens'
                // Otherwise it's very easy to get ArgumentOutOfRangeException
                if (guiIndex > m_expandedTweens.Count - 1)
                {
                    m_expandedTweens.Add(false);
                }

                // All of these do complex 'CalcHeight' and 'GUIStyle' stuff
                // Which causes the extreme lag on the 10000 elements debug view

                // Get target type using reflection instead, no need to pollute the interface,
                // as the interface works will be done using 'GetType' or 'is' keyword pattern matching.
                try
                {
                    m_expandedTweens[guiIndex] = GUILayout.Toggle(m_expandedTweens[guiIndex], $"[*] Tween {tweenIndex} | Type={tween.GetType()}, ID={tween.ID}", boxStyle);
                }
                catch (System.Exception e)
                {
                    m_expandedTweens[guiIndex] = GUILayout.Toggle(m_expandedTweens[guiIndex], $"[!] Tween {tweenIndex} | Exception={e.Message}", boxStyle);
                }

                if (m_expandedTweens[guiIndex])
                {
                    // Show more information about the tween
                    // Assume that this type is BXSTweenable, but the type details otherwise is runtime defined
                    // Interfaces always return concrete type : So GetType is used.
                    foreach (PropertyInfo v in tween.GetType().GetProperties())
                    {
                        // Unsupported index parameters, can be triggered by 'this[int idx]' expressions
                        if (v.GetIndexParameters().Length > 0)
                        {
                            continue;
                        }

                        GUILayout.Label(string.Format("  <color=#f3bd28>[ Property ]</color> <color=#2eb6ae>{0}</color> <color=#dcdcdc>{1}</color> = {2}", v.PropertyType.Name, v.Name, v.GetValue(tween)), detailsLabelStyle);
                    }
                    foreach (FieldInfo v in tween.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        // Don't draw properties twice
                        if (v.Name.Contains("k__BackingField"))
                        {
                            continue;
                        }

                        GUILayout.Label(string.Format("  <color=#f3bd28>[ Field    ]</color> <color=#2eb6ae>{0}</color> <color=#dcdcdc>{1}</color> = {2}", v.FieldType.Name, v.Name, v.GetValue(tween)), detailsLabelStyle);
                    }

                    // Draw options for the tweenable
                    // .. TODO : Draw more options
                    if (GUILayout.Button("Stop", GUILayout.Width(EditorGUIUtility.currentViewWidth)))
                    {
                        Debug.Log($"[BXSTween | EditorDebug] Stopped tween {tween}");
                        tween.Stop();
                    }
                }
            }
            if (BXSTween.RunningTweens.Count <= 0)
            {
                EditorGUILayout.HelpBox("There's no currently running tween.", MessageType.Info);
            }

            GUILayout.EndScrollView();
        }
    }
}
