using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BXFW.Tweening
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

        // Tweenables already has a reference to it's ID object.
        public int GetIDFromObject<TDispatchObject>(TDispatchObject idObject)
            where TDispatchObject : class
        {
            Object idUnityObject = (idObject as Object);
            return idUnityObject != null ? idUnityObject.GetInstanceID() : BXSTween.NoID;
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
