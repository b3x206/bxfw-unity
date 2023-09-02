#if UNITY_2018_3_OR_NEWER
using BXFW.Tweening.Next.Events;
using UnityEngine;

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

        public int FixedTickRate => (int)(Time.fixedDeltaTime * 1000f);

        public event BXSAction OnRunnerStart;
        public event BXSSetterAction<IBXSTweenRunner> OnRunnerTick;
        public event BXSSetterAction<IBXSTweenRunner> OnRunnerFixedTick;
        public event BXSExitAction OnRunnerExit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationLoad()
        {
            // TODO : Spawn GameObject
            Debug.Log("todo");
        }

        private void Awake()
        {
            BXSTween.Initialize(this);
            OnRunnerStart?.Invoke();
        }

        public TDispatchObject GetIDObject<TDispatchObject>(int id) where TDispatchObject : class
        {
            throw new System.NotImplementedException();
        }

        public int GetObjectID<TDispatchObject>(TDispatchObject idObject) where TDispatchObject : class
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            OnRunnerTick?.Invoke(this);
        }
        private void FixedUpdate()
        {
            OnRunnerTick?.Invoke(this);
        }

        private void OnApplicationQuit()
        {
            OnRunnerExit?.Invoke(true);
        }

        public void Kill()
        {
            OnRunnerExit?.Invoke(false);
            Destroy(gameObject);
        }
    }
}
#endif
