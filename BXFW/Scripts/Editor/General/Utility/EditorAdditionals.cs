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
        Before = 1 << 0,
        After = 1 << 1,
        Omit = 1 << 2,

        OmitAndInvoke = Omit | After
    }

    /// <summary>
    /// A class that contains information about the target gathered from a <see cref="SerializedProperty"/>.
    /// </summary>
    public class PropertyTargetInfo
    {
        /// <summary>
        /// The field information about the contained <see cref="value"/> object.
        /// </summary>
        public readonly FieldInfo fieldInfo;
        /// <summary>
        /// The target object value of the given property.
        /// </summary>
        public readonly object value;
        /// <summary>
        /// Parent object of this target.
        /// <br>If this is null, the target object is the parent object.</br>
        /// </summary>
        public readonly object parent;

        /// <summary>
        /// Tries to cast <see cref="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Whether if the casting was successful. Returns <see langword="false"/> if it was <b>NOT</b> successful.</returns>
        public bool TryCastValue<T>(out T value)
        {
            bool success = this.value is T;
            value = success ? (T)this.value : default;

            return success;
        }
        /// <summary>
        /// Tries to cast <see cref="parent"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Whether if the casting was successful. Returns <see langword="false"/> if it was <b>NOT</b> successful.</returns>
        public bool TryCastParent<T>(out T value)
        {
            bool success = parent is T;
            value = success ? (T)parent : default;

            return success;
        }

        /// <summary>
        /// Whether if the property is an <see cref="IEnumerable"/>.
        public bool TargetIsEnumerable => typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType);

        private PropertyTargetInfo() { }
        public PropertyTargetInfo(FieldInfo fi, object target, object parent)
        {
            fieldInfo = fi;
            value = target;
            this.parent = parent;
        }
    }

    /// <summary>
    /// Contains variety of editor related utilities.
    /// </summary>
    public static class EditorAdditionals
    {
        #region Other
        /// <summary>
        /// Directory of the 'Resources' file (for bxfw assets generally).
        /// <br>Returns the 'Editor' and other necessary folders for methods that take absolute paths.</br>
        /// </summary>
        public static readonly string ResourcesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Resources/");
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

        // Use <see cref="EditorUtility"/> & <see cref="AssetDatabase"/>'s utility functions to make meaning out of this method.
        // The 'how to use' was found from the U2D sprite spline stuff.
        /// <summary>
        /// Creates an instance of prefab <paramref name="prefabReferenceTarget"/> and renames it like an new object was created.
        /// <br><b>NOTE</b> : Make sure '<paramref name="prefabReferenceTarget"/>' is a prefab!</br>
        /// </summary>
        /// <param name="prefabReferenceTarget">The prefab target. Make sure this is a prefab.</param>
        /// <param name="path">Creation path. If left null the <see cref="Selection.activeObject"/> or the root "Assets" folder will be selected. (depending on which one is null)</param>
        /// <param name="onRenameEnd">Called when object is renamed. The <see cref="int"/> parameter is the InstanceID of the object.</param>
        public static void CopyPrefabReferenceAndRename(GameObject prefabReferenceTarget, string path = null, Action<int> onRenameEnd = null)
        {
            // Create at the selected directory
            if (string.IsNullOrEmpty(path))
            {
                path = Selection.activeObject == null ? "Assets" : AssetDatabase.GetAssetPath(Selection.activeObject);
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                throw new DirectoryNotFoundException(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Directory '{0}' does not exist. This method does not create directories.", path));
            }
            if (PrefabUtility.GetCorrespondingObjectFromSource(prefabReferenceTarget) == null)
            {
                throw new ArgumentException(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Prefab to copy is invalid (not a prefab). prefabTarget was = '{0}'", prefabReferenceTarget));
            }

            // Get path & target prefab to copy
            GameObject targetPrefabInst = prefabReferenceTarget;
            path = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(path, targetPrefabInst.name)}.prefab"); // we are copying prefabs anyway

            // Register 'OnFileNamingEnd' function.
            var assetEndNameAction = ScriptableObject.CreateInstance<CreateAssetEndNameEditAction>();
            assetEndNameAction.OnRenameEnd += (int instanceIDSaved) =>
            {
                var createdObject = EditorUtility.InstanceIDToObject(instanceIDSaved);
                Selection.activeObject = createdObject; // Select renamed object

                onRenameEnd?.Invoke(instanceIDSaved);   // Call custom event
            };

            // wow very obvious, this is where you get the proper icon otherwise the entire 'StartNameEditing...' function crashes unity (unity api moment)
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
        // TODO : Add method to also copy from already existing instance id (overwrite method?)
        #endregion

        #region Property Field / Custom Property Drawer Helpers
        // This allows for getting the property field target

        // we could use c# string method abuse or SerializedObject.GetArrayIndexSomething(index) method.
        // No, not really that is for getting the array object? idk this works good so no touchy unless it breaks
        private static readonly Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");
        /// <summary>
        /// String token used to define a <see cref="SerializedProperty"/> array element.
        /// </summary>
        private const string SPropArrayToken = "Array.data[";

        /// <summary>
        /// Returns a string that is traversed towards parent property names.
        /// </summary>
        private static string GetParentTraversedPropertyPathString(string propertyPath, int parentDepth)
        {
            int lastIndexOfPeriod = propertyPath.LastIndexOf('.');
            for (int i = 1; i < parentDepth; i++)
            {
                lastIndexOfPeriod = propertyPath.LastIndexOf('.', lastIndexOfPeriod - 1);
            }

            if (lastIndexOfPeriod == -1)
            {
                return string.Empty;
            }

            return propertyPath.Substring(0, lastIndexOfPeriod);
        }
        /// <summary>
        /// Internal method to get parent from these given parameters.
        /// <br>Traverses <paramref name="propertyRootParent"/> using reflection and finds the target field info + object ref in <paramref name="propertyPath"/>.</br>
        /// </summary>
        /// <param name="propertyRootParent">Target (parent) object of <see cref="SerializedProperty"/>. Pass <see cref="SerializedProperty.serializedObject"/>.targetObject.</param>
        /// <param name="propertyPath">Path of the property. Pass <see cref="SerializedProperty.propertyPath"/>.</param>
        /// <exception cref="InvalidCastException"/>
        private static PropertyTargetInfo GetTarget(UnityEngine.Object propertyRootParent, string propertyPath)
        {
            if (propertyRootParent == null)
            {
                throw new ArgumentNullException(nameof(propertyRootParent), "[EditorAdditionals::GetTarget] Given argument was null.");
            }

            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                throw new ArgumentNullException(nameof(propertyPath), "[EditorAdditionals::GetTarget] Given argument was null.");
            }

            object parent = null;
            FieldInfo targetInfo = null;
            object target = propertyRootParent;

            string[] propertyNames = propertyPath.Split('.');

            bool isNextPropertyArrayIndex = false;

            for (int i = 0; i < propertyNames.Length && target != null; i++)
            {
                // Alias the string name. (but we need for for the 'i' variable)
                string propName = propertyNames[i];

                // Array targets mostly contain typeless 'IEnumerable's
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
                        {
                            throw new InvalidCastException(string.Format(@"[EditorAdditionals::GetTarget] Error while casting targetAsArray.
-> Invalid cast : Tried to cast type {0} as IEnumerable. Current property is {1}.", target.GetType().Name, propName));
                        }

                        var enumerator = targetAsArray.GetEnumerator();
                        var isSuccess = false;
                        //enumerator.Reset();
                        for (int j = 0; enumerator.MoveNext(); j++)
                        {
                            object item = enumerator.Current;

                            if (arrayIndex == j)
                            {
                                // Update FieldInfo that will be returned

                                // oh wait, that's impossible, riiight.
                                // basically FieldInfo can't point into a c# array element member,
                                // only the parent array container as it's just the object
                                // (unless we are returning a managed memory pointer, which is not really possible unless unity does it)
                                // (+ which it most likely won't because our result data is in ''safe'' FieldInfo type)

                                // If the array contains a class or a struct, and the target is a member that actually is not an array value, it updates fine though.
                                // So you could use a wrapper class that just contains the field as the target
                                // (but we can't act like that, because c# arrays are covariant and casting c# arrays is not fun)
                                // whatever just look at this : https://stackoverflow.com/questions/13790527/c-sharp-fieldinfo-setvalue-with-an-array-parameter-and-arbitrary-element-type

                                // ---------- No Array Element FieldInfo? -------------
                                // (would like to put megamind here, but git will most likely break it)
                                parent = target; // Set parent to previous
                                target = item;
                                isSuccess = true;

                                break;
                            }
                        }

                        // Element doesn't exist in the array
                        if (!isSuccess)
                        {
                            throw new Exception(string.Format("[EditorAdditionals::GetTarget] Couldn't find SerializedProperty '{0}' in array '{1}'.", propertyPath, targetAsArray));
                        }
                    }
                    else // Array parse failure, should only happen on the ends of the array (i.e size field)
                    {
                        // Instead of throwing an exception, get the object
                        // (as this may be called for the 'int size field' on the editor, for some reason)
                        try
                        {
                            targetInfo = GetField(target, propName);
                            parent = target;
                            target = targetInfo.GetValue(target);
                        }
                        catch
                        {
                            // It can also have an non-existent field for some reason
                            // Because unity, so the method should give up (with the last information it has)
                            // Maybe this should print a warning, but it's not too much of a thing (just a fallback)

                            return new PropertyTargetInfo(targetInfo, target, parent);
                        }
                    }
                }
                else
                {
                    // Get next target + value.
                    targetInfo = GetField(target, propName);
                    parent = target;
                    target = targetInfo.GetValue(target);
                }
            }

            return new PropertyTargetInfo(targetInfo, target, parent);
        }

        /// <summary>
        /// Returns the c# object targets.
        /// <br>
        /// It is heavily suggested that you use <see cref="GetTargetsNoAlloc(SerializedProperty, List{KeyValuePair{FieldInfo, object}})"/> 
        /// instead for much better performance and most likely less memory leaks.<br/>(this method calls that method internally with a newly allocated array anyways)
        /// </br>
        /// </summary>
        public static List<PropertyTargetInfo> GetTargets(this SerializedProperty prop)
        {
            var infos = new List<PropertyTargetInfo>();
            GetTargetsNoAlloc(prop, infos);
            return infos;
        }
        /// <summary>
        /// Returns the c# object targets (without allocating new arrays).
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        /// <param name="prop">Target property.</param>
        /// <param name="targetInfos">Array to write the properties into. The array is cleared then written into.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void GetTargetsNoAlloc(this SerializedProperty prop, List<PropertyTargetInfo> targetInfos)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::GetTargets] Parameter 'prop' is null.");
            }

            if (targetInfos == null)
            {
                throw new ArgumentNullException(nameof(targetInfos), "[EditorAdditionals::GetTargets] Array Parameter 'targetPairs' is null.");
            }

            targetInfos.Clear();
            targetInfos.Capacity = prop.serializedObject.targetObjects.Length;

            for (int i = 0; i < prop.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetedObject = prop.serializedObject.targetObjects[i];
                if (targetedObject == null)
                {
                    continue;
                }

                targetInfos.Add(GetTarget(targetedObject, prop.propertyPath));
            }
        }

        /// <summary>
        /// Returns the c# object parent targets.
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        public static List<PropertyTargetInfo> GetParentsOfTargets(this SerializedProperty prop, int parentDepth = 1)
        {
            List<PropertyTargetInfo> infos = new List<PropertyTargetInfo>();
            GetParentsOfTargetsNoAlloc(prop, infos, parentDepth);
            return infos;
        }
        /// <summary>
        /// Returns the c# object parent targets (without allocating new arrays).
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        /// <param name="prop">Target property.</param>
        /// <param name="targetInfos">Array to write the properties into. The array is cleared then written into.</param>
        /// <param name="parentDepth">Depth of the target parent. Higher depths</param>
        /// <exception cref="ArgumentNullException"/>
        public static void GetParentsOfTargetsNoAlloc(this SerializedProperty prop, List<PropertyTargetInfo> targetInfos, int parentDepth = 1)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::GetParentsOfTargetsNoAlloc] Parameter 'prop' is null.");
            }

            if (targetInfos == null)
            {
                throw new ArgumentNullException(nameof(targetInfos), "[EditorAdditionals::GetParentsOfTargetsNoAlloc] Array Parameter 'targetPairs' is null.");
            }

            targetInfos.Clear();
            targetInfos.Capacity = prop.serializedObject.targetObjects.Length;

            for (int i = 0; i < prop.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetedObject = prop.serializedObject.targetObjects[i];
                if (targetedObject == null)
                {
                    continue;
                }

                targetInfos.Add(GetTarget(targetedObject, GetParentTraversedPropertyPathString(prop.propertyPath, parentDepth)));
            }
        }

        /// <summary>
        /// Returns the c# object's fieldInfo and the instance object it comes with.
        /// <br>
        /// <b>NOTE :</b> The instance object that gets returned with this method may be null.
        /// <br>In these cases use the <see langword="return"/>'s field info.</br>
        /// </br>
        /// <br/>
        /// <br>
        /// <b>NOTE 2 :</b> The <see cref="FieldInfo"/> returned may not be the exact <see cref="FieldInfo"/>, 
        /// as such case usually happens when you try to call 'GetTarget' on an array element.
        /// <br>In this case, to change the value of the array, you may need to copy the entire array,
        /// and call <see cref="FieldInfo.SetValue"/> to it.</br>
        /// </br>
        /// <br/>
        /// <br>
        /// <b>NOTE 3 :</b> Any value gathered from a normal <see langword="struct"/> child <see cref="SerializedProperty"/>
        /// (except for the 'FieldInfo') should be considered as a copy of target.
        /// <br>This is because <see cref="GetTarget(SerializedProperty)"/> cannot return struct references
        /// and it does not have low-level control of neither c# or unity.</br>
        /// </br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidCastException"/> 
        public static PropertyTargetInfo GetTarget(this SerializedProperty prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException("[EditorAdditionals::GetTarget] Field 'prop' is null!");
            }

            return GetTarget(prop.serializedObject.targetObject, prop.propertyPath);
        }

        /// <summary>
        /// Returns the c# object's fieldInfo and the PARENT object it comes with. (this is useful with <see langword="struct"/>)
        /// <br>Important NOTE : The instance object that gets returned with this method may be null (or not).
        /// In these cases use the return (the FieldInfo)</br>
        /// <br/>
        /// <br>If you are using this for <see cref="CustomPropertyDrawer"/> (that is on an array otherwise this note is invalid), this class has an <see cref="FieldInfo"/> property named <c>fieldInfo</c>, 
        /// you can use that instead of the bundled field info.</br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="InvalidCastException"/>
        public static PropertyTargetInfo GetParentOfTargetField(this SerializedProperty prop, int parentDepth = 1)
        {
            string propertyNameList = GetParentTraversedPropertyPathString(prop.propertyPath, parentDepth);

            if (string.IsNullOrEmpty(propertyNameList))
            {
                // No depth, instead return the field info from this scriptable object (use the parent scriptable object ofc)
                var fInfo = GetField(prop.serializedObject.targetObject, prop.name);

                // Return the 'serializedObject.targetObject' as target, because it isn't a field (is literally an pointer) 
                return new PropertyTargetInfo(fInfo, prop.serializedObject.targetObject, null);
            }

            var info = GetTarget(prop.serializedObject.targetObject, propertyNameList);
            return info;
        }

        /// <summary>
        /// Returns the type of the property's target.
        /// </summary>
        /// <param name="property">Property to get type from.</param>
        public static Type GetPropertyType(this SerializedProperty property)
        {
            return property.GetTarget().fieldInfo.FieldType;
        }
        /// <summary>
        /// Returns the (last array) index of this property in the array.
        /// <br>Returns <c>-1</c> if <paramref name="property"/> is not in an array.</br>
        /// </summary>
        public static int GetPropertyParentArrayIndex(this SerializedProperty property)
        {
            // Find whether if there's any array define token
            int arrayDefLastIndex = property.propertyPath.LastIndexOf(SPropArrayToken);
            // No define token
            if (arrayDefLastIndex < 0)
            {
                return -1;
            }

            // Remove the enclosing bracket ']' token
            string indStr = property.propertyPath.Substring(arrayDefLastIndex + SPropArrayToken.Length).TrimEnd(']');
            return int.Parse(indStr);
        }
        /// <summary>
        /// Internal helper method for getting field from properties.
        /// <br>Gets the target normally, if not found searches the field in <paramref name="targetType"/>'s <see cref="Type.BaseType"/>.</br>
        /// </summary>
        private static FieldInfo GetField(object target, string name, Type targetType = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "[EditorAdditionals::GetField] Error while getting field : Null 'target' object.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), string.Format("[EditorAdditionals::GetField] Error while getting field : Null 'name' field. (target: '{0}', targetType: '{1}')", target, targetType));
            }

            if (targetType == null)
            {
                targetType = target.GetType();
            }

            // This won't work for struct childs (it will, but it will return a copy of the struct)
            // because GetField does the normal c# behaviour (and it's because c# structs are stackalloc)
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

        // - Normal
        /// <summary>
        /// Return the target property drawer for <paramref name="targetType"/>.
        /// <br>This method throws <see cref="ArgumentException"/> if no property drawers were found.</br>
        /// </summary>
        public static PropertyDrawer GetPropertyDrawerFromType(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType), "[EditorAdditionals::GetTargetPropertyDrawer] Given Type argument is null.");
            }

            Type propertyDrawerType = (Type)Assembly.GetAssembly(typeof(PropertyDrawer))         // Internal class is contained in the same assembly (UnityEditor.CoreModule)
                .GetType("UnityEditor.ScriptAttributeUtility", true)                             // Internal class that has dictionary for all custom PropertyDrawer's
                .GetMethod("GetDrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static) // Utility method to get type from the internal class
                .Invoke(null, new object[] { targetType });                                      // Call with the type parameter. It will return a type that needs instantiation using Activator.

            if (propertyDrawerType == null)
            {
                throw new ArgumentException($"[EditorAdditionals::GetTargetPropertyDrawer] Given type {targetType} has no property drawer. Ensure the type is valid and serializable by unity.", nameof(targetType));
            }

            // PropertyDrawer's don't inherit UnityEngine.Object and thus can be created normally
            return (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
        }
        /// <summary>
        /// Return the target property drawer for <paramref name="targetType"/>.
        /// This method catches the thrown exceptions of <see cref="GetPropertyDrawerFromType(Type)"/>.
        /// </summary>
        public static bool TryGetPropertyDrawerFromType(Type targetType, out PropertyDrawer drawer)
        {
            drawer = null;

            try
            {
                drawer = GetPropertyDrawerFromType(targetType);
            }
            catch
            {
                return false;
            }

            return true;
        }
        // - Generic
        /// <inheritdoc cref="GetPropertyDrawerFromType(Type)"/>
        public static PropertyDrawer GetPropertyDrawerFromType<T>()
        {
            return GetPropertyDrawerFromType(typeof(T));
        }
        /// <inheritdoc cref="TryGetPropertyDrawerFromType(Type, out PropertyDrawer)"/>
        public static bool TryGetPropertyDrawerFromType<T>(out PropertyDrawer drawer)
        {
            return TryGetPropertyDrawerFromType(typeof(T), out drawer);
        }

        /// <summary>
        /// Get the property drawer for the field type that you are inspecting.
        /// <br>Very useful for <c><see cref="Attribute"/></c> targeting PropertyDrawers.</br>
        /// <br>Will throw <see cref="InvalidOperationException"/> if called from a property drawer that's target is an actual non-attribute class.</br>
        /// </summary>
        /// TODO : Maybe create a new 'PropertyDrawer' class named 'AttributePropertyDrawer' with better enforcement?
        public static PropertyDrawer GetTargetPropertyDrawer(PropertyDrawer requester)
        {
            if (requester == null)
            {
                throw new ArgumentNullException(nameof(requester), "[EditorAdditionals::GetPropertyDrawer] Passed parameter was null.");
            }

            // -- Assert the requester to be a property drawer for an attribute
            // - This is done to not get the same property drawer as the requester which may cause an overflow
            // Get the attribute type for target PropertyDrawer's CustomPropertyDrawer target type
            Type attributeTargetType = (Type)typeof(CustomPropertyDrawer)
                .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(requester.GetType().GetCustomAttribute(typeof(CustomPropertyDrawer)));
            // Check if the CustomPropertyDrawer is for an attribute
            if (!attributeTargetType.GetBaseTypes().Contains(typeof(Attribute)))
            {
                throw new InvalidOperationException(string.Format("[EditorAdditionals::GetPropertyDrawer] Tried to get a property drawer from drawer {0}, use this method only on ATTRIBUTE targeting property drawers. Returned 'PropertyDrawer' will be junk, and will cause 'StackOverflowException'.", requester.GetType()));
            }

            if (!TryGetPropertyDrawerFromType(requester.fieldInfo.FieldType, out PropertyDrawer resultDrawer))
            {
                // No default drawer
                return null;
            }

            if (resultDrawer != null)
            {
                // Leave m_Attribute as is, there's no need to access that (as this is most likely not a custom attribute property drawer)
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
            // Undocumented class = ActiveEditorTracker (Exists since 2017.1)
            foreach (var i in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (i.serializedObject != obj)
                {
                    continue;
                }

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
        /// Draw default inspector with commands inbetween. (Allowing to put custom gui between).
        /// <br>This works as same as <see cref="UnityEditor.Editor.OnInspectorGUI"/>'s <see langword="base"/> call.</br>
        /// </summary>
        /// <param name="onStringMatchEvent">
        /// The event <see cref="MatchGUIActionOrder"/> match. 
        /// If passed <see langword="null"/> this method will act like <see cref="UnityEditor.Editor.DrawDefaultInspector"/>.
        /// </param>
        /// <example>
        /// <![CDATA[
        /// serializedObject.DrawDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> 
        /// {
        ///     { nameof(FromAnyClass.ElementNameYouWant), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Before, () => 
        ///         {
        ///             // Write your commands here.
        ///         })
        ///     }
        /// });
        /// ]]>
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
                            bool hasInvokedCustomCommand = false; // Prevent the command from invoking twice, as this is now enum flags.

                            if ((Pair.Key & MatchGUIActionOrder.Before) == MatchGUIActionOrder.Before && !hasInvokedCustomCommand)
                            {
                                hasInvokedCustomCommand = true;

                                if (Pair.Value != null)
                                {
                                    try
                                    {
                                        Pair.Value();
                                    }
                                    catch (ExitGUIException)
                                    {
                                        return true;
                                    }
                                }
                            }

                            if ((Pair.Key & MatchGUIActionOrder.Omit) != MatchGUIActionOrder.Omit)
                            {
                                EditorGUILayout.PropertyField(property, true);
                            }

                            if ((Pair.Key & MatchGUIActionOrder.After) == MatchGUIActionOrder.After && !hasInvokedCustomCommand)
                            {
                                hasInvokedCustomCommand = true;

                                if (Pair.Value != null)
                                {
                                    try
                                    {
                                        Pair.Value();
                                    }
                                    // Stop drawing GUI if this was thrown
                                    // This is how the unity does flow control to it's interface, amazing really.
                                    catch (ExitGUIException)
                                    {
                                        return true;
                                    }
                                }
                            }

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
        /// Omit action for <see cref="DrawCustomDefaultInspector(SerializedObject, Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}})"/>.
        /// </summary>
        public static readonly KeyValuePair<MatchGUIActionOrder, Action> OMIT_ACTION = new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null);

        /// <summary>
        /// Returns whether if this 'SerializedObject' is disposed.
        /// </summary>
        public static bool IsDisposed(this SerializedObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[EditorAdditionals::IsDisposed] Target was null.");
            }

            return (IntPtr)typeof(SerializedObject).GetField("m_NativeObjectPtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj) == IntPtr.Zero;
        }
        /// <summary>
        /// Returns whether if this 'SerializedProperty' is disposed.
        /// </summary>
        public static bool IsDisposed(this SerializedProperty obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[EditorAdditionals::IsDisposed] Target was null.");
            }

            return (IntPtr)typeof(SerializedProperty).GetField("m_NativePropertyPtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj) == IntPtr.Zero;
        }

        /// <summary>
        /// Returns whether if Next is callable on <paramref name="prop"/>.
        /// </summary>
        public static bool IsEndOfData(this SerializedProperty prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::IsEndOfData] Target was null.");
            }

            return (bool)typeof(SerializedProperty).GetMethod("EndOfData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(prop, null);
        }

        /// <summary>
        /// Calls the 'OnValidate' method on <paramref name="obj"/>'s <see cref="SerializedObject.targetObjects"/>.
        /// </summary>
        /// <param name="obj">SerializedObject list to call 'OnValidate' on. This cannot be null or disposed.</param>
        /// <returns>Whether if any of the objects had 'OnValidate' method and those were called.</returns>
        public static bool CallOnValidate(this SerializedObject obj)
        {
            // GameObject.SendMessage is not viable due to the objects could be ScriptableObject,
            // which can have a OnValidate but ScriptableObject has no SendMessage, so use Reflection
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[EditorAdditionals::CallOnValidate] Target was null.");
            }
            if (obj.IsDisposed())
            {
                throw new ArgumentException("[EditorAdditionals::CallOnValidate] Given SerializedObject was disposed. Cannot do anything.", nameof(obj));
            }

            bool calledOnValidateOnce = false;
            foreach (UnityEngine.Object target in obj.targetObjects)
            {
                if (CallOnValidate(target))
                {
                    calledOnValidateOnce = true;
                }
            }

            return calledOnValidateOnce;
        }

        /// <summary>
        /// Calls the 'OnValidate' method on <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Target to call 'OnValidate' on. This can be null, but this method will always return in a case of null.</param>
        /// <returns>Whether if any of the objects had 'OnValidate' method and those were called.</returns>
        public static bool CallOnValidate(UnityEngine.Object target)
        {
            if (target == null)
            {
                return false;
            }

            // Looking for parameterless 'OnValidate' method
            MethodInfo onValidateMethod = target.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            if (onValidateMethod == null)
            {
                return false;
            }

            onValidateMethod.Invoke(target, null);
            return true;
        }

        /// <summary>
        /// Returns the children (regardless of visibility) of the SerializedProperty.
        /// <br/>
        /// <br>This method won't work with Linq methods that cast this <see cref="IEnumerable{T}"/> 
        /// to arrays because it doesn't 'Copy()' the returned element. You will need a custom delegate to convert each element to a copied one.</br>
        /// </summary>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            SerializedProperty nextElement = property.Copy();

            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            // Get next child
            property.NextVisible(true);

            do
            {
                // Skipped to the next element
                if (SerializedProperty.EqualContents(property, nextElement))
                {
                    yield break;
                }

                // yield return the current gathered child property.
                yield return property;
            }
            while (property.NextVisible(false));
        }

        /// <summary>
        /// Gets visible children of '<see cref="SerializedProperty"/>' at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent '<see cref="SerializedProperty"/>'.</param>
        /// <returns>Collection of '<see cref="SerializedProperty"/>' children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();     // Children iterating property
            SerializedProperty nextSiblingProperty = serializedProperty.Copy(); // Non-children property
            {
                // Move to the initial non-children visible in the next invisible sibling property
                nextSiblingProperty.NextVisible(false);
            }

            // Check initial visibility with children
            if (currentProperty.NextVisible(true))
            {
                do
                {
                    // Check if the 'currentProperty' is now equal to a 'non-children' property
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    {
                        break;
                    }

                    // Use '.Copy' for making 'Enumerable.ToArray' work
                    // This is due to yield return'd value will be always be the same 'currentProperty' if we don't copy it
                    // But for a linear read of this IEnumerable without laying it out to an array, it will be fine

                    // tl;dr : basically copy the value to make it different instead of a 'currentProperty' pass by value.
                    using SerializedProperty copyProp = currentProperty.Copy();
                    yield return copyProp;
                }
                while (currentProperty.NextVisible(false));
            }
        }
        #endregion
    }
}
