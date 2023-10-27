using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Callbacks;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Provides all of the types in this current domain.
    /// <br>Can be used with type searching related things, but the searching algorithm is still slow.</br>
    /// <br>This class provides a cached list of types for this domain and allows faster access and search.</br>
    /// </summary>
    public static class TypeListProvider
    {
        // TODO : A haystack type search method?
        /// <summary>
        /// The current domain's assemblys cache.
        /// <br>Reset and loaded once unity recompiles code.</br>
        /// </summary>
        public static Assembly[] CurrentDomainAssembliesCache;
        /// <summary>
        /// List of all types in the current <see cref="AppDomain"/>.
        /// <br>This should be respectfully considered read only, but it can be assigned into.</br>
        /// </summary>
        public static Type[] AllDomainTypesCache;

        /// <summary>
        /// List of all special types contained to get cached results for existing predicate types instead.
        /// </summary>
        private static Dictionary<long, Type[]> HashedTypeResultsPair = new Dictionary<long, Type[]>();

        [DidReloadScripts]
        public static void Refresh()
        {
            CurrentDomainAssembliesCache = AppDomain.CurrentDomain.GetAssemblies();
            AllDomainTypesCache = Array.Empty<Type>();
            HashedTypeResultsPair.Clear();

            for (int i = 0, copyIndex = 0; i < CurrentDomainAssembliesCache.Length; i++)
            {
                Assembly assembly = CurrentDomainAssembliesCache[i];
                // maybe if this 'GetTypes' returns values sorted we can do binary search
                // If not it can still be sorted
                Type[] asmTypes = assembly.GetTypes();

                // Copy types to a cache value
                Array.Resize(ref AllDomainTypesCache, AllDomainTypesCache.Length + asmTypes.Length);
                Array.Copy(asmTypes, 0, AllDomainTypesCache, copyIndex, asmTypes.Length);
                copyIndex += asmTypes.Length;
            }
        }

        /// <summary>
        /// Returns a list of types by predicate applied to types.
        /// <br>These results are cached according to the given <paramref name="predicate"/>'s results and returned fastly.</br>
        /// </summary>
        /// <param name="predicate">Function delegate to match by.</param>
        /// <param name="noHashCheck">Whether to always iterate the <see cref="AllDomainTypesCache"/>.</param>
        /// <returns>
        /// The type list <see cref="AllDomainTypesCache"/> filtered.
        /// The results are cached with the same predicate until domain reset.
        /// </returns>
        public static Type[] GetDomainTypesByPredicate(Predicate<Type> predicate, bool noHashCheck = false)
        {
            long predHash = predicate.GetHashCode();
            Type[] results;
            if (!noHashCheck)
            {
                if (HashedTypeResultsPair.TryGetValue(predHash, out results))
                {
                    return results;
                }
            }
            
            // This maneuver is gonna cost us 51 years
            if (AllDomainTypesCache.Length <= 0)
            {
                Refresh();
            }

            List<Type> tempResults = new List<Type>(32);
            foreach (Type t in AllDomainTypesCache)
            {
                if (predicate(t))
                    tempResults.Add(t);
            }

            results = tempResults.ToArray();
            HashedTypeResultsPair.Add(predHash, results);
            return tempResults.ToArray();
        }
    }
}
