using UnityEngine;

namespace Assets.Scripts
{
    internal class Matrix3
    {
        public float[] data = new float[9];

        public Matrix3()
        {
            data[0] = data[1] = data[2] = data[3] = data[4] = data[5] = data[6] = data[7] = data[8] = 0;
        }

        public Matrix3(Vector3 a, Vector3 b, Vector3 c)
        {
            SetColumns(a, b, c);
        }

        public Matrix3(float c0, float c1, float c2, float c3, float c4, float c5, float c6, float c7, float c8)
        {
            data[0] = c0; data[1] = c1; data[2] = c2;
            data[3] = c3; data[4] = c4; data[5] = c5;
            data[6] = c6; data[7] = c7; data[8] = c8;
        }

        public Matrix3(Matrix3 m)
        {
            data[0] = m[0]; data[1] = m[1]; data[2] = m[2];
            data[3] = m[3]; data[4] = m[4]; data[5] = m[5];
            data[6] = m[6]; data[7] = m[7]; data[8] = m[8];
        }

        public float this[int column, int row]
        {
            get { return data[row * 3 + column]; }
            set { data[row * 3 + column] = value; }
        }

        public float this[int n]
        {
            get { return data[n]; }
            set { data[n] = value; }
        }

        public static Vector3 operator *(Matrix3 m, Vector3 v)
        {
            return new Vector3(
                v.x * m[0] + v.y * m[1] + v.z * m[2],
                v.x * m[3] + v.y * m[4] + v.z * m[5],
                v.x * m[6] + v.y * m[7] + v.z * m[8]);
        }

        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            return new Matrix3(
                a.data[0] * b.data[0] + a.data[1] * b.data[3] + a.data[2] * b.data[6],
                a.data[0] * b.data[1] + a.data[1] * b.data[4] + a.data[2] * b.data[7],
                a.data[0] * b.data[2] + a.data[1] * b.data[5] + a.data[2] * b.data[8],

                a.data[3] * b.data[0] + a.data[4] * b.data[3] + a.data[5] * b.data[6],
                a.data[3] * b.data[1] + a.data[4] * b.data[4] + a.data[5] * b.data[7],
                a.data[3] * b.data[2] + a.data[4] * b.data[5] + a.data[5] * b.data[8],

                a.data[6] * b.data[0] + a.data[7] * b.data[3] + a.data[8] * b.data[6],
                a.data[6] * b.data[1] + a.data[7] * b.data[4] + a.data[8] * b.data[7],
                a.data[6] * b.data[2] + a.data[7] * b.data[5] + a.data[8] * b.data[8]
                );
        }

        public static Matrix3 operator *(Matrix3 m, int s)
        {
            Matrix3 res = new Matrix3();
            for (int i = 0; i < m.data.Length; i++)
                res[i] = m[i] * s;
            return res;
        }

        public static Matrix3 operator +(Matrix3 l, Matrix3 r)
        {
            Matrix3 res = new Matrix3();
            for (int i = 0; i < l.data.Length; i++)
                res[i] = l[i] + r[i];
            return res;
        }

        public Matrix3 Inverse()
        {
            var res = new Matrix3();
            res.SetInverse(this);
            return res;
        }

        public void Invert()
        {
            SetInverse(this);
        }

        public void SetColumns(Vector3 one, Vector3 two, Vector3 three)
        {
            data[0] = one.x;
            data[1] = two.x;
            data[2] = three.x;

            data[3] = one.y;
            data[4] = two.y;
            data[5] = three.y;

            data[6] = one.z;
            data[7] = two.z;
            data[8] = three.z;
        }

        public void SetDiagonal(float a, float b, float c)
        {
            data[0] = a;
            data[4] = b;
            data[8] = c;
        }

        public void SetDiagonal(Vector3 v)
        {
            SetDiagonal(v.x, v.y, v.z);
        }

        public void SetInverse(Matrix3 m)
        {
            float t4 = m[0] * m[4];
            float t6 = m[0] * m[5];
            float t8 = m[1] * m[3];
            float t10 = m[2] * m[3];
            float t12 = m[1] * m[6];
            float t14 = m[2] * m[6];

            // Calculate the determinant
            float t16 = (t4 * m[8] - t6 * m[7] - t8 * m[8] +
                        t10 * m[7] + t12 * m[5] - t14 * m[4]);

            // Make sure the determinant is non-zero.
            if (t16 == 0.0f) return;
            float t17 = 1f / t16;

            data[0] = (m[4] * m[8] - m[5] * m[7]) * t17;
            data[1] = -(m[1] * m[8] - m[2] * m[7]) * t17;
            data[2] = (m[1] * m[5] - m[2] * m[4]) * t17;
            data[3] = -(m[3] * m[8] - m[5] * m[6]) * t17;
            data[4] = (m[0] * m[8] - t14) * t17;
            data[5] = -(t6 - t10) * t17;
            data[6] = (m[3] * m[7] - m[4] * m[6]) * t17;
            data[7] = -(m[0] * m[7] - t12) * t17;
            data[8] = (t4 - t8) * t17;
        }

        public void SetSkewSymmetric(Vector3 v)
        {
            data[0] = data[4] = data[8] = 0;
            data[1] = -v.z;
            data[2] = v.y;
            data[3] = v.z;
            data[5] = -v.x;
            data[6] = -v.y;
            data[7] = v.x;
        }

        public void SetTransposed(Matrix3 m)
        {
            data[0] = m[0];
            data[1] = m[3];
            data[2] = m[6];
            data[3] = m[1];
            data[4] = m[4];
            data[5] = m[7];
            data[6] = m[2];
            data[7] = m[5];
            data[8] = m[8];
        }

        public Vector3 Transform(Vector3 v)
        {
            return this * v;
        }

        public Vector3 TransformTranspose(Vector3 v)
        {
            return new Vector3(
                v.x * data[0] + v.y * data[3] + v.z * data[6],
                v.x * data[1] + v.y * data[4] + v.z * data[7],
                v.x * data[2] + v.y * data[5] + v.z * data[8]
            );
        }

        public void Transpose()
        {
            SetTransposed(this);
        }

        public Matrix3 Transposed()
        {
            Matrix3 res = new Matrix3();
            res.SetTransposed(this);
            return res;
        }
    }
}