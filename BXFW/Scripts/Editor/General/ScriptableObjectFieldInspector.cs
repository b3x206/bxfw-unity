using BXFW.Tools.Editor;

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Data type that contains a draw command.
    /// </summary>
    public struct DrawGUICommand<T>
        where T : ScriptableObject
    {
        /// <summary>
        /// The order that this command can be called in.
        /// </summary>
        public MatchGUIActionOrder Order;
        /// <summary>
        /// Return the given GUI height. Similar to <see cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/>.
        /// <br><c>Param1 [In]  : </c> Target <see cref="ScriptableObject"/> of <see cref="ScriptableObjectFieldInspector{T}"/>.</br>
        /// <br><c>Return [out] : </c> Intended GUI height.</br>
        /// <br/>
        /// <br>If this method is left blank, it is to be ignored and height is assumed as 0.</br>
        /// </summary>
        public Func<T, float> GetGUIHeight;
        /// <summary>
        /// Create the GUI in this delegate.
        /// <br><c>Param 1 [In] : </c> Target <see cref="ScriptableObject"/> of <see cref="ScriptableObjectFieldInspector{T}"/>.</br>
        /// <br><c>Param 2 [In] : </c> When this 'DrawGUI' call was made. This event can be called twice with all order flags set.</br>
        /// <br><c>Param 3 [In] : </c> Allocated rectangle for the given GUI.</br>
        /// </summary>
        public Action<T, MatchGUIActionOrder, Rect> DrawGUI;
    }

    public struct DrawGUILayoutCommand
    {
        public MatchGUIActionOrder Order;

        public Func<SerializedProperty, MatchGUIActionOrder> DrawGUI;
    }

    /// <summary>
    /// Creates a ScriptableObject inspector.
    /// <br>Derive from this class and use the <see cref="CustomPropertyDrawer"/> attribute with same type as <typeparamref name="T"/>.</br>
    /// </summary>
    public class ScriptableObjectFieldInspector<T> : PropertyDrawer
        where T : ScriptableObject
    {
        // TODO (bit higher but still low priority) :
        // 1. Allow the scripts to change the collapsed interface (DrawGUICommand for the collapsed interface)
        // 2. Refactor the script to make it much neater (note : This script is actually simple, just a lot of boilerplate for things that should be simple to do)
        // 3. Completely fix the IMGUI layouting injection hack (because it's terrible), but first fix the bugs of it.
        // TODO (low priority) :
        // 1. Add support for GUIElements on custom inspector overrides (only the legacy OnInspectorGUI is taken to count)
        

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
        protected virtual string DefalutInspectorTypeName => "GenericInspector";
        /// <summary>
        /// Returns whether if <see cref="currentCustomInspector"/> isn't null.
        /// <br>Only valid if there's a target object in the target property.</br>
        /// <br>Override to disable/enable this function, as it's <b>experimental</b>.</br>
        /// </summary>
        public virtual bool UseCustomInspector => currentCustomInspector != null && currentCustomInspector.GetType().Name != DefalutInspectorTypeName;
        /// <summary>
        /// Custom commands for the default inspector.
        /// </summary>
        public virtual Dictionary<string, DrawGUICommand<T>> DefaultInspectorCustomCommands => null;
        /// <summary>
        /// The target SerializedObject.
        /// <br>This is assigned as the target object on <see langword="base"/>.<see cref="GetPropertyHeight(SerializedProperty, GUIContent)"/>.</br>
        /// </summary>
        protected SerializedObject TargetSerializedObject { get; private set; }

        /// <summary>
        /// Allows to change the scriptable object <see cref="UnityEngine.Object.name"/> directly using a input field.
        /// </summary>
        protected virtual bool DisplayObjectNameEditor => false;
        /// <summary>
        /// If the name editor is being displayed (<see cref="DisplayObjectNameEditor"/> is true),
        /// overriding this as true will set the name into GetType().Name instead of setting it empty.
        /// <br>
        /// This does not enforce different names on an array, so 2 objects with same names can coexist.
        /// (but depending on <see cref="MakeDrawnScriptableObjectsUnique"/>, those will be cloned)
        /// </br>
        /// </summary>
        protected virtual bool NameEditorEnforceNonNullName => false;

        /// <summary>
        /// Position given is incorrect on EventType.Layout
        /// <br>Drawing GUILayout editors require a correct Repaint area, so we need the correct 'position' from the <see cref="EventType.Repaint"/>.</br>
        /// </summary>
        private Rect lastRepaintPosition;
        /// <summary>
        /// Main rect context manager used.
        /// </summary>
        protected readonly PropertyRectContext mainCtx = new PropertyRectContext();

        /// <summary>
        /// Height of the drawn foldout options.
        /// </summary>
        protected const float PropertyFoldoutOptionsHeight = 20f;
        /// <summary>
        /// Flag to enable to copy the already existing Value on <see cref="DrawnScriptableObjects"/>.
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
        protected float PaddedSingleLineHeight => EditorGUIUtility.singleLineHeight + mainCtx.Padding;

        /// <summary>
        /// Sets value of target safely.
        /// <br>The <paramref name="obj"/> passed can be anything.</br>
        /// </summary>
        protected void SetValueOfTarget(SerializedProperty property, T obj, bool allowUndo = true)
        {
            if (property.IsDisposed() || property.serializedObject.IsDisposed())
            {
                throw new NullReferenceException("[ScriptableObjectFieldInspector::SetValueOfTarget] Passed 'SerializedProperty' is <b>disposed</b>. This most likely happened because this method was called from a delegate.\nTry refreshing the current window to try again.");
            }

            UnityEngine.Object dirtyTargetObject = property.serializedObject.targetObject;
            if (allowUndo)
            {
                Undo.RecordObject(dirtyTargetObject, string.Format("Set value of {0}", property.name));
            }

            // Set value
            // If the parent is an array, set the target index into the 'obj'
            PropertyTargetInfo parentTargetPair = property.GetParentOfTargetField();
            object parent = parentTargetPair.value;

            if (obj != null)
            {
                // Refresh the CustomInspector with a new target, because we can't set target (we can, but unity seems to not really like it).
                if (currentCustomInspector != null)
                {
                    UnityEngine.Object.DestroyImmediate(currentCustomInspector);
                }

                currentCustomInspector = Editor.CreateEditor(obj);

                if (DebugMode && currentCustomInspector == null)
                {
                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)::SetValueOfTarget(Search Custom Editor)] No suitable editor found for obj '{0}'.", obj));
                }
            }

            // why (c# array moment)
            if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                // Set the index directly
                // the returned field info cannot be a direct reference / pointer to an array element (because c# arrays are weird)
                // (heck, even the fieldInfo variable that unity gives points to that element's array parent)
                // so we have to copy the entire 'IEnumerable' thing and paste into that array.

                int index = property.GetPropertyParentArrayIndex();

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
                    {
                        Debug.LogWarning("[ScriptableObjectFieldInspector(DebugMode)::SetValueOfTarget] Target is in field parent with interface 'IEnumerable' but falling back to default FieldInfo set method.");
                    }

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
            {
                EditorUtility.SetDirty(dirtyTargetObject);
            }

            // This is very necessary to save the reference!
            // (Otherwise it doesn't save the file reference unless it's a default, non-prefab unity scene)
            AssetDatabase.SaveAssetIfDirty(dirtyTargetObject);
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
            {
                throw new NullReferenceException("[ScriptableObjectFieldInspector::SetValueOfTarget] Passed property parameter 'property' is null.");
            }

            // property.serializedObject could be disposed if this is called from a delegate
            // If this is disposed, assume this isn't in an prefab or it has an asset path.
            bool isDisposedAny = property.IsDisposed() || property.serializedObject.IsDisposed();
            bool targetParentIsPrefab = !isDisposedAny && PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);
            bool targetParentHasAssetPath = !isDisposedAny && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));

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
            property.Dispose(); // Dispose after being called from delegate, as the passed property is a clone.
        }

        /// <summary>
        /// Menu that contains the create names and delegates.
        /// </summary>
        protected TypeSelectorDropdown typeMenus;
        /// <summary>
        /// The <see cref="SearchDropdown.OnElementSelectedEvent"/>'s delegate callback serialized object for <see cref="typeMenus"/>.
        /// </summary>
        protected SerializedObject typeSelectorSo;
        /// <summary>
        /// The <see cref="SearchDropdown.OnElementSelectedEvent"/>'s delegate callback serialized property for <see cref="typeMenus"/>.
        /// </summary>
        protected SerializedProperty typeSelectorProperty;
        /// <summary>
        /// Returns the new list of the <see cref="typeMenus"/>.
        /// </summary>
        protected void GetTypeMenuListFromProperty(SerializedProperty property)
        {
            // Use a 'TypeSelectorDropdown' as it's nicer than 'GenericMenu'
            typeMenus = new TypeSelectorDropdown(null, (Type t) => t.IsSubclassOf(typeof(T)) && t != typeof(T) && !t.IsAbstract, false);

            // The given 'property' parameter automatically disposes so copy the values.
            typeSelectorSo = new SerializedObject(property.serializedObject.targetObject);
            typeSelectorProperty = typeSelectorSo.FindProperty(property.propertyPath);
            typeMenus.OnElementSelectedEvent += (SearchDropdownElement element) =>
            {
                if (!(element is TypeSelectorDropdown.Item item))
                {
                    return;
                }

                // Apply value
                SetValueOfTargetDelegate(typeSelectorProperty, (T)ScriptableObject.CreateInstance(Type.GetType(item.assemblyQualifiedName)));
                // Dispose tempoary vars (this works because the 'typeMenus' list gets cleared when an item gets assigned,
                // or it's going to throw a sneaky 'InvalidPropertyException', hope it's the latter as this class is a mess)
                typeSelectorSo.Dispose();
                typeSelectorProperty.Dispose();
                // Don't need to dispose those, as the way i wrote/handled the TypeMenuList setting is kinda sloppy
                // This is because i dispose the 'copySo and 'copyProp', i need to check if those are valid and if not just create a new delegate.
                // Time to create class globals B)
            };
            typeMenus.NoElementPlaceholderText = string.Format("Disabled (Make classes inheriting from '{0}')", typeof(T).Name);
        }

        protected float previousCustomInspectorHeight = 1f;
        /// <summary>
        /// The previous <see cref="GUILayoutGroup"/> accessed on the <see cref="EventType.Layout"/> event.
        /// </summary>
        protected InternalGUILayoutGroup previousGUILayoutGroup;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UnityEngine.Object propTarget = property.objectReferenceValue;
            T target = propTarget as T;

            // Get types inheriting from 'T' on all assemblies
            // (this stayed as 'PlayerPowerup' from my other game, lol)
            if (typeMenus == null || typeSelectorSo.IsDisposed() || typeSelectorSo.IsDisposed())
            {
                GetTypeMenuListFromProperty(property);

                // Since the following scope is called only once, we can leech off that
                // Get custom inspector
                if (currentCustomInspector == null && target != null)
                {
                    // Just assign, the 'UseCustomInspector' also ignores the default ones.
                    currentCustomInspector = Editor.CreateEditor(target);

                    if (DebugMode && currentCustomInspector == null)
                    {
                        Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)::GetPropertyHeight(Search Custom Editor)] No suitable editor found for obj '{0}'.", target));
                    }
                }
            }

            // Check if object is null (generically)
            // If null, don't 
            if (target == null || !property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            }

            // Total height calculated
            float height = 0f;
            // Always assign this as it's the previous behaviour (not needed outside of default inspectors though)
            TargetSerializedObject ??= new SerializedObject(target);

            // This is only checked if there's a valid target, meaning there's a custom editor created.
            if (!UseCustomInspector)
            {
                SerializedProperty prop = TargetSerializedObject.GetIterator();
                bool expanded = true;
                while (prop.NextVisible(expanded))
                {
                    if (prop.propertyPath == "m_Script")
                    {
                        continue;
                    }

                    DrawGUICommand<T> cmd = default;
                    bool hasCustomEditorCommands = DefaultInspectorCustomCommands != null && DefaultInspectorCustomCommands.TryGetValue(prop.name, out cmd);

                    if (hasCustomEditorCommands)
                    {
                        if ((cmd.Order & MatchGUIActionOrder.Before) == MatchGUIActionOrder.Before || (cmd.Order & MatchGUIActionOrder.After) == MatchGUIActionOrder.After)
                        {
                            height += (cmd.GetGUIHeight?.Invoke(target) ?? 0) + mainCtx.Padding; // Height is agnostic of order
                        }

                        if ((cmd.Order & MatchGUIActionOrder.Omit) != MatchGUIActionOrder.Omit)
                        {
                            height += EditorGUI.GetPropertyHeight(prop, true) + mainCtx.Padding; // Add padding
                        }
                    }
                    else
                    {
                        height += EditorGUI.GetPropertyHeight(prop, true) + mainCtx.Padding; // Add padding
                    }

                    expanded = false; // used for the expand arrow of unity
                }
            }
            else
            {
                // A :
                // Only do this layout allocation here as GetPropertyHeight is also called on Repaint
                // Note :
                // In repaint, this will start drawing the GUI while the height is being calculated
                // Because of this, the height may not be updated immediately causing excess height to stay (until interacted with ofc)?
                if (Event.current.type == EventType.Layout)
                {
                    // Remove from the cached layouts (to not leak)
                    // Clearing it does work for some reason?
                    GUIAdditionals.CurrentLayout.RootWindows.Clear();

                    previousGUILayoutGroup = GUIAdditionals.BeginLayoutPosition(new Vector2(lastRepaintPosition.x, lastRepaintPosition.y + PaddedSingleLineHeight), lastRepaintPosition.width);
                    currentCustomInspector.OnInspectorGUI();
                    GUIAdditionals.EndLayoutPosition();

                    previousCustomInspectorHeight = previousGUILayoutGroup.CalculateHeight();
                }

                // We don't know the adaptable height yet
                // Didn't have to tally, just get access to the 'GUILayoutGroup' value after 'CalcHeight'.
                height = previousCustomInspectorHeight;
            }

            // Add label height
            //height += PaddedSingleLineHeight;
            height += PropertyFoldoutOptionsHeight + mainCtx.Padding;

            return height;
        }

        private const int DrawnListSizeLimit = 64;
        /// <summary>
        /// Currently drawn list of the scriptable objects. (or also known as a editor memory leak)
        /// </summary> 
        /// The plan is that we control the parent of the drawn scriptable objects and if the parents of the scriptable object match we create a clone
        /// By doing this there's no duplicate scriptable objects on an array, so there's no problems.
        protected static readonly Dictionary<string, T> DrawnScriptableObjects = new Dictionary<string, T>(DrawnListSizeLimit);
        /// <summary>
        /// Prefix to make the drawn object parent's unique, allowing for same objects on different parents.
        /// </summary>
        protected const string IDKeyPrefix = "::";
        /// <summary>
        /// Removes left of the string after the following char sequence of <see cref="IDKeyPrefix"/>.
        /// </summary>
        protected static string TrimLeftPrefix(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s.Substring(s.IndexOf(IDKeyPrefix) + IDKeyPrefix.Length);
        }
        /// <summary>
        /// Removes right of the string after the following char sequence of <see cref="IDKeyPrefix"/>.
        /// </summary>
        protected static string TrimRightPrefix(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return s.Substring(0, s.IndexOf(IDKeyPrefix) + IDKeyPrefix.Length);
        }
        /// <summary>
        /// Returns the identification key of this property.
        /// </summary>
        protected static string GetPropertyKeyString(SerializedProperty property)
        {
            return string.Format("{0}{1}{2}", property.serializedObject.targetObject.GetInstanceID(), IDKeyPrefix, property.propertyPath);
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
            {
                return;
            }

            // Get whether if the same item was drawn twice on the same array / SerializedProperty
            // If so, clone the second (current) item, as it's a reference to the previous item
            // only control the absolute parent as that contains the script.
            // ---
            // Contains the object_name+key_name (prefixed)
            // Allowing for the same named variable object (on different objects) to be not cloned every time.
            // ---
            // Unity targetObject names could be different or string.Empty, so use GetInstanceID as this PropertyDrawer only runs tempoarily
            // So instance ID's are feasible
            string key = GetPropertyKeyString(property);
            if (!DrawnScriptableObjects.ContainsKey(key))
            {
                // If the size of the added elements will exceed the size of limit, remove the first before adding
                // This is because the dictionary ordering is undefined behaviour.
                if (DrawnScriptableObjects.Count >= DrawnListSizeLimit)
                {
                    DrawnScriptableObjects.Remove(DrawnScriptableObjects.First().Key);
                }

                // Register key if it doesn't exist.
                DrawnScriptableObjects.Add(key, target);
            }
            else // do not check on the first OnGUI call as the target could not be ready.
            {
                // Check if key is null, if so assign the target.
                if (DrawnScriptableObjects[key] == null)
                {
                    DrawnScriptableObjects[key] = target;
                }

                // If the key is not null (after assigning the target), don't ignore and start searching
                if (DrawnScriptableObjects[key] != null)
                {
                    KeyValuePair<string, T> propPair;

                    try
                    {
                        // Same key pairs could be included, as this may not return a Single object.
                        propPair = DrawnScriptableObjects.SingleOrDefault(p =>
                           // Key is different && Same object parent (using InstanceID as temp) && Same reference to object
                           p.Key != key && TrimRightPrefix(p.Key) == TrimRightPrefix(key) && p.Value == target);
                    }
                    catch (Exception e)
                    {
                        if (DebugMode)
                        {
                            Debug.LogWarning(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Exception occured while HandleDifferentDrawers. Not handling e={0}\n{1}", e.Message, e.StackTrace));
                        }

                        // just needs a refreshin clear.
                        // note : this is a crap solution and may cause issues.
                        DrawnScriptableObjects.Clear();
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
                            {
                                Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Property '{0}' doesn't exist, removing.", key));
                            }

                            DrawnScriptableObjects.Remove(key);
                        }
                        // Key is already contained on different drawn property, and is being planned to be drawn for different property path.
                        // The 'SingleOrDefault' returns null string if there's no matching target value.
                        // (ensure the passed pair object actually contains the clone on the appopriate property and fix issue #2)
                        else if (propPair.Key != key && existingProp.objectReferenceValue == propPair.Value && propPair.Value != null)
                        {
                            // Clone 'target' and paste it to 'target'
                            // TODO : Only do this in the arrays (or test if other objects work [such as 2 monobehaviours drawn at the same time with same scriptable object] correctly),
                            // other inspectors 'probably' could contain a reference to the 'target'

                            if (DebugMode)
                            {
                                Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] Copied target {0} to property {1}", target, existingProp.propertyPath));
                            }

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
                            DrawnScriptableObjects[key] = target;
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
                lastRepaintPosition = position;
            }

            // The 'property' is for the object field that is only for assigning.
            EditorGUI.BeginProperty(position, label, property);
            mainCtx.Reset();

            // Gather target info
            // This works, because we are working with ScriptableObject's
            // Otherwise use BXFW's EditorAdditionals.GetTarget(SerializedProperty)
            UnityEngine.Object propTarget = property.objectReferenceValue;
            T target = propTarget as T;

            // Parent settings. If any of these are true, the following happens:
            // Unity's unmodifiable serializer decides that 'inline ScriptableObject serialization' is too good and just serializes by FileID.
            // So, if any of these (next 2 vars) are true, the user may have a chance to lose all of their data! (exciting, i know)
            // Kindly offer them a button to make it actually exist on the project so they don't lose their temporary data.
            bool targetParentIsPrefab = PrefabUtility.IsPartOfAnyPrefab(property.serializedObject.targetObject);
            bool targetParentHasAssetPath = !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
            // Dynamic 'ShowOnProject' button + make unique is disabled if the prefab actually exists in project
            bool targetHasAssetPath = !string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(target));

            // 'target' passes as reference because it has to be a class.
            if (!targetParentIsPrefab && !targetHasAssetPath)
            {
                HandleDifferentDrawers(property, target);
            }

            EditorGUIAdditionals.MakeDragDropArea(() =>
            {
                if (DebugMode)
                {
                    Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] DragDrop: Dragged object stats => Length:{0}, Object:{1}", DragAndDrop.objectReferences.Length, DragAndDrop.objectReferences[0].GetType().BaseType));
                }

                if (
                    DragAndDrop.objectReferences.Length == 1 &&
                    // Accept drag if the previous base type matches
                    DragAndDrop.objectReferences[0].GetType().GetBaseTypes().Contains(typeof(T))
                )
                {
                    // Clear if there is previous object.
                    if (targetHasAssetPath)
                    {
                        SetValueOfTarget(property, null);
                    }

                    SetValueOfTarget(property, DragAndDrop.objectReferences[0] as T);
                    target = DragAndDrop.objectReferences[0] as T;

                    if (DebugMode)
                    {
                        Debug.Log(string.Format("[ScriptableObjectFieldInspector(DebugMode)] DragDrop: Accepted & assigned target to '{0}'.", target));
                    }

                    // this repaints the propertydrawer
                    property.serializedObject.Update();
                    property.serializedObject.ApplyModifiedProperties();
                    // notify the unity that we set a variable and scene is modified
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }, () => GUI.enabled, new Rect(position) { height = PropertyFoldoutOptionsHeight + mainCtx.Padding });

            // Null target gui.
            if (target == null)
            {
                // Since this has single element, we can change the 
                position = mainCtx.GetPropertyRect(EditorGUI.IndentedRect(lastRepaintPosition), PropertyFoldoutOptionsHeight);

                // EditorGUIUtility.labelWidth doesn't account for indentation
                Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - EditorGUIAdditionals.IndentValue, position.height);
                GUI.Label(labelRect, label);

                using EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(targetParentIsPrefab);
                Rect buttonsRect = new Rect(position.x + labelRect.width, position.y, Mathf.Max(position.width - labelRect.width, EditorGUIUtility.fieldWidth), position.height);

                Rect createObjectButtonRect = new Rect(buttonsRect)
                {
                    width = buttonsRect.width * 0.75f
                };
                if (GUI.Button(createObjectButtonRect, new GUIContent(
                    targetParentIsPrefab || targetParentHasAssetPath ? string.Format("Drag / Create {0} (child classes)", typeof(T).Name) : string.Format("Assign {0} (child classes)", typeof(T).Name),
                    targetParentIsPrefab || targetParentHasAssetPath ? @"Prefab scenes / Project Assets can't serialize local scriptable objects in themselves.
(This is due to how the unity serializer works and i can't modify the behaviour)" : "You can also drag scriptable objects."), EditorStyles.popup))
                {
                    if (typeMenus == null || typeSelectorSo.IsDisposed() || typeSelectorSo.IsDisposed())
                    {
                        GetTypeMenuListFromProperty(property);
                    }

                    typeMenus.Show(createObjectButtonRect);
                }

                Rect refreshListButtonRect = new Rect(buttonsRect)
                {
                    x = buttonsRect.x + createObjectButtonRect.width,
                    width = buttonsRect.width * 0.25f
                };
                if (GUI.Button(refreshListButtonRect, "Refresh"))
                {
                    typeMenus = null;
                    TypeListProvider.Refresh();
                }

                EditorGUI.EndProperty();

                // Remove group element to prevent it from leaking GUILayoutGroups
                GUIAdditionals.CurrentLayout.RootWindows.Remove(previousGUILayoutGroup);
                return;
            }

            // -- Property label
            #region Property Collapse Header
            float positionWidth = position.width - EditorGUIAdditionals.IndentValue;
            Rect propertyFoldoutOptsRect = EditorGUI.IndentedRect(mainCtx.GetPropertyRect(position, PropertyFoldoutOptionsHeight));
            const float ShowProjectBtnWidth = 0.25f;
            const float DeleteBtnWidth = 0.15f;
            const float FoldoutBtnMinWidth = 0.033f;

            float foldoutOptsWidth = 1f - DeleteBtnWidth;
            if (targetHasAssetPath)
            {
                foldoutOptsWidth -= ShowProjectBtnWidth;
            }

            // set DisplayObjectNameEditor condition inline as we need the 'rFoldoutRefWidth' reference for the label name editor.
            propertyFoldoutOptsRect.width = positionWidth * (DisplayObjectNameEditor ? FoldoutBtnMinWidth : foldoutOptsWidth);

            property.isExpanded = EditorGUI.Foldout(propertyFoldoutOptsRect, property.isExpanded, DisplayObjectNameEditor ? GUIContent.none : label);
            // This GUI element is inserted (foldout space is squished for this property), so yeah.
            if (DisplayObjectNameEditor)
            {
                using EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(targetHasAssetPath);

                propertyFoldoutOptsRect.x += positionWidth * FoldoutBtnMinWidth;
                float lblNameEditorWidth = 1f - (1f - (foldoutOptsWidth - FoldoutBtnMinWidth));
                propertyFoldoutOptsRect.width = positionWidth * lblNameEditorWidth;

                // Make 'read-only' if the file actually exists in project
                // Having Object.name and the file name different, unity doesn't like.
                EditorGUI.BeginChangeCheck();

                // Display a 'placeholder colored text' that is actually the name, if enforcing non-null names
                GUIStyle tNameFieldStyle = new GUIStyle(GUI.skin.textField);
                if (target.name == target.GetType().Name && NameEditorEnforceNonNullName)
                {
                    tNameFieldStyle.normal.textColor = Color.gray;
                }

                string tName = EditorGUI.TextField(new Rect(propertyFoldoutOptsRect), target.name, tNameFieldStyle);

                // Object name is only mutable through the 'Project' window changing file name if the target has an asset path
                if (!targetHasAssetPath)
                {
                    // Enforcing non-null
                    if (NameEditorEnforceNonNullName && string.IsNullOrEmpty(tName))
                    {
                        // SetDirty for this, so that it's an automated thing
                        EditorUtility.SetDirty(target);
                        target.name = target.GetType().Name;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "set name");
                        target.name = tName;
                    }
                }
                propertyFoldoutOptsRect.x -= positionWidth * FoldoutBtnMinWidth;
            }

            // 'Show On Project' button
            propertyFoldoutOptsRect.x += positionWidth * foldoutOptsWidth;   // Start pos
            propertyFoldoutOptsRect.width = positionWidth * ShowProjectBtnWidth;
            if (targetHasAssetPath)
            {
                // Display a button to highlight the asset source
                if (GUI.Button(propertyFoldoutOptsRect, "Show On Project"))
                {
                    // Highlight the source object in the 'Project' folder
                    ProjectWindowUtil.ShowCreatedAsset(target);
                }

                propertyFoldoutOptsRect.x += positionWidth * ShowProjectBtnWidth;    // Add 1 more button to pos
            }

            propertyFoldoutOptsRect.width = positionWidth * DeleteBtnWidth;
            if (GUI.Button(propertyFoldoutOptsRect, "Delete"))
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
            #endregion

            // -- Main GUI Drawing
            if (property.isExpanded)
            {
                if (!UseCustomInspector)
                {
                    // Draw the custom inspector if we have that.
                    TargetSerializedObject ??= new SerializedObject(target);
                    TargetSerializedObject.UpdateIfRequiredOrScript();

                    // Draw fields
                    EditorGUI.indentLevel += 1;

                    SerializedProperty prop = TargetSerializedObject.GetIterator();
                    bool expanded = true;
                    while (prop.NextVisible(expanded))
                    {
                        if (prop.propertyPath == "m_Script")
                        {
                            continue;
                        }

                        DrawGUICommand<T> cmd = default;
                        bool hasCustomEditorCommands = DefaultInspectorCustomCommands != null && DefaultInspectorCustomCommands.TryGetValue(prop.name, out cmd);

                        if (hasCustomEditorCommands)
                        {
                            if ((cmd.Order & MatchGUIActionOrder.Before) == MatchGUIActionOrder.Before)
                            {
                                cmd.DrawGUI(target, MatchGUIActionOrder.Before, mainCtx.GetPropertyRect(position, cmd.GetGUIHeight(target)));
                            }

                            if ((cmd.Order & MatchGUIActionOrder.Omit) != MatchGUIActionOrder.Omit)
                            {
                                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, prop), prop, true);
                            }

                            if ((cmd.Order & MatchGUIActionOrder.After) == MatchGUIActionOrder.After)
                            {
                                cmd.DrawGUI(target, MatchGUIActionOrder.After, mainCtx.GetPropertyRect(position, cmd.GetGUIHeight(target)));
                            }
                        }
                        else
                        {
                            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, prop), prop, true);
                        }

                        expanded = false;
                    }
                    EditorGUI.indentLevel -= 1;

                    TargetSerializedObject.ApplyModifiedProperties();
                }
                else
                {
                    // This, was, that easy?
                    // Okay. Fine. I will be better next time. Or you know do more research.
                    // Or maybe i got more experienced in GUI? Who knows. The stars have aligned.
                    // I knew this wasn't gonna be this easy.
                    // So enter the 1238173 lines of more code and a day of research and here we are.

                    // --
                    // Here's the workaround : 
                    // A : Do the Event.current.type == EventType.Layout related laying out in GetPropertyHeight
                    // B : Do the repaint and the rest of the events in here.
                    // -- 
                    // Note : Exceptions still may occur, this is NOT the finalized version.
                    // --
                    // B :
                    // Background Drawing Rect (for prettier display)
                    EditorGUI.indentLevel += 1;
                    Rect areaRect = new Rect(
                        lastRepaintPosition.x, lastRepaintPosition.y + propertyFoldoutOptsRect.height + mainCtx.Padding, 
                        lastRepaintPosition.width, lastRepaintPosition.height - (propertyFoldoutOptsRect.height + mainCtx.Padding)
                    );
                    EditorGUI.DrawRect(areaRect, EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.91f, 0.91f, 0.91f));

                    // Flex space with nesting? WE HAVE THOSE NOW!
                    // Note : Nested elements don't draw if we send a 'Used' event on the parent 'OnInspectorGUI's
                    // --
                    // Whoops, ReorderableList is now trying to do it's own things
                    // Time to inject GUILayoutEntry into the current window context while in Repaint. HAHAHAHAHAHAHA

                    // Injection "works", *BUT
                    // *1 : The 'OnInspectorGUI's layout elements are not taken to the account,
                    // causing the same stupid "cannot make GUI element because EventType.Layout
                    // didn't allocate elements" because the ReorderableList didn't call 'GetHeight' again
                    // --
                    // So the injection code will stay (until i refactor the stuff)
                    // And the next method will just be forcing redraw if we are on a 'ReorderableList'
                    // --
                    // 2 : Cache the GUILayoutGroup of the 'OnInspectorGUI'
                    // This will most likely work?
                    // Yup that solved it. Caching the GUILayoutGroup and then pushing it to the layout from a cached layouting is working.
                    // 3 :
                    // EventType.Used > EventType.Layout causes nested GUI to be positioned incorrectly (position correction needed !!)
                    // This is fixed by also positioning the children because setting EntryRect doesn't position children.

                    // --
                    // Because, RootWindows, has to do with IMGUI input
                    // Yay. Who could have guessed it?
                    if (previousGUILayoutGroup != null)
                    {
                        // Fix the 'EntryRect' shifting around for absolutely no reason
                        Rect correctEntryRect = new Rect(areaRect.x, areaRect.y, areaRect.width, 0f);
                        previousGUILayoutGroup.EntryRect = correctEntryRect;
                        previousGUILayoutGroup.FixAllChildEntryRects(correctEntryRect.position, correctEntryRect.width);

                        GUIAdditionals.CurrentLayout.PushLayoutGroup(previousGUILayoutGroup);

                        try
                        {
                            currentCustomInspector.OnInspectorGUI();

                            if (EditorUtility.IsDirty(target))
                            {
                                EditorAdditionals.CallOnValidate(target);
                            }
                        }
                        catch (Exception e)
                        {
                            if (DebugMode)
                            {
                                Debug.LogWarning("[ScriptableObjectFieldInspector::OnGUI] An exception occurred during drawing of the overriding Editor GUI. The next log will contain the details.");
                                Debug.LogException(e);
                            }
                        }

                        // My beloved debugging box, useful for times where to see whether if we are leaking UI Groups / Entries of course
                        if (DebugMode)
                        {
                            GUI.Box(new Rect(areaRect) { height = 95f }, new GUIContent($"[CustomInspector {Event.current.type}]\nEntryRect : {previousGUILayoutGroup?.EntryRect}\nPreviousGUILayoutGroup Count : {previousGUILayoutGroup?.Count ?? -1}\nRootWindows Count : {GUIAdditionals.CurrentLayout.RootWindows.Count}\n LastTopLevel Count : {GUIAdditionals.CurrentLayout.LastTopLevelGroup.Count}"));
                        }

                        // We aren't layouting
                        // This means that 'RootWindows' won't clear itself
                        // 'RootWindows' is used in event proccessing
                        // So only remove it if it's gonna be a duplicate
                        // --
                        // oh and yes, we also have to manually manage this crap as well
                        previousGUILayoutGroup.ResetCursor();
                        GUIAdditionals.CurrentLayout.PopLastLayoutGroup();
                    }

                    // and when injected, we aren't actually starting a new GUI group
                    // starting a new GUI group offsets for no reason
                    // --
                    // TODO : Allow for creation of a clipped area
                    // For some reason GUI.BeginArea and GUI.EndArea seems to shift the rect position by some value (huh i wonder why)
                    // And GUI.BeginClip only renders one UI with 'areaRect'
                    // GUILayout.EndArea();

                    EditorGUI.indentLevel -= 1;
                }
            }

            EditorGUI.EndProperty();
        }

        ~ScriptableObjectFieldInspector()
        {
            // Dispose required objects
            if (currentCustomInspector != null)
            {
                UnityEngine.Object.DestroyImmediate(currentCustomInspector);
            }
        }
    }
}
