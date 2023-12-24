namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Contains the data saved on <see cref="SerializedPropertyCustomData"/> as values.
    /// </summary>
    public class PropertyCustomDataContainer : ScriptableObjectSingleton<PropertyCustomDataContainer>
    {
        public SerializableDictionary<string, long> savedIntValues = new SerializableDictionary<string, long>();
        public SerializableDictionary<string, string> savedStringValues = new SerializableDictionary<string, string>();

        public void Reset()
        {
            savedIntValues.Clear();
            savedStringValues.Clear();
        }
    }
}
