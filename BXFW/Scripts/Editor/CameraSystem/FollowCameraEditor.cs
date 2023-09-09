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
            bool drawPosClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;

            return (drawPosClamp ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight * 3) + 12;
        }

        /// <summary>
        /// Returns a single rect sized property rect.
        /// </summary>
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

                // indent by 15
                Rect indentRect = new Rect(position) { x = position.x + 15f, width = position.width - 15f };
                EditorGUI.PropertyField(GetPropertyRect(indentRect, 3), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosXClamp)));
                EditorGUI.PropertyField(GetPropertyRect(indentRect, 4), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosYClamp)));
                EditorGUI.PropertyField(GetPropertyRect(indentRect, 5), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosZClamp)));
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
            var styleLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            styleLabel.normal.textColor = Color.white;

            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            if (targets.Any(cam => cam.useFollowPositionInstead))
                dict.Add(nameof(FollowCamera.followTransform), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            if (targets.Any(cam => cam.followTransform != null && !cam.useFollowPositionInstead))
                dict.Add(nameof(FollowCamera.followPosition), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));

            // Base Inspector
            serializedObject.DrawCustomDefaultInspector(dict);

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
            GUILayout.Label($"---- Current Index : {(multipleHasDifferentOffset ? "~" : currentCameraOffsetIndexTest.ToString())}", styleLabel);
            if (GUILayout.Button("Set Camera Position Offset From Position"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("set camera position offset from position");
                int undoID = Undo.GetCurrentGroup();
                foreach (var target in targets)
                {
                    var FollowPosition = target.followTransform == null ? target.followPosition : target.followTransform.position;
                    Undo.RecordObject(target.transform, string.Empty);

                    target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].Position = target.transform.position - FollowPosition;
                    target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].EulerRotation = target.transform.rotation.eulerAngles;
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
                    var FollowPosition = target.followTransform == null ? target.followPosition : target.followTransform.position;
                    Undo.RecordObject(target.transform, string.Empty);

                    target.transform.position = target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].Position + FollowPosition;
                    target.transform.rotation = Quaternion.Euler(target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].EulerRotation);
                }
                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
