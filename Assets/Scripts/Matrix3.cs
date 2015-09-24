using UnityEngine;

namespace Assets.Scripts
{
    internal class Matrix3
    {
        private float[,] data = new float[3, 3];

        public void SetColumns(Vector3 one, Vector3 two, Vector3 three)
        {
            data[0, 0] = one.x;
            data[0, 1] = two.x;
            data[0, 2] = three.x;

            data[1, 0] = one.y;
            data[1, 1] = two.y;
            data[1, 2] = three.y;

            data[2, 0] = one.z;
            data[2, 1] = two.z;
            data[2, 2] = three.z;
        }

        public void SetDiagVector(Vector3 v) {
            data[0, 0] = v.x;
            data[1, 1] = v.y;
            data[2, 2] = v.z;
        }

        public void Transpose()
        {
            Vector3 one, two, three;
            ToColumnVectors(out one, out two, out three);
            data[0, 0] = one.x;
            data[1, 0] = two.x;
            data[2, 0] = three.x;

            data[0, 1] = one.y;
            data[1, 1] = two.y;
            data[2, 1] = three.y;

            data[0, 2] = one.z;
            data[1, 2] = two.z;
            data[2, 2] = three.z;
        }

        public float this[int column, int row]
        {
            get { return data[column, row]; }
            set { data[column, row] = value; }
        }

        public float this[int n]
        {
            get { int i = n / 3; int j = n % 3; return data[i, j]; }
            set { int i = n / 3; int j = n % 3; data[i, j] = value; }
        }

        public void ToColumnVectors(out Vector3 one, out Vector3 two, out Vector3 three)
        {
            one = new Vector3(this[0, 0], this[0, 1], this[0, 2]);
            two = new Vector3(this[1, 0], this[1, 1], this[1, 2]);
            three = new Vector3(this[2, 0], this[2, 1], this[2, 2]);
        }

        public Vector3 Transform(Vector3 p)
        {
            return new Vector3(
                p.x * data[0, 0] + p.y * data[1, 0] + p.z * data[2, 0],
                p.x * data[0, 1] + p.y * data[1, 1] + p.z * data[2, 1],
                p.x * data[0, 2] + p.y * data[1, 2] + p.z * data[2, 2]);
        }

        public Vector3 TransformTranspose(Vector3 p)
        {
            return new Vector3(
                p.x * data[0, 0] + p.y * data[0, 1] + p.z * data[0, 2],
                p.x * data[1, 0] + p.y * data[1, 1] + p.z * data[1, 2],
                p.x * data[2, 0] + p.y * data[2, 1] + p.z * data[2, 2]);
        }
    }
}