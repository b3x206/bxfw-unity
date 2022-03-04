using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Property drawer for the <see cref="FollowCamera.CameraOffset"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(FollowCamera.CameraOffset))]
    internal class FollowCameraOffsetEditor : PropertyDrawer
    {
        private SerializedProperty CurrentOffset;
        private bool DrawXYZClamp
        {
            get
            {
                return CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)).boolValue;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (CurrentOffset == null)
                CurrentOffset = property;

            return (DrawXYZClamp ? EditorGUIUtility.singleLineHeight * 6 : EditorGUIUtility.singleLineHeight * 3) + 12;
        }

        private Rect GetPropertyRect(Rect parentRect, int index)
        {
            return new Rect(parentRect.x, parentRect.y + (18 * index), parentRect.width, 22f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (CurrentOffset == null)
                CurrentOffset = property;

            EditorGUI.PropertyField(GetPropertyRect(position, 0), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.UseCameraPosClamp)));
            if (DrawXYZClamp)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(GetPropertyRect(position, 1), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosXClamp)));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosYClamp)));
                EditorGUI.PropertyField(GetPropertyRect(position, 3), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.CameraPosZClamp)));
                EditorGUI.indentLevel--;

                // Rest of the gui
                EditorGUI.PropertyField(GetPropertyRect(position, 4), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.Position)));
                EditorGUI.PropertyField(GetPropertyRect(position, 5), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.EulerRotation)));
            }
            else
            {
                // Without the clamp stuff
                EditorGUI.PropertyField(GetPropertyRect(position, 1), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.Position)));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), CurrentOffset.FindPropertyRelative(nameof(FollowCamera.CameraOffset.EulerRotation)));
            }

            CurrentOffset.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(FollowCamera))]
    internal class FollowCameraEditor : Editor
    {
        [MenuItem("GameObject/Player Camera")]
        public static void CreatePlayerCamera()
        {
            var objCamera = new GameObject("PlayerCamera").AddComponent<FollowCamera>();
            objCamera.tag = "Main Camera";
            objCamera.FollowTransform = Selection.activeTransform;
            objCamera.SetCurrentCameraOffsetIndex(0);
        }

        public override void OnInspectorGUI()
        {
            // Variable
            var Target = target as FollowCamera;
            var StyleLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            StyleLabel.normal.textColor = Color.white;

            // Base Inspector
            base.OnInspectorGUI();
            Target.CurrentCameraOffsetIndex = EditorGUILayout.IntField("Current Camera Offset Index", Target.CurrentCameraOffsetIndex);
            // Custom Inspector
            GUILayout.Label($"---- Current Index : {Target.CurrentCameraOffsetIndex}", StyleLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Camera Position Offset From Position"))
            {
                var FollowPosition = Target.FollowTransform == null ? Target.FollowVector3 : Target.FollowTransform.position;
                Undo.RecordObject(Target, "Set Camera Position");

                Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].Position = Target.transform.position - FollowPosition;
                Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].EulerRotation = Target.transform.rotation.eulerAngles;
            }
            if (GUILayout.Button("Get Position From Camera Position Offset"))
            {
                var FollowPosition = Target.FollowTransform == null ? Target.FollowVector3 : Target.FollowTransform.position;
                Undo.RecordObject(Target.transform, "Get Camera Position");

                Target.transform.position = Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].Position + FollowPosition;
                Target.transform.rotation = Quaternion.Euler(Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].EulerRotation);
            }
            GUILayout.EndHorizontal();
        }
    }
}