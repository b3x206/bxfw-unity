using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using Codice.Client.BaseCommands.BranchExplorer;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Order of drawing GUI when a match is satisfied in method :
    /// <see cref="EditorAdditionals.DrawCustomDefaultInspector(SerializedObject, Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}})"/>.
    /// </summary>
    [Flags]
    public enum MatchGUIActionOrder
    {
        // Default Value
        Before = 0,
        After = 1 << 0,
        Omit = 1 << 1,

        OmitAndInvoke = After | Omit
    }

    public static class EditorAdditionals
    {
        #region Other
        /// <summary>
        /// Directory of the 'Resources' file (for bxfw assets generally).
        /// <br>Returns the 'Editor' and other necessary folders for methods that take absolute paths.</br>
        /// </summary>
        public static readonly string ResourcesDirectory = string.Format("{0}/Assets/Resources", Directory.GetCurrentDirectory());
        #endregion

        #region Prefab Utility
        /// <summary>
        /// NOTES ABOUT THIS CLASS:
        /// <para>
        ///     1: It handles creation 
        ///     <br>2: It edits (because it's callback of <see cref="ProjectWindowUtil.StartNameEditingIfProjectWindowExists(int, EndNameEditAction, string, Texture2D, string)"/>, what type of method is that?)</br>
        /// </para>
        /// </summary>
        internal class CreateAssetEndNameEditAction : EndNameEditAction
        {
            /// <summary>
            /// Called when the creation ends.
            /// <br>The int parameter returns the <see cref="UnityEngine.Object.GetInstanceID"/>.</br>
            /// </summary>
            internal event Action<int> OnRenameEnd;

            /// <summary>
            /// Action to invoke.
            /// <br>If the object exists (<paramref name="instanceId"/> isn't invalid) it will use <see cref="AssetDatabase.CreateAsset(UnityEngine.Object, string)"/></br>
            /// <br>If the object does NOT exist (<paramref name="instanceId"/> IS invalid) it will use <see cref="AssetDatabase.CopyAsset(string, string)"/>.</br>
            /// </summary>
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string uniqueName = AssetDatabase.GenerateUniqueAssetPath(pathName);
                if ((instanceId == 0 || instanceId == int.MaxValue - 1) && !string.IsNullOrEmpty(resourceFile))
                {
                    // Copy existing asset (if no reference asset was given)
                    AssetDatabase.CopyAsset(resourceFile, uniqueName);
                }
                else
                {
                    // Create new asset from asset (if reference asset was given)
                    AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), uniqueName);
                }

                // Handle events
                OnRenameEnd?.Invoke(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(uniqueName).GetInstanceID());
            }
        }

        /// <summary>
        /// Creates an instance of prefab <paramref name="prefabReferenceTarget"/> and renames it like an new object was created.
        /// <br><b>NOTE</b> : Make sure '<paramref name="prefabReferenceTarget"/>' is an prefab!</br>
        /// </summary>
        /// <param name="prefabReferenceTarget">The prefab target. Make sure this is an prefab.</param>
        /// <param name="path">Creation path. If left null the current folder will be selected.</param>
        /// <param name="onRenameEnd">Called when object is renamed. The <see cref="int"/> parameter is the InstanceID of the object.</param>
        // Use <see cref="EditorUtility"/> & <see cref="AssetDatabase"/>'s utility functions to make meaning out of it.
        public static void CopyPrefabReferenceAndRename(GameObject prefabReferenceTarget, string path = null, Action<int> onRenameEnd = null)
        {
            // Create at the selected directory
            if (string.IsNullOrEmpty(path))
                path = Selection.activeObject == null ? "Assets" : AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                Debug.LogError(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Directory '{0}' does not exist.", path));
                return;
            }
            if (PrefabUtility.GetCorrespondingObjectFromSource(prefabReferenceTarget) == null)
            {
                Debug.LogError(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Prefab to copy is invalid (not a prefab). prefabTarget was = '{0}'", prefabReferenceTarget));
                return;
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                // Get path & target prefab to copy
                GameObject targetPrefabInst = prefabReferenceTarget;
                path = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(path, targetPrefabInst.name)}.prefab"); // we are copying prefabs anyway

                // Register 'OnFileNamingEnd' function.
                var assetEndNameAction = ScriptableObject.CreateInstance<CreateAssetEndNameEditAction>();
                assetEndNameAction.OnRenameEnd += (int instanceIDSaved) =>
                {
                    var createdObject = EditorUtility.InstanceIDToObject(instanceIDSaved);

                    Selection.activeObject = createdObject; // Select renamed object
                };
                assetEndNameAction.OnRenameEnd += onRenameEnd;

                // wow very obvious (unity api moment)
                Texture2D icon = AssetPreview.GetMiniThumbnail(targetPrefabInst); // Get the thumbnail from the target prefab

                // Since this method is 'very well documented' here's what i found =>
                // instanceID   = Target instance ID to edit (this is handled in the file rename callback ending)
                //      (if it exists it will also edit that file alongside, we will create our own asset path so we pass invalid value, otherwise the object will be cloned.)
                // pathName     = Directory to file of the destination asset
                // resourceName = Directory to file of the source asset
                // icon         = Asset icon, not very necessary (can be null)

                // THIS. IS. SO. DUMB. (that even unity's asset developers wrote a wrapper function for this method lol)
                // Note : Pass invalid 'Instance ID' for editing an new object
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(int.MaxValue - 1, assetEndNameAction, path, icon, AssetDatabase.GetAssetPath(targetPrefabInst.GetInstanceID()));
            }
            else
            {
                Debug.LogWarning($"[ShopItemPreviewEditor::CopyPrefabInstanceAndRename] Path received for creating prefab '{path}' is not a path.");
            }
        }
        // TODO : Add method to also copy from already existing instance id (overwrite method?)
        #endregion

        #region Property Field / Custom Property Drawer Helpers
        // This allows for getting the property field target

        // we could use c# string method abuse or SerializedObject.GetArrayIndexSomething(index) method.
        // No, not really that is for getting the array object? idk this works good so no touchy unless it breaks
        private static readonly Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

        /// <summary>
        /// Returns the c# object's fieldInfo and the instance object it comes with.
        /// <br>Important NOTE : The instance object that gets returned with this method may be null.</br>
        /// <br>In these cases use the <see langword="return"/>'s field info.</br>
        /// <br>
        /// NOTE 2 : The field info returned may not be the exact field info, 
        /// as such case usually happens when you try to call 'GetTarget' on an array element.
        /// In this case, to change the value of the array, you may need to copy the entire array, and call <see cref="FieldInfo.SetValue"/> to it.
        /// </br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/> <exception cref="NullReferenceException"/>
        public static KeyValuePair<FieldInfo, object> GetTarget(this SerializedProperty prop)
        {
            if (prop == null)
                throw new NullReferenceException("[EditorAdditionals::GetTarget] Field 'prop' is null!");

            return GetTarget(prop.serializedObject.targetObject, prop.propertyPath);
        }
        /// <summary>
        /// Returns the c# object's fieldInfo and the PARENT object it comes with. (this is useful with <see langword="struct"/>)
        /// <br>Important NOTE : The instance object that gets returned with this method may be null (or not).
        /// In these cases use the return (the FieldInfo)</br>
        /// <br/>
        /// <br>If you are using this for <see cref="CustomPropertyDrawer"/>, this class has an <see cref="FieldInfo"/> property named <c>fieldInfo</c>, 
        /// you can use that instead of the bundled field info.</br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/> <exception cref="NullReferenceException"/>
        public static KeyValuePair<FieldInfo, object> GetParentOfTargetField(this SerializedProperty prop, int parentDepth = 1)
        {
            int lastIndexOfPeriod = prop.propertyPath.LastIndexOf('.');
            for (int i = 1; i < parentDepth; i++)
                lastIndexOfPeriod = prop.propertyPath.LastIndexOf('.', lastIndexOfPeriod - 1);

            if (lastIndexOfPeriod == -1)
            {
                // No depth, instead return the field info from this scriptable object (use the parent scriptable object ofc)
                var fInfo = GetField(prop.serializedObject.targetObject, prop.name);

                // Return the 'serializedObject.targetObject' as target, because it isn't a field (is literally an pointer) 
                return new KeyValuePair<FieldInfo, object>(fInfo, prop.serializedObject.targetObject);
            }

            // lastPropertyName is buggy, it usually most likely assumes the invalid depth?
            string propertyNamesExceptLast = prop.propertyPath.Substring(0, lastIndexOfPeriod);

            var pair = GetTarget(prop.serializedObject.targetObject, propertyNamesExceptLast);

            //return new KeyValuePair<FieldInfo, object>(pair.Key.FieldType.GetField(lastPropertyName), pair.Value);
            return pair;
        }

        /// <summary>
        /// Internal method to get parent from these given parameters.
        /// <br>Traverses <paramref name="propertyRootParent"/> using reflection and with the help of <paramref name="propertyPath"/>.</br>
        /// </summary>
        /// <param name="propertyRootParent">Target (parent) object of <see cref="SerializedProperty"/>. Pass <see cref="SerializedProperty.serializedObject"/>.targetObject.</param>
        /// <param name="propertyPath">Path of the property. Pass <see cref="SerializedProperty.propertyPath"/>.</param>
        /// <exception cref="Exception"/>
        private static KeyValuePair<FieldInfo, object> GetTarget(UnityEngine.Object propertyRootParent, string propertyPath)
        {
            object target = propertyRootParent; // This is kinda required
            FieldInfo targetInfo = null;
            string[] propertyNames = propertyPath.Split('.');

            bool isNextPropertyArrayIndex = false;

            for (int i = 0; i < propertyNames.Length && target != null; i++)
            {
                // Alias the string name. (but we need for for the 'i' variable)
                string propName = propertyNames[i];

                if (propName == "Array" && target is IEnumerable)
                {
                    // Arrays in property path's are seperated like -> Array.data[index]
                    isNextPropertyArrayIndex = true;
                }
                else if (isNextPropertyArrayIndex)
                {
                    // Gather -> data[index] -> the value on the 'index'
                    isNextPropertyArrayIndex = false;
                    Match m = ArrayIndexCapturePattern.Match(propName);

                    // Object is actually an array that unity serializes
                    if (m.Success)
                    {
                        var arrayIndex = int.Parse(m.Groups[1].Value);

                        if (!(target is IEnumerable targetAsArray))
                            throw new Exception(string.Format(@"[EditorAdditionals::GetTarget] Error while casting targetAsArray.
-> Invalid cast : Tried to cast type {0} as IEnumerable. Current property is {1}.", target.GetType().Name, propName));

                        // FIXME : Should use 'MoveNext' but i don't care. (stupid 'IEnumerator' wasn't started errors).
                        var cntIndex = 0;
                        var isSuccess = false;
                        foreach (object item in targetAsArray)
                        {
                            if (cntIndex == arrayIndex)
                            {
                                // Update FieldInfo

                                // oh wait, that's impossible, riiight.
                                // basically FieldInfo can't point into a c# array element member, only the target as it's just the object
                                // Because it isn't an actual field.
                                // could use some unsafe {} but that doesn't put a guarantee of whether if will solve it.
                                // (which it most likely won't because our result data is in ''safe'' FieldInfo type)

                                // If the array contains a member that actually has a field, it updates fine though.
                                // So you could use a wrapper class that just contains an explicit field (but we can't act like that, because c# arrays are covariant)
                                // whatever just look at this : https://stackoverflow.com/questions/13790527/c-sharp-fieldinfo-setvalue-with-an-array-parameter-and-arbitrary-element-type

                                // ---------- No FieldInfo? -------------
                                // (would like to put ascii megamind here, but git will most likely break it)
                                target = item;
                                isSuccess = true;

                                break;
                            }

                            cntIndex++;
                        }

                        if (!isSuccess)
                            throw new Exception(string.Format("[EditorAdditionals::GetTarget] Couldn't find SerializedProperty {0} in array {1}.", propertyPath, targetAsArray));
                    }
                    else // Array parse failure, should only happen on the ends of the array (i.e size field)
                    {
                        // Instead of throwing an exception, get the object
                        // (as this may be called for the 'int size field' on the editor, for some reason)
                        try
                        {
                            targetInfo = GetField(target, propName);
                            target = targetInfo.GetValue(target);
                        }
                        catch
                        {
                            // It can also have an non-existent field for some reason
                            // Because unity, so we give up (with the last information we have)
                            // Maybe we should print a warning, but it's not too much of a thing (just a fallback)

                            return new KeyValuePair<FieldInfo, object>(targetInfo, target);
                        }
                    }
                }
                else
                {
                    targetInfo = GetField(target, propName);
                    target = targetInfo.GetValue(target);
                }
            }

            return new KeyValuePair<FieldInfo, object>(targetInfo, target);
        }
        /// <summary>
        /// Returns the type of the property's target.
        /// </summary>
        /// <param name="property">Property to get type from.</param>
        public static Type GetPropertyType(this SerializedProperty property)
        {
            return property.GetTarget().Key.FieldType;
        }
        /// <summary>
        /// Returns the (last) index of this property in the array.
        /// <br>Returns <c>-1</c> if not in an array.</br>
        /// </summary>
        public static int GetPropertyArrayIndex(this SerializedProperty property)
        {
            const string AD_STR = "Array.data[";
            int arrayDefLastIndex = property.propertyPath.LastIndexOf(AD_STR);
            if (arrayDefLastIndex < 0)
                return -1;

            string indStr = property.propertyPath.Substring(arrayDefLastIndex + AD_STR.Length).TrimEnd(']');
            return int.Parse(indStr);
        }
        /// <summary>
        /// Internal helper method for getting field from properties.
        /// <br>Gets the target normally, if not found searches in <see cref="Type.BaseType"/>.</br>
        /// </summary>
        private static FieldInfo GetField(object target, string name, Type targetType = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Null 'name' field. (target: '{0}', targetType: '{1}')", target, targetType));

            if (targetType == null)
            {
                targetType = target.GetType();
            }

            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // If the field info is present.
            if (fi != null)
            {
                return fi;
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetField(target, name, targetType.BaseType);
            }

            throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Could not find '{0}' on '{1}' and it's children.", name, target));
        }
        
        /// <summary>
        /// Get the property drawer for the field type that you are inspecting.
        /// <br>Very useful for <c>Attribute</c> targeting PropertyDrawers.</br>
        /// <br>Will throw <see cref="InvalidOperationException"/> if called from a property drawer that's target is an actual class.</br>
        /// </summary>
        /// TODO : Maybe create a new 'PropertyDrawer' class named 'AttributePropertyDrawer' with better enforcement?
        public static PropertyDrawer GetTargetPropertyDrawer(PropertyDrawer requester)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester), "[EditorAdditionals::GetPropertyDrawer] Passed parameter was null.");

            // Get the attribute type for target PropertyDrawer's CustomPropertyDrawer target type
            Type attributeTargetType = (Type)typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(requester.GetType().GetCustomAttribute(typeof(CustomPropertyDrawer)));
            if (!attributeTargetType.GetBaseTypes().Contains(typeof(Attribute)))
                throw new InvalidOperationException(string.Format("[EditorAdditionals::GetPropertyDrawer] Tried to get a property drawer from drawer {0}, use this method only on ATTRIBUTE targeting property drawers. Returned 'PropertyDrawer' will be junk, and will cause 'StackOverflowException'.", requester.GetType()));

            Type propertyDrawerType = (Type)Assembly.GetAssembly(typeof(SceneView)).             // Internal class is contained in the same assembly (UnityEditor.CoreModule)
                GetType("UnityEditor.ScriptAttributeUtility", true).                             // Internal class that has dictionary for all custom PropertyDrawer's
                GetMethod("GetDrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static). // Utility method to get type from the internal class
                Invoke(null, new object[] { requester.fieldInfo.FieldType });                    // Call with the type parameter. It will return a type that needs instantiation using Activator.

            // Ignore this, this means that there's no 'PropertyDrawer' implemented.
            if (propertyDrawerType == null)
                return null;

            PropertyDrawer resultDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            if (resultDrawer != null)
            {
                // Leave m_Attribute as is, there's no need to access that.
                typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(resultDrawer, requester.fieldInfo);
            }

            return resultDrawer;
        }
        #endregion

        #region Inspector-Editor Draw
        /// <summary>
        /// Repaints inspector(s) with target <see cref="SerializedObject"/> <paramref name="obj"/>.
        /// <br>NOTE: Only works for custom editors stored inside the <c>Inspector</c> window, for all other windows use <see cref="RepaintAll"/>.</br>
        /// </summary>
        public static void RepaintInspector(SerializedObject obj)
        {
            foreach (var i in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (i.serializedObject != obj)
                    continue;

                i.Repaint();
            }
        }
        /// <summary>
        /// Repaints all editor windows that's currently open.
        /// <br>NOTE: This method is a performance hog. Please use it on events and such.</br>
        /// </summary>
        public static void RepaintAll()
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                w.Repaint();
            }
        }
        /// <summary>
        /// Make gui area drag and droppable.
        /// </summary>
        public static void MakeDroppableAreaGUI(Action onDragAcceptAction, Func<bool> shouldAcceptDragCheck, Rect? customRect = null)
        {
            var shouldAcceptDrag = shouldAcceptDragCheck.Invoke();
            if (!shouldAcceptDrag) return;

            MakeDroppableAreaGUI(onDragAcceptAction, customRect);
        }
        /// <summary>
        /// Make gui area drag and drop.
        /// <br>This always accepts drops.</br>
        /// <br>Usage : <see cref="DragAndDrop.objectReferences"/> is all you need.</br>
        /// </summary>
        public static void MakeDroppableAreaGUI(Action onDragAcceptAction, Rect? customRect = null)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Rect dropArea = customRect ??
                        GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (!dropArea.Contains(evt.mousePosition))
                        return;

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
        /// Draw default inspector with commands inbetween. (Allowing to put custom gui between).
        /// <br>This works as same as <see cref="UnityEditor.Editor.OnInspectorGUI"/>'s <see langword="base"/> call.</br>
        /// </summary>
        /// <param name="onStringMatchEvent">
        /// The event <see cref="MatchGUIActionOrder"/> match. 
        /// If passed <see langword="null"/> this method will act like <see cref="UnityEditor.Editor.DrawDefaultInspector"/>.
        /// </param>
        /// <example>
        /// // Info : The Generic '/\' is replaced with '[]'.
        /// serializedObject.DrawDefaultInspector(new Dictionary[string, KeyValuePair[MatchGUIActionOrder, Action]] 
        /// {
        ///     { nameof(FromAnyClass.ElementNameYouWant), new KeyValuePair[MatchGUIActionOrder, System.Action](MatchGUIActionOrder.Before, () => 
        ///         {
        ///             // Write your commands here.
        ///         })
        ///     }
        /// });
        /// </example>
        /// <returns><see cref="EditorGUI.EndChangeCheck"/> (whether if a field was modified inside this method)</returns>
        public static bool DrawCustomDefaultInspector(this SerializedObject obj, Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> onStringMatchEvent)
        {
            if (obj == null)
            {
                Debug.LogError("[BXFW::EditorAdditionals::DrawCustomDefaultInspector] Passed serialized object is null.");
                return false;
            }

            // Do the important steps (otherwise the inspector won't work)
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();

            // Loop through properties and create one field (including children) foreach top level property.
            // Why unity? includeChildren = 'expanded'
            SerializedProperty property = obj.GetIterator();
            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                // Disable if 'm_Script' field that unity adds automatically is being drawn.
                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                {
                    // -- Check if there is a match
                    if (onStringMatchEvent != null)
                    {
                        string MatchingKey = null;
                        foreach (string s in onStringMatchEvent.Keys)
                        {
                            if (s.Equals(property.name))
                            {
                                MatchingKey = s;
                                break;
                            }
                        }

                        // -- If there's a match of events, use the event.
                        // Once the scope enters here, the upcoming 'EditorGUILayout.PropertyField' is not invoked.
                        if (!string.IsNullOrEmpty(MatchingKey))
                        {
                            var Pair = onStringMatchEvent[MatchingKey];

                            if (Pair.Key == MatchGUIActionOrder.OmitAndInvoke)
                            {
                                if (Pair.Value != null)
                                    Pair.Value();

                                expanded = false;
                                continue;
                            }

                            // -- Omit GUI
                            if (Pair.Key == MatchGUIActionOrder.Omit)
                            {
                                expanded = false;
                                continue;
                            }

                            // -- Standard draw
                            if (Pair.Key == MatchGUIActionOrder.After)
                            { EditorGUILayout.PropertyField(property, true); }

                            if (Pair.Value != null)
                                Pair.Value();

                            if (Pair.Key == MatchGUIActionOrder.Before)
                            { EditorGUILayout.PropertyField(property, true); }

                            expanded = false;
                            continue;
                        }
                    }

                    EditorGUILayout.PropertyField(property, true);
                }

                expanded = false;
            }

            // Save & end method
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }

        /// <summary>
        /// Returns the children of the SerializedProperty.
        /// </summary>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);

            while (true)
            {
                if (SerializedProperty.EqualContents(property, nextElement))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets visible children of '<see cref="SerializedProperty"/>' at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent '<see cref="SerializedProperty"/>'.</param>
        /// <returns>Collection of '<see cref="SerializedProperty"/>' children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    // Use '.Copy' for making 'Linq ToArray' work
                    using var ret = currentProperty.Copy();
                    yield return ret;
                }
                while (currentProperty.NextVisible(false));
            }
        }

        [Flags]
        public enum EditorListOption
        {
            None = 0,
            ListSize = 1,
            ListLabel = 2,
            Default = ListSize | ListLabel
        }
        /// <summary>
        /// Shows an array inspector (using unity default).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="options"></param>
        public static void ShowEditorList(SerializedProperty list, EditorListOption options = EditorListOption.Default)
        {
            bool showListLabel = (options & EditorListOption.ListLabel) != 0, showListSize = (options & EditorListOption.ListSize) != 0;

            if (showListLabel)
            {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }

            if (!showListLabel || list.isExpanded)
            {
                if (showListSize)
                { EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size")); }

                for (int i = 0; i < list.arraySize; i++)
                { EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i)); }
            }

            if (showListLabel)
            { EditorGUI.indentLevel -= 1; }
        }
        /// <summary>
        /// Create array with fields.
        /// This is a more primitive array drawer, but it works.
        /// </summary>
        /// <param name="obj">Serialized object of target.</param>
        /// <param name="arrayName">Array field name.</param>
        public static void UnityArrayGUI(this SerializedObject obj, string arrayName, Action<int> OnArrayFieldDrawn = null)
        {
            int prev_indent = EditorGUI.indentLevel;
            var propertyTarget = obj.FindProperty(string.Format("{0}.Array.size", arrayName));

            // Get size of array
            int arrSize = propertyTarget.intValue;

            // Create the size field
            EditorGUI.indentLevel = 1;
            int curr_arr_Size = EditorGUILayout.IntField("Size", arrSize);
            // Clamp
            curr_arr_Size = curr_arr_Size < 0 ? 0 : curr_arr_Size;
            EditorGUI.indentLevel = 3;

            if (curr_arr_Size != arrSize)
            {
                propertyTarget.intValue = curr_arr_Size;
            }

            // Create the array fields (stupid)
            for (int i = 0; i < arrSize; i++)
            {
                // Create property field.
                var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", arrayName, i));

                // If our property is null, ignore.
                if (prop == null)
                    continue;

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop);
                if (OnArrayFieldDrawn != null)
                    OnArrayFieldDrawn(i);
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prev_indent;
        }

        /// <summary>
        /// Create custom array with fields.
        /// <br>Known issues : Only support standard arrays and data types convertible to it. 
        /// <br>Need to take an persistent bool value for dropdown menu. Pass true always if required to be open.</br>
        /// </summary>
        /// <param name="toggle">Toggle boolean for the dropdown state. Required to keep an persistant state. Pass true if not intend to use.</param>
        /// <param name="genericDrawList">Generic draw target array. Required to be passed by reference as it's resized automatically.</param>
        /// <param name="onArrayFieldDrawn">Event to draw generic ui when fired. <c>THIS IS REQUIRED.</c></param>
        public static bool UnityArrayGUICustom<T>(bool toggle, IEnumerable<T> genericDrawList, Action<int> onArrayFieldDrawn)
            where T : new()
        {
            int prevIndent = EditorGUI.indentLevel;
            int arrayLength = genericDrawList.Count();
            IList<T> drawList = null;
            T[] drawArray = null;
            {
                if (genericDrawList is IList<T> list)
                {
                    drawList = list;
                }
                else if (genericDrawList is T[] array)
                {
                    drawArray = array;
                }
                else
                {
                    throw new ArgumentException(string.Format("[EditorAdditionals::UnityArrayGUICustom] Passed 'GenericDrawList' is an immutable type ({0}).", genericDrawList.GetType()));
                }
            }

            void ResizeGenericArray(int size)
            {
                if (drawList != null)
                {
                    drawList.Resize(size);
                    return;
                }
                if (drawArray != null)
                {
                    Array.Resize(ref drawArray, size);
                    return;
                }
            }
            T GetElementGenericArray(int index)
            {
                if (drawList != null)
                {
                    return drawList[index];
                }
                if (drawArray != null)
                {
                    return drawArray[index];
                }

                throw new NullReferenceException("[EditorAdditionals::UnityArrayGUICustom::GetElementGenericArray] Arrays are null.");
            }

            EditorGUI.indentLevel = prevIndent + 2;
            // Create the size & dropdown field
            GUILayout.BeginHorizontal();

            var currToggleDropdwnState = GUILayout.Toggle(toggle, string.Empty, EditorStyles.popup, GUILayout.MaxWidth(20f));
            EditorGUILayout.LabelField(string.Format("{0} List", typeof(T).Name), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            int currentSizeField = Mathf.Clamp(EditorGUILayout.IntField("Size", arrayLength, GUILayout.MaxWidth(200f), GUILayout.MinWidth(150f)), 0, int.MaxValue);
            GUILayout.EndHorizontal();

            if (toggle)
            {
                // Resize array
                if (currentSizeField != arrayLength)
                {
                    ResizeGenericArray(currentSizeField);
                }

                EditorGUI.indentLevel = prevIndent + 3;
                // Create the array fields (stupid)
                for (int i = 0; i < arrayLength; i++)
                {
                    if (GetElementGenericArray(i) == null)
                        continue;

                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField(string.Format("Element {0}", i), EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    onArrayFieldDrawn.Invoke(i);
                }

                EditorGUI.indentLevel = prevIndent + 1;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    ResizeGenericArray(currentSizeField + 1);
                }
                if (GUILayout.Button("-"))
                {
                    ResizeGenericArray(currentSizeField - 1);
                }
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prevIndent;
            return currToggleDropdwnState;
        }
        #endregion
    }
}
