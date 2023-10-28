using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Callbacks;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Defines a list of type flags to use with <see cref="TypeListProvider"/> methods.
    /// <br><see cref="None"/> =&gt; No assembly determined. This parameter is ignored like <see cref="Uncategorized"/>.</br>
    /// <br><see cref="MsCorlib"/> =&gt; The mscorlib System thing.</br>
    /// <br><see cref="SystemLib"/> =&gt; Library that is a 'System' that is on the global assembly cache.</br>
    /// <br><see cref="UnityAssembly"/> =&gt; Assembly used by unity's core libraries (this is determined by the 'SharedInternalsModule')</br>
    /// <br><see cref="AssemblyCSharp"/> =&gt; Scripts only contained in Assembly-CSharp. 'Plugins' assets are also included here (firstpass).</br>
    /// <br><see cref="AssemblyCSharpEditor"/> =&gt; Scripts only contained in Assembly-CSharp-Editor, the default editor scripts assembly.</br>
    /// <br><see cref="BXFW"/> =&gt; Scripts contained in BXFW assembly.</br>
    /// <br><see cref="BXFWEditor"/> =&gt; Scripts contained in BXFW.Editor assembly.</br>
    /// <br><see cref="Dynamic"/> =&gt; Assemblies that don't fit into a category but is <see cref="Assembly.IsDynamic"/>.</br>
    /// <br><see cref="AssetScriptAssembly"/> =&gt; Assemblies contained in <see cref="Directory.GetCurrentDirectory"/>/ScriptAssemblies.</br>
    /// <br><see cref="All"/> =&gt; Inclusive of all assemblies contained.</br>
    /// <br><see cref="Uncategorized"/> =&gt; Uncategorized / undefined scripts.</br>
    /// <br></br>
    /// </summary>
    [Flags]
    public enum AssemblyFlags
    {
        None = 0,
        MsCorlib = 1 << 0,
        SystemLib = 1 << 1,
        UnityAssembly = 1 << 2,
        AssemblyCSharp = 1 << 3,
        AssemblyCSharpEditor = 1 << 4,
        BXFW = 1 << 5,
        BXFWEditor = 1 << 6,
        Dynamic = 1 << 7,
        AssetScriptAssembly = 1 << 8,

        All = unchecked(~0),
        Uncategorized = unchecked(1 << 31),
    }

    /// <summary>
    /// Provides all of the types in this current domain.
    /// <br>Can be used with type searching related things, but the searching algorithm is still slow (but it's cached).</br>
    /// <br>This class provides a cached list of types for this domain and allows faster access and search.</br>
    /// </summary>
    public static class TypeListProvider
    {
        /// <summary>
        /// The current domain's assemblys cache.
        /// <br>Reset and loaded once unity recompiles code.</br>
        /// </summary>
        public static Assembly[] CurrentDomainAssembliesCache;
        /// <summary>
        /// List of all types in the current <see cref="AppDomain"/>.
        /// <br>This should be respectfully considered read only, but it can be assigned into.</br>
        /// </summary>
        //public static Type[] AllDomainTypesCache;
        public static Dictionary<AssemblyFlags, Type[]> DomainTypesList = new Dictionary<AssemblyFlags, Type[]>(255);

        /// <summary>
        /// List of all special types contained to get cached results for existing predicate types instead.
        /// </summary>
        private static Dictionary<long, Type[]> HashedTypeResultsPair = new Dictionary<long, Type[]>();

        /// <summary>
        /// Determines the assembly type flag information about the given <paramref name="asm"/>.
        /// </summary>
        public static AssemblyFlags GetAssemblyFlag(Assembly asm)
        {
            if (Assembly.GetAssembly(typeof(object)) == asm)
            {
                return AssemblyFlags.MsCorlib;
            }
            // Check if the assembly is in MS_GAC or GAC
            if (asm.GlobalAssemblyCache)
            {
                // Check name patterning (note : GAC should enforce this well for the time being?)
                string assemblyName = asm.FullName;
                if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Microsoft."))
                {
                    return AssemblyFlags.SystemLib;
                }
            }
            // These are fragile
            if (asm.GetName().Name == "UnityEngine.SharedInternalsModule" || asm.GetReferencedAssemblies().Any((asmName) => asmName.Name == "UnityEngine.SharedInternalsModule"))
            {
                return AssemblyFlags.UnityAssembly;
            }
            if (asm.FullName.StartsWith("Assembly-CSharp"))
            {
                return asm.FullName.Contains("Editor") ? AssemblyFlags.AssemblyCSharpEditor : AssemblyFlags.AssemblyCSharp;
            }
            if (asm.FullName.StartsWith("BXFW"))
            {
                return asm.FullName.Contains("Editor") ? AssemblyFlags.BXFWEditor : AssemblyFlags.BXFW;
            }
            // Getting assembly location while the assembly is dynamic is not supported.
            if (asm.IsDynamic)
            {
                return AssemblyFlags.Dynamic;
            }
            // Contained in 'ProjectRoot/ScriptAssemblies' file of unity
            if (asm.Location.Contains(Directory.GetCurrentDirectory()))
            {
                return AssemblyFlags.AssetScriptAssembly;
            }
            
            return AssemblyFlags.Uncategorized;
        }

        private static IEnumerable<Type[]> GetValuesFromFlags(AssemblyFlags flags)
        {
            if (flags == AssemblyFlags.All)
            {
                foreach (Type[] t in DomainTypesList.Values)
                {
                    yield return t;
                }
            }

            for (int i = 0; i < 32; i++)
            {
                unchecked
                {
                    int currentFlagBit = (1 << i);
                    if (((int)flags & currentFlagBit) == currentFlagBit && DomainTypesList.TryGetValue((AssemblyFlags)currentFlagBit, out Type[] types))
                    {
                        yield return types;
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the list of assembly types.
        /// </summary>
        [DidReloadScripts]
        public static void Refresh()
        {
            CurrentDomainAssembliesCache = AppDomain.CurrentDomain.GetAssemblies();
            //AllDomainTypesCache = Array.Empty<Type>();
            DomainTypesList.Clear();
            HashedTypeResultsPair.Clear();

            // int copyIndex = 0;
            for (int i = 0; i < CurrentDomainAssembliesCache.Length; i++)
            {
                Assembly assembly = CurrentDomainAssembliesCache[i];

                // maybe if this 'GetTypes' returns values sorted we can do binary search
                // If not it can still be sorted
                Type[] asmTypes = assembly.GetTypes();
                AssemblyFlags asmFlag = GetAssemblyFlag(assembly);
                
                // UnityEngine.Debug.Log($"[Adding] | Assembly : C:{asmFlag} N:{assembly.GetName()}, Category Exists: {DomainTypesList.ContainsKey(asmFlag)}");
                if (DomainTypesList.TryGetValue(asmFlag, out Type[] flagTypes))
                {
                    int prevSize = flagTypes.Length;
                    Array.Resize(ref flagTypes, flagTypes.Length + asmTypes.Length);
                    Array.Copy(asmTypes, 0, flagTypes, prevSize, asmTypes.Length);
                }
                else
                {
                    DomainTypesList.Add(asmFlag, asmTypes);
                }

                //Type[] asmTypes = assembly.GetTypes();

                //// Copy types to a cache value
                //Array.Resize(ref AllDomainTypesCache, AllDomainTypesCache.Length + asmTypes.Length);
                //Array.Copy(asmTypes, 0, AllDomainTypesCache, copyIndex, asmTypes.Length);
                //copyIndex += asmTypes.Length;
            }
            
            // Ensure that the dictionary only uses the memory it needs.
            DomainTypesList.TrimExcess();
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
        public static Type[] GetDomainTypesByPredicate(Predicate<Type> predicate, AssemblyFlags flags = AssemblyFlags.All, bool noHashCheck = false)
        {
            long argsHash = HashCode.Combine(flags, predicate.GetHashCode());
            Type[] results;
            if (!noHashCheck)
            {
                if (HashedTypeResultsPair.TryGetValue(argsHash, out results))
                {
                    return results;
                }
            }

            // This maneuver is gonna cost us 51 years
            if (HashedTypeResultsPair.Count <= 0)
            {
                Refresh();
            }

            List<Type> tempResults = new List<Type>(32);
            foreach (Type[] list in GetValuesFromFlags(flags))
            {
                foreach (Type t in list)
                {
                    if (predicate(t))
                        tempResults.Add(t);
                }
            }

            results = tempResults.ToArray();
            HashedTypeResultsPair.Add(argsHash, results);
            return tempResults.ToArray();
        }
    }
}
