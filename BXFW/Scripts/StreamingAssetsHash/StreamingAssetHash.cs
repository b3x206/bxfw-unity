using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BXFW.Tools
{
    /// <summary>
    /// Class that contains the following :
    /// <br> * File name (relative directory) of to be protected file.</br>
    /// <br> * Hash of the file. (Stored in an array inside <see cref="StreamingAssetHashOptions"/></br>
    /// <br> * Actions to do when the hash fails.
    /// (This can store a custom method equipped with matching string attribute of <see cref=""/>, inside <c>Assembly-CSharp</c>)</br>
    /// <br>NOTE : This class DOES compute the hash & check the hash for loaded object.</br>
    /// </summary>
    [Serializable]
    public class StreamingAssetHash
    {
        public const string DEFAULT_HASH = "NOT_DEFINED";
        private string _currentAssetHash = DEFAULT_HASH;
        public string CurrentAssetHash
        {
            get 
            {
                if (_currentAssetHash == DEFAULT_HASH)
                    ComputeHash();

                return _currentAssetHash; 
            }
        }
        public readonly string CurrentRelativeAssetDirectory;
        public string CurrentAssetDirectory
        {
            get
            {
                return string.Format("{0}{1}", Application.streamingAssetsPath, CurrentRelativeAssetDirectory);
            }
        }

        /// <summary>
        /// Creates an hash object.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public StreamingAssetHash(string assetDirectory)
        {
            if (string.IsNullOrWhiteSpace(assetDirectory))
                throw new ArgumentException("[StreamingAssetHash::ctor()] Error while constructing a hash : 'assetDirectory' is null.");

            CurrentRelativeAssetDirectory = assetDirectory;
        }

        /// <summary>
        /// Computes an hash for file in <see cref="CurrentRelativeAssetDirectory"/>.
        /// <br>NOTE : This is more suitable for </br>
        /// </summary>
        public void ComputeHash()
        {
            var sha = new SHA256Managed();
            // Load the file
            // NOTE : In android, we need UnityWebRequest for StreamingAssets.
            // HOWEVER you can use UnityWebRequest for mostly anything.
            var loadingRequest = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, CurrentRelativeAssetDirectory));
            loadingRequest.SendWebRequest();
            while (!loadingRequest.isDone)
            {
                if (loadingRequest.result == UnityWebRequest.Result.ConnectionError ||
                    loadingRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    loadingRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(
                        string.Format("[StreamingAssetHash::ComputeHash] Error while trying to load from WebRequest : {0} | File Directory : {1}",
                            loadingRequest.error, CurrentAssetDirectory)
                        );
                    return;
                }
            }

            // TODO : Later have a 'StreamReader' version too.
            //using (StreamReader r = new StreamReader(CurrentAssetDirectory))
            //{
            //
            //}

            _currentAssetHash = Encoding.UTF8.GetString(sha.ComputeHash(loadingRequest.downloadHandler.data));
        }

        public static bool operator ==(StreamingAssetHash lhs, StreamingAssetHash rhs)
        {
            return lhs.CurrentAssetHash == rhs.CurrentAssetHash;
        }
        public static bool operator !=(StreamingAssetHash lhs, StreamingAssetHash rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object obj)
        {
            // Not meant to compare different type.
            if (obj.GetType() != GetType())
                return false;

            return this == (StreamingAssetHash)obj;
        }
        public override int GetHashCode()
        {
            return CurrentAssetHash.GetHashCode();
        }
    }
}