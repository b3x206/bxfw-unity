using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System.Linq;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// A 'OnSceneGUI' based solution to be able to edit the paths on the editor window.
    /// <br>Supports 2D and 3D.</br>
    /// </summary>
    public class BezierPathSceneEditor
    {
        /// <summary>
        /// The currently editing axis.
        /// <br>This axis must have atleast 2 axis.</br> 
        /// </summary>
        private TransformAxis m_EditAxis = TransformAxis.XYZAxis;
        /// <summary>
        /// The currently editing axis.
        /// <br>This axis must have atleast 2 axis. Setting it invalid will add/draw the next axis with warnings printed.</br>
        /// </summary>
        public TransformAxis EditAxis
        {
            get => m_EditAxis;
            set
            {
                // Check if value atleast has 2 flags
                int flagsCount = 0;
                foreach (TransformAxis flag in Enum.GetValues(typeof(TransformAxis)).Cast<TransformAxis>())
                {
                    if ((flag & value) == value)
                    {
                        flagsCount++;
                    }
                }
                // Otherwise print a warning and don't set.
                if (flagsCount < 2)
                {
                    Debug.LogWarning($"[BezierPathSceneEditor::(set)EditAxis] Given value flags \"{value}\" doesn't have more than 2 flags. Please ensure that you are atleast setting 2 flags.");
                    return;
                }

                m_EditAxis = value;
            }
        }

        /// <summary>
        /// Call this on your own implementation of OnSceneGUI.
        /// </summary>
        public void OnSceneGUI(SerializedProperty targetProperty)
        {
            if (targetProperty == null)
            {
                throw new ArgumentNullException(nameof(targetProperty), "[BezierPathSceneEditor::OnSceneGUI] Given 'targetProperty' is null.");
            }
            if (targetProperty.serializedObject.isEditingMultipleObjects)
            {
                throw new ArgumentException("[BezierPathSceneEditor::OnSceneGUI] Given 'targetProperty's serializedObject is editing multiple objects. No support exists for multiple objects.", nameof(targetProperty));
            }
            // Check if the target property is indeed pointing to a BezierPath
            if (!typeof(BezierPath).IsAssignableFrom(targetProperty.GetPropertyType()))
            {
                throw new ArgumentException("[BezierPathSceneEditor::OnSceneGUI] Given 'targetProperty's type mismatches. It doesn't derive from BezierPath.", nameof(targetProperty));
            }

            // selectable control points
            using SerializedProperty controlPointsArrayProperty = targetProperty.FindPropertyRelative("m_ControlPoints");
            // path
            using SerializedProperty pathPointsArrayProperty = targetProperty.FindPropertyRelative(nameof(BezierPath.PathPoints));

            (targetProperty.GetTarget().value as BezierPath)?.UpdatePath();
            OnSceneGUI(
                controlPointsArrayProperty.arraySize,
                (int index) =>
                {
                    using SerializedProperty controlPointProperty = controlPointsArrayProperty.GetArrayElementAtIndex(index);

                    using SerializedProperty cpPositionProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.position));
                    using SerializedProperty cpHandleProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.handle));

                    return new BezierPoint(cpPositionProperty.vector3Value, cpHandleProperty.vector3Value);
                },
                (int index, BezierPoint setValue) =>
                {
                    using SerializedProperty controlPointProperty = controlPointsArrayProperty.GetArrayElementAtIndex(index);

                    using SerializedProperty cpPositionProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.position));
                    using SerializedProperty cpHandleProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.handle));

                    cpPositionProperty.vector3Value = setValue.position;
                    cpHandleProperty.vector3Value = setValue.handle;
                },
                pathPointsArrayProperty.arraySize,
                (int index) =>
                {
                    using SerializedProperty pointProperty = pathPointsArrayProperty.GetArrayElementAtIndex(index);

                    return pointProperty.vector3Value;
                }
            );

            controlPointsArrayProperty.serializedObject.ApplyModifiedProperties();
            pathPointsArrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Call this on your own implementation of OnSceneGUI.
        /// </summary>
        public void OnSceneGUI(BezierPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "[BezierPathSceneEditor::OnSceneGUI] Given 'path' is null.");
            }

            path.UpdatePath();
            OnSceneGUI(
                path.Count, (int index) => path[index], (int index, BezierPoint value) => path[index] = value,
                path.PathPoints.Count, (int index) => path.PathPoints[index]
            );
        }

        // TODO : Add selection thing
        private HashSet<int> m_SelectedHandles = new HashSet<int>();

        /// <summary>
        /// The internal delegate based drawer, using delegates to change values.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        private void OnSceneGUI(int controlPointsArraySize, Func<int, BezierPoint> getCPointOnIndex, Action<int, BezierPoint> setCPointOnIndex, int pathPointsArraySize, Func<int, Vector3> getPathPointOnIndex)
        {
            if (getCPointOnIndex == null)
            {
                throw new ArgumentNullException(nameof(getCPointOnIndex), "[BezierPathSceneEditor::OnSceneGUI] Given 'getCPointOnIndex' is null.");
            }
            if (setCPointOnIndex == null)
            {
                throw new ArgumentNullException(nameof(setCPointOnIndex), "[BezierPathSceneEditor::OnSceneGUI] Given 'setCPointOnIndex' is null.");
            }
            if (getPathPointOnIndex == null)
            {
                throw new ArgumentNullException(nameof(getPathPointOnIndex), "[BezierPathSceneEditor::OnSceneGUI] Given 'getPathPointOnIndex' is null.");
            }

            // Draw the selectable control points
            for (int i = 0; i < controlPointsArraySize; i++)
            {
                BezierPoint previousPoint = getCPointOnIndex(i);

                float previousAlpha = Handles.color.a;
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.4f);
                Handles.SphereHandleCap(0, previousPoint.position, Quaternion.identity, 0.2f, Event.current.type);
                Handles.SphereHandleCap(0, previousPoint.handle, Quaternion.identity, 0.2f, Event.current.type);
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, previousAlpha);

                previousPoint.position = Handles.DoPositionHandle(previousPoint.position, Quaternion.identity).AxisVector(m_EditAxis);
                previousPoint.handle = Handles.DoPositionHandle(previousPoint.handle, Quaternion.identity).AxisVector(m_EditAxis);
                Handles.DrawLine(previousPoint.position, previousPoint.handle);

                setCPointOnIndex(i, previousPoint);
            }
            
            for (int i = 0; i < pathPointsArraySize - 1; i++)
            {
                Vector3 prevPoint = getPathPointOnIndex(i);
                Vector3 nextPoint = getPathPointOnIndex(i + 1);
                Handles.DrawLine(prevPoint, nextPoint);
            }

            // Draw the GUI for selection
            // Note : This is a deprecated way of doing this, figure out how to create a dockable window for SceneView?
            SceneView lastView = SceneView.lastActiveSceneView;
            Rect guiArea = new Rect(lastView.position.width - 110f, lastView.position.height - 135f, 100f, 100f);
            Handles.BeginGUI();
            GUI.Box(guiArea, GUIContent.none);
            GUILayout.BeginArea(guiArea);

            GUILayout.Label("stuff go here");

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }

    /// <summary>
    /// Updates the path whenever a change happens.
    /// </summary>
    [CustomPropertyDrawer(typeof(BezierPath))]
    public class BezierPathEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (!property.isExpanded)
            {
                return height;
            }

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp) + mainCtx.Padding;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            EditorGUI.BeginChangeCheck();
            foreach (var visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Re-generate path because something was changed
                // FIXME : This won't work on struct parents, the path will be generated in runtime instead
                Undo.RecordObject(property.serializedObject.targetObject, "set value");
                ((BezierPath)property.GetTarget().value).UpdatePath();
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
