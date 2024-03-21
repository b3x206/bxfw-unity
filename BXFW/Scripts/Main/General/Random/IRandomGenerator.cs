namespace BXFW
{
    /// <summary>
    /// An interface that random generators implement.
    /// <br>Pre-implemented versions exist for unity <see cref="UnityEngine.Random"/> (<see cref="UnityRNG"/>) and mscorlib <see cref="System.Random"/> ().</br>
    /// </summary>
    public interface IRandomGenerator
    {
        /// <summary>
        /// A method to use for <paramref name="seed"/> initialization.
        /// </summary>
        public void SetSeed(int seed);
        /// <summary>
        /// A method to use for <paramref name="seed"/> initialization.
        /// </summary>
        public void SetSeed(long seed);

        /// <summary>
        /// Get the next int between <paramref name="minInclusive"/> and <paramref name="maxExclusive"/>.
        /// </summary>
        /// <param name="minInclusive">Minimum value. This value is included.</param>
        /// <param name="maxExclusive">Maximum value. This value is excluded.</param>
        public int NextInt(int minInclusive, int maxExclusive);
        /// <summary>
        /// Get the next int. Lower value is 0.
        /// </summary>
        public int NextInt(int maxExclusive)
        {
            return NextInt(0, maxExclusive);
        }
        /// <summary>
        /// Get the next int between <paramref name="minValue"/> and <paramref name="maxValue"/>. (both values included)
        /// </summary>
        public float NextFloat(float minValue, float maxValue);
        /// <summary>
        /// Get the next float value. Lower value is 0.
        /// </summary>
        public float NextFloat(float maxValue)
        {
            return NextFloat(0f, maxValue);
        }
    }
}
