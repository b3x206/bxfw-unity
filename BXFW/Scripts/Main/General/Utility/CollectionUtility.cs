using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace BXFW
{
    /// <summary>
    /// Contains general utility of arrays.
    /// <br>Utility list :</br>
    /// <br>* Conversions &amp; casting of arrays to other array types,</br>
    /// <br>* Getting random values from enumerables,</br>
    /// <br>* Additional <see cref="Enumerable"/> linq methods,</br>
    /// <br>* Resizing and batch setting values of IList's.</br>
    /// </summary>
    public static class CollectionUtility
    {
        /// <summary>
        /// Converts a 2 dimensional array to an array of arrays.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="src">2-Dimensional array</param>
        /// <returns>Array of arrays of the same size the <paramref name="src"/>.</returns>
        public static TDest[][] Convert2DArray<TDest>(this TDest[,] src)
        {
            // Match input
            if (src == null)
            {
                return null;
            }

            // Get array dimensions
            int height = src.GetLength(0);
            int width = src.GetLength(1);

            // Create the new array
            TDest[][] tgt = new TDest[height][];

            // Cast the array to the arrays of array.
            for (int i = 0; i < height; i++)
            {
                // Create new member on index 'i' with the size of the first dimension
                tgt[i] = new TDest[width];

                // Set source.
                for (int j = 0; j < width; j++)
                {
                    tgt[i][j] = src[i, j];
                }
            }

            // Return it
            return tgt;
        }
        /// <summary>
        /// Converts a 3 dimensional array to an array of arrays.
        /// </summary>
        /// <typeparam name="TSrc">Type of array.</typeparam>
        /// <typeparam name="TDest">Destination type <c>(? Undocumented)</c></typeparam>
        /// <param name="src">3 Dimensional array.</param>
        /// <returns>Array of arrays of the same size the <paramref name="src"/>.</returns>
        public static TDest[][][] Convert3DArray<TSrc, TDest>(this TSrc[,,] src, Func<TSrc, TDest> converter)
        {
            // Match input
            if (src == null)
            {
                return null;
            }

            if (converter is null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            // Get array dimensions
            int iLen = src.GetLength(0);
            int jLen = src.GetLength(1);
            int kLen = src.GetLength(2);

            // Create the new array
            TDest[][][] tgt = new TDest[iLen][][];
            for (int i = 0; i < iLen; i++)
            {
                tgt[i] = new TDest[jLen][];
                for (int j = 0; j < jLen; j++)
                {
                    tgt[i][j] = new TDest[kLen];

                    for (int k = 0; k < kLen; k++)
                    {
                        tgt[i][j][k] = converter(src[i, j, k]);
                    }
                }
            }

            // Return it
            return tgt;
        }

        /// <summary>
        /// Get a random enum from enum type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="enumExceptionList">Enum list of values to ignore from. Duplicate values are ignored.</param>
        /// <returns>Randomly selected enum.</returns>
        /// <exception cref="InvalidCastException">Thrown when the type isn't enum. (<see cref="Type.IsEnum"/> is false)</exception>
        public static T GetRandomEnum<T>(T[] enumExceptionList = null)
            where T : Enum
        {
            List<T> enumList = new List<T>(Enum.GetValues(typeof(T)).Cast<T>());
            if (enumExceptionList.Length >= enumList.Count)
            {
#if UNITY_5_3_OR_NEWER
                Debug.LogWarning(string.Format("[CollectionUtility::GetRandomEnum] EnumToIgnore list is longer than array, returning 'default'. Bool : {0} >= {1}", enumExceptionList.Length, enumList.Count));
#endif
                return default;
            }

            // Convert 'enumExceptionList' to something binary searchable or fast
            HashSet<T> exceptionedEnums = new HashSet<T>(enumExceptionList);
            enumList.RemoveAll(e => exceptionedEnums.Contains(e));

            return enumList[UnityEngine.Random.Range(0, enumList.Count)];
        }

        /// <summary>
        /// Get a random value from a created IEnumerator.
        /// </summary>
        /// <param name="moveNextSize">Maximum times that the <see cref="IEnumerator.MoveNext"/> can be called. (array size basically)</param>
        /// <param name="enumerator">The iterable enumerator itself.</param>
        private static T GetRandomEnumeratorInternal<T>(int moveNextSize, IEnumerator<T> enumerator)
        {
            // Get size + check            
            if (moveNextSize <= 0)
            {
                // Reset value (it could be less than 0)
                moveNextSize = 0;

                // Count manually
                while (enumerator.MoveNext())
                {
                    moveNextSize++;
                }

                // Reset
                enumerator.Reset();
            }

            // Still zero? do nothing as there's no size.
            if (moveNextSize <= 0)
            {
                return default;
            }

            // Get rng value (according to size)
#if UNITY_5_3_OR_NEWER
            int rngValue = UnityEngine.Random.Range(0, moveNextSize);
#else
            var rand = new System.Random(unchecked((int)DateTime.Now.Ticks));
            int rngValue = rand.Next(0, moveNextSize);
#endif
            int current = 0;

            // Move the iterator manually
            while (enumerator.MoveNext())
            {
                if (current == rngValue)
                {
                    return enumerator.Current;
                }

                current++;
            }

            throw new IndexOutOfRangeException(string.Format("[CollectionUtility::GetRandom] Failed getting random : rngValue '{0}' was not in range of array sized '{1}'.", rngValue, current));
        }
        /// <summary>
        /// Returns a random value from an IEnumerable.
        /// <br>Also allows filtering using a predicate.</br>
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> values, Predicate<T> predicate)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "[CollectionUtility::GetRandom] 'values' is null.");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "[CollectionUtility::GetRandom] 'predicate' is null.");
            }

            IEnumerable<T> GetValuesFiltered()
            {
                foreach (T elem in values)
                {
                    if (!predicate(elem))
                    {
                        continue;
                    }

                    yield return elem;
                }
            }

            return GetRandom(GetValuesFiltered());
        }
        /// <summary>
        /// Returns a random value from an IEnumerable.
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "[CollectionUtility::GetRandom] 'values' is null.");
            }

            // Won't use the 'Linq Enumerable.Count' for saving 1 GetEnumerator creation+disposal (when the size is undefined).
            int valuesSize = -1;

            if (values is ICollection<T> collection)
            {
                valuesSize = collection.Count;
            }

            if (values is ICollection collection1)
            {
                valuesSize = collection1.Count;
            }

            // Get size + check
            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                return GetRandomEnumeratorInternal(valuesSize, enumerator);
            }
        }
        /// <summary>
        /// Returns a random value from an array. (faster)
        /// </summary>
        public static T GetRandom<T>(this IList<T> values)
        {
#if UNITY_5_3_OR_NEWER
            int randValue = UnityEngine.Random.Range(0, values.Count);
#else
            var rand = new System.Random(unchecked((int)DateTime.Now.Ticks));
            int randValue = rand.Next(0, values.Count);
#endif
            return values[randValue];
        }
        /// <summary>
        /// Returns a random value from an array matching the <paramref name="predicate"/>.
        /// </summary>
        public static T GetRandom<T>(this IList<T> values, Predicate<T> predicate)
        {
            // Create a filtered List?
            // .. this will GC.Alloc ..
            // I take 'GetEnumerator' over 'new List' with undefined size.
            return GetRandom((IEnumerable<T>)values, predicate);
        }

        /// <summary>
        /// Returns the index of <paramref name="value"/>, using <paramref name="predicate"/>.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> values, Predicate<T> predicate)
        {
            int i = 0;
            foreach (T cValue in values)
            {
                if (predicate(cValue))
                {
                    return i;
                }

                i++;
            }

            // Nothing found
            return -1;
        }
        /// <summary>
        /// Returns the index of value using <paramref name="comparer"/>.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> values, T value, IEqualityComparer<T> comparer)
        {
            int i = 0;
            foreach (T checkValue in values)
            {
                if (comparer.Equals(checkValue, value))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        /// <summary>
        /// Returns the minimum value in collection, but does not throw exceptions if the array is empty.
        /// </summary>
        public static T MinOrDefault<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            return MinOrDefault(collection, (T)default);
        }
        /// <summary>
        /// Returns the minimum value in collection, but does not throw exceptions if the array is empty.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the array is empty.</param>
        public static T MinOrDefault<T>(this IEnumerable<T> collection, T defaultValue) where T : IComparable<T>
        {
            T min = defaultValue;
            if (collection != null)
            {
                foreach (T elem in collection)
                {
                    if (UnitySafeEqualityComparer.Default.Equals(elem, null))
                    {
                        continue;
                    }

                    // Smaller
                    if (elem.CompareTo(min) < 0)
                    {
                        min = elem;
                    }
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the maximum value in collection, but does not throw exceptions if the array is empty.
        /// </summary>
        public static T MaxOrDefault<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            return MaxOrDefault(collection, (T)default);
        }
        /// <summary>
        /// Returns the maximum value in collection, but does not throw exceptions if the array is empty.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the array is empty.</param>
        public static T MaxOrDefault<T>(this IEnumerable<T> collection, T defaultValue) where T : IComparable<T>
        {
            T max = defaultValue;
            if (collection != null)
            {
                foreach (T elem in collection)
                {
                    if (UnitySafeEqualityComparer.Default.Equals(elem, null))
                    {
                        continue;
                    }

                    // Larger
                    if (elem.CompareTo(max) > 0)
                    {
                        max = elem;
                    }
                }
            }

            return max;
        }

        /// <summary>
        /// Returns the first value or the given <paramref name="defaultValue"/> if no elements exist.
        /// <br>Useful for value types which cannot be nullable.</br>
        /// </summary>
        public static T FirstOrDefault<T>(this IEnumerable<T> collection, T defaultValue)
        {
            if (collection != null)
            {
                // Return the first element immediately as no predicates
                // This foreach loop won't begin if there's no elements inside 'collection'.
                foreach (T element in collection)
                {
                    return element;
                }
            }

            return defaultValue;
        }
        /// <summary>
        /// Returns the first value or the given <paramref name="defaultValue"/> if no elements exist.
        /// <br>Useful for value types which cannot be nullable.</br>
        /// </summary>
        /// <param name="predicate">
        /// The predicate to match for all elements. 
        /// Matching first element is returned. This must not be null.
        /// </param>
        /// <exception cref="NullReferenceException"/>
        public static T FirstOrDefault<T>(this IEnumerable<T> collection, Predicate<T> predicate, T defaultValue)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "[CollectionUtility::FirstOrDefault] Given 'predicate' parameter is null.");
            }

            if (collection != null)
            {
                foreach (T element in collection)
                {
                    if (predicate(element))
                    {
                        return element;
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>Replaces multiple chars in a string built by <paramref name="builder"/>.</summary>
        /// <param name="builder">The string to modify. Put in a string builder.</param>
        /// <param name="toReplace">Chars to replace.</param>
        /// <param name="replacement">New chars put after replacing.</param>
        public static void MultiReplace(this StringBuilder builder, char[] toReplace, char replacement)
        {
            for (int i = 0; i < builder.Length; ++i)
            {
                char currentCharacter = builder[i];

                // Check if there's a match with the chars to replace.
                if (toReplace.Any(c => currentCharacter == c))
                {
                    builder[i] = replacement;
                }
            }
        }
        /// <summary>
        /// <see cref="Enumerable.Cast{TResult}(System.Collections.IEnumerable)"/> with a converter delegate.
        /// </summary>
        /// <typeparam name="TResult">Target type to cast into.</typeparam>
        /// <typeparam name="TParam">Gathered parameter type.</typeparam>
        /// <param name="enumerable">Enumerable itself. (usually an array)</param>
        /// <param name="converter">Converter delegate. (method throws <see cref="NullReferenceException"/> if null)</param>
        public static IEnumerable<TResult> Cast<TResult, TParam>(this IEnumerable<TParam> enumerable, Func<TParam, TResult> converter)
        {
            if (converter == null)
            {
                throw new NullReferenceException("[CollectionUtility::Cast] Given 'converter' parameter is null.");
            }

            foreach (TParam t in enumerable)
            {
                yield return converter(t);
            }
        }
        /// <summary>
        /// Allows <see cref="IEnumerable{T}"/> anything to be iterable with an index.
        /// </summary>
        public static IEnumerable<KeyValuePair<int, T>> Indexed<T>(this IEnumerable<T> enumerable)
        {
            int i = -1;
            foreach (T value in enumerable)
            {
                i++;
                yield return new KeyValuePair<int, T>(i, value);
            }
        }

        // -- Array Utils
        public static void RemoveRange<T>(this IList<T> l, int index, int count)
        {
            for (; count > 0; count--, index++)
            {
                l.RemoveAt(index);
            }
        }
        public static void AddRange<T>(this IList<T> l, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                l.Add(item);
            }
        }

        /// <summary>
        /// Resizes an IList array.
        /// </summary>
        /// <param name="newT">
        /// The instance of a new generic.
        /// This is added due to '<typeparamref name="T"/>' not being a '<see langword="new"/> <typeparamref name="T"/>()' able type.
        /// </param>
        public static void Resize<T>(this IList<T> list, int sz, T newT)
        {
            int cur = list.Count;
            if (sz < cur)
            {
                list.RemoveRange(sz, cur - sz);
            }
            else if (sz > cur)
            {
                list.AddRange(Enumerable.Repeat(newT, sz - cur));
            }
        }
        /// <summary>
        /// Resizes an IList array.
        /// </summary>
        public static void Resize<T>(this IList<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }
        public static void Resize<T>(this List<T> list, int sz, T newT)
        {
            // Optimize
            if (sz > list.Capacity)
            {
                list.Capacity = sz;
            }

            Resize((IList<T>)list, sz, newT);
        }
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }

        /// <summary>Resets array values to their default values.</summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="array">The array to reset it's values.</param>
        public static void ResetArray<T>(this T[] array)
        {
            T genDefValue = default;

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = genDefValue;
            }
        }
        /// <summary>
        /// Converts a <see cref="Array"/> to a typed array.
        /// <br>This creates a new array.</br>
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public static T[] ToTypeArray<T>(this Array target)
        {
            T[] arrayReturn = new T[target.Length];

            for (int i = 0; i < target.Length; i++)
            {
                arrayReturn[i] = (T)target.GetValue(i);
            }

            return arrayReturn;
        }
    }
}
