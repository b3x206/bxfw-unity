using System;
using UnityEngine;
using static BXFW.SaveMigratorData;

namespace BXFW
{
    /// <summary>
    /// A recipe used to migrate save data.
    /// This recipe defines how types migrate to each other on which serialization context.
    /// </summary>
    public abstract class SaveMigratorRecipe : ScriptableObject
    {
        /// <summary>
        /// Defines a type hint for a recipe element.
        /// <br>Can be used in cases where the serializer doesn't do type independent serialization.</br>
        /// </summary>
        [Flags]
        public enum TypeHint
        {
            None = 0,
            /// <summary>
            /// Generic type, can be converted to serialization string format.
            /// </summary>
            Generic = 1 << 0,
            /// <summary>
            /// String type, can be written to string directly or be converted to bytes.
            /// </summary>
            String = 1 << 1,
            /// <summary>
            /// Integer (of any size) type.
            /// </summary>
            Integer = 1 << 2,
            /// <summary>
            /// Float (of any size) type.
            /// </summary>
            Float = 1 << 3,

            /// <summary>
            /// All type serialization flags.
            /// </summary>
            All = ~0
        }

        public abstract TypeHint TypeSerializationSupportFlags { get; }

        // How does a save migrator work?
        // A : Store a series of keys or a data structure layout
        // B : Search for the old keys or load data struct according to old layout on the recipe.
        // C : Store the old keys values
        // D : Remove old keys and it's values
        // E : Make the new keys it's values (if the value type mismatches create an error buffer for keys and error type)
        // E : ???
        // F : Profit

        /// <summary>
        /// Returns the key from the saved data list.
        /// </summary>
        /// <param name="key">Key of this given data.</param>
        public abstract object GetValueFromKey(KeyMatcher key);

        /// <summary>
        /// Use this method to set value to the corresponding key.
        /// </summary>
        /// <param name="key">Key to set into.</param>
        /// <param name="value">Value to set the key.</param>
        public abstract void SetValueToKey(string key, object value);

        /// <summary>
        /// Method to use if key removal is needed.
        /// </summary>
        public virtual void RemoveKey(KeyMatcher key)
        { }

        /// <summary>
        /// Method to use if key exists.
        /// </summary>
        public abstract bool HasKey(KeyMatcher key);

        /// <summary>
        /// Use this method to finalize your serialization.
        /// <br>If your system is something like <see cref="PlayerPrefs"/>, this can be left blank.</br>
        /// </summary>
        public abstract void FinalizeSerialization();
    }
}
