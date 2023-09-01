using UnityEngine.Events;

namespace BXFW.Tweening.Next.Events
{
    /// <summary>
    /// A way of setting the event value if plausible.
    /// </summary>
    public enum ActionSetMode
    {
        Add, Equals, Subtract
    }

    /// <summary>
    /// A blank void action.
    /// </summary>
    public delegate void BXSAction();
    
    /// <summary>
    /// A boolean action that returns a condition.
    /// </summary>
    public delegate bool BXSConditionAction();
    /// <summary>
    /// A tick condition return action.
    /// </summary>
    public delegate TickConditionSuspendType BXSTickConditionAction();
    /// <summary>
    /// A predicate action.
    /// </summary>
    public delegate bool BXSPredicateAction<in T>(T value);

    /// <summary>
    /// A action called when the <see cref="IBXSTweenRunner{TDispatchObject}"/> is about to quit.
    /// </summary>
    /// <param name="applicationQuit">Whether if the tween runner was closed because the application was being closed.</param>
    public delegate void BXSExitAction(bool applicationQuit);

    /// <summary>
    /// A action called to get a value for a tween.
    /// </summary>
    public delegate T BXSGetterAction<out T>();

    /// <summary>
    /// A void action that takes a generic parameter with value.
    /// <br>Can be used for a setter action or a ticker method with float only.</br>
    /// </summary>
    public delegate void BXSAction<in T>(T value);

    /// <summary>
    /// A ease action.
    /// <br>Expected to return from-to 0-1, but can overshoot.</br>
    /// </summary>
    /// <param name="time">Time for this tween. This parameter is linearly interpolated.</param>
    /// <returns>The eased value.</returns>
    public delegate float BXSEaseAction(float time);

    /// <summary>
    /// A unity event that takes a <see cref="BXSTweenable"/>.
    /// </summary>
    public class BXSTweenUnityAction : UnityEvent<BXSTweenable> { }
}
