using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BXFW.Collections;

namespace BXFW.Data
{
    /// <summary>
    /// A simple serializer that is roughly made but should work better at serializing other types (with less GC).
    /// <br/>
    /// <br>Still WIP, stuff may not work or is subject to change.</br>
    /// </summary>
    public static class BXSerializer
    {
        // BXSerializer : 
        // Serialize using either custom plain text data or just parse JSON?
        // Option 1 : Custom plain text =>
        //     This will allow the most control, but since c# is just GC collected language it may end up having more GC compared to PlayerPrefsUtility
        //     It has to be programmed in a careful way, but with c#, GCless strings are very hard or something. (and c# is UTF-16 by default which also sucks again)
        //     Except for the control benefit this doesn't seem to be any better or something?
        // Option 2 : RapidJSON wrapper of Unity 'JSONUtility' =>
        //     This has the least control and the least amount of GC allocated.
        //     But it has very low amount of control (just serialize+deserialize functions), we may need to manage multiple files (which would suck on something like NTFS)
        //     And it's not the most efficient way of serializing things
        // --
        // -- So yeah, idk what to choose. Both have their drawbacks and advantages.
        // --
        // Other than that, BXSerializer will have a Dirty/Save function pair, it will periodically check a Save task that occurs every given 'seconds'
        // The file handle will be kept open during the lifetime of the game and when the game quits (OnApplicationQuit) the changes set to the BXSerializer (or call this something else?) will be written.
        // This just only has the drawback of in a case of a crash data will be lost unlike just constantly setting PlayerPrefs directly
        // but i mean, it is just better to write to the disk when the data is dirty (set values per key are different) and do it so periodically or just do it on programmer's demand.

        /// <summary>
        /// Contains a list of binary bytes.
        /// </summary>
        [Serializable]
        private sealed class BinaryArray : List<byte> { }

        /// <summary>
        /// Contains the internal data to be serialized.
        /// </summary>
        [Serializable]
        private sealed class DataSerializationContainer
        {
            /// <summary>
            /// Contains all general purpose numbers.
            /// </summary>
            public SerializableDictionary<string, long> longValues;
            /// <summary>
            /// Contains all general purpose strings.
            /// </summary>
            public SerializableDictionary<string, string> stringValues;
            /// <summary>
            /// Contains all general purpose binary datas.
            /// </summary>
            public SerializableDictionary<string, BinaryArray> binaryValues;

            /// <summary>
            /// Empty serialization container ctor.
            /// </summary>
            public DataSerializationContainer()
            {
                longValues = new SerializableDictionary<string, long>();
                stringValues = new SerializableDictionary<string, string>();
                binaryValues = new SerializableDictionary<string, BinaryArray>();
            }

            /// <summary>
            /// Creates a container with given capacity.
            /// </summary>
            public DataSerializationContainer(int capacity)
            {
                longValues = new SerializableDictionary<string, long>(capacity);
                stringValues = new SerializableDictionary<string, string>(capacity);
                binaryValues = new SerializableDictionary<string, BinaryArray>(capacity);
            }
        }

        /// <summary>
        /// Whether if the serializer is dirty (a key was assigned a object value).
        /// </summary>
        public static bool IsDirty { get; private set; }

        private static readonly string DefaultSavePath = Application.persistentDataPath;
        /// <inheritdoc cref="SavePath"/>
        private static string m_SavePath = DefaultSavePath;
        /// <summary>
        /// The save path to save the files into.
        /// <br>Validated on the <see langword="set"/>ter and will throw <see cref="ArgumentException"/> if the given path is not valid.</br>
        /// </summary>
        public static string SavePath
        {
            get
            {
                return m_SavePath;
            }
            set
            {
                if (!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
                {
                    throw new ArgumentException($"[BXSerializer::SavePath(set)] Given path value \"{value}\" is not valid.", nameof(value));
                }

                m_SavePath = value;
            }
        }

        private static readonly string DefaultSaveName = "bxs-data";
        /// <summary>
        /// Name of the file to save as.
        /// </summary>
        public static string SaveName = DefaultSaveName;

        /// <summary>
        /// Each seconds to wait for the next auto serialize for the total data.
        /// </summary>
        public static float AutoSaveInterval { get; set; } = 30f;
        /// <summary>
        /// Minimum acceptable auto-saving interval.
        /// </summary>
        public const float MinimumAutoSaveInterval = 0.2f;

        private static DataSerializationContainer m_dataSerializationContainer = null;
        private static float m_currentAutoSaveTimer = 0f;
        private static FileStream m_primaryFileStream = null;
        private static byte[] m_readBytes = new byte[255];
        private static byte[] m_writeBytes = new byte[255];

        static BXSerializer()
        {
            // Hook for the auto save + save on exit feature
            if (MainTickRunner.Instance != null)
            {
                if (AutoSaveInterval > MinimumAutoSaveInterval)
                {
                    MainTickRunner.Instance.OnTick += OnTick;
                }

                MainTickRunner.Instance.OnExit += OnExit;
            }
        }

        private static void OnTick(ITickRunner runner)
        {
            if (m_currentAutoSaveTimer > AutoSaveInterval)
            {
                m_currentAutoSaveTimer = 0;
                Save();
                return;
            }

            m_currentAutoSaveTimer += runner.UnscaledDeltaTime;
        }
        private static void OnExit(bool applicationQuit)
        {
            if (applicationQuit)
            {
                Save();
            }
        }

        /// <summary>
        /// Loads the SaveData from the file.
        /// <br>This should only be done if the current session desynchronizes with the save data file or on the start if the m_dataSerializationContainer isn't loaded.</br>
        /// </summary>
        public static void Load()
        {
            string filePath = Path.Combine(SavePath, SaveName);
            m_primaryFileStream ??= File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (m_readBytes.Length < m_primaryFileStream.Length)
            {
                m_readBytes = new byte[m_primaryFileStream.Length + 1];
            }

            m_primaryFileStream.Read(m_readBytes, 0, (int)m_primaryFileStream.Length);
            m_primaryFileStream.Position = 0;

            // TODO : This may throw?
            m_dataSerializationContainer = JsonUtility.FromJson<DataSerializationContainer>(Encoding.UTF8.GetString(m_readBytes));
            // Ensure that the 'm_dataSerializationContainer' is no longer null after calling 'Load'.
            m_dataSerializationContainer ??= new DataSerializationContainer();
        }

        /// <summary>
        /// Saves all unwritten information to the disk and undirties the data.
        /// </summary>
        public static void Save()
        {
            string filePath = Path.Combine(SavePath, SaveName);
            m_primaryFileStream ??= File.Exists(filePath) ? File.Open(filePath, FileMode.Truncate, FileAccess.ReadWrite) : File.Create(filePath);
            m_primaryFileStream.SetLength(0);

            string toWrite = JsonUtility.ToJson(m_dataSerializationContainer);
            int byteCount = Encoding.UTF8.GetMaxByteCount(toWrite.Length);
            if (m_writeBytes.Length < byteCount)
            {
                m_writeBytes = new byte[byteCount];
            }

            int writeBytesLength = Encoding.UTF8.GetBytes(toWrite, 0, toWrite.Length, m_writeBytes, 0);
            m_primaryFileStream.Write(m_writeBytes, 0, writeBytesLength);
            m_primaryFileStream.Position = 0;

            IsDirty = false;
        }

        /// <summary>
        /// Sets the data to as dirty to have the autosave scheduled.
        /// </summary>
        public static void MarkDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Returns whether if any data with the <paramref name="key"/> is saved.
        /// </summary>
        /// <param name="key">Key to check whether if it's saved. This cannot be null or empty or whitespace.</param>
        /// <exception cref="ArgumentException"/>
        public static bool HasKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("[BXSerializer::SetLong] Given 'key' argument is invalid. Key cannot be null, string.Empty or whitespace.", nameof(key));
            }

            return m_dataSerializationContainer.longValues.ContainsKey(key) || m_dataSerializationContainer.stringValues.ContainsKey(key) || m_dataSerializationContainer.binaryValues.ContainsKey(key);
        }

        /// <summary>
        /// Gets a long int corresponding to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to get it's corresponding <paramref name="key"/> to. This cannot be null or empty or whitespace.</param>
        /// <param name="defaultValue"></param>
        /// <exception cref="ArgumentException"/>
        public static long GetLong(string key, long defaultValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("[BXSerializer::SetLong] Given 'key' argument is invalid. Key cannot be null, string.Empty or whitespace.", nameof(key));
            }
            
            if (m_dataSerializationContainer == null)
            {
                Load();
            }

            if (m_dataSerializationContainer.longValues.TryGetValue(key, out long resultValue))
            {
                return resultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a long int to <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to set it's corresponding <paramref name="value"/> to. This cannot be null or empty or whitespace.</param>
        /// <param name="value">Value to set.</param>
        /// <exception cref="ArgumentException"/>
        public static void SetLong(string key, long value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("[BXSerializer::SetLong] Given 'key' argument is invalid. Key cannot be null, string.Empty or whitespace.", nameof(key));
            }

            if (m_dataSerializationContainer.longValues.TryGetValue(key, out long currentValue))
            {
                // Same value, no need to change or check.
                if (currentValue == value)
                {
                    return;
                }

                m_dataSerializationContainer.longValues[key] = value;
                MarkDirty();
                return;
            }

            m_dataSerializationContainer.longValues.Add(key, value);
            MarkDirty();
        }

    }
}
