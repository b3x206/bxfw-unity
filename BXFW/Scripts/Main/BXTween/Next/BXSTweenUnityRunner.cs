using System;
using System.Linq;
using UnityEngine;
using BXFW.Tweening.Next.Events;
using Object = UnityEngine.Object;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// The primary runner for the unity game engine.
    /// <br>Initializes the <see cref="BXSTween"/> by a <see cref="RuntimeInitializeOnLoadMethodAttribute"/> method.</br>
    /// </summary>
    public class BXSTweenUnityRunner : MonoBehaviour, IBXSTweenRunner
    {
        public int ElapsedTickCount => Time.frameCount;
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public float TimeScale => Time.timeScale;

        public bool SupportsFixedTick => true;
        public float FixedUnscaledDeltaTime => Time.fixedUnscaledDeltaTime;

        public event BXSAction OnRunnerStart;
        public event BXSSetterAction<IBXSTweenRunner> OnRunnerTick;
        public event BXSSetterAction<IBXSTweenRunner> OnRunnerFixedTick;
        public event BXSExitAction OnRunnerExit;

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
                    runner.OnRunnerStart?.Invoke();

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
                    return 1;
                if (unityObject == null)
                    return other.unityObject == null ? 0 : 1;

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
                return null;

            int index = m_idObjectRegistries.FindIndex(reg => GetIDFromObject(reg.unityObject) == id);
            if (index < 0)
                return null;

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
            OnRunnerExit?.Invoke(false);
            Destroy(gameObject);
        }

        private void Update()
        {
            OnRunnerTick?.Invoke(this);
        }
        private void FixedUpdate()
        {
            OnRunnerFixedTick?.Invoke(this);
        }

        private void OnApplicationQuit()
        {
            OnRunnerExit?.Invoke(true);
        }
    }
}
