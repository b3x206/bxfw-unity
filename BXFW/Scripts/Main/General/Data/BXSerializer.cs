

namespace BXFW.Data
{
    /// <summary>
    /// A simple serializer.
    /// </summary>
    public static class BXSerializer
    {
        // BXSerializer : 
        // Serialize using either custom plain text data or just parse JSON?
        // Option 1 : Custom plain text =>
        //     This will allow the most control, but since c# is just GC collected language it may end up having more GC compared to PlayerPrefs
        //     It has to be programmed in a careful way, but with c#, GCless strings are very hard or something. (and c# is UTF-16 by default which also sucks again)
        //     Except for the control benefit this doesn't seem to be any better or something?
        // Option 2 : RapidJSON wrapper of Unity 'JSONUtility' =>
        //     This has the least control and the least amount of GC allocated.
        //     But it has very low amount of control (just serialize+deserialize functions), we may need to manage multiple files (which would suck on something like NTFS)
        // -- So yeah, idk what to choose.
        // Other than that, BXSerializer will have a Dirty/Save function pair, it will periodically check a Save task that occurs every given 'seconds'
        // The file handle will be kept open during the lifetime of the game and when the game quits (OnApplicationQuit) the changes set to the BXSerializer (or call this something else?) will be written.
        // This just only has the drawback of in a case of a crash data will be lost unlike just constantly setting PlayerPrefs directly
        // but i mean, it is just better to write to the disk when the data is dirty (set values per key are different) and do it so periodically or just do it on programmer's demand.

        /// <inheritdoc cref="IsDirty"/>
        private static bool m_IsDirty = false;
        /// <summary>
        /// Whether if the serializer is dirty.
        /// </summary>
        public static bool IsDirty => m_IsDirty;

    }
}
