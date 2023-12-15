using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// The primary runner for the unity game engine.
    /// <br>Initializes the <see cref="BXSTween"/> by a <see cref="RuntimeInitializeOnLoadMethodAttribute"/> method.</br>
    /// </summary>
    public class BXSTweenUnityRunner : MonoBehaviour, IBXSTweenRunner
    {
        // Depending on the current frame, provide the delta time depending on the current frame and cache it (use 'lastFrameDeltaTime')
        // This is because there's a 1ms access penalty for delta time on higher iteration counts (for some reason)
        public int ElapsedTickCount => Time.frameCount;
        private float m_PreviousUnscaledDeltaTime = 0f;
        public float UnscaledDeltaTime => m_PreviousUnscaledDeltaTime;

        // This also has performance penalty, but this one is okay as a tween MAY change Time.timeScale
        // (bad practice, you shouldn't do it but it's possible)
        public float TimeScale => Time.timeScale;

        public bool SupportsFixedTick => true;
        private float m_PreviousUnscaledFixedDeltaTime = 0f;
        public float FixedUnscaledDeltaTime => m_PreviousUnscaledFixedDeltaTime;

        public event Action OnStart;
        public event Action<ITickRunner> OnTick;
        public event Action<ITickRunner> OnFixedTick;
        public event ITickRunner.ExitAction OnExit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationLoad()
        {
            if (BXSTween.MainRunner == null)
            {
                // Initialize the static things
                BXSTween.Initialize(() =>
                {
                    // Spawn object
                    BXSTweenUnityRunner runner = new GameObject("BXSTween").AddComponent<BXSTweenUnityRunner>();
                    runner.OnStart?.Invoke();

                    // Add it to 'DontDestroyOnLoad'
                    DontDestroyOnLoad(runner.gameObject);

                    // Return it as this is the getter if the main IBXSTweenRunner is null
                    return runner;
                }, new Logger(Debug.Log, Debug.LogWarning, Debug.LogError, Debug.LogException));
            }
        }

        /// <summary>
        /// A registry for a UnityEngine object.
        /// </summary>
        private class ObjectRegistry : IComparable<ObjectRegistry>
        {
            public Object unityObject;

            public ObjectRegistry() { }
            public ObjectRegistry(Object assignObject)
            {
                unityObject = assignObject;
            }

            public int CompareTo(ObjectRegistry other)
            {
                if (other == null || other.unityObject == null)
                {
                    return 1;
                }

                if (unityObject == null)
                {
                    return other.unityObject == null ? 0 : 1;
                }

                return unityObject.GetInstanceID().CompareTo(other.unityObject.GetInstanceID());
            }
        }
        /// <summary>
        /// List of the registered objects.
        /// </summary>
        private readonly SortedList<ObjectRegistry> m_idObjectRegistries = new SortedList<ObjectRegistry>();

        public TDispatchObject GetObjectFromID<TDispatchObject>(int id)
            where TDispatchObject : class
        {
            if (id == BXSTween.NoID)
            {
                return null;
            }

            int index = m_idObjectRegistries.FindIndex(reg => GetIDFromObject(reg.unityObject) == id);
            if (index < 0)
            {
                return null;
            }

            return m_idObjectRegistries[index].unityObject as TDispatchObject;
        }

        public int GetIDFromObject<TDispatchObject>(TDispatchObject idObject)
            where TDispatchObject : class
        {
            Object idUnityObject = (idObject as Object);
            if (idUnityObject != null)
            {
                if (m_idObjectRegistries.All(reg => reg.unityObject != idUnityObject))
                {
                    m_idObjectRegistries.Add(new ObjectRegistry(idUnityObject));
                }

                return idUnityObject.GetInstanceID();
            }

            return BXSTween.NoID;
        }

        public void Kill()
        {
            OnExit?.Invoke(false);
            Destroy(gameObject);
        }

        private void Update()
        {
            m_PreviousUnscaledDeltaTime = Time.unscaledDeltaTime;
            OnTick?.Invoke(this);
        }
        private void FixedUpdate()
        {
            m_PreviousUnscaledFixedDeltaTime = Time.fixedUnscaledDeltaTime;
            OnFixedTick?.Invoke(this);
        }

        private void OnApplicationQuit()
        {
            OnExit?.Invoke(true);
        }
    }
}
