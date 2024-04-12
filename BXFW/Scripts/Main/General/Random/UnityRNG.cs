using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// An <see cref="IRandomGenerator"/> that uses unity's <see cref="Random"/>.
    /// </summary>
    public sealed class UnityRNG : IRandomGenerator
    {
        /// <summary>
        /// The default UnityRNG.
        /// </summary>
        public static readonly UnityRNG Default = new UnityRNG();

        public void SetSeed(int seed)
        {
            Random.InitState(seed);
        }
        public void SetSeed(long seed)
        {
            Random.InitState(unchecked((int)seed));
        }

        public float NextFloat(float minValue, float maxValue)
        {
            return Random.Range(minValue, maxValue);
        }
        public int NextInt(int minValue, int maxValue)
        {
            return Random.Range(minValue, maxValue);
        }
    }
}
