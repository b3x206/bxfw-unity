using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A recipe used to migrate save data.
    /// This recipe defines how types migrate to each other on which serialization context.
    /// </summary>
    public abstract class SaveMigratorRecipe : ScriptableObject
    {
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
        public abstract object GetValueFromKey(string key);

        /// <summary>
        /// Use this method to set value to the corresponding key.
        /// </summary>
        /// <param name="key">Key to set into.</param>
        /// <param name="value">Value to set the key.</param>
        public abstract void SetValueToKey(string key, object value);

        /// <summary>
        /// Method to use if key removal is needed.
        /// </summary>
        public virtual void RemoveKey(string key)
        { }

        /// <summary>
        /// Use this method to finalize your serialization.
        /// <br>If your system is something like <see cref="PlayerPrefs"/>, this can be left blank.</br>
        /// </summary>
        public abstract void FinalizeSerialization();
    }
}
