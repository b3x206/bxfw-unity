using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BXFW
{
    /// <summary>
    /// Manages pooling of <see cref="GameObject"/>'s.
    /// <br>For other, more general purpose object pools use unity's <see cref="ObjectPool{T}"/>.</br>
    /// </summary>
    public sealed class ObjectPooler : MonoBehaviour
    {
        /// <summary>
        /// A queue element used to search gameobjects faster on removal, etc.
        /// <br>This uses the <see cref="UnityEngine.Object.GetInstanceID"/> (which is exactly what the <see cref="UnityEngine.Object.GetHashCode"/> does) to search for objects in queue.</br>
        /// <br>This works fine because it's way faster to compare 300 ints as compared to 300 <see cref="UnityEngine.Object"/>s.</br>
        /// </summary>
        internal sealed class PoolQueueElement : IEquatable<PoolQueueElement>
        {
            /// <summary>
            /// Instance ID assigned to this object.
            /// </summary>
            public readonly int instanceID;
            /// <summary>
            /// The gameobject corresponding to this queue element.
            /// </summary>
            public readonly GameObject gameObject;

            /// <summary>
            /// Creates a QueueElement.
            /// </summary>
            /// <param name="obj">Object to create from. This can't be null.</param>
            /// <exception cref="ArgumentNullException"/>
            public PoolQueueElement(GameObject obj)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj), "[ObjectPooler::QueueElement::ctor] Given object is null.");
                }

                gameObject = obj;
                instanceID = obj.GetInstanceID();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as PoolQueueElement);
            }

            public override int GetHashCode()
            {
                // This is how the 'UnityEngine.Object' implements this.
                return instanceID;
            }

            public bool Equals(PoolQueueElement other)
            {
                if (other is null)
                {
                    return false;
                }

                return instanceID == other.instanceID;
            }
        }

        /// <summary>
        /// Defines a pooling prefab, to create a pooled object collection.
        /// </summary>
        [Serializable]
        public sealed class Pool
        {
            /// <summary>
            /// Tag of this pool.
            /// </summary>
            public string tag;

            [SerializeField, Tooltip("Prefab contained in a pool. This prefab will be spawned if the value isn't empty.")]
            private GameObject m_Prefab;
            /// <summary>
            /// (Read Only) Prefab item contained in this pool.
            /// <br>To change the prefab of a pool, use the 'CreateNewPool' method.</br>
            /// </summary>
            public GameObject Prefab
            {
                get => m_Prefab;
                internal set => m_Prefab = value;
            }

            [SerializeField, Clamp(0, int.MaxValue)]
            private int m_Count;
            /// <summary>
            /// Count of pooled objects.
            /// </summary>
            public int Count
            {
                get => m_Count;
                set => m_Count = Mathf.Max(0, value);
            }

            // TODO : Make this optional, for the time being all prefabs are dequeued and enqueued
            // So that the prefabs can be reused when we run out of the pooled objects. (basically a loop)
            // This is currently not viable due to the despawning depending on the 'm_poolQueue'.
            // public bool reuseAlreadyUsedPrefabs = true;

            /// <summary>
            /// Returns whether if the tag is not whitespace and that the prefab exists.
            /// </summary>
            public bool IsValid => !string.IsNullOrWhiteSpace(tag) && Prefab != null;

            /// <summary>
            /// Current list of spawned objects in the pool.
            /// <br>This is not meant to be directly manipulated.</br>
            /// </summary>
            [NonSerialized]
            internal List<PoolQueueElement> m_poolQueue = new List<PoolQueueElement>();

            /// <summary>
            /// Creates a blank pool.
            /// </summary>
            public Pool()
            { }

            /// <summary>
            /// Creates a pool with some values.
            /// </summary>
            public Pool(string tag, GameObject prefab, int count)
            {
                this.tag = tag;
                m_Prefab = prefab;
                Count = count;
            }

            public override string ToString()
            {
                return $"Tag={tag}, Prefab={(Prefab == null ? "<null>" : Prefab.ToString())}, Count={Count}";
            }
        }

        /// <summary>
        /// If this is true, the generated pooled objects will have a 'OnDestroy' debug-logger.
        /// <br>
        /// This option only works on development builds and editor,
        /// as it attaches many MonoBehaviour Components which may cause performance problems.
        /// </br>
        /// </summary>
        [Header("Debug"), Tooltip("If this is true, the generated pooled objects will have a 'OnDestroy' debug-logger.\nThis only works in editor and debug builds.")]
        public bool attachDestroyInterceptor = false;

        /// <summary>
        /// If this is true, each GameObject registered to the pool will be checked for nulls.
        /// <br>If a null exists in this case, the Pool will generate a new element with warnings printed.</br>
        /// </summary>
        [Header("Settings")]
        [Tooltip("Sets this object pooler as DontDestroyOnLoad, which makes it persistent between scenes.")]
        public bool isDontDestroyOnLoad = false;
        [Tooltip("This object pooler calls 'IPooledBehaviour's callbacks recursively (on all children GameObject attached to the pooled root object).\nThis may slow down the ObjectPooler.")]
        public bool recursivePooledBehaviourCallback = false;
        [Tooltip("Removes null objects from the spawn Queue if a null object exists. This may happen if a GameObject of the pool was destroyed.")]
        public bool clearPoolQueueIfNullExist = true;
        [InspectorLine(LineColor.Gray), SerializeField]
        private List<Pool> m_pools = new List<Pool>();

        private static ObjectPooler m_instance;
        /// <summary>
        /// Returns whether if this object pooler can be used.
        /// </summary>
        public static bool CanUsePooler => m_instance != null;

        private bool m_IsQuitting = false;

        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;

                if (isDontDestroyOnLoad)
                {
                    if (transform.parent != null)
                    {
                        transform.SetParent(null);
                    }

                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Debug.LogError($"[ObjectPooler::Awake] Duplicate object pooler. Not setting instance.", this);
                return;
            }

            Application.quitting += SetQuittingFlag;

            foreach (Pool pool in m_pools)
            {
                GeneratePoolObjects(pool);
            }
        }
        private void OnDestroy()
        {
            Application.quitting -= SetQuittingFlag;
        }
        private void SetQuittingFlag()
        {
            m_IsQuitting = true;
        }

        private void GeneratePoolObjects(Pool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool), "[ObjectPooler::GeneratePoolObjects] Given 'pool' parameter was null.");
            }
            if (!pool.IsValid)
            {
                Debug.LogWarning($"[ObjectPooler::GeneratePoolObjects] Given pool ({pool}) is not valid. A pool needs a non-whitespace-only non-null tag and a prefab assigned to be generated.", this);
                return;
            }

            // Depending on the pool to generate for, generate until the existing objectPool's size
            // Or remove items from it (basically allow resizing)
            if (pool.Count > pool.m_poolQueue.Capacity)
            {
                pool.m_poolQueue.Capacity = pool.Count;
            }

            // Add loop
            while (pool.m_poolQueue.Count < pool.Count)
            {
                GameObject instObj = Instantiate(pool.Prefab, transform);
                instObj.SetActive(false);
                instObj.name = instObj.name.Replace("(Clone)", "_Pooled");
#if UNITY_EDITOR
                if (attachDestroyInterceptor)
                {
                    // Add destroy interceptor if the option to debug those were enabled
                    instObj.AddComponent<PoolObjectDestroyInterceptor>().Initialize();
                }
#endif
                pool.m_poolQueue.Add(new PoolQueueElement(instObj));
            }
            // Removal loop
            while (pool.m_poolQueue.Count > pool.Count)
            {
                // There's no unpooling callback, that is called the 'OnDestroy'
                GameObject removeObj = pool.m_poolQueue[0].gameObject;
                pool.m_poolQueue.RemoveAt(0);
#if UNITY_EDITOR
                if (attachDestroyInterceptor)
                {
                    // Set removal intent flag to true if the object has component
                    if (removeObj.TryGetComponent(out PoolObjectDestroyInterceptor interceptor))
                    {
                        interceptor.isDestroyedWithCleanupIntent = true;
                    }
                }
#endif
                Destroy(removeObj);
            }
        }

        /// <summary>
        /// Removes all objects from the <paramref name="pool"/>.
        /// <br>This only clears the internal queue of <paramref name="pool"/>, count is not reset.</br>
        /// </summary>
        private void ClearPoolObjects(Pool pool)
        {
            // There's no unpooling callback, that is called the 'OnDestroy'
            foreach (PoolQueueElement element in pool.m_poolQueue)
            {
                GameObject removeObj = element.gameObject;
#if UNITY_EDITOR
                if (attachDestroyInterceptor)
                {
                    // Set removal intent flag to true if the object has component
                    if (removeObj.TryGetComponent(out PoolObjectDestroyInterceptor interceptor))
                    {
                        interceptor.isDestroyedWithCleanupIntent = true;
                    }
                }
#endif
                Destroy(removeObj);
            }

            pool.m_poolQueue.Clear();
        }

        /// <summary>
        /// Finds the pool with the given <paramref name="tag"/>.
        /// <br>Will return null if no pool.</br>
        /// </summary>
        private static Pool PoolWithTag(string tag)
        {
            for (int i = 0; i < m_instance.m_pools.Count; i++)
            {
                Pool pool = m_instance.m_pools[i];

                if (pool.tag == tag)
                {
                    return pool;
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the pool with the given GameObject.
        /// <br>This method will take to the account whether if the given <paramref name="gameObject"/> is equal to the pool prefabs.</br>
        /// </summary>
        private static Pool PoolWithObject(GameObject gameObject, out string tag)
        {
            tag = null;

            if (gameObject == null)
            {
                return null;
            }

            PoolQueueElement objectComparableElement = new PoolQueueElement(gameObject);
            for (int i = 0; i < m_instance.m_pools.Count; i++)
            {
                Pool pool = m_instance.m_pools[i];

                if (pool.Prefab == gameObject || pool.m_poolQueue.Contains(objectComparableElement))
                {
                    tag = pool.tag;
                    return pool;
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the pool with the given <paramref name="prefabObject"/> as <see cref="Pool.Prefab"/>.
        /// <br>This is faster as it's just a o(N) operation now.</br>
        /// </summary>
        private static Pool PoolWithPrefabObject(GameObject prefabObject, out string tag)
        {
            tag = null;

            if (prefabObject == null)
            {
                return null;
            }

            for (int i = 0; i < m_instance.m_pools.Count; i++)
            {
                Pool pool = m_instance.m_pools[i];

                if (pool.Prefab == prefabObject)
                {
                    tag = pool.tag;
                    return pool;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether if the given <paramref name="gameObject"/> is a pool object.
        /// <br>This is a <c>o(n[m_pools]*n[m_poolQueue])</c> operation due to the linear searches done. (and unity object comparison is slow)</br>
        /// </summary>
        /// <param name="gameObject">GameObject to search for. This can be the prefab or the current world instance of the pooled elements.</param>
        /// <param name="poolTag">Tag of the pool that the <paramref name="gameObject"/> was found on. This is null if no objects were found.</param>
        public static bool IsPoolObject(GameObject gameObject, out string poolTag)
        {
            return PoolWithObject(gameObject, out poolTag) != null;
        }
        /// <inheritdoc cref="IsPoolObject(GameObject, out string)"/>
        public static bool IsPoolObject(GameObject gameObject)
        {
            return IsPoolObject(gameObject, out string _);
        }

        /// <summary>
        /// Returns whether if the given <paramref name="gameObject"/> is a pool object.
        /// <br>This is a <c>o(N)</c> operation but this variation only checks the <see cref="Pool.Prefab"/> of the pools instead of the currently spawned objects.</br>
        /// </summary>
        /// <param name="prefabObject">
        /// GameObject to search for. This can only be the prefab, currently existing pool prefab instances won't work.
        /// <br>To search for the prefab instances as well use any of the <see cref="IsPoolObject(GameObject, out string)"/>.</br>
        /// </param>
        /// <param name="poolTag">Tag of the pool that the <paramref name="prefabObject"/> was found on. This is null if no objects were found.</param>
        public static bool IsPoolPrefabObject(GameObject prefabObject, out string poolTag)
        {
            return PoolWithPrefabObject(prefabObject, out poolTag) != null;
        }
        /// <inheritdoc cref="IsPoolPrefabObject(GameObject, out string)"/>
        public static bool IsPoolPrefabObject(GameObject prefabObject)
        {
            return IsPoolPrefabObject(prefabObject, out string _);
        }

        /// <summary>
        /// Does the same thing as <see cref="HasPoolWithTag(string)"/>, but on this instance of the object.
        /// </summary>
        internal bool TagExists(string tag)
        {
            for (int i = 0; i < m_pools.Count; i++)
            {
                Pool pool = m_pools[i];

                if (pool.tag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether if this object pooler has an object pool with given <paramref name="tag"/>.
        /// </summary>
        public static bool HasPoolWithTag(string tag)
        {
            return m_instance.TagExists(tag);
        }

        /// <summary>
        /// Adds a new pool to the pool list.
        /// <br>The pool may not have it's tag already existing (check with <see cref="HasPoolWithTag(string)"/>) and it's prefab should not be null.</br>
        /// </summary>
        /// <param name="pool">The pool to add. The prefab contained cannot be null and a duplicate of the tag cannot exist.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void AddPool(Pool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool), "[ObjectPooler::AddPool] Given parameter was null.");
            }
            if (!pool.IsValid)
            {
                throw new ArgumentException($"[ObjectPooler::AddPool] Given pool ({pool}) was !IsValid.", nameof(pool));
            }
            if (HasPoolWithTag(pool.tag))
            {
                throw new ArgumentException($"[ObjectPooler::AddPool] A pool with given tag ({pool.tag}) already exists. Tags of the pools have to be different.");
            }

            m_instance.m_pools.Add(pool);
            m_instance.GeneratePoolObjects(pool);
        }
        /// <summary>
        /// Removes a pool.
        /// <br>Use with caution. This will destroy every object on the <paramref name="pool"/>.</br>
        /// </summary>
        public static void RemovePool(Pool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool), "[ObjectPooler::RemovePool] Given parameter was null.");
            }

            pool.Count = 0;
            m_instance.ClearPoolObjects(pool);
            m_instance.m_pools.Remove(pool);
        }
        /// <summary>
        /// Removes a pool with given tag <paramref name="poolTag"/>.
        /// <br>Use with caution. This will destroy every object on the pool with tag <paramref name="poolTag"/>.</br>
        /// </summary>
        public static void RemovePool(string poolTag)
        {
            if (string.IsNullOrWhiteSpace(poolTag))
            {
                throw new ArgumentNullException(nameof(poolTag), "[ObjectPooler::RemovePool] Given tag is invalid or null.");
            }
            Pool targetPool = PoolWithTag(poolTag);
            if (targetPool == null)
            {
                throw new ArgumentException($"[ObjectPooler::RemovePool] Given tag '{poolTag}' has no corresponding pool.", nameof(poolTag));
            }

            RemovePool(targetPool);
        }
        /// <summary>
        /// Sets a pool's prefab to given <paramref name="targetPrefab"/>.
        /// <br>
        /// No object/prefab seperation is done during runtime,
        /// but it is preffered that the <paramref name="targetPrefab"/> never gets destroyed.
        /// </br>
        /// <br>This will cause the pool objects to be re-instantiated, so register pool prefabs from the start if possible.</br>
        /// </summary>
        /// <param name="poolTag">The pool tag to search for.</param>
        /// <param name="targetPrefab">Prefab to set the <paramref name="poolTag"/>'s corresponding pool's prefab into.</param>
        /// <param name="cloneTargetPrefab">Whether to clone the <paramref name="targetPrefab"/> as a security measure.</param>
        public static void SetPoolPrefab(string poolTag, GameObject targetPrefab, bool cloneTargetPrefab = false)
        {
            if (string.IsNullOrWhiteSpace(poolTag))
            {
                throw new ArgumentNullException(nameof(poolTag), "[ObjectPooler::SetPoolPrefab] Given 'poolTag' argument is invalid.");
            }
            if (targetPrefab == null)
            {
                throw new ArgumentNullException(nameof(targetPrefab), "[ObjectPooler::SetPoolPrefab] Given 'targetPrefab' argument is null.");
            }

            Pool targetPool = PoolWithTag(poolTag);
            if (targetPool == null)
            {
                throw new ArgumentException($"[ObjectPooler::SetPoolPrefab] Cannot find pool with tag '{poolTag}'.", nameof(poolTag));
            }

            m_instance.ClearPoolObjects(targetPool);
            targetPool.Prefab = cloneTargetPrefab ? Instantiate(targetPrefab) : targetPrefab;
            m_instance.GeneratePoolObjects(targetPool);
        }

        /// <summary>
        /// Tries setting the pool with the given <paramref name="poolTag"/>'s size.
        /// </summary>
        /// <param name="poolTag">Tag of the pool.</param>
        /// <param name="poolCount">Size to set the pool. This value will be never smaller than 0.</param>
        /// <returns>Whether if it was successful to set the pool size. The setting may fail because of the <paramref name="poolTag"/> not existing.</returns>
        public static bool TrySetPoolSize(string poolTag, int poolCount)
        {
            Pool targetPool = PoolWithTag(poolTag);
            if (targetPool == null)
            {
                return false;
            }

            targetPool.Count = poolCount;
            m_instance.GeneratePoolObjects(targetPool);

            return true;
        }
        /// <summary>
        /// Sets the pool with the tag <paramref name="poolTag"/>'s size.
        /// </summary>
        /// <param name="poolTag">Tag of the pool.</param>
        /// <param name="poolCount">Size to set the pool. This value will be never smaller than 0.</param>
        /// <exception cref="ArgumentException"/>
        public static void SetPoolSize(string poolTag, int poolCount)
        {
            if (!TrySetPoolSize(poolTag, poolCount))
            {
                throw new ArgumentException($"[ObjectPooler::SetPoolSize] Failed to set the pool size : Most likely the 'poolTag ({poolTag})' is wrong.", nameof(poolTag));
            }
        }

        /// <summary>
        /// Tries ensuring that the pool with the given <paramref name="poolTag"/> has enough size to accomodate for <paramref name="poolCount"/> size.
        /// <br>This may spawn new objects depending on whether if the <paramref name="poolCount"/> is not enough.</br>
        /// </summary>
        /// <param name="poolTag">Tag of the pool.</param>
        /// <param name="poolCount">Size to ensure. If this is zero or less than the pools size this method will return true and won't do anything.</param>
        /// <returns>Whether if it was successful to ensure the pool size. The ensuring may fail because of the <paramref name="poolTag"/> not existing.</returns>
        public static bool TryEnsurePoolCapacity(string poolTag, int poolCount)
        {
            Pool targetPool = PoolWithTag(poolTag);
            if (targetPool == null)
            {
                return false;
            }

            if (targetPool.Count >= poolCount)
            {
                return true;
            }

            // Generate pool
            targetPool.Count = poolCount;
            m_instance.GeneratePoolObjects(targetPool);

            return true;
        }
        /// <summary>
        /// Ensures that the pool with the given <paramref name="poolTag"/> has enough size to accomodate for <paramref name="poolCount"/> size.
        /// <br>This may spawn new objects depending on whether if the <paramref name="poolCount"/> is not enough.</br>
        /// </summary>
        /// <param name="poolTag">Tag of the pool.</param>
        /// <param name="poolCount">Size to ensure. If this is zero or less than the pools size this method will return true and won't do anything.</param>
        /// <exception cref="ArgumentException"/>
        public static void EnsurePoolCapacity(string poolTag, int poolCount)
        {
            if (!TryEnsurePoolCapacity(poolTag, poolCount))
            {
                throw new ArgumentException($"[ObjectPooler::EnsurePoolCapacity] Failed to ensure the pool size : Most likely the 'poolTag ({poolTag})' is wrong.", nameof(poolTag));
            }
        }

        // Spawning a PooledObject from another IPooledObject's callback causes an issue due to the shared state Instance
        // This causes more than 1 times access to the m_PooledBehavioursCache in the same loop.
        // Or you know, finally acknowledge that Singletons are a bad idea because of situations like this.

        /// <summary>
        /// Does a callback on the <see cref="IPooledBehaviour"/>s on the given <paramref name="poolBehaviourObj"/>.
        /// </summary>
        /// <param name="poolBehaviourObj">Object to get it's components and do the <see cref="IPooledBehaviour"/> callbacks on the components of it.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <remarks>
        /// Maybe do this with a simple boolean flag instead of a whole delegate?
        /// Nah, I will definitely add stuff to the <see cref="IPooledBehaviour"/>, definitely (he's not going to).
        /// </remarks>
        private static void DoCallbackOnPooledBehaviours(GameObject poolBehaviourObj, Action<IPooledBehaviour> callbackAction)
        {
            if (poolBehaviourObj == null)
            {
                throw new ArgumentNullException(nameof(poolBehaviourObj), "[ObjectPooler::DoCallbackOnPooledBehaviours] Given argument 'poolBehaviourObj' was null.");
            }
            if (callbackAction == null)
            {
                throw new ArgumentNullException(nameof(callbackAction), "[ObjectPooler::DoCallbackOnPooledBehaviours] Given argument 'callbackAction' was null.");
            }

            // apparently 'ListPool' + 'CollectionPool' is a thing, it doesn't exactly solve the problems of 'local static List<>' but it's more 'Callback'/Multithreading friendly
            // sorry freya for sharing an incorrect information and trying to teach somebody who's double.MaxValue times more knowledgable in unity and c# and everything really..
            using (PooledObject<List<IPooledBehaviour>> poolObject = ListPool<IPooledBehaviour>.Get(out List<IPooledBehaviour> pooledBehaviours))
            {
                poolBehaviourObj.GetComponents(pooledBehaviours);

                foreach (IPooledBehaviour behaviour in pooledBehaviours)
                {
                    if (behaviour == null)
                    {
                        continue;
                    }

                    callbackAction(behaviour);
                }
            }
        }

        /// <summary>
        /// Recurses the entire child tree of a transform.
        /// </summary>
        /// <param name="transform">Transform to recurse.</param>
        /// <param name="recurseAction">
        /// The delegate to call on the recursed children.
        /// The tree position is not presented and the first encountered <see cref="Transform"/>s children will be given.
        /// <br>This event is <b>never</b> called for <paramref name="transform"/>, you will have to call your own action on that transform instead..</br>
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        private static void RecurseTransformChildren(Transform transform, Action<Transform> recurseAction)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform), "[ObjectPooler::DoCallbackOnPooledBehaviours] Given argument 'transform' was null.");
            }
            if (recurseAction == null)
            {
                throw new ArgumentNullException(nameof(recurseAction), "[ObjectPooler::DoCallbackOnPooledBehaviours] Given argument 'recurseAction' was null.");
            }

            // The thing with recursion in here is that it will implicitly create 1289312 for loops nested,
            // which is nice, something i want to get and not manually manage with List<> or Span<>'s
            // -- 
            // The 'Transform' IEnumerable only enumerates the 1st layer of the children, i want to go all in and enumerate the entire tree.
            // And the integers are going to the stack with recursion instead of my 'List<>' based solution allocating in the heap, nice.
            // --
            // Or maybe i am dumb and the recursion is abstracting away something that i cannot think of because i am stupid.
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                recurseAction(child);

                if (child.childCount > 0)
                {
                    // Call the same stuff and wait for recursing of the children
                    RecurseTransformChildren(child, recurseAction);
                    // .. and that's it.
                }
            }
        }

        /// <inheritdoc cref="SpawnFromPool(string, Vector3, Quaternion, Transform)"/>
        private GameObject SpawnFromPoolInternal(string tag, Vector3 position, Quaternion rotation, Transform parent)
        {
            Pool targetPool = PoolWithTag(tag);
            if (targetPool == null)
            {
                Debug.LogError($"[ObjectPooler::SpawnFromPool] Pool with tag ({tag}) doesn't exist.", this);
                return null;
            }

            // Get + Dequeue
            PoolQueueElement objToSpawn = targetPool.m_poolQueue[0];
            // Security checks
            if (clearPoolQueueIfNullExist && objToSpawn == null)
            {
                int removedNullCount = targetPool.m_poolQueue.RemoveAll(obj => obj.gameObject == null);
                Debug.LogWarning($"[ObjectPooler::SpawnFromPool] Pool with tag ({tag}) has null enqueued objects. Cleared every null object (count:{removedNullCount}) and generating new objects.", this);

                // Regenerate objects
                m_instance.GeneratePoolObjects(targetPool);
                objToSpawn = targetPool.m_poolQueue[0];
            }

            targetPool.m_poolQueue.RemoveAt(0);

            objToSpawn.gameObject.SetActive(true);
            objToSpawn.gameObject.transform.SetParent(parent);
            objToSpawn.gameObject.transform.SetPositionAndRotation(position, rotation);

            DoCallbackOnPooledBehaviours(objToSpawn.gameObject, (b) => b.OnPoolSpawn());

            if (recursivePooledBehaviourCallback)
            {
                // Callback order is as following
                // > RootPooledObject   | 1st
                // >> ChildPooled1      | 2nd
                // >>> ChildOfChild2    | 3rd
                // >> ChildPooled2      | 4th
                // --
                // Time to kill the CPU with weird memory accesses :)
                RecurseTransformChildren(
                    objToSpawn.gameObject.transform,
                    (Transform callbackTransform) => DoCallbackOnPooledBehaviours(callbackTransform.gameObject, (b) => b.OnPoolSpawn())
                );
            }

            // Re-enqueue the object to the last spawn place
            // (Only do this if the object's pooled prefabs are reusable)
            targetPool.m_poolQueue.Add(objToSpawn);

            return objToSpawn.gameObject;
        }
        /// <summary>
        /// Spawns an object from the pool.
        /// </summary>
        public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent)
        {
            return m_instance.SpawnFromPoolInternal(tag, position, rotation, parent);
        }
        /// <inheritdoc cref="SpawnFromPool(string, Vector3, Quaternion, Transform)"/>
        public static GameObject SpawnFromPool(string tag, Transform parent)
        {
            return m_instance.SpawnFromPoolInternal(tag, Vector3.zero, Quaternion.identity, parent);
        }
        /// <inheritdoc cref="SpawnFromPool(string, Vector3, Quaternion, Transform)"/>
        public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            return m_instance.SpawnFromPoolInternal(tag, position, rotation, null);
        }
        /// <inheritdoc cref="SpawnFromPool(string, Vector3, Quaternion, Transform)"/>
        public static GameObject SpawnFromPool(string tag)
        {
            return m_instance.SpawnFromPoolInternal(tag, Vector3.zero, Quaternion.identity, null);
        }

        /// <inheritdoc cref="DespawnPoolObject(string, GameObject)"/>
        private bool InternalDespawnPoolObject(Pool targetPool, GameObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[ObjectPooler::DespawnPoolObject] Given object was null.");
            }

            // TODO : This removal sucks in terms of performance
            // Maybe store the index in the pool element / GameObject itself (hashset)?
            PoolQueueElement objectElement = new PoolQueueElement(obj);
            bool removalResult = targetPool.m_poolQueue.Remove(objectElement);

            if (!removalResult)
            {
                // GameObject does not exist as a pooled object
                Debug.LogError($"[ObjectPooler::DespawnPoolObject] Pool(tag={targetPool}) doesn't contain object named '{obj.name}'.", this);
                return false;
            }

            obj.SetActive(false);
            if (!m_IsQuitting)
            {
                obj.transform.SetParent(transform);
            }

            DoCallbackOnPooledBehaviours(obj, (b) => b.OnPoolDespawn());

            if (recursivePooledBehaviourCallback)
            {
                RecurseTransformChildren(
                    obj.transform,
                    (Transform callbackTransform) => DoCallbackOnPooledBehaviours(callbackTransform.gameObject, (b) => b.OnPoolDespawn())
                );
            }

            targetPool.m_poolQueue.Add(objectElement);
            return true;
        }
        private bool InternalDespawnPoolObject(GameObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[ObjectPooler::DespawnPoolObject] Given object was null.");
            }

            Pool targetPool = PoolWithObject(obj, out string _);
            if (targetPool == null)
            {
                return false;
            }

            return InternalDespawnPoolObject(targetPool, obj);
        }
        /// <summary>
        /// Destroys / despawns a pooled object.
        /// </summary>
        /// <param name="tag">The tag of the pool that the given <paramref name="obj"/> exists in.</param>
        /// <param name="obj">The object that exists in the pool with the given <paramref name="tag"/>.</param>
        /// <returns>Whether if the operation was successful.</returns>
        public static bool DespawnPoolObject(string tag, GameObject obj)
        {
            Pool targetPool = PoolWithTag(tag);
            if (targetPool == null)
            {
                return false;
            }

            return m_instance.InternalDespawnPoolObject(targetPool, obj);
        }

        /// <summary>
        /// Destroys / despawns a pooled object. (with timer)
        /// </summary>
        /// <param name="tag">The tag of the pool that the given <paramref name="obj"/> exists in.</param>
        /// <param name="obj">The object that exists in the pool with the given <paramref name="tag"/>.</param>
        /// <returns>Whether if the destruction timer was queued in.</returns>
        public static bool DespawnPoolObject(string tag, GameObject obj, float timer)
        {
            if (timer <= 0f)
            {
                return DespawnPoolObject(tag, obj);
            }

            // Do checks
            PoolQueueElement objectElement = new PoolQueueElement(obj);
            Pool targetPool = PoolWithTag(tag);
            if (targetPool == null)
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pooler does not contain a pool with tag ({tag}).", nameof(tag));
            }
            if (!targetPool.m_poolQueue.Contains(objectElement))
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pool ({targetPool}) does not contain object named ({obj.name}).", nameof(tag));
            }

            TaskTimer.Schedule(() => m_instance.InternalDespawnPoolObject(targetPool, obj), timer, m_instance);
            return true;
        }
        /// <summary>
        /// Destroys / despawns a pooled object.
        /// <br>Note : This method finds where the GameObject belongs to and despawns the object.</br>
        /// </summary>
        /// <param name="obj">The object that exists in the pool, somewhere.</param>
        /// <returns>Whether if the operation was successful. This may fail if the given <paramref name="obj"/> does not exist.</returns>
        public static bool DespawnPoolObject(GameObject obj)
        {
            return m_instance.InternalDespawnPoolObject(obj);
        }
        /// <summary>
        /// Destroys / despawns a pooled object. (with timer)
        /// <br>Note : This method finds where the GameObject belongs to and despawns the object.</br>
        /// </summary>
        /// <param name="obj">The object that exists in the pool, somewhere.</param>
        /// <param name="timer">Timer to wait out (in seconds) to despawn the pool object.</param>
        /// <returns>Whether if the destruction timer was queued in.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool DespawnPoolObject(GameObject obj, float timer)
        {
            if (timer <= 0f)
            {
                return m_instance.InternalDespawnPoolObject(obj);
            }

            Pool targetPool = PoolWithObject(obj, out string tag);
            if (targetPool == null)
            {
                return false;
            }

            if (targetPool == null)
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pooler does not contain a pool with tag ({tag}).", nameof(tag));
            }
            // This will cause the poolQueue to be checked twice.
            if (!targetPool.m_poolQueue.Contains(new PoolQueueElement(obj)))
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pool ({targetPool}) does not contain object named ({obj.name}).", nameof(tag));
            }

            TaskTimer.Schedule(() => m_instance.InternalDespawnPoolObject(targetPool, obj), timer, m_instance);
            return true;
        }
    }
}
