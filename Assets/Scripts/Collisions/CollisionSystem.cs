using Assets.Scripts.Physics;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Calculates collisions for each object. Siehe "gaffer on games" und "Ultrapede"
    /// </summary>
    internal class CollisionSystem
    {
        public static float friction = 0f;
        public static float bounce = 10f;

        public static float overlapElasticity = 0.2f;

        private CollisionInfo coll = new CollisionInfo();

        /// <summary>
        /// Starts the collision calculation process
        /// </summary>
        /// <param name="dt"> timestep </param>
        /// <param name="cubes"> array containing all cubes </param>
        /// <param name="Walls"> array containing all walls </param>
        public void CalculateCollisions(float dt, List<GameObject> cubes, GameObject[] Walls)
        {
            GameObject cube0;
            GameObject cube1;
            for (var i = 0; i < cubes.Count; i++)
            {
                cube0 = cubes[i];
                WallCollisionSolver.CollideWithWalls(ref cube0, Walls);

                for (var j = 0; j < cubes.Count; j++)
                {
                    cube1 = cubes[j];
                    if (cube0 == cube1) continue;
                    var collision = _Collide(cube0, cube1, dt);
                    if (collision)
                    {
                        ContactGenerator.ComputeCollisionInfo(ref coll);
                        _CalculateCollisionResponse(cube0, cube1);

                        _ChangeColor(cube0, cube1);
                    }
                }
            }
        }

        private void _ChangeColor(GameObject cube0, GameObject cube1)
        {
            var script = cube0.GetComponent<SmallCubeController>();
            if (script != null)
            {
                script.ChangeColor(Time.realtimeSinceStartup);
            }
            script = cube1.GetComponent<SmallCubeController>();
            if (script != null)
            {
                script.ChangeColor(Time.realtimeSinceStartup);
            }
        }

        /// <summary>
        /// Checks collision between two cubes
        /// </summary>
        /// <param name="cube0"></param>
        /// <param name="cube1"></param>
        /// <param name="dt"></param>
        /// <returns> true, if collision occured, all information is stored in coll</returns>
        private bool _Collide(GameObject cube0, GameObject cube1, float dt)
        {
            coll.SetValues(cube0, cube1, dt);

            return _StaticCollision();

            //if (_StaticCollision()) return true;

            //return _DynamicCollision();
        }

        private bool _StaticCollision()
        {
            if (!CollisionSolver.FaceNormalsIntersect(ref coll)) return false;

            if (coll.hasParallelAxis)
                return CollisionSolver.Intersect2D(ref coll);

            return CollisionSolver.EdgesIntersect(ref coll);
        }

        private bool _DynamicCollision()
        {
            var mag = coll.relativeVelocity.sqrMagnitude;
            if (coll.relativeVelocity.sqrMagnitude > Vector3.kEpsilon)
                if (CollisionSolver.IntervalIntersectTime(ref coll))
                {
                    ContactGenerator.FindDynamicPoint(ref coll);
                }
            return false;
        }

        /// <summary>
        /// divides the two cubes and add forces
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void _CalculateCollisionResponse(GameObject a, GameObject b)
        {
            if (coll.contacts.Count <= 0) return;
            int i, index;
            Vector3[] positionChange = new Vector3[2], rotationChange = new Vector3[2];
            float[] rotationAngle = new float[2];
            float max;
            Vector3 cp;

            int positionIterationsUsed = 0;
            while (positionIterationsUsed < 4)
            {
                max = 0;//positionEpsilon;
                index = -1;
                for (i = 0; i < coll.contacts.Count; i++)
                {
                    if (coll.contacts[i].depth > max)
                    {
                        max = coll.contacts[i].depth;
                        index = i;
                    }
                }
                if (index == -1) break;

                ResolveOverlap(coll.contacts[index], ref positionChange, ref rotationChange, ref rotationAngle);

                UpdatePenetrations(coll.contacts, index, ref positionChange, ref rotationChange, ref rotationAngle);

                positionIterationsUsed++;
            }

            //ResolveCollision(coll.contacts[i]);

            //ResolveOverlap(a, b);

            ResolveCollision(a, b);

            coll.Clear();
        }

        /// <summary>
        /// Updates the penetration of all other collision points depending on change from previous position correction
        /// </summary>
        /// <param name="c"> collision points </param>
        /// <param name="index"> index of last changed point </param>
        /// <param name="positionChange"></param>
        /// <param name="rotationChange"></param>
        /// <param name="rotationAmount"></param>
        private void UpdatePenetrations(List<Contact> c, int index, ref Vector3[] positionChange, ref Vector3[] rotationChange, ref float[] rotationAmount)
        {
            Vector3 cp;
            for(int i = 0; i < c.Count; i++)
            {
                if (c[i].gameObject[0] != null)
                {
                    if (c[i].gameObject[0] == c[index].gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[0]);
                        cp += positionChange[0];

                        c[i].depth -= Vector3.Dot(cp, c[i].normal) * rotationAmount[0];
                    }
                    else if (c[i].gameObject[0] == c[index].gameObject[1])
                    {
                        cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[0]);
                        cp += positionChange[1];

                        c[i].depth -= Vector3.Dot(cp, c[i].normal) * rotationAmount[1];
                    }
                }
                if (c[i].gameObject[1] != null)
                {
                    if(c[i].gameObject[1] == c[index].gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[1]);
                        cp += positionChange[0];

                        c[i].depth += Vector3.Dot(cp, c[i].normal) * rotationAmount[0];
                    }
                }
                else if(c[i].gameObject[1] == c[index].gameObject[1])
                {
                    cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[1]);
                    cp += positionChange[1];

                    c[i].depth += Vector3.Dot(cp, c[i].normal) * rotationAmount[1];
                }
            }
        }

        /// <summary>
        /// Resolves the overlap of two objects in a collision point
        /// </summary>
        /// <param name="contact"> Contact containing collision data </param>
        /// <param name="positionChange"></param>
        /// <param name="rotationDirection"></param>
        /// <param name="rotationAmount"></param>
        private void ResolveOverlap(Contact contact, ref Vector3[] positionChange, ref Vector3[] rotationDirection, ref float[] rotationAmount)
        {
            float[] masses = new float[2];
            Matrix3[] inertias = new Matrix3[2];
            State[] state = new State[2];
            state[0] = contact.gameObject[0].GetComponent<ObjectController>().nextState;
            masses[0] = state[0].inverseMass;
            inertias[0] = coll.A.inverseInertiaTensorWorld;

            if (contact.gameObject[1] != null)
            {
                state[1] = contact.gameObject[1].GetComponent<ObjectController>().nextState;
                masses[1] = state[1].inverseMass;
                inertias[1] = coll.B.inverseInertiaTensorWorld;
            }

            ContactResolver.ResolveOverlap(contact, masses, inertias, ref positionChange, ref rotationDirection, ref rotationAmount);

            Transform[] trans = new Transform[2];

            trans[0] = contact.gameObject[0].GetComponent<Transform>();
            trans[0].position += positionChange[0] * masses[0];
            trans[0].rotation *= Quaternion.AngleAxis(rotationAmount[0] * Mathf.PI, rotationDirection[0]);

            if (contact.gameObject[1] != null)
            {
                trans[1] = contact.gameObject[1].GetComponent<Transform>();
                trans[1].position += positionChange[1] * masses[1];
                trans[1].rotation *= Quaternion.AngleAxis(rotationAmount[1] * Mathf.PI, rotationDirection[1]);
            }
        }

        /// <summary>
        /// Adds forces to colliding cubes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void ResolveCollision(GameObject a, GameObject b)
        {
            for (int i = 0; i < coll.contacts.Count; i++)
            {
                Rigidbody rigitbodyA = a.GetComponent<Rigidbody>(),
                    rigitbodyB = b.GetComponent<Rigidbody>();
                State stateA = a.GetComponent<ObjectController>().nextState,
                    stateB = b.GetComponent<ObjectController>().nextState;
                Transform transA = a.GetComponent<Transform>(),
                    transB = b.GetComponent<Transform>();

                Vector3 force0 = Vector3.zero;
                Vector3 torque0 = Vector3.zero;

                Vector3 force1 = Vector3.zero;
                Vector3 torque1 = Vector3.zero;

                ContactResolver.ResolveCollision(coll.contacts[i].normal, coll.contacts[i].depth, coll.contacts[i].point,
                    transA.position, rigitbodyA.velocity, rigitbodyA.angularVelocity, stateA.inverseMass, stateA.mass, stateA.inverseInertiaTensor, ref force0, ref torque0,
                    transB.position, rigitbodyB.velocity, rigitbodyB.angularVelocity, stateB.inverseMass, stateB.mass, stateB.inverseInertiaTensor, ref force1, ref torque1);

                if (stateA.inverseMass > 0f && stateA.mass > 1)
                {
                    rigitbodyA.velocity += force0;
                    rigitbodyA.angularVelocity += torque0;
                    //rigitbodyA.AddForce(force0);
                    //rigitbodyA.AddTorque(torque0);
                }

                if (stateB.inverseMass > 0f && stateB.mass > 1)
                {
                    rigitbodyB.velocity += force1;
                    rigitbodyB.angularVelocity += torque1;
                    //rigitbodyB.AddForce(force1);
                    //rigitbodyB.AddTorque(torque1);
                }
            }
        }
    }
}