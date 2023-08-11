using BXFW.Tools.Editor;
using BXFW.Tweening;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(BXTweenPropertyBase), true)]
    public class BXTweenPropertyEditor : PropertyDrawer
    {
        private bool shouldUpdateProperty = false; // Call 'UpdateProperty' after drawing gui.
        private int currentPropRect = -1;          // Property rect index.
        private Rect GetPropertyRect(Rect parentRect, float customHeight = -1f)
        {
            // Always add +1 to property rect as in this class we call this after 'EditorGUI.BeginProperty()'.
            currentPropRect++;

            var propHeight = customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight;
            return new Rect(parentRect.xMin, parentRect.yMin + (EditorGUIUtility.singleLineHeight * (currentPropRect + 1)) + 8, parentRect.size.x, propHeight);
        }

        private SerializedProperty propDuration;
        private SerializedProperty propDelay;
        private SerializedProperty propRepeatAmount;
        private SerializedProperty propRepeatType;
        private SerializedProperty propTargetObject;
        private SerializedProperty propAllowCustomCurveOvershoot;
        private SerializedProperty propTweenCurve;
        private SerializedProperty propTweenEase;
        private SerializedProperty propInvokeOnManualStop;
        private SerializedProperty propOnEndAction;

        /// <summary>
        /// Sets up the relative property variables inside this class.
        /// </summary>
        private void SetupSerializedPropertyRelative(SerializedProperty property)
        {
            // Refresh properties always because this code is reused for all properties
            // Basically there's one instance of this script running.
            if (property == null)
            {
                Debug.LogError("[BXTweenPropertyEditor] Error : Passed property is null for initilazing 'SerializedProperty' variables.");
                return;
            }

            propDuration = property.FindPropertyRelative("_Duration");
            propDelay = property.FindPropertyRelative("_Delay");
            propRepeatAmount = property.FindPropertyRelative("_RepeatAmount");
            propRepeatType = property.FindPropertyRelative("_TweenRepeatType");
            propTargetObject = property.FindPropertyRelative("_TargetObject");
            // _UseTweenCurve is drawn using EditorGUI.Toggle to activate the property getter-setter.
            propAllowCustomCurveOvershoot = property.FindPropertyRelative("_AllowInterpolationEaseOvershoot");
            propTweenCurve = property.FindPropertyRelative("_TweenCurve");
            propTweenEase = property.FindPropertyRelative("_TweenEase");
            // -- event
            propInvokeOnManualStop = property.FindPropertyRelative("InvokeEventOnManualStop");
            propOnEndAction = property.FindPropertyRelative("OnEndAction");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupSerializedPropertyRelative(property);

            EditorGUI.BeginProperty(position, label, property); // This also sets property.isExpanded and other stuff.
            Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

            // Current property index drawing 
            var targetValue = (BXTweenPropertyBase)property.GetTarget().Value;
            
            bool useTwCurve = false;
            if (targetValue != null)
            {
                useTwCurve = targetValue.UseTweenCurve;
            }
            // Call UpdateProperty here (after first OnGUI) to update in realtime?
            // seems to work fine, this isn't an absolute necessity, it's editor stuff
            // The 'UpdateProperty' is called with the assigned parameters from inspector in 'SetupProperty'
            // So, this is just an editor improvement, as EditorGUI.EndProperty doesn't seem to update the ease properly
            // (curve works fine as it probably calls OnGUI more than 1 time).
            if (shouldUpdateProperty)
            {
                targetValue.UpdateProperty();
            }

            // Reset 'GetPropertyRect' positioning.
            currentPropRect = -1;
            shouldUpdateProperty = false;
            if (property.isExpanded)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(GetPropertyRect(position), propDuration, new GUIContent("Duration", "The duration of the tween. Should be higher than 0."));
                EditorGUI.PropertyField(GetPropertyRect(position), propDelay, new GUIContent("Delay", "The delay of the tween. Values lower than 0 is ignored."));
                EditorGUI.PropertyField(GetPropertyRect(position), propRepeatAmount, new GUIContent("Repeat Amount", "Repeat amount of the tween. 0 and lower is no repeat."));
                // Show repeat type if we are using repeats
                if (propRepeatAmount.intValue > 0)
                    EditorGUI.PropertyField(GetPropertyRect(position), propRepeatType, new GUIContent("Repeat Type", "Repeat type of the tween. PingPong: Switch values for 1 repeat, Reset:"));
                EditorGUI.PropertyField(GetPropertyRect(position), propTargetObject, new GUIContent("Target Object", "Tween target object. Set this to a value to keep the tween stop when the object is invalid/null."));
                EditorGUI.PropertyField(GetPropertyRect(position), propAllowCustomCurveOvershoot, new GUIContent("Allow Curve/Ease Overshoot", "Tween curve/ease can exceed time values over 0-1."));

                // This is an 'EditorGUI.Toggle' for proper checking of the inspector.
                targetValue.UseTweenCurve = EditorGUI.Toggle(GetPropertyRect(position), new GUIContent("Use Custom Curve", "Use a custom easing curve."), targetValue.UseTweenCurve);

                if (useTwCurve)
                    EditorGUI.PropertyField(GetPropertyRect(position), propTweenCurve, new GUIContent("Curve", "Custom easing curve."));
                else 
                    EditorGUI.PropertyField(GetPropertyRect(position), propTweenEase, new GUIContent("Ease", "Pre-defined easing curve."));
                
                EditorGUI.PropertyField(GetPropertyRect(position), propInvokeOnManualStop, new GUIContent("Invoke Ending On Manual Stops", 
                    "When 'StartTween' is called, if the tween is already running 'StopTween' will invoke 'OnEnd' function (this may produce unwanted results on certain occassions). This prevents that. [Property-specific issue.]"));
                EditorGUI.PropertyField(GetPropertyRect(position, EditorGUI.GetPropertyHeight(propOnEndAction)), propOnEndAction, new GUIContent("OnTweenEnd", "Ending action for the tween. Assign object listeners here."));
                EditorGUI.indentLevel--;

                shouldUpdateProperty = EditorGUI.EndChangeCheck();
            }
            if (shouldUpdateProperty)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // FIXME : Calculate height properly (this very much works for now so idc)
            // (yes, the proper way of doing this [unfortunately] is iterating all properties and getting their heights)
            // This will work fine for single drawn properties, but for stuff like arrays, this is a problem if there's more than 2 expanded properties.

            var totalLinesDrawn = 1; // Always include one for the 'BeginProperty' call

            if (property.isExpanded)
            {
                // Was 15, this automatically receives the height from 'GetPropertyRect' calls.
                totalLinesDrawn = currentPropRect + 7;
            }

            return (EditorGUIUtility.singleLineHeight * totalLinesDrawn) + 4;
        }
    }
}