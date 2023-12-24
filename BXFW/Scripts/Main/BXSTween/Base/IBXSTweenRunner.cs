namespace BXFW.Tweening
{
    /// <summary>
    /// Contains a tweening runner.
    /// <br>Includes the necessary update hooks, timing variables, etc.</br>
    /// <br>If a runner no longer exists, it will be recreated when needed by <see cref="BXSTween"/>.</br>
    /// </summary>
    public interface IBXSTweenRunner : ITickRunner
    {
        /// <summary>
        /// Returns a tweening id from the given object.
        /// <br>Implement this according to your id system, or always return <see cref="BXSTween.NoID"/> if no id.</br>
        /// </summary>
        public int GetIDFromObject<TDispatchObject>(TDispatchObject idObject) where TDispatchObject : class;
    }
}
