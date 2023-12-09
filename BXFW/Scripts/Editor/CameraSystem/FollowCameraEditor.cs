using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Property drawer for the <see cref="FollowCamera.CameraOffset"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(FollowCamera.CameraOffset), true)]
    public class FollowCameraOffsetEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // This line is pain?
            // Unity editor GUI is pain.
            bool DrawXYZClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;

            // Atleast this ui is 'not very dynamic'
            return (DrawXYZClamp ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight * 3) + 12;
        }

        private Rect GetPropertyRect(Rect parentRect, int index)
        {
            return new Rect(parentRect.x, parentRect.y + (18 * index), parentRect.width, 22f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool DrawXYZClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;

            EditorGUI.PropertyField(GetPropertyRect(position, 0), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)));
            if (DrawXYZClamp)
            {
                // Rest of the gui
                EditorGUI.PropertyField(GetPropertyRect(position, 1), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.Position)));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.EulerRotation)));

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(GetPropertyRect(position, 3), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosXClamp)));
                EditorGUI.PropertyField(GetPropertyRect(position, 4), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosYClamp)));
                EditorGUI.PropertyField(GetPropertyRect(position, 5), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosZClamp)));
                EditorGUI.indentLevel--;
            }
            else
            {
                // Without the clamp stuff
                EditorGUI.PropertyField(GetPropertyRect(position, 1), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.Position)));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.EulerRotation)));
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(FollowCamera), true), CanEditMultipleObjects]
    public class FollowCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Variable
            var targets = base.targets.Cast<FollowCamera>().ToArray();
            var showMixed = EditorGUI.showMixedValue;
            var StyleLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            StyleLabel.normal.textColor = Color.white;
            var inspectorDict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            if (targets.Any(cam => cam.UseFollowVecInstead))
            {
                inspectorDict.Add(nameof(FollowCamera.FollowTransform), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }

            if (targets.Any(cam => cam.FollowTransform != null && !cam.UseFollowVecInstead))
            {
                inspectorDict.Add(nameof(FollowCamera.FollowVector3), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }

            // Base Inspector
            serializedObject.DrawCustomDefaultInspector(inspectorDict);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            int currentCameraOffsetIndexTest = targets[0].CurrentCameraOffsetIndex;
            bool multipleHasDifferentOffset = targets.Any(c => c.CurrentCameraOffsetIndex != currentCameraOffsetIndexTest);
            EditorGUI.showMixedValue = multipleHasDifferentOffset;
            EditorGUI.BeginChangeCheck();
            int currentCameraOffsetIndexValue = EditorGUILayout.IntField("Current Camera Offset Index", currentCameraOffsetIndexTest);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("set camera offset index");
                int undoID = Undo.GetCurrentGroup();

                foreach (var target in targets)
                {
                    Undo.RecordObject(target, string.Empty);
                    target.CurrentCameraOffsetIndex = currentCameraOffsetIndexValue;
                }

                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUI.showMixedValue = showMixed;
            
            // Custom Inspector
            GUILayout.Label($"---- Current Index : {(multipleHasDifferentOffset ? "~" : currentCameraOffsetIndexTest.ToString())}", StyleLabel);
            if (GUILayout.Button("Set Camera Position Offset From Position"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("set camera position offset from position");
                int undoID = Undo.GetCurrentGroup();
                foreach (var target in targets)
                {
                    var FollowPosition = target.FollowTransform == null ? target.FollowVector3 : target.FollowTransform.position;
                    Undo.RecordObject(target.transform, string.Empty);

                    target.CameraOffsetTargets[target.CurrentCameraOffsetIndex].Position = target.transform.position - FollowPosition;
                    target.CameraOffsetTargets[target.CurrentCameraOffsetIndex].EulerRotation = target.transform.rotation.eulerAngles;
                }
                Undo.CollapseUndoOperations(undoID);
            }
            if (GUILayout.Button("Get Position From Camera Position Offset"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("get position from camera position offset");
                int undoID = Undo.GetCurrentGroup();
                foreach (var target in targets)
                {
                    var FollowPosition = target.FollowTransform == null ? target.FollowVector3 : target.FollowTransform.position;
                    Undo.RecordObject(target.transform, string.Empty);

                    target.transform.position = target.CameraOffsetTargets[target.CurrentCameraOffsetIndex].Position + FollowPosition;
                    target.transform.rotation = Quaternion.Euler(target.CameraOffsetTargets[target.CurrentCameraOffsetIndex].EulerRotation);
                }
                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUILayout.EndVertical();
        }
    }
}