using System;

namespace BXFW
{
    /// <summary>
    /// An <see cref="IRandomGenerator"/> that uses mscorlib <see cref="Random"/>.
    /// <br/>
    /// <br>The internal random is initialized with the current <see cref="DateTime.Now"/>'s <see cref="DateTime.Ticks"/>.</br>
    /// <br>The seed can be set using any of the <see cref="SetSeed"/> methods.</br>
    /// </summary>
    public class SystemRNG : IRandomGenerator
    {
        /// <summary>
        /// The default SystemRNG.
        /// </summary>
        public static readonly SystemRNG Default = new SystemRNG();

        // this can't be readonly, see SetSeed
        private Random m_rand = new Random(unchecked((int)DateTime.Now.Ticks));
        public void SetSeed(int seed)
        {
            // Setting 'System.Random' seed is way more complicated than it needs to be
            // Creating a new Random is more viable.
            m_rand = new Random(seed);
        }
        public void SetSeed(long seed)
        {
            m_rand = new Random(unchecked((int)seed));
        }

        public float NextFloat(float minValue, float maxValue)
        {
            // Swap values as random correction
            if (minValue > maxValue)
            {
                (minValue, maxValue) = (maxValue, minValue);
            }

            // Newer versions of .NET has 'NextSingle', use 'NextDouble' for this one..
            return (float)(minValue + (m_rand.NextDouble() * (maxValue - minValue)));
        }
        public int NextInt(int minInclusive, int maxExclusive)
        {
            // max is exclusive in the default mscorlib Random too.
            return m_rand.Next(minInclusive, maxExclusive);
        }
    }
}
