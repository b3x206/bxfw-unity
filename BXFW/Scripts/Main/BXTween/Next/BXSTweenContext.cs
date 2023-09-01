namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a typed context.
    /// <br>The actual setters are contained here.</br>
    /// </summary>
    public class BXSTweenContext<TValue> : BXSTweenable
    {
        /// <summary>
        /// The current gathered starting value.
        /// </summary>
        public TValue StartValue { get; private set; }
        /// <summary>
        /// The current gathered ending value.
        /// </summary>
        public TValue EndValue { get; private set; }

        protected override void OnSwitchTargetValues()
        {
            throw new System.NotImplementedException("i took too long");
        }
    }
}
