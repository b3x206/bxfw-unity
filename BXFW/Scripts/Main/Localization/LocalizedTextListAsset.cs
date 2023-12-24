using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// A <see cref="LocalizedTextData"/> list asset.
    /// <br>The inline editor allows text ID uniqueness and adding/removing those.</br>
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "TextListAsset", menuName = "BXFW/Localization/Text List Asset")]
    public class LocalizedTextListAsset : ScriptableObjectSingleton<LocalizedTextListAsset>, IList<LocalizedTextData>
    {
        /// <summary>
        /// Pragmatic behaviour definitions for the LocalizedTextListAsset.
        /// </summary>
        /// <br>TODO : Proper key selector for this?</br>
        public SerializableDictionary<string, string> pragmaDefinitons = new SerializableDictionary<string, string>();
        /// <summary>
        /// Data contained inside this text list asset.
        /// </summary>
        [SerializeField] private List<LocalizedTextData> m_textList = new List<LocalizedTextData>();

        /// <summary>
        /// Returns whether if the given <paramref name="textID"/> already exists in the text list.
        /// </summary>
        public bool TextIDExists(string textID)
        {
            for (int i = 0; i < m_textList.Count; i++)
            {
                if (m_textList[i].TextID == textID)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Returns whether if the given <paramref name="data"/>'s <see cref="LocalizedTextData.TextID"/> already exists in the text list.
        /// </summary>
        public bool TextIDExists(LocalizedTextData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "[LocalizedTextListAsset::IsTextIDUnique] Given LocalizedTextData is null.");
            }

            return TextIDExists(data.TextID);
        }

        /// <summary>
        /// <br><c><see langword="get"/> : </c></br>
        /// <br>The pretty standard list getter.</br>
        /// <br><c><see langword="set"/> : </c></br>
        /// <br>Sets a value to the <see cref="LocalizedTextListAsset"/>.</br>
        /// <br>The given value cannot be null or have it's text id already existing (check using <see cref="TextIDExists(LocalizedTextData)"/>).</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public LocalizedTextData this[int index]
        {
            get => m_textList[index];
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "[LocalizedTextListAsset::this[]::set] Given LocalizedTextData is null.");
                }
                if (TextIDExists(value))
                {
                    throw new ArgumentException($"[LocalizedTextListAsset::this[]::set] Given LocalizedTextData's TextID {value.TextID} is not unique.", nameof(value));
                }

                m_textList[index] = value;
            }
        }

        public int Count => m_textList.Count;
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a value to the <see cref="LocalizedTextListAsset"/>.
        /// <br>The given value cannot be null or have it's text id already existing (check using <see cref="TextIDExists(LocalizedTextData)"/>).</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void Add(LocalizedTextData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "[LocalizedTextListAsset::Add] Given LocalizedTextData is null.");
            }
            if (TextIDExists(item))
            {
                throw new ArgumentException($"[LocalizedTextListAsset::Add] Given LocalizedTextData's TextID {item.TextID} is not unique.", nameof(item));
            }

            m_textList.Add(item);
        }

        /// <summary>
        /// Adds a range of values to the <see cref="LocalizedTextListAsset"/>.
        /// <br>The given value cannot be null or have it's text id already existing (check using <see cref="TextIDExists(LocalizedTextData)"/>).</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void AddRange(IEnumerable<LocalizedTextData> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "[LocalizedTextListAsset::AddRange] Given collection was null.");
            }

            foreach (LocalizedTextData data in collection)
            {
                Add(data);
            }
        }
        /// <inheritdoc cref="AddRange(IEnumerable{LocalizedTextData})"/>
        public void AddRange(IList<LocalizedTextData> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list), "[LocalizedTextListAsset::AddRange] Given list was null.");
            }

            for (int i = 0; i < list.Count; i++)
            {
                Add(list[i]);
            }
        }

        public void Clear()
        {
            m_textList.Clear();
        }

        public bool Contains(LocalizedTextData item)
        {
            return m_textList.Contains(item);
        }

        public void CopyTo(LocalizedTextData[] array, int arrayIndex)
        {
            m_textList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<LocalizedTextData> GetEnumerator()
        {
            return m_textList.GetEnumerator();
        }

        public int IndexOf(LocalizedTextData item)
        {
            return m_textList.IndexOf(item);
        }

        /// <summary>
        /// Inserts a value to the <see cref="LocalizedTextListAsset"/>.
        /// <br>The given value cannot be null or have it's text id already existing (check using <see cref="TextIDExists(LocalizedTextData)"/>).</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void Insert(int index, LocalizedTextData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "[LocalizedTextListAsset::Insert] Given LocalizedTextData is null.");
            }
            if (TextIDExists(item))
            {
                throw new ArgumentException($"[LocalizedTextListAsset::Insert] Given LocalizedTextData's TextID {item.TextID} is not unique.", nameof(item));
            }

            m_textList.Insert(index, item);
        }

        public bool Remove(LocalizedTextData item)
        {
            return m_textList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            m_textList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_textList.GetEnumerator();
        }
    }
}
