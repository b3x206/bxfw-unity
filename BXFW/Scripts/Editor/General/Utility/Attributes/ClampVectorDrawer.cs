using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ClampVectorAttribute))]
    public class ClampVectorDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private ClampVectorAttribute Attribute => attribute as ClampVectorAttribute;
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector3 &&
                property.propertyType != SerializedPropertyType.Vector4)
            {
                addHeight += WarningBoxHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + Padding;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= Padding;
            position.y += Padding / 2f;

            bool previousShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            #region Vector2
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2Field(position, label, property.vector2Value);
                var vClamped = new Vector2(
                    Mathf.Clamp(v.x, (float)Attribute.minX, (float)Attribute.maxX),
                    Mathf.Clamp(v.y, (float)Attribute.minY, (float)Attribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                var vClamped = new Vector2Int(
                    Mathf.Clamp(v.x, (int)Attribute.minX, (int)Attribute.maxX),
                    Mathf.Clamp(v.y, (int)Attribute.minY, (int)Attribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2IntValue = vClamped;
                }
            }
            #endregion
            #region Vector3
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3Field(position, label, property.vector3Value);
                var vClamped = new Vector3(
                    Mathf.Clamp(v.x, (float)Attribute.minX, (float)Attribute.maxX),
                    Mathf.Clamp(v.y, (float)Attribute.minY, (float)Attribute.maxY),
                    Mathf.Clamp(v.z, (float)Attribute.minZ, (float)Attribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector3Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                var vClamped = new Vector3Int(
                    Mathf.Clamp(v.x, (int)Attribute.minX, (int)Attribute.maxX),
                    Mathf.Clamp(v.y, (int)Attribute.minY, (int)Attribute.maxY),
                    Mathf.Clamp(v.z, (int)Attribute.minZ, (int)Attribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3IntValue = vClamped;
                }
            }
            #endregion
            #region Vector4
            else if (property.propertyType == SerializedPropertyType.Vector4)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector4Field(position, label, property.vector4Value);
                var vClamped = new Vector4(
                    Mathf.Clamp(v.x, (float)Attribute.minX, (float)Attribute.maxX),
                    Mathf.Clamp(v.y, (float)Attribute.minY, (float)Attribute.maxY),
                    Mathf.Clamp(v.z, (float)Attribute.minZ, (float)Attribute.maxZ),
                    Mathf.Clamp(v.w, (float)Attribute.minZ, (float)Attribute.maxW)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector4Value = vClamped;
                }
            }
            #endregion
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either Vector or VectorInt.", MessageType.Warning);
            }
            EditorGUI.showMixedValue = previousShowMixedValue;
        }
    }
}
