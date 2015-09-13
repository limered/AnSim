using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Calculates collisions for each object. Siehe "gaffer on games" und "Ultrapede"
    /// </summary>
    internal class CollisionSystem
    {
        private float _epsilon = 0.001f;
        private float _parallelCutof;
        private CollisionInfo coll = new CollisionInfo();

        internal struct CollisionInfo
        {
            public OrientedBox3D A;
            public OrientedBox3D B;

            public Vector3 relativePos;
            public Vector3 relativePosRot;
            public Vector3 relativeVel;

            public float[,] R, AbsR;
            public float ra, rb;

            public float r_min;
            public Vector3 n_min;
            public Vector3 normal_c;
            public bool invert_normal;
            public int code;
            public int tested_axis_0;
            public int tested_axis_1;

            public List<Vector3> contactPointsA;
            public List<float> contactDepthsA;
            public List<Vector3> contactPointsB;
            public List<float> contactDepthsB;

            public Vector3 n;
            public float t;

            public void SetValues(OrientedBox3D a, OrientedBox3D b, float dt)
            {
                if (R == null)  //init R
                {
                    R = new float[3, 3];
                    AbsR = new float[3, 3];
                }

                A = a;
                B = b;

                relativePos = B.center - A.center;
                relativeVel = B.velocity * dt - A.velocity * dt;

                n.Set(0, 0, 0);
                t = 1.0f;

                if (contactPointsA == null)
                {
                    contactPointsA = new List<Vector3>();
                    contactDepthsA = new List<float>();
                    contactPointsB = new List<Vector3>();
                    contactDepthsB = new List<float>();
                }
                else
                {
                    contactPointsA.Clear();
                    contactDepthsA.Clear();
                    contactPointsB.Clear();
                    contactDepthsB.Clear();
                }

                r_min = float.MinValue;
                normal_c = Vector3.zero;

                // Calculate matrix for translation of b in a rotation space
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        R[i, j] = Vector3.Dot(A.axis[i], b.axis[j]);

                // Bring displacement Vec in a rotation space
                relativePosRot = new Vector3(Vector3.Dot(relativePos, A.axis[0]), Vector3.Dot(relativePos, A.axis[1]), Vector3.Dot(relativePos, A.axis[2]));

                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        AbsR[i, j] = Mathf.Abs(R[i, j]) + Vector3.kEpsilon;
            }
        }

        public void CalculateCollisions(float dt, List<GameObject> cubes)
        {
            GameObject cube0;
            GameObject cube1;
            _parallelCutof = 1f - _epsilon;
            for (var i = 0; i < cubes.Count; i++)
            {
                cube0 = cubes[i];
                var renderer = cube0.GetComponent<MeshRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                for (var j = 0; j < cubes.Count; j++)
                {
                    cube1 = cubes[j];
                    if (cube0 == cube1) continue;
                    var collision = _Collide(cube0.GetComponent<ObjectController>().anSimCollider, cube1.GetComponent<ObjectController>().anSimCollider, dt);
                    if (collision)
                    {
                        _ChangeColor(cube1, cube0);

                        CollisionPointSolver.ComputeCollisionInfo(ref coll);
                        _CalculateCollisionResponse();
                    }
                }
            }
        }

        private void _ChangeColor(GameObject cube1, GameObject cube0)
        {
            var renderer = cube1.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer = cube0.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private bool _Collide(OrientedBox3D cube0, OrientedBox3D cube1, float dt)
        {
            coll.SetValues(cube0, cube1, dt);

            var mag = coll.relativeVel.sqrMagnitude;
            if (coll.relativeVel.sqrMagnitude > Vector3.kEpsilon)
                //return _IntervalIntersectTime();
                if (!CollisionSolver.IntervalIntersectTime(ref coll)) return false;

            // Test all face normals of A
            if (!CollisionSolver.FaceNormalsIntersect(ref coll)) return false;

            return CollisionSolver.EdgesIntersect(ref coll);
        }

        private void _CalculateCollisionResponse()
        {
            //if (coll.contactPointsA.Count > 0 && coll.contactPointsA[0].sqrMagnitude > Vector3.kEpsilon)
            //{
            //    Vector3 point = Vector3.zero;
            //    for (var i = 0; i < coll.contactPointsA.Count; i++)
            //    {
            //        point += coll.contactPointsA[i];
            //    }
            //    point /= coll.contactPointsA.Count;

            //    GameObject.Find("CollisionPoint").GetComponent<Transform>().position = point;
            //}
        }
    }
}