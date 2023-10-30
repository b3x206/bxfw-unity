using System.Collections.Generic;
using UnityEditor;

namespace BXFW.Tweening.Next.Editor
{
    [CustomPropertyDrawer(typeof(BXSTweenExtensionGeneratorTask), true)]
    public class BXSTweenExtensionGeneratorDrawer : ScriptableObjectFieldInspector<BXSTweenExtensionGeneratorTask>
    {
        public override Dictionary<string, DrawGUICommand<BXSTweenExtensionGeneratorTask>> DefaultInspectorCustomCommands => 
            new Dictionary<string, DrawGUICommand<BXSTweenExtensionGeneratorTask>>
        {
        };
    }
}
