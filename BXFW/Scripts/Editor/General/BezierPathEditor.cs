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
    /// <example>
    /// <![CDATA[
    /// using UnityEditor;
    /// using UnityEngine;
    /// using BXFW.ScriptEditor;
    /// 
    /// /// <summary>
    /// /// An example class using this path editor on it's OnSceneGUI.
    /// /// </summary>
    /// [CustomEditor(typeof(PathContainer))]
    /// public class PathContainerEditor : Editor
    /// {
    ///     private BezierPathSceneEditor sceneEditor;
    ///     private SerializedProperty targetPathProperty;
    ///     private GameObject targetObject;
    /// 
    ///     private void OnSceneGUI()
    ///     {
    ///         if (targetObject != null && sceneEditor?.TargetObject != targetObject)
    ///         {
    ///             sceneEditor = new BezierPathSceneEditor(targetObject);
    ///             // 2D axis
    ///             // sceneEditor.EditAxis = BXFW.TransformAxis.XAxis | BXFW.TransformAxis.YAxis;
    ///         }
    /// 
    ///         // Move the path with the GameObject
    ///         if (targetObject != null && sceneEditor != null)
    ///         {
    ///             sceneEditor.positionOffset = targetObject.transform.position;
    ///         }
    /// 
    ///         // Draw the GUI
    ///         if (targetPathProperty != null)
    ///         {
    ///             sceneEditor.OnSceneGUI(targetPathProperty);
    ///         }
    ///     }
    /// 
    ///     public override void OnInspectorGUI()
    ///     {
    ///         // Get values that cannot be accessed from 'OnSceneGUI'
    ///         targetPathProperty = serializedObject.FindProperty(nameof(PathContainer.path));
    ///         targetObject = (target as PathContainer).gameObject;
    /// 
    ///         // Your inspector here.. Or keep the default one (like this) if you don't care.
    ///         base.OnInspectorGUI();
    ///     }
    /// }
    /// ]]>
    /// </example>
    public class BezierPathSceneEditor
    {
        // TODO : Maybe a special editor tool would have been better?
        // Unity has those, so add that after adding the OnSceneGUI version

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
                    if ((value & flag) == flag)
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
        /// The target game object.
        /// </summary>
        private GameObject m_TargetObject;
        /// <summary>
        /// The target game object.
        /// <br>This cannot be null.</br>
        /// </summary>
        public GameObject TargetObject
        {
            get => m_TargetObject;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "[BezierPathEditor::(set)TargetObject] Given target object is null.");
                }

                m_TargetObject = value;
            }
        }

        /// <summary>
        /// The control points offset position.
        /// <br>This is not applied to the actual points itself.</br>
        /// <br>
        /// To rotate or change other matrix properties of the control points being edited, 
        /// use the <see cref="Handles.DrawingScope"/> with a matrix. (Only applies for 'OnSceneGUI' versions)
        /// </br>
        /// </summary>
        public Vector3 positionOffset = Vector3.zero;

        /// <summary>
        /// Makes the selectable point's drawn size constant regardless of distance to the point.
        /// </summary>
        public bool constantSelectableSize = true;

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

            void SetPointInIndexDelegate(int index, BezierPoint setValue)
            {
                using SerializedProperty controlPointProperty = controlPointsArrayProperty.GetArrayElementAtIndex(index);

                using SerializedProperty cpPositionProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.position));
                using SerializedProperty cpHandleProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.handle));

                cpPositionProperty.vector3Value = setValue.position;
                cpHandleProperty.vector3Value = setValue.handle;
            }

            OnSceneGUI(
                controlPointsArrayProperty.arraySize,
                (int index) =>
                {
                    using SerializedProperty controlPointProperty = controlPointsArrayProperty.GetArrayElementAtIndex(index);

                    using SerializedProperty cpPositionProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.position));
                    using SerializedProperty cpHandleProperty = controlPointProperty.FindPropertyRelative(nameof(BezierPoint.handle));

                    return new BezierPoint(cpPositionProperty.vector3Value + positionOffset, cpHandleProperty.vector3Value + positionOffset);
                },
                SetPointInIndexDelegate,
                (BezierPoint addValue) =>
                {
                    controlPointsArrayProperty.arraySize++;
                    SetPointInIndexDelegate(controlPointsArrayProperty.arraySize - 1, addValue);
                },
                (int removeIndex) =>
                {
                    controlPointsArrayProperty.DeleteArrayElementAtIndex(removeIndex);
                },
                pathPointsArrayProperty.arraySize,
                (int index) =>
                {
                    using SerializedProperty pointProperty = pathPointsArrayProperty.GetArrayElementAtIndex(index);

                    return pointProperty.vector3Value + positionOffset;
                }
            );

            controlPointsArrayProperty.serializedObject.ApplyModifiedProperties();
            pathPointsArrayProperty.serializedObject.ApplyModifiedProperties();

            (targetProperty.GetTarget().value as BezierPath)?.UpdatePath();
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
                path.Count, (int index) => path[index].Offset(positionOffset), (int index, BezierPoint value) => path[index] = value, path.Add, path.RemoveAt,
                path.PathPoints.Count, (int index) => path.PathPoints[index]
            );
        }

        private readonly HashSet<int> m_SelectedHandles = new HashSet<int>();

        public bool AnySelected => m_SelectedHandles.Count > 0;
        public bool IsSelected(int index)
        {
            return m_SelectedHandles.Contains(index);
        }
        public bool Deselect(int index)
        {
            return m_SelectedHandles.Remove(index);
        }
        public void DeselectAll()
        {
            m_SelectedHandles.Clear();
        }
        /// <summary>
        /// Selects a handle.
        /// <br>This method does not do bound checking, check the size of your <see cref="SerializedProperty"/> or the <see cref="BezierPath"/>. Out of bound selections will do nothing.</br>
        /// </summary>
        /// <param name="index">Index of the handle to select.</param>
        /// <returns><see langword="true"/> if a new handle was selected, <see langword="false"/> if no new handle was selected.</returns>
        public bool Select(int index)
        {
            // Index has to be positive
            if (index < 0)
            {
                return false;
            }

            return m_SelectedHandles.Add(index);
        }

        /// <summary>
        /// The internal delegate based drawer, using delegates to change values.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        private void OnSceneGUI(int controlPointsArraySize, Func<int, BezierPoint> getCPointOnIndex, Action<int, BezierPoint> setCPointOnIndex, Action<BezierPoint> addCPoint, Action<int> removeCPoint, int pathPointsArraySize, Func<int, Vector3> getPathPointOnIndex)
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

            SceneView lastView = SceneView.lastActiveSceneView;

            // Draw the selectable control points
            bool selectedAny = false;
            for (int i = 0; i < controlPointsArraySize; i++)
            {
                BezierPoint previousPoint = getCPointOnIndex(i);

                bool isCurrentSelected = IsSelected(i);

                Color previousColor = Handles.color;
                Handles.color = new Color(Mathf.Clamp01(previousColor.r - 0.5f), previousColor.g, previousColor.b, 0.4f);
                float positionHandleSize = constantSelectableSize ? HandleUtility.GetHandleSize(previousPoint.position) * 0.3f : 1.5f;
                if (Handles.Button(previousPoint.position, Quaternion.identity, positionHandleSize, positionHandleSize, Handles.SphereHandleCap))
                {
                    if (!Event.current.control)
                    {
                        DeselectAll();
                    }

                    Select(i);
                    selectedAny = true;
                }

                Handles.color = new Color(previousColor.r, previousColor.g, previousColor.b, 0.4f);
                float handleHandleSize = constantSelectableSize ? HandleUtility.GetHandleSize(previousPoint.handle) * 0.25f : 1.3f;
                if (Handles.Button(previousPoint.handle, Quaternion.identity, handleHandleSize, handleHandleSize, Handles.SphereHandleCap))
                {
                    if (!Event.current.control)
                    {
                        DeselectAll();
                    }

                    Select(i);
                    selectedAny = true;
                }

                Handles.color = new Color(0f, previousColor.g, 0f, previousColor.a);
                Handles.DrawLine(previousPoint.position, previousPoint.handle);

                Handles.color = previousColor;

                if (!isCurrentSelected)
                {
                    continue;
                }

                previousPoint.position = Handles.DoPositionHandle(previousPoint.position, Quaternion.identity).AxisVector(EditAxis) - positionOffset.AxisVector(EditAxis);
                previousPoint.handle = Handles.DoPositionHandle(previousPoint.handle, Quaternion.identity).AxisVector(EditAxis) - positionOffset.AxisVector(EditAxis);
                
                // Position handles were touched
                // TODO : Maybe the only reliable way of detecting whether if the position
                // handle was interacted with was creating a custom capped position handle function?
                if (!selectedAny && GUIUtility.hotControl != 0)
                {
                    selectedAny = true;
                }

                setCPointOnIndex(i, previousPoint);
            }
            // None was selected + Mouse is down + Down button is the primary button
            if (!selectedAny && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                DeselectAll();
            }

            // Draw the path
            // (TODO : Path lags due to low amount of updates, maybe use the 'Interpolate' method?)
            for (int i = 0; i < pathPointsArraySize - 1; i++)
            {
                Vector3 prevPoint = getPathPointOnIndex(i);
                Vector3 nextPoint = getPathPointOnIndex(i + 1);
                Handles.DrawLine(prevPoint, nextPoint);
            }

            // Draw the GUI for selection
            // Note : This is a deprecated way of doing this, figure out how to create a dockable window for SceneView?
            float guiWidth = AnySelected ? 175f : 100f, guiHeight = AnySelected ? 80f : 37f;
            Rect guiArea = new Rect(lastView.position.width - (guiWidth + 10f), lastView.position.height - (guiHeight + 35f), guiWidth, guiHeight);
            Handles.BeginGUI();
            EditorGUI.DrawRect(guiArea, new Color(0.3f, 0.3f, 0.3f, 0.8f));
            GUILayout.BeginArea(new RectOffset(6, 6, 6, 6).Remove(guiArea));

            // Draw the add/remove buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int selectOption = GUILayout.Toolbar(-1, new string[] { "+", "-" }, GUILayout.Height(25f), GUILayout.Width(70f));
            switch (selectOption)
            {
                case 0:
                    // Add a point on to the front of the given scene view
                    addCPoint(new BezierPoint(lastView.pivot.AxisVector(EditAxis), lastView.pivot.AxisVector(EditAxis) + (lastView.rotation * Vector3.one.AxisVector(EditAxis))));
                    break;
                case 1:
                    // If a single element is selected, remove that
                    int lastOnlySelectedIndex = m_SelectedHandles.Count == 1 ? m_SelectedHandles.Single() : -1;

                    removeCPoint(lastOnlySelectedIndex < 0 ? controlPointsArraySize - 1 : lastOnlySelectedIndex);
                    controlPointsArraySize--;
                    break;

                default:
                    break;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Draw the selected nodes information
            for (int i = 0; i < controlPointsArraySize; i++)
            {
                if (!IsSelected(i))
                {
                    continue;
                }

                BezierPoint previousPoint = getCPointOnIndex(i);

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(5f);
                previousPoint.position = EditorGUIAdditionals.AxisVector3FieldGUILayout(EditAxis, previousPoint.position, "P:") - positionOffset;
                previousPoint.handle = EditorGUIAdditionals.AxisVector3FieldGUILayout(EditAxis, previousPoint.handle, "H:") - positionOffset;

                if (EditorGUI.EndChangeCheck())
                {
                    setCPointOnIndex(i, previousPoint);
                }
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        /// <summary>
        /// Creates a BezierPathSceneEditor.
        /// </summary>
        /// <param name="targetObject">
        /// The target object to inspect for.
        /// This cannot be null, but it can be changed if the <see cref="TargetObject"/> is null.
        /// </param>
        public BezierPathSceneEditor(GameObject targetObject)
        {
            TargetObject = targetObject;
            // Selection interaction (TODO : This will be most likely only possible as an editor tool)
            // HandleUtility.pickGameObjectCustomPasses += OnPickElements;
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
                // FIXME : This won't work on struct parents (due to the fact that GetTarget().value is a copy if GetTarget().parent is a struct)
                // the path will be generated in runtime instead
                Undo.RecordObject(property.serializedObject.targetObject, "set value");
                ((BezierPath)property.GetTarget().value).UpdatePath();
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
