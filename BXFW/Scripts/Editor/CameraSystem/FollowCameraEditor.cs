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
    public sealed class FollowCameraOffsetEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool drawPositionClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;

            // use pos clamp toggle + pos + rot
            float height = (EditorGUIUtility.singleLineHeight + mainCtx.Padding) * 3;

            if (drawPositionClamp)
            {
                // position clamps
                height += (EditorGUIUtility.singleLineHeight + mainCtx.Padding) * 3;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool drawXYZClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;
            mainCtx.Reset();

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)));
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.Position)));
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.EulerRotation)));

            if (drawXYZClamp)
            {
                // indent by 15
                Rect indentRect = new Rect(position) { x = position.x + 15f, width = position.width - 15f };
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosXClamp)));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosYClamp)));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosZClamp)));
            }

            EditorGUI.EndProperty();
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