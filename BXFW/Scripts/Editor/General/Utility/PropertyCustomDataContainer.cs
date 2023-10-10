namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Contains the data saved on <see cref="SerializedPropertyCustomData"/> as values.
    /// </summary>
    public class PropertyCustomDataContainer : ScriptableObjectSingleton<PropertyCustomDataContainer>
    {
        public SerializableDictionary<string, int> savedIntValues = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, string> savedStringValues = new SerializableDictionary<string, string>();
    }
}
