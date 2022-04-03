using BXFW.Tweening;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(BXTweenCore))]
    public class BXTweenCoreInspector : Editor
    {
        private BXTweenCore Target;
        private GUIStyle headerTextStyle;

        private void OnEnable()
        {
            Target = (BXTweenCore)target;

            headerTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22
            };

            // Put repaint of this inspector to update to get realtime data viewed on inspector.
            EditorApplication.update += Repaint;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            // Draw a field for 'm_Script'
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            // Draw ReadOnly status properties 
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("[CTween] CTween (for now) only works in runtime.", MessageType.Warning);
                GUI.enabled = gEnabled;
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("-- [CTween] --", headerTextStyle, GUILayout.Height(40f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            // Draw stats from the CTween class
            EditorGUILayout.LabelField(string.Format("Tween Amount (running) = {0}", BXTween.CurrentRunningTweens.Count));
            // more of a placeholder here.
            EditorGUILayout.LabelField(string.Format("Core Status = {0}", BXTween.CheckStatus() ? "OK" : "Error (null?)")); 

            // Re enable (or disable, we don't know) the GUI.
            GUI.enabled = gEnabled;
        }
    }
}