namespace BXFW
{
    /// <summary>
    /// This determines which UnityEngine update to use.
    /// <br><see cref="Update"/>      = Updates in MonoBehaviour.Update     </br>
    /// <br><see cref="LateUpdate"/>  = Updates in MonoBehaviour.LateUpdate </br>
    /// <br><see cref="FixedUpdate"/> = Updates in MonoBehaviour.FixedUpdate</br>
    /// <br/>
    /// <br>This setting enumeration on scripts depends on how your target is moving or what purpose the behaviour will serve.</br>
    /// <br>For example, if you are following a transform with <see cref="UnityEngine.Rigidbody"/> class on it, use <see cref="FixedUpdate"/> mode.</br>
    /// <br>If you are following a transform that is being tweened or just moves with the MonoBehaviour.Update() method,
    /// you can use <see cref="Update"/> or <see cref="LateUpdate"/> mode.</br>
    /// <br>Basically at it's core, this depends on which method the target is being updates.
    /// This value matching with the target's update method will minimize jittery movement + following.</br>
    /// </summary>
    public enum BehaviourUpdateMode
    {
        Update,
        LateUpdate,
        FixedUpdate,
    }
}
