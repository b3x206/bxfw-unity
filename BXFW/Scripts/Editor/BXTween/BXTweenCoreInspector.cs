﻿using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using System.Linq;
using BXFW.Tweening;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(BXTweenCore))]
    public class BXTweenCoreInspector : Editor
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
        /// Tween interface <see cref="ITweenCTX"/> filtering filter.
        /// </summary>
        private struct TweenFilter
        {
            public bool IgnoreNullTargetObject;
            public Object TargetObject;

            public bool ShouldFilter(ITweenCTX tw)
            {
                return (IgnoreNullTargetObject && tw.TargetObject == null) || (TargetObject != null && tw.TargetObject != TargetObject);
            }
        }

        private TweenFilter currentFilter;

        private GUIStyle boxStyle;
        private GUIStyle headerTextStyle;
        private GUIStyle miniTextStyle;
        private GUIStyle buttonStyle;

        private Vector2 runningTwScroll;
        private bool expandFilterDebugTweens = false;
        private readonly List<bool> expandedTweens = new List<bool>();
        public override void OnInspectorGUI()
        {
            boxStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = false,
                fixedWidth = 300f,
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
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            // Draw ReadOnly status properties 
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("[BXTween] BXTween (for now) only works in runtime.", MessageType.Warning);
                GUI.enabled = gEnabled;
                return;
            }

            EditorGUILayout.LabelField("::[BXTweenCore]", headerTextStyle, GUILayout.Height(32f));

            // Draw stats from the BXTween class
            EditorGUILayout.LabelField(string.Format("Tween Amount (running) = {0}", BXTween.CurrentRunningTweens.Count));
            EditorGUILayout.LabelField(string.Format("Core Status = {0}", BXTween.CheckStatus() ? "OK" : "Error (null?)"));
            // Re enable (or disable, we don't know) the GUI.
            GUI.enabled = gEnabled;

            // Draw the list of running tweens
            const float scrollAreaHeight = 250;
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
                currentFilter.IgnoreNullTargetObject = EditorGUILayout.Toggle("Ignore Null Target Object", currentFilter.IgnoreNullTargetObject);
                currentFilter.TargetObject = EditorGUILayout.ObjectField("Target Object", currentFilter.TargetObject, typeof(Object), true);
                
                EditorGUI.indentLevel -= 2;
            }

            runningTwScroll = GUILayout.BeginScrollView(runningTwScroll, GUILayout.Height(scrollAreaHeight));
            // Draw the list of current running tweens (with name)
            for (int i = 0; i < BXTween.CurrentRunningTweens.Count; i++)
            {
                ITweenCTX tw = BXTween.CurrentRunningTweens[i];

                if (i > expandedTweens.Count - 1)
                    expandedTweens.Add(false);

                if (currentFilter.ShouldFilter(tw))
                    continue;

                // Get target type using reflection instead, no need to pollute the interface,
                // as the interface works will be done using 'GetType' or 'is' keyword pattern matching.
                expandedTweens[i] = GUILayout.Toggle(expandedTweens[i], $"Tween {i} | Type={tw.GetType().GenericTypeArguments.SingleOrDefault()}, Target={tw.TargetObject}", boxStyle);

                if (expandedTweens[i])
                {
                    // Show more information about the tween
                    // Assume that this type is BXTweenCTX, but the generic is unknown
                    // Interfaces always return concrete type : So GetType is used.
                    foreach (var v in tw.GetType().GetProperties())
                    {
                        GUILayout.Label(string.Format("    [Property] {0}:::{1} = {2}", v.Name, v.PropertyType, v.GetValue(tw)));
                    }
                    foreach (var v in tw.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        // Don't draw properties twice
                        if (v.Name.Contains("k__BackingField"))
                            continue;

                        GUILayout.Label(string.Format("    [Field] {0}:::{1} = {2}", v.Name, v.FieldType, v.GetValue(tw)));
                    }
                }
            }
            if (BXTween.CurrentRunningTweens.Count <= 0)
            {
                EditorGUILayout.HelpBox("There's no currently running tween.", MessageType.Info);
            }
            GUILayout.EndScrollView();
        }
    }
}