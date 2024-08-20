using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// The singleton tick runner. Can be accessed globally or be used by a runner.
    /// <br>This runner is only for the Runtime.</br>
    /// </summary>
    public sealed class MainTickRunner : MonoBehaviour, ITickRunner
    {
        public int ElapsedTickCount => Time.frameCount;

        private float m_PreviousUnscaledDeltaTime = 0f;
        public float UnscaledDeltaTime => m_PreviousUnscaledDeltaTime;

        public bool SupportsFixedTick => true;
        private float m_PreviousFixedUnscaledDeltaTime = 0f;
        public float FixedUnscaledDeltaTime => m_PreviousFixedUnscaledDeltaTime;

        public float TimeScale => Time.timeScale;

        public event Action OnStart;
        public event Action<ITickRunner> OnTick;
        public event Action<ITickRunner> OnFixedTick;
        public event ITickRunner.ExitAction OnExit;

        private static MainTickRunner m_Instance;
        public static MainTickRunner Instance
        {
            get
            {
#if UNITY_EDITOR
                // Don't create an object if we aren't playing.
                if (!Application.isPlaying)
                {
                    return m_Instance;
                }
#endif
                // Create object if the reference is null.
                if (m_Instance == null)
                {
                    GameObject tickRunnerObject = new GameObject("MainTickRunner");
                    DontDestroyOnLoad(tickRunnerObject);
                    m_Instance = tickRunnerObject.AddComponent<MainTickRunner>();
                }

                return m_Instance;
            }
        }

        private void Start()
        {
            OnStart?.Invoke();
        }

        private void Update()
        {
            m_PreviousUnscaledDeltaTime = Time.unscaledDeltaTime;
            OnTick?.Invoke(this);
        }

        private void FixedUpdate()
        {
            m_PreviousFixedUnscaledDeltaTime = Time.fixedUnscaledDeltaTime;
            OnFixedTick?.Invoke(this);
        }

        private void OnApplicationQuit()
        {
            m_IsKillingWithKillIntent = true;
            OnExit?.Invoke(true);
        }

        private bool m_IsKillingWithKillIntent = false;
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (!m_IsKillingWithKillIntent)
            {
                throw new InvalidOperationException("[MainTickRunner::Kill] Cannot destroy 'MainTickRunner' on runtime. Use the 'Kill()' method instead.");
            }
        }

        public void Kill()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            Debug.LogWarning("[MainTickRunner::Kill] Called kill on this object. This may or may not be intended.");
            m_IsKillingWithKillIntent = true;
            OnExit?.Invoke(false);
            Destroy(gameObject);
        }
    }
}
