using UnityEngine;
using UnityEngine.Events;

namespace BXFW
{
    /// <summary>
    /// A class that exposes the <see cref="IPooledBehaviour"/> events to the unity inspector.
    /// <br/>
    /// <br>As always, if you don't enable the <see cref="ObjectPooler.recursivePooledBehaviourCallback"/>,
    /// this component will also not work at children <see cref="GameObject"/>s but <b>only on the ROOT</b> <see cref="GameObject"/>.</br>
    /// <br/>
    /// <br>But it can be used to expose events to desired children without having to enable <see cref="ObjectPooler.recursivePooledBehaviourCallback"/>.</br>
    /// </summary>
    public sealed class PooledObjectEventReceiver : MonoBehaviour, IPooledBehaviour
    {
        [InspectorLine(LineColor.Gray)]
        [Tooltip("Called when 'IPooledBehaviour.OnPoolSpawn', or the object spawn from the ObjectPooler.")]
        public UnityEvent onPoolSpawn;
        [Tooltip("Called when 'IPooledBehaviour.OnPoolDespawn', or the object despawn from the ObjectPooler.")]
        public UnityEvent onPoolDespawn;

        public void OnPoolSpawn()
        {
            onPoolSpawn?.Invoke();
        }
        public void OnPoolDespawn()
        {
            onPoolDespawn?.Invoke();
        }
    }
}
