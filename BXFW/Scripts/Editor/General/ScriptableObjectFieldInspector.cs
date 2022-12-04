using BXFW.Tools.Editor;

using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using static UnityEngine.GraphicsBuffer;

namespace BXFW
{
    /// <summary>
    /// Creates a ScriptableObject inspector.
    /// <br>Derive from this class and use the <see cref="CustomPropertyDrawer"/> attribute with same type as <typeparamref name="T"/>.</br>
    /// </summary>
    public class ScriptableObjectFieldInspector<T> : PropertyDrawer
        where T : ScriptableObject
    {
        /// <summary>
        /// Menu that contains the create names and delegates.
        /// </summary>
        protected GenericMenu typeMenus;
        /// <summary>
        /// The target SerializedObject.
        /// <br>This is assigned as the target object on <see langword="base"/>.<see cref="GetPropertyHeight(SerializedProperty, GUIContent)"/>.</br>
        /// </summary>
        protected SerializedObject SObject { get; private set; }

        /// <summary>
        /// Padding height between ui elements (so that it's not claustrophobic)
        /// </summary>
        protected virtual float HEIGHT_PADDING => 2;

        /// <summary>
        /// Currently drawn list of the scriptable objects.
        /// </summary> 
        /// The plan is that we control the parent of the drawn scriptable objects and if the parents of the scriptable object match we create a clone
        /// By doing this there's no duplicate scriptable objects on an array, so there's no problems.
        protected readonly Dictionary<string, T> drawnScriptableObjects = new Dictionary<string, T>();
        /// <summary>
        /// Prefix to make the drawn object parent's unique, allowing for same objects on different parents.
        /// </summary>
        protected const string KEY_PREFIX = "::";
        /// <summary>
        /// Removes left of the string after the following char sequence of <see cref="KEY_PREFIX"/>.
        /// </summary>
        protected static string TrimLeftPrefix(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s.Substring(s.IndexOf(KEY_PREFIX) + KEY_PREFIX.Length);
        }
        /// <summary>
        /// Removes right of the string after the following char sequence of <see cref="KEY_PREFIX"/>.
        /// </summary>
        protected static string TrimRightPrefix(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s.Substring(0, s.IndexOf(KEY_PREFIX) + KEY_PREFIX.Length);
        }
        /// <summary>
        /// Flag to enable to copy the already existing Value on <see cref="drawnScriptableObjects"/>.
        /// <br>1 : Object has to have the same parent object. (Using <see cref="UnityEngine.Object.GetInstanceID"/> here.)</br>
        /// <br>2 : Object has to have the same reference. (2 objects pointing to same <see cref="ScriptableObject"/>)</br>
        /// <br/>
        /// <br>Disabling this will disable the copying of the objects.</br>
        /// </summary>
        protected virtual bool MakeDrawnScriptableObjectsUnique => true;
        /// <summary>
        /// Enabling this will output diagnostic messages to the inspector.
        /// </summary>
        protected virtual bool DebugMode => false;

        // TODO (low priority) :
        // AdvancedDropdown implementation on UnityEditor.IMGUI.Controls

        private float SingleLineHeight => EditorGUIUtility.singleLineHeight + HEIGHT_PADDING;

        /// <summary>
        /// Sets value of target.
        /// <br/>
        /// <br>NOTE : Always pass newly created (<see cref="ScriptableObject.CreateInstance(Type)"/>) variables here.</br>
        /// <br>Because if the setting fails (this only applies if the target property is contained inside a prefab)
        /// it calls <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/> to it.</br>
        /// </summary>
        private void SetValueOfTarget(SerializedProperty property, T obj)
        {
            if (property == null)
                throw new NullReferenceException("[ScriptableObjectFieldInspector::SetValueOfTarget] Passed property parameter 'property' is null.");
            if (obj == null)
                throw new NullReferenceException("[ScriptableObjectFieldInspector::SetValueOfTarget] Passed object parameter 'obj' is null.");

            bool targetIsPrefab = PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);

            if (targetIsPrefab)
            {
                // Pull the creation window thing
                string absPath = EditorUtility.SaveFilePanelInProject(string.Format("Create {0} Instance", typeof(T).Name), "", "asset", ""); // contains absolute path to file
                if (string.IsNullOrWhiteSpace(absPath))
                {
                    // Destroy temp object + cancel
                    UnityEngine.Object.DestroyImmediate(obj);
                    return;
                }
                
                string relPath = absPath.Substring(absPath.IndexOf(Directory.GetCurrentDirectory()) + 1); // relPath => contains path to file (relative)
                int indOfSlashRelPath = relPath.LastIndexOf('/');
                indOfSlashRelPath = indOfSlashRelPath != -1 ? indOfSlashRelPath : relPath.IndexOf('\\');

                // Base asset folder (if it exists)
                string baseAssetFolder = relPath.Substring(0, indOfSlashRelPath);

                if (AssetDatabase.IsValidFolder(baseAssetFolder))
                {
                    AssetDatabase.CreateAsset(obj, relPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    ProjectWindowUtil.ShowCreatedAsset(obj);
                }
            }

            // Set value
            fieldInfo.SetValue(property.GetParentOfTargetField().Value, obj);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get types inheriting from player powerup on all assemblies
            if (typeMenus == null)
            {
                // Use a 'GenericMenu' as it's much more convenient than using EditorGUI.Popup
                typeMenus = new GenericMenu();

                // Get all assemblies
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsClass)
                        {
                            if (type.IsSubclassOf(typeof(T)) && type != typeof(T))
                            {
                                var pow = ScriptableObject.CreateInstance(type) as T;

                                typeMenus.AddItem(new GUIContent(string.Format("New {0}", type.Name)), false, () =>
                                {
                                    SetValueOfTarget(property, (T)ScriptableObject.CreateInstance(type));
                                });
                            }
                        }
                    }
                }

                // No items
                if (typeMenus.GetItemCount() <= 0)
                    typeMenus.AddDisabledItem(new GUIContent(string.Format("Disabled (Make classes inheriting from '{0}')", typeof(T).Name)), true);
            }

            var propTarget = property.objectReferenceValue;
            var target = propTarget as T;

            // Check if object is null (generically)
            // If null, don't 
            if (target == null)
                return SingleLineHeight;

            if (!property.isExpanded)
                return SingleLineHeight;

            SObject ??= new SerializedObject(target);
            float h = 0f;
            SerializedProperty prop = SObject.GetIterator();
            bool expanded = true;
            while (prop.NextVisible(expanded))
            {
                if (prop.propertyPath == "m_Script")
                {
                    continue;
                }

                h += EditorGUI.GetPropertyHeight(prop, true) + HEIGHT_PADDING; // Add padding
                expanded = false; // used for the expand arrow of unity
            }

            // Add label height
            h += SingleLineHeight;

            return h;
        }

        private float currentY;
        private Rect GetPropertyRect(Rect position, SerializedProperty prop)
        {
            return GetPropertyRect(position, EditorGUI.GetPropertyHeight(prop, true));
        }
        private Rect GetPropertyRect(Rect position, float height = -1f)
        {
            // Reuse the copied struct
            position.y = currentY;
            position.height = height + HEIGHT_PADDING;

            // assuming that the height is added after first rect.
            if (height < 0f)
                height = SingleLineHeight;

            currentY += height + HEIGHT_PADDING;
            return position;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // The 'property' is for the object field that is only for assigning.
            EditorGUI.BeginProperty(position, label, property);
            currentY = position.y;

            // Gather target info
            // This works, because we are working with ScriptableObject's
            // Otherwise use BXFW's EditorAdditionals.GetTarget(SerializedProperty)
            UnityEngine.Object propTarget = property.objectReferenceValue;
            var target = propTarget as T;
            bool targetIsPrefab = PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);
            // Dynamic 'ShowOnProject' button + make unique is disabled if the prefab actually exists in project
            string currentAssetPath = AssetDatabase.GetAssetPath(target);
            bool hasAssetPath = !string.IsNullOrWhiteSpace(currentAssetPath);

            // Do not make unique if the target is prefab or we have asset path
            // Because : prefabs can't store local scriptable objects (only scenes)
            // :: If the object exists in project view, no need to clone; it's very easy to clone anyways + we can use 'Delete' to lose reference on the current object.
            if (MakeDrawnScriptableObjectsUnique && !targetIsPrefab && !hasAssetPath)
            {
                // Get whether if the same item was drawn twice on the same array / SerializedProperty
                // If so, clone the second (current) item, as it's a reference to the previous item
                // only control the absolute parent as that contains the script.
                // ---
                // Contains the object_name+key_name (prefixed)
                // Allowing for the same named variable object (on different objects) to be not cloned every time.
                // ---
                // Unity targetObject names could be different or string.Empty, so use GetInstanceID as this PropertyDrawer only runs tempoarily
                // So instance ID's are feasible
                string key = string.Format("{0}{1}{2}", property.serializedObject.targetObject.GetInstanceID().ToString(), KEY_PREFIX, property.propertyPath);
                if (!drawnScriptableObjects.ContainsKey(key))
                {
                    // Register key if it doesn't exist.
                    drawnScriptableObjects.Add(key, target);
                }
                else // do not check on the first OnGUI call as the target could not be ready.
                {
                    // the following code is bad. pls find better ways (such as seperating this to a seperate guard claused function/method/whatever)

                    // Check if key is null, if so assign the target.
                    if (drawnScriptableObjects[key] == null)
                        drawnScriptableObjects[key] = target;
                    // If the key is not null (after assigning the target), don't ignore and start searching
                    if (drawnScriptableObjects[key] != null)
                    {
                        KeyValuePair<string, T> propPair = drawnScriptableObjects.SingleOrDefault(p =>
                            // Key is different && Same object parent (using InstanceID as temp) && Same reference to object
                            p.Key != key && TrimRightPrefix(p.Key) == TrimRightPrefix(key) && p.Value == target);

                        if (!string.IsNullOrEmpty(propPair.Key))
                        {
                            string uniquePropPath = TrimLeftPrefix(propPair.Key);
                            SerializedProperty existingProp = property.serializedObject.FindProperty(uniquePropPath);
                            bool uniquePropertyExists = existingProp != null; // Object slot actually exists?

                            // Remove junk item from dict if property doesn't exist at all (on current object).
                            if (!uniquePropertyExists)
                            {
                                if (DebugMode)
                                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Property '{0}' doesn't exist, removing.", key));

                                drawnScriptableObjects.Remove(key);
                            }
                            // Key is already contained on different drawn property, and is being planned to be drawn for different property path.
                            // The 'SingleOrDefault' returns null string if there's no matching target value.
                            else if (propPair.Key != key)
                            {
                                // Clone 'target' and paste it to 'target'
                                // TODO : Only do this in the arrays (or test if other objects work [such as 2 monobehaviours drawn at the same time with same scriptable object] correctly),
                                // other inspectors 'probably' could contain a reference to the 'target'

                                if (DebugMode)
                                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Copied target {0} to property {1}", target, existingProp.propertyPath));

                                // Copy cloned object reference into an actual clone
                                T instObject = UnityEngine.Object.Instantiate(propPair.Value);

                                // Assign the clone (ASSIGN LIKE THIS, JUST SETTING THE 'property.objectReferenceValue' DOES NOT WORK)
                                fieldInfo.SetValue(property.GetParentOfTargetField().Value, instObject);
                                target = instObject;                           // Set tempoary variables too

                                // Set the target dirty
                                // With this way, cloning of the 'ReorderableList' no longer causes issues,
                                // because this script now hunts for the same existing references with the same parent.
                                EditorUtility.SetDirty(target);
                                // Make name prettier
                                instObject.name = instObject.name.Replace("(Clone)", "_c");

                                // Set the current dictionary key to be the 'instObject' (so that the object isn't cloned twice, or more)
                                drawnScriptableObjects[key] = target;
                            }
                        }
                    }
                }
            }

            // GUI related
            var previousWidth = position.width;
            var gEnabled = GUI.enabled;

            if (target == null)
            {
                EditorAdditionals.MakeDroppableAreaGUI(
                () => // OnDrag
                {
                    if (
                        DragAndDrop.objectReferences.Length == 1 &&
                        // Accept drag if the previous base type matches
                        DragAndDrop.objectReferences[0].GetType().BaseType == typeof(T)
                    )
                    {
                        fieldInfo.SetValue(property.GetParentOfTargetField().Value, DragAndDrop.objectReferences[0]);
                        target = DragAndDrop.objectReferences[0] as T;

                        if (DebugMode)
                            Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] DragDrop: Assigned target to '{0}'.", target));

                        // this repaints the propertydrawer
                        property.serializedObject.Update();
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }, position);

                position.width = previousWidth * .4f;
                GUI.Label(position, label);
                position.x += previousWidth * .4f;

                position.width = previousWidth * .45f;
                if (GUI.Button(position, new GUIContent(
                    targetIsPrefab ? string.Format("Drag / Create {0} (child classes)", typeof(T).Name) : string.Format("Assign {0} (child classes)", typeof(T).Name),
                    targetIsPrefab ? "Prefab scenes can't serialize local scriptable objects in prefabs." : "You can also drag scriptable objects."), EditorStyles.popup))
                {
                    typeMenus.ShowAsContext();
                }
                position.x += previousWidth * .46f;

                GUI.enabled = !targetIsPrefab;

                position.width = previousWidth * .14f; // 1 - (.46f + .4f)
                if (GUI.Button(position, "Refresh"))
                {
                    typeMenus = null;
                }

                GUI.enabled = gEnabled;
                EditorGUI.EndProperty();
                return;
            }

            // Property label
            Rect propFoldoutLabel = GetPropertyRect(position, SingleLineHeight); // width is equal to 'previousWidth'
            const float BTN_SHOWPRJ_WIDTH = .25f;
            const float BTN_DELETE_WIDTH = .15f;
            float foldoutLabelWidth = hasAssetPath ? 1f - (BTN_DELETE_WIDTH + BTN_SHOWPRJ_WIDTH) : 1f - BTN_DELETE_WIDTH;

            propFoldoutLabel.width = previousWidth * foldoutLabelWidth;
            property.isExpanded = EditorGUI.Foldout(propFoldoutLabel, property.isExpanded, label);

            propFoldoutLabel.x += previousWidth * foldoutLabelWidth;   // Start pos
            
            propFoldoutLabel.width = previousWidth * BTN_SHOWPRJ_WIDTH;
            if (hasAssetPath)
            {
                // Display a button to highlight the asset source
                if (GUI.Button(propFoldoutLabel, "Show On Project"))
                {
                    // Highlight the source object in the 'Project' folder
                    ProjectWindowUtil.ShowCreatedAsset(target);
                }

                propFoldoutLabel.x += previousWidth * BTN_SHOWPRJ_WIDTH;    // Add 1 more button to pos
            }

            propFoldoutLabel.width = previousWidth * BTN_DELETE_WIDTH;
            if (GUI.Button(propFoldoutLabel, "Delete"))
            {
                // If the object would still like to exist, don't do 'DestroyObjectImmediate', instead just remove the reference
                if (!hasAssetPath)
                {
                    Undo.DestroyObjectImmediate(target);
                }

                // Remove reference (no matter what, so that the reference is cleared and setting values to the previous one doesn't change 2 objects)
                fieldInfo.SetValue(property.GetParentOfTargetField().Value, null);
                EditorGUI.EndProperty();
                return;
            }

            // Main drawing
            if (property.isExpanded)
            {
                SObject ??= new SerializedObject(target);

                // Draw fields
                EditorGUI.indentLevel += 1;
                SerializedProperty prop = SObject.GetIterator();
                bool expanded = true;
                while (prop.NextVisible(expanded))
                {
                    if (prop.propertyPath == "m_Script")
                        continue;

                    EditorGUI.PropertyField(GetPropertyRect(position, prop), prop, true);

                    expanded = false;
                }
                EditorGUI.indentLevel -= 1;

                SObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();

            GUI.enabled = gEnabled;
        }
    }
}
