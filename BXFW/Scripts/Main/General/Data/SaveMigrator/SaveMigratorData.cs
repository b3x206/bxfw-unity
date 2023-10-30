using System;
using System.Text.RegularExpressions;

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
        public struct KeyMatcher
        {
            public string keyValue;
            public bool isRegex;

            /// <summary>
            /// Returns whether if the given string matches.
            /// </summary>
            public bool IsMatch(string s)
            {
                if (!isRegex)
                    return keyValue.Equals(s, StringComparison.Ordinal);

                Regex r = new Regex(keyValue);
                return r.IsMatch(s);
            }
        }

        /// <summary>
        /// Key to new key value switching pairs.
        /// </summary>
        public SerializableDictionary<KeyMatcher, string> keySwitchPairs = new SerializableDictionary<KeyMatcher, string>();
    }
}
