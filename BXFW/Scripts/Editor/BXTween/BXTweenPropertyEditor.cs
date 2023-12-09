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
        private readonly PropertyRectContext mainCtx = new PropertyRectContext(2f);

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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (!property.isExpanded)
            {
                return height;
            }

            if (propDuration == null)
            {
                SetupSerializedPropertyRelative(property);
            }

            BXTweenPropertyBase targetValue = property.GetTarget().Value as BXTweenPropertyBase;

            // Add all of the property heights + their paddings
            height += EditorGUI.GetPropertyHeight(propDuration) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propDelay) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propRepeatAmount) + mainCtx.Padding;
            if (propRepeatAmount.intValue > 0)
            {
                height += EditorGUI.GetPropertyHeight(propRepeatType) + mainCtx.Padding;
            }
            height += EditorGUI.GetPropertyHeight(propTargetObject) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propAllowCustomCurveOvershoot) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(targetValue.UseTweenCurve ? propTweenCurve : propTweenEase) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propTweenEase) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propInvokeOnManualStop) + mainCtx.Padding;
            height += EditorGUI.GetPropertyHeight(propOnEndAction) + mainCtx.Padding;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupSerializedPropertyRelative(property);

            EditorGUI.BeginProperty(position, label, property); // This also sets property.isExpanded and other stuff.
            mainCtx.Reset();
            Rect rectFoldout = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

            // Current property index drawing 
            BXTweenPropertyBase targetValue = property.GetTarget().Value as BXTweenPropertyBase;
            
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

            shouldUpdateProperty = false;
            if (property.isExpanded)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propDuration), propDuration, new GUIContent("Duration", "The duration of the tween. Should be higher than 0."));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propDelay), propDelay, new GUIContent("Delay", "The delay of the tween. Values lower than 0 is ignored."));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propRepeatAmount), propRepeatAmount, new GUIContent("Repeat Amount", "Repeat amount of the tween. 0 and lower is no repeat."));
                // Show repeat type if we are using repeats
                if (propRepeatAmount.intValue > 0)
                {
                    EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propRepeatType), propRepeatType, new GUIContent("Repeat Type", "Repeat type of the tween. PingPong: Switch values for 1 repeat. Reset: Don't switch and keep the start and end values same."));
                }

                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propTargetObject), propTargetObject, new GUIContent("Target Object", "Tween target object. Set this to a value to keep the tween stop when the object is invalid/null."));
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propAllowCustomCurveOvershoot), propAllowCustomCurveOvershoot, new GUIContent("Allow Curve/Ease Overshoot", "Tween curve/ease can exceed time values over 0-1."));

                // This is an 'EditorGUI.Toggle' for proper checking of the inspector.
                targetValue.UseTweenCurve = EditorGUI.Toggle(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), new GUIContent("Use Custom Curve", "Use a custom easing curve."), targetValue.UseTweenCurve);

                if (useTwCurve)
                {
                    EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propTweenCurve), propTweenCurve, new GUIContent("Curve", "Custom easing curve."));
                }
                else
                {
                    EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propTweenEase), propTweenEase, new GUIContent("Ease", "Pre-defined easing curve."));
                }

                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, propInvokeOnManualStop), propInvokeOnManualStop, new GUIContent("Invoke Ending On Manual Stops", 
                    "When 'StartTween' is called, if the tween is already running 'StopTween' will invoke 'OnEnd' function (this may produce unwanted results on certain occassions). This prevents that. [Property-specific issue.]"));
                EditorGUI.PropertyField(EditorGUI.IndentedRect(mainCtx.GetPropertyRect(position, propOnEndAction)), propOnEndAction, new GUIContent("OnTweenEnd", "Ending action for the tween. Assign object listeners here."));
                EditorGUI.indentLevel--;

                shouldUpdateProperty = EditorGUI.EndChangeCheck();
            }
            if (shouldUpdateProperty)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}
