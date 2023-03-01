using BXFW.Tools.Editor;

using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using System.IO;
using System.Linq;
using System.Collections;

namespace BXFW
{
    /// <summary>
    /// Creates a ScriptableObject inspector.
    /// <br>Derive from this class and use the <see cref="CustomPropertyDrawer"/> attribute with same type as <typeparamref name="T"/>.</br>
    /// </summary>
    public class ScriptableObjectFieldInspector<T> : PropertyDrawer
        where T : ScriptableObject
    {
        // TODO (bit higher but still low priority) :
        // 1. Fix Inspector being unable to draw custom fields
        // TODO (low priority) :
        // 1. Add support for GUIElements on custom inspector overrides (only the legacy OnInspectorGUI is taken to count)
        // 2. AdvancedDropdown implementation for null field selector on UnityEditor.IMGUI.Controls

        /// <summary>
        /// Menu that contains the create names and delegates.
        /// </summary>
        protected GenericMenu typeMenus;
        /// <summary>
        /// <see cref="CustomEditor"/> attribute for <see cref="T"/>.
        /// <br>Set to a valid editor when <see cref="SetValueOfTarget(SerializedProperty, T)"/> is called.</br>
        /// </summary>
        protected Editor currentCustomInspector;
        /// <summary>
        /// Name of the default inspector assigned to all objects by unity (when <see cref="Editor.CreateEditor(UnityEngine.Object)"/> is called on object)
        /// <br/>
        /// <br>See class : <see cref="GenericInspector"/>. Only override this 'string' field if the name was changed or this '&lt;see&gt;' directs to nowhere.</br>
        /// </summary>
        protected virtual string DEFAULT_INSPECTOR_TYPE_NAME => "GenericInspector";
        /// <summary>
        /// Returns whether if <see cref="currentCustomInspector"/> isn't null.
        /// <br>Only valid if there's a target object in the target property.</br>
        /// <br>Override to disable/enable this function, as it's <b>experimental</b>.</br>
        /// </summary>
        public virtual bool UseCustomInspector => currentCustomInspector != null && currentCustomInspector.GetType().Name != DEFAULT_INSPECTOR_TYPE_NAME;
        /// <summary>
        /// Scroll position on the reserved rect.
        /// </summary>
        protected Vector2 customInspectorScrollPosition;
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
        /// Reserved height for the <see cref="currentCustomInspector"/>.
        /// </summary>
        protected virtual float ReservedHeightCustomEditor => 300;
        /// <summary>
        /// Allows to change the scriptable object <see cref="UnityEngine.Object.name"/> directly using a input field.
        /// </summary>
        protected virtual bool DisplayObjectNameEditor => false;

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

        /// <summary>
        /// Size of a single line height with padding applied.
        /// </summary>
        protected float SingleLineHeight => EditorGUIUtility.singleLineHeight + HEIGHT_PADDING;

        /// <summary>
        /// Sets value of target safely.
        /// <br>The <paramref name="obj"/> passed can be anything.</br>
        /// </summary>
        protected void SetValueOfTarget(SerializedProperty property, T obj, bool allowUndo = true)
        {
            if (allowUndo)
                Undo.RecordObject(property.serializedObject.targetObject, string.Format("Set value of {0}", property.name));
            
            // Set value
            // If the parent is an array, set the target index into the 'obj'
            var parentTargetPair = property.GetParentOfTargetField();
            object parent = parentTargetPair.Value;

            if (obj != null)
            {
                // Refresh the CustomInspector with a new target, because we can't set target (we can, but unity seems to not really like it).
                if (currentCustomInspector != null)
                    UnityEngine.Object.DestroyImmediate(currentCustomInspector);

                currentCustomInspector = Editor.CreateEditor(obj);

                if (DebugMode && currentCustomInspector == null)
                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)::SetValueOfTarget(Search Custom Editor)] No suitable editor found for obj '{0}'.", obj));
            }

            // why (c# array moment)
            if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                // Set the index directly
                // the returned field info cannot be a direct reference / pointer to an array element (because c# arrays are weird)
                // (heck, even the fieldInfo internal variable that unity returns points to that element's array parent)
                // so we have to copy the entire 'IEnumerable' thing and paste into that array.

                int index = property.GetPropertyArrayIndex();

                // We also have to ensure the array is a 'members that you can set' type
                // Object we cast to is reference, but singular objects may still need FieldInfo set.
                if (parent is IList<T> refList)
                {
                    if (index >= refList.Count)
                    {
                        // Array is most likely resized, but we still have a dead link to the property.
                        // ok
                        Debug.LogWarning(string.Format("[ScriptableObjectFieldInspector::SetValueOfTarget] Given SerializedProperty index {0} is larger than array size. Letting the exception carnage happen.", index));
                    }

                    refList[index] = obj;
                }
                else if (parent is Array refArray)
                {
                    refArray.SetValue(obj, index);
                }
                else
                {
                    // List is not settable, fallback to previous method
                    // This most likely shouldn't happen (unless using a custom field parent that has IEnumerable interface)
                    // as unity doesn't serialize read-only Lists or weird c# lists.
                    if (DebugMode)
                        Debug.LogWarning("[ScriptableObjectFieldInspector(DebugMode)::SetValueOfTarget] Target is in field parent with interface 'IEnumerable' but falling back to default FieldInfo set method.");

                    try
                    {
                        fieldInfo.SetValue(parent, obj);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(string.Format("[ScriptableObjectFieldInspector::SetValueOfTarget] Tried to set target that has field parent with 'IEnumerable' but failed with exception : {0}\nStackTrace:{1}", e.Message, e.StackTrace));
                    }
                }
            }
            else
            {
                // Set value directly, as it's not contained in an array and
                // the fieldInfo directly points to the object's actual field
                fieldInfo.SetValue(parent, obj);
            }

            // Forces unity to serialize the assigned object
            // (Undo.RecordObject does this anyways, this is no undo force serialization)
            if (!allowUndo)
                EditorUtility.SetDirty(property.serializedObject.targetObject);

            // This is very necessary to save the reference!
            // (Otherwise it doesn't save the file reference unless it's a default, non-prefab unity scene)
            AssetDatabase.SaveAssetIfDirty(property.serializedObject.targetObject);
        }
        /// <summary>
        /// Sets value of target.
        /// <br>Summons a scriptable object creation window if the inline create was called.</br>
        /// <br/>
        /// <br>NOTE : Always pass newly created (<see cref="ScriptableObject.CreateInstance(Type)"/>) variables here.</br>
        /// <br>Because if the setting fails (this only applies if the target property is contained inside a prefab)
        /// it calls <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/> to it.</br>
        /// </summary>
        protected void SetValueOfTargetDelegate(SerializedProperty property, T obj)
        {
            if (property == null)
                throw new NullReferenceException("[ScriptableObjectFieldInspector::SetValueOfTarget] Passed property parameter 'property' is null.");

            bool targetParentIsPrefab = PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);
            bool targetParentHasAssetPath = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));

            if ((targetParentIsPrefab || targetParentHasAssetPath) && obj != null)
            {
                // Pull the creation window thing
                string absPath = EditorUtility.SaveFilePanelInProject(string.Format("Create {0} Instance", typeof(T).Name), string.Empty, "asset", string.Empty); // contains absolute path to file
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
                    AssetDatabase.SaveAssetIfDirty(obj);
                    AssetDatabase.Refresh();

                    ProjectWindowUtil.ShowCreatedAsset(obj);
                }
                else
                {
                    Debug.LogWarning(string.Format("[ScriptableObjectFieldInspector::SetValueOfTarget] Asset path '{0}' isn't valid. Couldn't create file.", baseAssetFolder));
                    UnityEngine.Object.DestroyImmediate(obj);
                    return;
                }
            }

            SetValueOfTarget(property, obj);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propTarget = property.objectReferenceValue;
            var target = propTarget as T;

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

                                typeMenus.AddItem(new GUIContent(
                                    string.Format("New {0}{1}",
                                    // append a dot (.) if the namespace isn't blank.
                                    !string.IsNullOrWhiteSpace(type.Namespace) ? type.Namespace + "." : string.Empty, type.Name)), false,
                                    () =>
                                    {
                                        // Apply value
                                        SetValueOfTargetDelegate(property, (T)ScriptableObject.CreateInstance(type));
                                    });
                            }
                        }
                    }
                }

                // No items
                if (typeMenus.GetItemCount() <= 0)
                    typeMenus.AddDisabledItem(new GUIContent(string.Format("Disabled (Make classes inheriting from '{0}')", typeof(T).Name)), true);

                // Since the following scope is called only once, we can leech off that
                // Get custom inspector
                if (currentCustomInspector == null && target != null)
                {
                    // Just assign, the 'UseCustomInspector' also ignores the default ones.
                    currentCustomInspector = Editor.CreateEditor(target);

                    if (DebugMode && currentCustomInspector == null)
                        Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)::GetPropertyHeight(Search Custom Editor)] No suitable editor found for obj '{0}'.", target));
                }
            }

            // Check if object is null (generically)
            // If null, don't 
            if (target == null || !property.isExpanded)
                return SingleLineHeight;

            SObject ??= new SerializedObject(target);
            float h = 0f; // instead of using currentY, use a different inline variable

            // This is only checked if there's a valid target, meaning there's a custom editor created.
            if (!UseCustomInspector)
            {
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
            }
            else
            {
                // We don't know the adaptable height yet
                // Could use GUILayoutUtility.GetRect but wont't work because we need height before draw.
                // As a workaround, jk there's no workaround
                h = ReservedHeightCustomEditor;
            }

            // Add label height
            h += SingleLineHeight;

            return h;
        }

        // hack : position given is incorrect on EventType.Layout
        // drawing GUILayout editors require an GUILayout area, so we need the correct 'position'.
        private Rect correctPosition;
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

        /// <summary>
        /// Handles drawn <see cref="ScriptableObject"/>s, doing as the default behaviour for drawn non-<see cref="UnityEngine.Object"/> objects.
        /// </summary>
        protected void HandleDifferentDrawers(SerializedProperty property, T target)
        {
            // Do not make unique if the target is prefab or we have asset path
            // Because : prefabs can't store local scriptable objects (only scenes)
            // :: If the object exists in project view, no need to clone; it's very easy to clone anyways + we can use 'Delete' to lose reference on the current object.
            if (!MakeDrawnScriptableObjectsUnique)
                return;

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
                // Check if key is null, if so assign the target.
                if (drawnScriptableObjects[key] == null)
                {
                    drawnScriptableObjects[key] = target;
                }

                // If the key is not null (after assigning the target), don't ignore and start searching
                if (drawnScriptableObjects[key] != null)
                {
                    KeyValuePair<string, T> propPair;

                    try
                    {
                        // Same key pairs could be included, as this may not return a Single object.
                        propPair = drawnScriptableObjects.SingleOrDefault(p =>
                           // Key is different && Same object parent (using InstanceID as temp) && Same reference to object
                           p.Key != key && TrimRightPrefix(p.Key) == TrimRightPrefix(key) && p.Value == target);
                    }
                    catch (Exception e)
                    {
                        if (DebugMode)
                            Debug.LogWarning(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Exception occured while HandleDifferentDrawers. Not handling e={0}\n{1}", e.Message, e.StackTrace));
                        
                        // just needs a refreshin clear.
                        // note : this is a crap solution and may cause issues.
                        drawnScriptableObjects.Clear();
                        return;
                    }

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
                        // (ensure the passed pair object actually contains the clone on the appopriate property and fix issue #2)
                        else if (propPair.Key != key && existingProp.objectReferenceValue == propPair.Value)
                        {
                            // Clone 'target' and paste it to 'target'
                            // TODO : Only do this in the arrays (or test if other objects work [such as 2 monobehaviours drawn at the same time with same scriptable object] correctly),
                            // other inspectors 'probably' could contain a reference to the 'target'

                            if (DebugMode)
                                Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Copied target {0} to property {1}", target, existingProp.propertyPath));

                            // Copy cloned object reference into an actual clone
                            T instObject = UnityEngine.Object.Instantiate(propPair.Value);

                            // Assign the clone (ASSIGN LIKE THIS, JUST SETTING THE 'property.objectReferenceValue' DOES NOT WORK)
                            SetValueOfTarget(property, instObject, false);
                            target = instObject;                    // Set tempoary variables too

                            // Set the target dirty
                            // With this way, cloning of the 'ReorderableList' no longer causes issues,
                            // because this script (tells unity to) hunt for the same existing references with the same parent.
                            EditorUtility.SetDirty(target);

                            // Set the current dictionary key to be the 'instObject' (so that the object isn't cloned twice, or more)
                            drawnScriptableObjects[key] = target;
                        }
                    }
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // correct position if we aren't layouting
            if (Event.current.type != EventType.Layout)
            {
                correctPosition = position;
            }

            // The 'property' is for the object field that is only for assigning.
            EditorGUI.BeginProperty(position, label, property);
            currentY = position.y;

            // Gather target info
            // This works, because we are working with ScriptableObject's
            // Otherwise use BXFW's EditorAdditionals.GetTarget(SerializedProperty)
            UnityEngine.Object propTarget = property.objectReferenceValue;
            T target = propTarget as T;

            // Parent settings. If any of these are true, the following happens:
            // Unity's unmodifiable serializer decides that 'inline serialization' is too good and just serializes by fileid.
            // So, if any of these are true
            bool targetParentIsPrefab = PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);
            bool targetParentHasAssetPath = !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
            // Dynamic 'ShowOnProject' button + make unique is disabled if the prefab actually exists in project
            bool targetHasAssetPath = !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(target));

            // 'target' passes as reference because it has to be a class.
            if (!targetParentIsPrefab && !targetHasAssetPath)
                HandleDifferentDrawers(property, target);

            // GUI related
            float previousWidth = position.width;
            bool gEnabled = GUI.enabled;

            // Drag-Drop gui.
            EditorAdditionals.MakeDroppableAreaGUI(onDragAcceptAction: () =>
            {
                if (DebugMode)
                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] DragDrop: Dragged object stats => Length:{0}, Object:{1}", DragAndDrop.objectReferences.Length, DragAndDrop.objectReferences[0].GetType().BaseType));

                if (
                    DragAndDrop.objectReferences.Length == 1 &&
                    // Accept drag if the previous base type matches
                    DragAndDrop.objectReferences[0].GetType().GetBaseTypes().Contains(typeof(T))
                )
                {
                    // Clear if there is previous object.
                    if (targetHasAssetPath)
                        SetValueOfTarget(property, null);

                    SetValueOfTarget(property, DragAndDrop.objectReferences[0] as T);
                    target = DragAndDrop.objectReferences[0] as T;

                    if (DebugMode)
                        Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] DragDrop: Accepted & assigned target to '{0}'.", target));

                    // this repaints the propertydrawer
                    property.serializedObject.Update();
                    property.serializedObject.ApplyModifiedProperties();
                    // notify the unity that we set a variable and scene is modified
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }, new Rect(position) { height = SingleLineHeight });

            // Null target gui.
            if (target == null)
            {
                position.width = previousWidth * .4f;
                GUI.Label(position, label);
                position.x += previousWidth * .4f;

                position.width = previousWidth * .45f;
                if (GUI.Button(position, new GUIContent(
                    targetParentIsPrefab || targetParentHasAssetPath ? string.Format("Drag / Create {0} (child classes)", typeof(T).Name) : string.Format("Assign {0} (child classes)", typeof(T).Name),
                    targetParentIsPrefab || targetParentHasAssetPath ? @"Prefab scenes / Project Assets can't serialize local scriptable objects in themselves.
(This is due to how the unity serializer works, and i can't modify the behaviour)" : "You can also drag scriptable objects."), EditorStyles.popup))
                {
                    if (typeMenus != null)
                        typeMenus.ShowAsContext();
                    else
                    {
                        var m = new GenericMenu();
                        m.AddDisabledItem(new GUIContent("Click refresh to refresh the list."), true);
                        m.ShowAsContext();
                    }
                }
                position.x += previousWidth * .46f;

                GUI.enabled = !targetParentIsPrefab;

                position.width = previousWidth * .14f; // 1 - (.46f + .4f)
                if (GUI.Button(position, "Refresh"))
                {
                    typeMenus = null;
                }

                GUI.enabled = gEnabled;
                EditorGUI.EndProperty();
                return;
            }

            // why am i allowed to write GUI code? this is an abomination
            // well, atleast this is easily fixable, unlike my life xDDxDXDXD
            // buut, it works (and it has suprisingly ok performance) so why touch it?

            // -- Property label
            Rect rInspectorInfo = GetPropertyRect(position, SingleLineHeight); // width is equal to 'previousWidth'
            const float BTN_SHOWPRJ_WIDTH = .25f;
            const float BTN_DELETE_WIDTH = .15f;
            const float BTN_FOLDOUT_MIN_WIDTH = .033f;
            float rFoldoutRefWidth = 1f - BTN_DELETE_WIDTH;

            if (targetHasAssetPath)
                rFoldoutRefWidth -= BTN_SHOWPRJ_WIDTH;
            // set DisplayObjectNameEditor condition inline as we need the 'rFoldoutRefWidth' reference for the label name editor.
            rInspectorInfo.width = previousWidth * (DisplayObjectNameEditor ? BTN_FOLDOUT_MIN_WIDTH : rFoldoutRefWidth);

            property.isExpanded = EditorGUI.Foldout(rInspectorInfo, property.isExpanded, DisplayObjectNameEditor ? GUIContent.none : label);
            // This GUI element is inserted (foldout space is squished for this property), so yeah.
            if (DisplayObjectNameEditor)
            {
                GUI.enabled = !targetHasAssetPath;

                rInspectorInfo.x += previousWidth * BTN_FOLDOUT_MIN_WIDTH;
                float lblNameEditorWidth = 1f - (1f - (rFoldoutRefWidth - BTN_FOLDOUT_MIN_WIDTH));
                rInspectorInfo.width = previousWidth * lblNameEditorWidth;

                // Make 'read-only' if the file actually exists in project
                // Having Object.name and the file name different, unity doesn't like.
                EditorGUI.BeginChangeCheck();
                string tName = EditorGUI.TextField(rInspectorInfo, target.name);

                if (EditorGUI.EndChangeCheck() && !targetHasAssetPath)
                {
                    Undo.RecordObject(target, "set name");
                    target.name = tName;
                }
                rInspectorInfo.x -= previousWidth * BTN_FOLDOUT_MIN_WIDTH;

                GUI.enabled = true;
            }

            // 'Show On Project' button
            rInspectorInfo.x += previousWidth * rFoldoutRefWidth;   // Start pos
            rInspectorInfo.width = previousWidth * BTN_SHOWPRJ_WIDTH;
            if (targetHasAssetPath)
            {
                // Display a button to highlight the asset source
                if (GUI.Button(rInspectorInfo, "Show On Project"))
                {
                    // Highlight the source object in the 'Project' folder
                    ProjectWindowUtil.ShowCreatedAsset(target);
                }

                rInspectorInfo.x += previousWidth * BTN_SHOWPRJ_WIDTH;    // Add 1 more button to pos
            }

            rInspectorInfo.width = previousWidth * BTN_DELETE_WIDTH;
            if (GUI.Button(rInspectorInfo, "Delete"))
            {
                // If the object would still like to exist, don't do 'DestroyObjectImmediate', instead just remove the reference
                if (!targetHasAssetPath)
                {
                    Undo.DestroyObjectImmediate(target);
                }

                // Remove reference (no matter what, so that the reference is cleared and setting values to the previous one doesn't change 2 objects)
                SetValueOfTarget(property, null);
                EditorUtility.SetDirty(property.serializedObject.targetObject);

                EditorGUI.EndProperty();
                return;
            }

            // -- Main drawing
            if (property.isExpanded)
            {
                if (!UseCustomInspector)
                {
                    // Draw the custom inspector if we have that.
                    SObject ??= new SerializedObject(target);
                    SObject.UpdateIfRequiredOrScript();

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
                else
                {
                    // apparently the only problem was that we weren't getting the correct PropertyDrawer position in Event.current.type == EventType.Layout
                    // that was (not only) the issue.
                    // Reserve gui area on the bottom and create a scrollable custom inspector rect

                    // eh whatever this will do if it works, we can't out-inspector the unity inspector (because c# internal keyword and reflection trash).
                    // in inspector we are doing nested GUILayout areas (which is big no-no)
                    // If we are in an reorderable array however, the CustomPropertyDrawer allows us for some reason

                    // all of the effort, just to be able to be only drawn inside other nested CustomPropertyDrawers
                    // unity W, me L
                    // If you want to tackle this too (i have schizophrenia, there's no you) just know that unity will most likely win
                    // because it likes to delete your GUILayout area, you also can't just call OnInspectorGUI inside other inspectors (nested GUILayout area moment)

                    // As workaround, just open the entire ScriptableObject in the inspector
                    // yes, waste of space. but unity won
                    try
                    {
                        // Save the current GUILayout setting (?)

                        // Begin, draw, exit new area
                        GUILayout.BeginArea(new Rect(correctPosition.x, correctPosition.y + SingleLineHeight, correctPosition.width, ReservedHeightCustomEditor));
                        customInspectorScrollPosition = GUILayout.BeginScrollView(customInspectorScrollPosition);

                        currentCustomInspector.OnInspectorGUI();

                        GUILayout.EndScrollView();
                        GUILayout.EndArea();

                        // Load the previous GUILayout settings (?, again)
                    }
                    catch (Exception e)
                    {
                        if (DebugMode)
                            Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)::DrawCustomInspector] Exception occurred while calling CustomInspector. Falling back to solely inspector view. This is a known issue. e={0}\n{1}", e.Message, e.StackTrace));

                        // OnInspectorGUI creates another area inside of another area, no matter what
                        // We have to convince the inspector of unity that : yes we are totally drawing the CustomPropertyDrawer, there's no other editors drawn here

                        // ok unity, you win. i give up
                        // the button will just select scriptable object if we can't draw the OnInspectorGUI
                        if (GUI.Button(new Rect(correctPosition.x, correctPosition.y + SingleLineHeight, correctPosition.width, ReservedHeightCustomEditor),
                            "View on Current Inspector\n(object has custom editor, thus cannot be\n viewed in a nested GUILayout area.)"))
                        {
                            AssetDatabase.OpenAsset(target);
                        }
                    }
                }
            }

            GUI.enabled = gEnabled;
            EditorGUI.EndProperty();
        }

        ~ScriptableObjectFieldInspector()
        {
            // Dispose required objects
            if (currentCustomInspector != null)
                UnityEngine.Object.DestroyImmediate(currentCustomInspector);
        }
    }
}
