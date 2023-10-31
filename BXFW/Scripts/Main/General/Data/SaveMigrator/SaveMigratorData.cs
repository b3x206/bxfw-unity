using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BXFW
{
    public class SaveMigratorData : ScriptableObjectSingleton<SaveMigratorData>
    {
        /// <summary>
        /// Recipe to use (i.e how data is handled, etc.)
        /// </summary>
        public SaveMigratorRecipe migrationRecipe;

        /// <summary>
        /// A data structure used for key matching.
        /// </summary>
        public struct KeyMatcher : IEquatable<KeyMatcher>
        {
            public string keyValue;
            public bool isRegex;

            public bool Equals(KeyMatcher other)
            {
                return keyValue == other.keyValue && isRegex == other.isRegex;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(keyValue, isRegex);
            }

            /// <summary>
            /// Returns whether if the given string matches.
            /// </summary>
            public bool IsMatch(string s)
            {
                if (!isRegex)
                {
                    return keyValue.Equals(s, StringComparison.Ordinal);
                }

                Regex r = new Regex(keyValue);
                return r.IsMatch(s);
            }
        }

        /// <summary>
        /// Key to new key value switching pairs.
        /// </summary>
        public SerializableDictionary<KeyMatcher, string> keySwitchPairs = new SerializableDictionary<KeyMatcher, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void DoMigration()
        {
            // Do nothing if there's no migration things
            if (Instance == null)
            {
                return;
            }

            if (Instance.migrationRecipe == null)
            {
                // Only log output if there's saves to migrate.
                if (Instance.keySwitchPairs.Count > 0)
                {
                    Debug.LogError($"[SaveMigratorData::DoMigration] Failed to do migration : Given MigratorData({Instance.name})'s recipe is null.");
                }

                return;
            }

            // Instance does exist, start migration
            foreach (KeyValuePair<KeyMatcher, string> pair in Instance.keySwitchPairs)
            {
                if (!Instance.migrationRecipe.HasKey(pair.Key))
                {
                    continue;
                }

                object temp = Instance.migrationRecipe.GetValueFromKey(pair.Key);
                Instance.migrationRecipe.SetValueToKey(pair.Value, temp);
                Instance.migrationRecipe.RemoveKey(pair.Key);
            }
            
            Instance.migrationRecipe.FinalizeSerialization();
        }
    }
}
