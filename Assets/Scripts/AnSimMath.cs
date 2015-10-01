using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Class containing some helper functions for mathematics.
    /// </summary>
    public class AnSimMath
    {
        /// <summary>
        /// Multiplies two verctor scalar by calar
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 VecMultiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// The glorious fast inverse sqrt function by the big master john carmack, see: https://en.wikipedia.org/wiki/Fast_inverse_square_root
        /// Unsafe, because pointer stuff, it's ok like that.
        /// </summary>
        /// <param name="number">Number to be rooted </param>
        /// <returns></returns>
        public static unsafe float Fast_Inv_Sqrt(float number)
        {
            long i;
            float x2, y;
            const float threehalfs = 1.5F;

            x2 = number * 0.5F;
            y = number;
            i = *(long*)&y;                             // evil floating point bit level hacking
            i = 0x5f3759df - (i >> 1);                  // what the fuck?
            y = *(float*)&i;
            y = y * (threehalfs - (x2 * y * y));        // 1st iteration
            y = y * (threehalfs - (x2 * y * y));   // 2nd iteration, this can be removed
            return y;
        }

        /// <summary>
        /// Normalizes a given Quaternion.
        /// </summary>
        /// <param name="q">Quaternion to normalize</param>
        /// <returns></returns>
        public static Quaternion NormalizeQuaternion(Quaternion q)
        {
            float qmagsq = Fast_Inv_Sqrt(QuatLengthSq(q));
            if (qmagsq <= Quaternion.kEpsilon) return Quaternion.identity;
            q.x *= qmagsq;
            q.y *= qmagsq;
            q.z *= qmagsq;
            q.w *= qmagsq;
            return q;
        }

        /// <summary>
        /// Gets the squared magnitude/length of a quaternion.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static float QuatLengthSq(Quaternion q)
        {
            return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
        }

        /// <summary>
        /// Multiplies a quaternion with a scalar.
        /// </summary>
        /// <param name="q">quaternion to multiply</param>
        /// <param name="n">scalar to multiply with</param>
        /// <returns></returns>
        public static Quaternion QuatScale(Quaternion q, float n)
        {
            q.Set(q.x * n, q.y * n, q.z * n, q.w * n);
            return q;
        }

        public static Quaternion QuatRotateByVector(Quaternion q, Vector3 v)
        {
            return q * new Quaternion(v.x, v.y, v.z, 0);
        }

        public static Quaternion QuatAddScaledVector(Quaternion q, Vector3 v, float s)
        {
            Quaternion qv = new Quaternion(v.x * s, v.y * s, v.z * s, 0);
            return new Quaternion(qv.x * 0.5f, qv.y * 0.5f, qv.z * 0.5f, qv.w * 0.5f);
        }

        public static Quaternion QuatAddQuat(Quaternion a, Quaternion b)
        {
            Quaternion res = Quaternion.identity;
            for (int i = 0; i < 4; i++) res[i] = a[i] + b[i];
            return res;
        }
    }
}