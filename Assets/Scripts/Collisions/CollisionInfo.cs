using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class CollisionInfo
    {
        public GameObject object_A;
        public GameObject object_B;

        // Boxes to collide
        public OrientedBox3D A;

        public OrientedBox3D B;

        // relative values
        public Vector3 relativePosition;

        public Vector3 relativePositionRotated;
        public Vector3 relativeVelocity;

        // matrices and values cotaining relative rotations
        public float[,] R, AbsR;

        public float ra, rb;

        // collision info !!!TODO replace with COntacts!!!
        public float r_min;     // deepest penetration depth

        public Vector3 n_min;   // normal for deepest penetration
        public Vector3 normal_c;// normal for edge-edge collision
        public bool invert_normal;
        public int code;        // collision code 1-6 face-vertex, 7-15 edge-edge
        public int tested_axis_0;// if collision true -> tested axis with deepest penetration
        public int tested_axis_1;
        public bool hasParallelAxis;    // abort, if parallel axis in face-vertex collision

        // contact point data       !!!TODO replace with Contacts!!!
        public List<Contact> contacts = new List<Contact>();

        // final normal
        public Vector3 n;

        public CollisionInfo()
        {
            R = new float[3, 3];
            AbsR = new float[3, 3];
        }

        public void SetValues(GameObject a, GameObject b, float dt)
        {
            object_A = a;
            object_B = b;
            A = a.GetComponent<ObjectController>().anSimCollider;
            B = b.GetComponent<ObjectController>().anSimCollider;

            relativePosition = B.center - A.center;

            relativePositionRotated = new Vector3(Vector3.Dot(relativePosition, A.axis[0]), Vector3.Dot(relativePosition, A.axis[1]), Vector3.Dot(relativePosition, A.axis[2]));
            relativeVelocity = A.velocity * dt - B.velocity * dt;

            n = Vector3.zero;

            r_min = float.MinValue;
            normal_c = Vector3.zero;

            _GenerateTranslationMatrix();
        }

        public void Clear()
        {
            contacts.Clear();
        }

        public void AddContact(Contact c)
        {
            contacts.Add(c);
        }

        private void _GenerateTranslationMatrix()
        {
            // Calculate matrix for translation of B in As rotation space
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    R[i, j] = Vector3.Dot(A.axis[i], B.axis[j]);
                    AbsR[i, j] = Mathf.Abs(R[i, j]) + Vector3.kEpsilon;
                    if (AbsR[i, j] > 1 - Vector3.kEpsilon)
                        hasParallelAxis = true;// -> collision only in 2d face-vertex is enough
                }
            }
        }
    }
}