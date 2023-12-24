
namespace BXFW
{
    /// <summary>
    /// The targeted ticking type.
    /// <br><see cref="Variable"/> : The tweening will be called on a method like 'Update', where it is called every frame.</br>
    /// <br><see cref="Fixed"/> : The tweening will be called on a method like 'FixedUpdate', where it is called every n times per second.</br>
    /// <br/>
    /// <br>Unlike <see cref="BehaviourUpdateMode"/>, this aims to be a generic/engine agnostic tick type.</br>
    /// </summary>
    public enum TickType
    {
        Variable, Fixed
    }
}
