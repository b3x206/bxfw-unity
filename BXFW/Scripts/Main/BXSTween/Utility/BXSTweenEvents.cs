namespace BXFW.Tweening.Events
{
    /// <summary>
    /// A blank void action.
    /// </summary>
    public delegate void BXSAction();
    /// <summary>
    /// A void action that takes a generic parameter with value.
    /// </summary>
    public delegate void BXSAction<in T>(T value);

    /// <summary>
    /// A boolean action that returns a condition.
    /// </summary>
    public delegate bool BXSConditionAction();
    /// <summary>
    /// A tick condition return action.
    /// </summary>
    public delegate TickSuspendType BXSTickConditionAction();
    /// <summary>
    /// A predicate action.
    /// </summary>
    public delegate bool BXSPredicateAction<in T>(T value);

    /// <summary>
    /// A action called when the <see cref="IBXSTweenRunner"/> is about to quit.
    /// </summary>
    /// <param name="cleanup">Whether if the tween runner was closed for good.</param>
    public delegate void BXSExitAction(bool cleanup);

    /// <summary>
    /// A action called to get a value out.
    /// </summary>
    public delegate T BXSGetterAction<out T>();
    /// <summary>
    /// A action called to set a value on given delegate.
    /// </summary>
    public delegate void BXSSetterAction<in T>(T value);
    /// <summary>
    /// A linear interpolation method. The <paramref name="time"/> parameter is the value to ease.
    /// </summary>
    public delegate T BXSLerpAction<T>(T a, T b, float time);
    /// <summary>
    /// A math action used for adding/subtracting/dividing/multiplying.
    /// </summary>
    public delegate T BXSMathAction<T>(T lhs, T rhs);

    /// <summary>
    /// A ease action.
    /// <br>Expected to return from-to 0-1, but can overshoot.</br>
    /// </summary>
    /// <param name="time">Time for this tween. This parameter is linearly interpolated.</param>
    /// <returns>The eased value.</returns>
    public delegate float BXSEaseAction(float time);
}
