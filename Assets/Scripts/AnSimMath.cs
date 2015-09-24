using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Class containing some helper functions for mathematics.
    /// </summary>
    public class AnSimMath
    {
        /// <summary>
        /// Normalizes a given Quaternion. For small lengths approximates the normalization trough Padé approximant, see http://www.scholarpedia.org/article/Pad%C3%A9_approximant.
        /// </summary>
        /// <param name="q">Quaternion to normalize</param>
        /// <returns></returns>
        public static Quaternion NormalizeQuaternion(Quaternion q)
        {
            float qmagsq = QuatLengthSq(q);
            if (Mathf.Abs(1.0f - qmagsq) < 2.107342e-08)
            {
                q = QuatScale(q, 2.0f / (1.0f + qmagsq));
            }
            else
            {
                q = QuatScale(q, 1.0f / Mathf.Sqrt(qmagsq));
            }
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
    }
}