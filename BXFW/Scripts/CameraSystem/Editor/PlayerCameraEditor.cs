using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerCamera.CameraOffset))]
public class PlayerCameraOffsetEditor : PropertyDrawer
{
    private SerializedProperty CurrentOffset;
    private bool DrawXYZClamp
    {
        get
        {
            return CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.UseCameraPosClamp)).boolValue;
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

        EditorGUI.PropertyField(GetPropertyRect(position, 0), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.UseCameraPosClamp)));
        if (DrawXYZClamp)
        {
            EditorGUI.PropertyField(GetPropertyRect(position, 1), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.CameraPosXClamp)));
            EditorGUI.PropertyField(GetPropertyRect(position, 2), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.CameraPosYClamp)));
            EditorGUI.PropertyField(GetPropertyRect(position, 3), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.CameraPosZClamp)));
            EditorGUI.PropertyField(GetPropertyRect(position, 4), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.Position)));
            EditorGUI.PropertyField(GetPropertyRect(position, 5), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.EulerRotation)));
        }
        else
        {
            EditorGUI.PropertyField(GetPropertyRect(position, 1), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.Position)));
            EditorGUI.PropertyField(GetPropertyRect(position, 2), CurrentOffset.FindPropertyRelative(nameof(PlayerCamera.CameraOffset.EulerRotation)));
        }

        CurrentOffset.serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(PlayerCamera))]
public class PlayerCameraEditor : Editor
{
    [MenuItem("GameObject/Player Camera")]
    public static void CreatePlayerCamera()
    {
        var objCamera = new GameObject("PlayerCamera").AddComponent<PlayerCamera>();
        objCamera.tag = "Main Camera";
        objCamera.FollowTransform = Selection.activeTransform;
        objCamera.SetCurrentCameraOffsetIndex(0);
    }

    public override void OnInspectorGUI()
    {
        // Variable
        var Target = target as PlayerCamera;
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
            Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].Position = Target.transform.position - Target.FollowTransform.position;
            Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].EulerRotation = Target.transform.rotation.eulerAngles;

            Undo.RecordObject(Target.transform, "Undo Set Camera Position");
        }
        if (GUILayout.Button("Get Position From Camera Position Offset"))
        {
            Target.transform.position = Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].Position + Target.FollowTransform.position;
            Target.transform.rotation = Quaternion.Euler(Target.CameraOffsetTargets[Target.CurrentCameraOffsetIndex].EulerRotation);

            Undo.RecordObject(Target.transform, "Undo Get Camera Position");
        }
        GUILayout.EndHorizontal();
    }
}
