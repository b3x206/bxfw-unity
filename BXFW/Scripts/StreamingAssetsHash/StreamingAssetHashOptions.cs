using UnityEditor;
using UnityEngine;

namespace BXFW.Tools
{
    /// <summary>
    /// Contains the array of <see cref="StreamingAssetHash"/>
    /// <br>Loaded in a <see cref="RuntimeInitializeOnLoadMethodAttribute"/> method.</br>
    /// </summary>
    public class StreamingAssetHashOptions : ScriptableObject
    {
        public StreamingAssetHash[] currentHashes;
    }
}