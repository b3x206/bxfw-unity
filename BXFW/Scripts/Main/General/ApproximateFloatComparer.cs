using System.Collections.Generic;

namespace BXFW
{
    /// <summary>
    /// A comparer that checks if floating point numbers are approximately equal.
    /// <br>Note : The <see cref="GetHashCode(double)"/> functions returns the actual hash code of the number directly.</br>
    /// <br/>
    /// <br>Because of this, it is kinda pointless unless you use the '<see cref="Equals(double, double)"/>' methods (which <see cref="Dictionary{TKey, TValue}"/> does not).</br>
    /// </summary>
    public sealed class ApproximateFloatComparer : IEqualityComparer<float>, IEqualityComparer<double>
    {
        /// <summary>
        /// The default comparer that can be used for an <see cref="IEqualityComparer{T}"/> of types <see cref="float"/> or <see cref="double"/>.
        /// </summary>
        public static readonly ApproximateFloatComparer Default = new ApproximateFloatComparer();

        public bool Equals(float x, float y)
        {
            return MathUtility.Approximately(x, y);
        }

        public bool Equals(double x, double y)
        {
            return MathUtility.Approximately(x, y);
        }

        public int GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}
