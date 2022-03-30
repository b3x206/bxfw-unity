using UnityEngine;

namespace BXFW.Tools
{
    /// <summary>
    /// Contains the array of <see cref="StreamingAssetHash"/>
    /// <br>Loaded in a <see cref="RuntimeInitializeOnLoadMethodAttribute"/> method.</br>
    /// </summary>
    public class StreamingAssetHashOptions : ScriptableObjectSingleton<StreamingAssetHashOptions>
    {
        [Header("Hash Lists")]
        public StreamingAssetHash[] currentHashes;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CheckCurrentHashes()
        {
            // Check currentHashes array for each stuff.
            foreach (var assetHash in Instance.currentHashes)
            {
                if (assetHash.)
            }
        }
    }
}