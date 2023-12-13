using System;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Manages pooling of GameObject's.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        // TODO : Editor for the pool reorderable array for adding unique tags only.

        /// <summary>
        /// Defines a pooling prefab, to create a pooled object collection.
        /// </summary>
        [Serializable]
        public class Pool
        {
            /// <summary>
            /// Tag of this pool.
            /// </summary>
            public string tag;

            [SerializeField]
            private GameObject m_Prefab;
            /// <summary>
            /// (Read Only) Prefab item contained in this pool.
            /// <br>To change the prefab of a pool, use the 'CreateNewPool' method.</br>
            /// </summary>
            public GameObject Prefab => m_Prefab;

            [SerializeField, Range(0, int.MaxValue)]
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
            internal List<GameObject> m_poolQueue = new List<GameObject>();

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
        /// <br>This option only works on development builds and editor.</br>
        /// </summary>
        public bool debugAttachDestroyInterceptor = false;
        /// <summary>
        /// If this is true, each GameObject registered to the pool will be checked for nulls.
        /// <br>If a null exists in this case, the Pool will generate a new element with warnings printed.</br>
        /// </summary>
        public bool clearPoolQueueIfNullExist = true;
        [InspectorLine(LineColor.Gray), SerializeField]
        private List<Pool> m_pools = new List<Pool>();

        private static ObjectPooler m_instance;

        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                Debug.LogError($"[ObjectPooler::Awake] Duplicate object pooler. Not setting instance.", this);
            }

            foreach (Pool pool in m_pools)
            {
                GeneratePoolObjects(pool);
            }
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

                pool.m_poolQueue.Add(instObj);
            }
            // Removal loop
            while (pool.m_poolQueue.Count > pool.Count)
            {
                // There's no unpooling callback, that is called the 'OnDestroy'
                GameObject removeObj = pool.m_poolQueue[0];
                pool.m_poolQueue.RemoveAt(0);

                Destroy(removeObj);
            }
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

            for (int i = 0; i < m_instance.m_pools.Count; i++)
            {
                Pool pool = m_instance.m_pools[i];

                if (pool.Prefab == gameObject || pool.m_poolQueue.Contains(gameObject))
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
        /// Returns whether if this object pooler has an object pool with given <paramref name="tag"/>.
        /// </summary>
        public static bool HasPoolWithTag(string tag)
        {
            return PoolWithTag(tag) != null;
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

        /// <summary>
        /// List of the pooled behaviours gathered from the <see cref="GameObject.GetComponents{T}(List{T})"/>.
        /// <br>This is used to call every <see cref="IPooledBehaviour"/> on the given pooled root object.</br>
        /// </summary>
        private static readonly List<IPooledBehaviour> m_PooledBehavioursCache = new(16);
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
            GameObject objToSpawn = targetPool.m_poolQueue[0];
            // Security checks
            if (clearPoolQueueIfNullExist && objToSpawn == null)
            {
                int removedNullCount = targetPool.m_poolQueue.RemoveAll(obj => obj == null);
                Debug.LogWarning($"[ObjectPooler::SpawnFromPool] Pool with tag ({tag}) has null enqueued objects. Cleared every null object (count:{removedNullCount}) and generating new objects.", this);

                // Regenerate objects
                m_instance.GeneratePoolObjects(targetPool);
                objToSpawn = targetPool.m_poolQueue[0];
            }

            targetPool.m_poolQueue.RemoveAt(0);

            objToSpawn.SetActive(true);
            objToSpawn.transform.SetParent(parent);
            objToSpawn.transform.SetPositionAndRotation(position, rotation);
            objToSpawn.GetComponents(m_PooledBehavioursCache);

            foreach (IPooledBehaviour behaviour in m_PooledBehavioursCache)
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.OnSpawn();
            }

            // Re-enqueue the object to the last spawn place
            // (Only do this if the object's pooled prefabs are reusable)
            targetPool.m_poolQueue.Add(objToSpawn);

            return objToSpawn;
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
            bool removalResult = targetPool.m_poolQueue.Remove(obj);

            if (!removalResult)
            {
                // GameObject does not exist as a pooled object
                Debug.LogError(string.Format("[ObjectPooler::DespawnPoolObject] Pool(tag={0}) doesn't contain object named '{1}'.", targetPool, obj.name));
                return false;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            obj.GetComponents(m_PooledBehavioursCache);

            foreach (IPooledBehaviour behaviour in m_PooledBehavioursCache)
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.OnDespawn();
            }

            targetPool.m_poolQueue.Add(obj);
            return true;
        }
        private bool InternalDespawnPoolObject(GameObject obj)
        {
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
            Pool targetPool = PoolWithTag(tag);
            if (targetPool == null)
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pooler does not contain a pool with tag ({tag}).", nameof(tag));
            }
            if (!targetPool.m_poolQueue.Contains(obj))
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
        /// <returns>Whether if the operation was successful.</returns>
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
            if (!targetPool.m_poolQueue.Contains(obj))
            {
                throw new ArgumentException($"[ObjectPooler::DespawnPoolObject] Pool ({targetPool}) does not contain object named ({obj.name}).", nameof(tag));
            }

            TaskTimer.Schedule(() => m_instance.InternalDespawnPoolObject(targetPool, obj), timer, m_instance);
            return true;
        }
    }
}
