using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEditor.ProjectWindowCallback;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// An enum used to denote the order of drawing.
    /// <br>On certain methods, multiple GUI can be drawn on different orders if plausible.</br>
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
    /// Contains variety of editor related utilities.
    /// </summary>
    public static class EditorAdditionals
    {
        // -- Prefab Utility
        /// <summary>
        /// NOTES ABOUT THIS CLASS:
        /// <para>
        ///     1: It handles creation 
        ///     <br>2: It edits (because it's callback of <see cref="ProjectWindowUtil.StartNameEditingIfProjectWindowExists(int, EndNameEditAction, string, Texture2D, string)"/>, what type of method is that?)</br>
        /// </para>
        /// </summary>
        private class CreateAssetEndNameEditAction : EndNameEditAction
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
        /// <exception cref="ArgumentException"/>
        /// <exception cref="DirectoryNotFoundException"/>
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
            CreateAssetEndNameEditAction assetEndNameAction = ScriptableObject.CreateInstance<CreateAssetEndNameEditAction>();
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

        // -- Inspector-Editor Draw
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
        public static readonly KeyValuePair<MatchGUIActionOrder, Action> OmitAction = new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null);

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
        /// <param name="target">Target to call 'OnValidate' on. This can be null, but this method will always return <see langword="false"/> in a case of null.</param>
        /// <returns>Whether if the given <paramref name="target"/> had 'OnValidate' method it was called.</returns>
        public static bool CallOnValidate(UnityEngine.Object target)
        {
            if (target == null)
            {
                return false;
            }

            // Looking for parameterless 'OnValidate' method
            MethodInfo onValidateMethod = target.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
            if (onValidateMethod == null)
            {
                return false;
            }

            onValidateMethod.Invoke(target, null);
            return true;
        }
    }
}
