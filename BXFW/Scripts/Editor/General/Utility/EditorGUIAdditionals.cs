using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Extensions to the <see cref="EditorGUI"/> class to workaround array drawing bugs mainly, but may contain more <see cref="EditorGUI"/> utility.
    /// <br>For general purpose <see cref="GUI"/> utility use <see cref="GUIAdditionals"/>.</br>
    /// </summary>
    /// <remarks>
    /// * This class is prefixed with 'Additionals' because namespace 'UnityEditor' already has EditorGUIUtility
    /// </remarks>
    public static class EditorGUIAdditionals
    {
        /// <summary>
        /// Current Indent value of the <see cref="EditorGUI"/>, controlled by <see cref="EditorGUI.indentLevel"/>.
        /// </summary>
        public static float IndentValue
        {
            get
            {
                // This indent value is just 'EditorGUI.indentLevel * 15f'
                // return (float)typeof(EditorGUI).GetProperty("indent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
                return EditorGUI.indentLevel * 15f;
            }
        }

        /// <summary>
        /// Make gui area drag and drop.
        /// <br>Usage : Use the <see cref="DragAndDrop"/> class for getting event details.</br>
        /// </summary>
        public static void MakeDragDropArea(Action onDragAcceptAction, Func<bool> shouldAcceptDragCheck, Rect? customRect = null)
        {
            var shouldAcceptDrag = shouldAcceptDragCheck.Invoke();
            if (!shouldAcceptDrag)
            {
                return;
            }

            MakeDragDropArea(onDragAcceptAction, customRect);
        }
        /// <summary>
        /// Make gui area drag and drop. This method makes the given <paramref name="customRect"/> area / 
        /// or the <see cref="GUILayoutUtility.GetRect(float, float, float, float)"/> area accept the drag always.
        /// <br>This method always accepts drops.</br>
        /// <br>Usage : <see cref="DragAndDrop.objectReferences"/> is all you need.</br>
        /// </summary>
        public static void MakeDragDropArea(Action onDragAcceptAction, Rect? customRect = null)
        {
            Event evt = Event.current;
            if (evt == null)
            {
                throw new InvalidOperationException("[EditorGUIAdditionals::MakeDragDropArea] Called 'MakeDragDropArea' while 'Event.current' is null. This may happen if this was not called from OnGUI.");
            }

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Rect dropArea = customRect ?? GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        onDragAcceptAction?.Invoke();
                    }
                    break;
            }
        }

        /// <summary>
        /// Draw an array to the layouted gui of editor.
        /// <br>
        /// This is a more primitive array drawer (compared to <see cref="UnityEditorInternal.ReorderableList"/> and 
        /// more advanced compared to <see cref="NonReorderableAttribute"/>), but unlike 'ReorderableList' it actually works.
        /// </br>
        /// </summary>
        /// <param name="toggle">Whether if this array interface is collapsed or not.</param>
        /// <param name="property">The target property. This must target to an array <see cref="SerializedProperty"/> otherwise <see cref="ArgumentException"/> is thrown.</param>
        /// <returns>The toggle state.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="NullReferenceException"/>
        public static bool DrawArray(bool toggle, SerializedProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property), "[EditorGUIAdditionals::DrawArray] Given SerializedProperty is null.");
            }

            if (!property.isArray)
            {
                throw new ArgumentException("[EditorGUIAdditionals::DrawArray] Given SerializedProperty is not an array.", nameof(property));
            }

            // Also draws the 'PropertyField'
            // Create this function as it's more ergonomic compared to writing an inline delegate in this case.
            void OnFieldDrawnCustom(int i)
            {
                // Should not retrieve the 'property.arraySize'th element
                if (i >= property.arraySize)
                {
                    return;
                }

                // Create property field.
                SerializedProperty prop = property.GetArrayElementAtIndex(i);

                // If our property is null, ignore.
                if (prop == null)
                {
                    return;
                    // throw new NullReferenceException(string.Format("[EditorGUIAdditionals::DrawArray] The drawn property at index {0} does not exist. This should not happen.", i));
                }

                EditorGUILayout.PropertyField(prop);
            }

            toggle = InternalDrawArrayGUILayout(toggle, new GUIContent(property.displayName), property.arraySize, (int size) => property.arraySize = size, OnFieldDrawnCustom);
            property.serializedObject.ApplyModifiedProperties();
            return toggle;
        }
        /// <summary>
        /// <inheritdoc cref="DrawArray(bool, SerializedProperty)"/>
        /// This GUI is always shown.
        /// </summary>
        /// <param name="property"><inheritdoc cref="DrawArray(bool, SerializedProperty)"/></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="NullReferenceException"/>
        public static void DrawArray(SerializedProperty property)
        {
            DrawArray(true, property);
        }
        /// <summary>
        /// <inheritdoc cref="DrawArray(bool, SerializedProperty)"/>
        /// </summary>
        /// <param name="toggle">Whether if this array interface is collapsed or not.</param>
        /// <param name="obj">Serialized object of target.</param>
        /// <param name="arrayName">Array field name.</param>
        public static bool DrawArray(bool toggle, SerializedObject obj, string arrayName)
        {
            using SerializedProperty arrayTarget = obj.FindProperty(arrayName);
            return DrawArray(toggle, arrayTarget);
        }
        /// <inheritdoc cref="DrawArray(bool, SerializedObject, string)"/>
        public static void DrawArray(SerializedObject obj, string arrayName)
        {
            DrawArray(true, obj, arrayName);
        }

        /// <summary>
        /// Create custom GUI array with fields. (if you are lazy for doing a <see cref="PropertyDrawer"/> or <typeparamref name="T"/> is not serializable)
        /// </summary>
        /// <param name="toggle">Toggle boolean for the dropdown state. Required to keep an persistant state. Pass true if not intend to use.</param>
        /// <param name="label">Label to draw for the array.</param>
        /// <param name="array">Generic draw target array. Required to be passed by reference as it's resized automatically.</param>
        /// <param name="onArrayFieldDrawn">Event to draw generic ui when fired. <c>This field is required for list based drawers.</c></param>
        /// <returns>The toggle state.</returns>
        public static bool DrawArray<T>(bool toggle, GUIContent label, in IList<T> array, Action<int> onArrayFieldDrawn)
        {
            // Edit : type extensions work, but no anonymous delegates or local methods.
            return InternalDrawArrayGUILayout(toggle, label, array.Count, array.Resize, onArrayFieldDrawn);
        }
        /// <inheritdoc cref="DrawArray{T}(bool, GUIContent, in IList{T}, Action{int})"/>
        public static bool DrawArray<T>(bool toggle, in IList<T> array, Action<int> onArrayFieldDrawn)
        {
            return DrawArray(toggle, new GUIContent(string.Format("{0} List", typeof(T).Name)), array, onArrayFieldDrawn);
        }

        /// <summary>
        /// Draws a Vector3 field with only the given <paramref name="axis"/> editable.
        /// </summary>
        /// <param name="axis">Axis to draw the fields of. Depending on this value only the given axis fields will be drawn.</param>
        /// <param name="position">Rect position to draw this vector3 field on.</param>
        /// <param name="value">Value to have assigned. The return value will be the new modified value.</param>
        /// <param name="content">The content to draw for.</param>
        public static Vector3 AxisVector3Field(TransformAxis axis, Rect position, Vector3 value, GUIContent content)
        {
            content ??= GUIContent.none;

            bool canDrawX = (axis & TransformAxis.XAxis) == TransformAxis.XAxis;
            bool canDrawY = (axis & TransformAxis.YAxis) == TransformAxis.YAxis;
            bool canDrawZ = (axis & TransformAxis.ZAxis) == TransformAxis.ZAxis;

            int drawableAxisSize = Convert.ToInt32(canDrawX) + Convert.ToInt32(canDrawY) + Convert.ToInt32(canDrawZ);

            // Today we are learning how 2 array
            // First step : don't do this crap
            // Second step : if it works it's fine though.
            GUIContent[] subLabels = new GUIContent[drawableAxisSize];
            int currentDrawableArrayIndex = 0;
            if (canDrawX)
            {
                subLabels[currentDrawableArrayIndex] = new GUIContent("X");
                currentDrawableArrayIndex++;
            }
            if (canDrawY)
            {
                subLabels[currentDrawableArrayIndex] = new GUIContent("Y");
                currentDrawableArrayIndex++;
            }
            if (canDrawZ)
            {
                subLabels[currentDrawableArrayIndex] = new GUIContent("Z");
            }

            float[] vectorValues = new float[drawableAxisSize];
            currentDrawableArrayIndex = 0;
            if (canDrawX)
            {
                vectorValues[currentDrawableArrayIndex] = value.x;
                currentDrawableArrayIndex++;
            }
            if (canDrawY)
            {
                vectorValues[currentDrawableArrayIndex] = value.y;
                currentDrawableArrayIndex++;
            }
            if (canDrawZ)
            {
                vectorValues[currentDrawableArrayIndex] = value.z;
            }

            // The 'MultiFloatField' puts it's content label and other things in seperate columns, which looks ugly
            // Use 'GUIStyle.CalcWidth' and dynamically scale the label and the multi float field
            // There's no 'CalcWidth' but we can do 'CalcMinMaxWidth'
            Rect multiFloatRect = position;
            if (content != GUIContent.none)
            {
                GUI.skin.label.CalcMinMaxWidth(content, out float minLabelWidth, out float _);

                Rect labelRect = new Rect(position.x, position.y, minLabelWidth, position.height);
                GUI.Label(labelRect, content);
                multiFloatRect = new Rect(position.x + minLabelWidth, position.y, position.width - minLabelWidth, position.height);
            }

            EditorGUI.MultiFloatField(multiFloatRect, GUIContent.none, subLabels, vectorValues);

            currentDrawableArrayIndex = 0;
            if (canDrawX)
            {
                value.x = vectorValues[currentDrawableArrayIndex];
                currentDrawableArrayIndex++;
            }
            if (canDrawY)
            {
                value.y = vectorValues[currentDrawableArrayIndex];
                currentDrawableArrayIndex++;
            }
            if (canDrawZ)
            {
                value.z = vectorValues[currentDrawableArrayIndex];
            }

            return value;
        }
        /// <inheritdoc cref="AxisVector3Field(TransformAxis, Rect, Vector3, GUIContent)"/>
        public static Vector3 AxisVector3Field(TransformAxis axis, Rect position, Vector3 value, string label)
        {
            return AxisVector3Field(axis, position, value, new GUIContent(label));
        }
        /// <inheritdoc cref="AxisVector3Field(TransformAxis, Rect, Vector3, GUIContent)"/>
        public static Vector3 AxisVector3FieldGUILayout(TransformAxis axis, Vector3 value, GUIContent content, params GUILayoutOption[] options)
        {
            Rect layoutRect = GUIAdditionals.GetOptionalGUILayoutRect(0f, float.MaxValue, EditorGUIUtility.singleLineHeight, float.MaxValue, options);

            return AxisVector3Field(axis, layoutRect, value, content);
        }
        /// <inheritdoc cref="AxisVector3Field(TransformAxis, Rect, Vector3, GUIContent)"/>
        public static Vector3 AxisVector3FieldGUILayout(TransformAxis axis, Vector3 value, string label, params GUILayoutOption[] options)
        {
            return AxisVector3FieldGUILayout(axis, value, new GUIContent(label), options);
        }

        /// <summary>
        /// Draws a custom array view with a delegate that contains resizing function, and a custom GUI drawing function supplied by the client.
        /// </summary>
        private static bool InternalDrawArrayGUILayout(bool toggle, GUIContent label, int arraySize, Action<int> onArrayResize, Action<int> onArrayFieldDrawn)
        {
            int prevIndent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = prevIndent + 2;
            // Create the size & dropdown field
            GUILayout.BeginHorizontal();

            bool toggleDropdownState = GUILayout.Toggle(toggle, string.Empty, EditorStyles.popup, GUILayout.MaxWidth(20f));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            int currentSizeField = Mathf.Clamp(EditorGUILayout.IntField("Size", arraySize, GUILayout.MaxWidth(200f), GUILayout.MinWidth(150f)), 0, int.MaxValue);
            // Resize array (GUI size was changed)
            if (currentSizeField != arraySize)
            {
                onArrayResize(currentSizeField);
            }
            GUILayout.EndHorizontal();

            if (toggle)
            {
                EditorGUI.indentLevel = prevIndent + 3;
                // Create the array fields (stupid)
                for (int i = 0; i < arraySize; i++)
                {
                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField(string.Format("Element {0}", i), EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    onArrayFieldDrawn(i);
                }

                EditorGUI.indentLevel = prevIndent + 1;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    currentSizeField = Mathf.Clamp(currentSizeField + 1, 0, int.MaxValue);
                    onArrayResize(currentSizeField);
                }
                if (GUILayout.Button("-"))
                {
                    currentSizeField = Mathf.Clamp(currentSizeField - 1, 0, int.MaxValue);
                    onArrayResize(currentSizeField);
                }
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prevIndent;

            return toggleDropdownState;
        }
    }
}
