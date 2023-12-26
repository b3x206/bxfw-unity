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
            bool drawPositionClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.usePositionClamp)).boolValue;

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
            bool drawXYZClamp = property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.usePositionClamp)).boolValue;
            mainCtx.Reset();

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.usePositionClamp)));
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.position)));
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.eulerRotation)));

            if (drawXYZClamp)
            {
                // indent by 15
                Rect indentRect = new Rect(position) { x = position.x + 15f, width = position.width - 15f };
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.posXClamp)));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.posYClamp)));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentRect, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative(nameof(FollowCamera.CameraOffset.posZClamp)));
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
            FollowCamera[] targets = base.targets.Cast<FollowCamera>().ToArray();
            bool showMixed = EditorGUI.showMixedValue;
            GUIStyle styleLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            styleLabel.normal.textColor = Color.white;

            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            if (targets.Any(cam => cam.useFollowPositionInstead))
            {
                dict.Add(nameof(FollowCamera.followTransform), EditorAdditionals.OmitAction);
            }

            if (targets.Any(cam => cam.followTransform != null && !cam.useFollowPositionInstead))
            {
                dict.Add(nameof(FollowCamera.followPosition), EditorAdditionals.OmitAction);
            }

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

                    target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].position = target.transform.position - FollowPosition;
                    target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].eulerRotation = target.transform.rotation.eulerAngles;
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

                    target.transform.position = target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].position + FollowPosition;
                    target.transform.rotation = Quaternion.Euler(target.cameraOffsetTargets[target.CurrentCameraOffsetIndex].eulerRotation);
                }
                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
