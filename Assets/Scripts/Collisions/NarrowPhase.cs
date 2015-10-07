using Assets.Scripts.Physics;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class NarrowPhase
    {
        private CollisionInfo coll = new CollisionInfo();
        private BatchingProcessor batcher = new BatchingProcessor();

        public Dictionary<int, GameObject> PerformPhase(List<GameObject[]> pairs)
        {
            if (pairs.Count <= 0) return new Dictionary<int, GameObject>();

            Dictionary<int, GameObject> moved = new Dictionary<int, GameObject>();

            GameObject cube0, cube1;
            batcher.Clear();
            for (int i = 0; i < pairs.Count; i++)
            {
                cube0 = pairs[i][0];
                cube1 = pairs[i][1];
                var collision = _Collide(cube0, cube1, MainProgram.TIMESTEP);
                if (collision)
                {
                    ContactGenerator.ComputeCollisionInfo(ref coll);

                    batcher.AddContacts(coll.contacts);

                    //_CalculateCollisionResponse(cube0, cube1);

                    if (!moved.ContainsKey(cube0.GetInstanceID()))
                        moved.Add(cube0.GetInstanceID(), cube0);
                    if (!moved.ContainsKey(cube1.GetInstanceID()))
                        moved.Add(cube1.GetInstanceID(), cube1);
                    coll.Clear();
                }
            }
            //batcher.BatchContacts();
            foreach (ContactBatch cb in batcher.batches)
            {
                var cList = cb.GetAllContacts();
                CalculateCollisionResponse(ref cList);
            }
            coll.Clear();
            return moved;
        }

        private void AddPlayerForceToCube(GameObject player, GameObject cube, float dt)
        {
            var cubeController = cube.GetComponent<ObjectController>();
            var cubeCollider = cube.GetComponent<ObjectController>().anSimCollider;

            var playerController = player.GetComponent<BigCubeController>();
            var playerCollider = playerController.anSimCollider;

            Vector3 distanceVector = cubeCollider.center - playerCollider.center;

            if (distanceVector.sqrMagnitude < playerController.AffectingRadius * playerController.AffectingRadius)
            {
                cubeController.SetAwake(true);

                Vector3 dir = distanceVector * AnSimMath.Fast_Inv_Sqrt(distanceVector.sqrMagnitude);
                cubeController.AddForce(dir * playerController.PushForce, false);
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
            // Only player adds forces
            if (cube0.GetComponent<ObjectController>().isPlayer && cube0.GetComponent<BigCubeController>().AffectingRadius > 0f)
            {
                AddPlayerForceToCube(cube0, cube1, dt);
                if (!cube0.GetComponent<BigCubeController>().Collision) return false;
            }
            else if (cube1.GetComponent<ObjectController>().isPlayer && cube1.GetComponent<BigCubeController>().AffectingRadius > 0f)
            {
                AddPlayerForceToCube(cube1, cube0, dt);
                if (!cube1.GetComponent<BigCubeController>().Collision) return false;
            }

            
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

        // Not used
        private bool _DynamicCollision()
        {
            var mag = coll.relativeVelocity.sqrMagnitude;
            if (coll.relativeVelocity.sqrMagnitude > Vector3.kEpsilon)
                if (CollisionSolver.IntervalIntersectTime(ref coll))
                {
                    ContactGenerator.FindDynamicPoint(ref coll);
                    if(coll.contacts.Count > 0)
                        return true;
                }
            return false;
        }


        private void CalculateCollisionResponse(ref Contact[] cList)
        {
            if (cList.Length <= 0) return;
            Contact current;
            Vector3[] positionChange = new Vector3[2],
                rotationChange = new Vector3[2],
                velocityChange = new Vector3[2];
            float[] rotationAngle = new float[2];
            float max;

            int positionIterationsUsed = 0;
            while (positionIterationsUsed < cList.Length * 0.5) {

                max = 0f;
                current = null;
                foreach (Contact c in cList)
                {
                    if (c.depth > max)
                    {
                        max = c.depth;
                        current = c;
                    }
                }
                if (current == null) break;

                current.MatchAwakeState();

                ResolveOverlap(ref current, ref velocityChange, ref rotationChange, ref rotationAngle);

                UpdatePenetrations(ref cList, ref current, ref velocityChange, ref rotationChange, ref rotationAngle);

                positionIterationsUsed++;
            }

            max = 0f;

            int velocityIterationsUsed = 0;
            while (velocityIterationsUsed < cList.Length * 0.5)
            {
                max = 0f;
                current = null;
                foreach (Contact c in cList)
                {
                    if (c.desiredDeltaVelocity > max)
                    {
                        max = c.desiredDeltaVelocity;
                        current = c;
                    }
                }
                if (current == null) break;

                current.MatchAwakeState();

                ResolveCollision(ref current, ref velocityChange, ref rotationChange);

                UpdatePenetrationsVel(ref cList, ref current, ref velocityChange, ref rotationChange);

                velocityIterationsUsed++;
            }
        }

        /// <summary>
        /// Seperates the two cubes and add forces
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void _CalculateCollisionResponse(GameObject a, GameObject b)
        {
            if (coll.contacts.Count <= 0) return;
            int i, index;
            Vector3[] positionChange = new Vector3[2],
                rotationChange = new Vector3[2],
                velocityChange = new Vector3[2];
            float[] rotationAngle = new float[2];
            float max;

            int positionIterationsUsed = 0;
            while (positionIterationsUsed < 4)
            {
                max = MainProgram.POSITION_EPSOLON;
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

                coll.contacts[index].MatchAwakeState();
                var curr = coll.contacts[index];
                ResolveOverlap(ref curr, ref velocityChange, ref rotationChange, ref rotationAngle);

                //UpdatePenetrations(ref coll.contacts, ref curr, ref velocityChange, ref rotationChange, ref rotationAngle);

                positionIterationsUsed++;
            }

            max = 0;

            int velocityIterationsUsed = 0;
            while (velocityIterationsUsed < 4)
            {
                max = MainProgram.VELOCITY_EPSILON;
                index = -1;
                for (i = 0; i < coll.contacts.Count; i++)
                {
                    if (coll.contacts[i].desiredDeltaVelocity > max)
                    {
                        max = coll.contacts[i].desiredDeltaVelocity;
                        index = i;
                    }
                }
                if (index == -1) break;

                coll.contacts[index].MatchAwakeState();

                //ResolveCollision(coll.contacts[index], ref velocityChange, ref rotationChange);

                //UpdatePenetrationsVel(coll.contacts, index, ref velocityChange, ref rotationChange);

                velocityIterationsUsed++;
            }

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
        private void UpdatePenetrations(ref Contact[] c, ref Contact current, ref Vector3[] velocityChange, ref Vector3[] rotationChange, ref float[] rotationAmount)
        {
            Vector3 cp;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i].gameObject[0] != null)
                {
                    if (c[i].gameObject[0] == current.gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[0]);
                        cp += velocityChange[0];

                        c[i].depth -= Vector3.Dot(cp, c[i].normal) * rotationAmount[0];
                    }
                    else if (c[i].gameObject[0] == current.gameObject[1])
                    {
                        cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[0]);
                        cp += velocityChange[1];

                        c[i].depth -= Vector3.Dot(cp, c[i].normal) * rotationAmount[1];
                    }
                }
                if (c[i].gameObject[1] != null)
                {
                    if (c[i].gameObject[1] == current.gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[1]);
                        cp += velocityChange[0];

                        c[i].depth += Vector3.Dot(cp, c[i].normal) * rotationAmount[0];
                    }
                    else if (c[i].gameObject[1] == current.gameObject[1])
                    {
                        cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[1]);
                        cp += velocityChange[1];

                        c[i].depth += Vector3.Dot(cp, c[i].normal) * rotationAmount[1];
                    }
                }
            }
        }

        /// <summary>
        /// Updates the relative Velocities in all point after a separation of one point
        /// </summary>
        /// <param name="c"> List of Contact points </param>
        /// <param name="index"> index of the last corrected contact </param>
        /// <param name="velocityChange"> last/future velocity change of object </param>
        /// <param name="rotationChange"> last/future rotational velocity change of object </param>
        private void UpdatePenetrationsVel(ref Contact[] c, ref Contact current, ref Vector3[] velocityChange, ref Vector3[] rotationChange)
        {
            Vector3 cp;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i].gameObject[0] != null)
                {
                    if (c[i].gameObject[0] == current.gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[0]);
                        cp += velocityChange[0];

                        c[i].contactVelocity += c[i].contactToWorld.TransformTranspose(cp);
                        c[i].CalculateDesiredDeltaVelocity();
                    }
                    else if (c[i].gameObject[0] == current.gameObject[1])
                    {
                        cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[0]);
                        cp += velocityChange[1];

                        c[i].contactVelocity += c[i].contactToWorld.TransformTranspose(cp);
                        c[i].CalculateDesiredDeltaVelocity();
                    }
                }
                if (c[i].gameObject[1] != null)
                {
                    if (c[i].gameObject[1] == current.gameObject[0])
                    {
                        cp = Vector3.Cross(rotationChange[0], c[i].relativeContactPosition[1]);
                        cp += velocityChange[0];

                        c[i].contactVelocity -= c[i].contactToWorld.TransformTranspose(cp);
                        c[i].CalculateDesiredDeltaVelocity();
                    }
                    else if (c[i].gameObject[1] == current.gameObject[1])
                    {
                        cp = Vector3.Cross(rotationChange[1], c[i].relativeContactPosition[1]);
                        cp += velocityChange[1];

                        c[i].contactVelocity -= c[i].contactToWorld.TransformTranspose(cp);
                        c[i].CalculateDesiredDeltaVelocity();
                    }
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
        private void ResolveOverlap(ref Contact contact, ref Vector3[] velocityChange, ref Vector3[] rotationDirection, ref float[] rotationAmount)
        {
            float[] masses = new float[2];
            Matrix3[] inertias = new Matrix3[2];
            State[] state = new State[2];
            state[0] = contact.gameObject[0].GetComponent<ObjectController>().nextState;
            masses[0] = state[0].inverseMass;
            inertias[0] = state[0].inverseInertiaTensorWorld;

            Vector3[] positionChange = new Vector3[2];

            if (contact.gameObject[1] != null)
            {
                state[1] = contact.gameObject[1].GetComponent<ObjectController>().nextState;
                masses[1] = state[1].inverseMass;
                inertias[1] = state[1].inverseInertiaTensorWorld;
            }

            ContactResolver.ResolveOverlap(contact, masses, inertias, ref positionChange, ref velocityChange, ref rotationDirection, ref rotationAmount);

            Transform[] trans = new Transform[2];

            var controller = contact.gameObject[0].GetComponent<ObjectController>();
            if (controller.IsAnimated)
            {
                state[0].position += positionChange[0];
                state[0].orientation = AnSimMath.QuatAddScaledVector(state[0].orientation, rotationDirection[0], rotationAmount[0]);
                //state[0].orientation *= new Quaternion(rotationDirection[0].x * rotationAmount[0] * 0.5f,
                //    rotationDirection[0].y * rotationAmount[0] * 0.5f,
                //    rotationDirection[0].z * rotationAmount[0] * 0.5f,
                //    0); //Quaternion.AngleAxis(rotationAmount[0], rotationDirection[0]);
            }

            controller = contact.gameObject[1].GetComponent<ObjectController>();

            if (controller.IsAnimated && contact.gameObject[1] != null)
            {
                state[1].position += positionChange[1];
                state[1].orientation = AnSimMath.QuatAddScaledVector(state[1].orientation, rotationDirection[1], rotationAmount[1]);
                //state[1].orientation *= new Quaternion(rotationDirection[1].x * rotationAmount[1] * 0.5f,
                //    rotationDirection[1].y * rotationAmount[1] * 0.5f,
                //    rotationDirection[1].z * rotationAmount[1] * 0.5f,
                //    0); //Quaternion.AngleAxis(rotationAmount[1], rotationDirection[1]);
            }
        }

        /// <summary>
        /// Resolves the collision of two objects
        /// </summary>
        /// <param name="c"></param>
        /// <param name="velocityChange"></param>
        /// <param name="rotationChange"></param>
        private void ResolveCollision(ref Contact c, ref Vector3[] velocityChange, ref Vector3[] rotationChange)
        {
            State[] states = new State[2];
            float[] masses = new float[2];
            Matrix3[] inertias = new Matrix3[2];
            ObjectController[] controllers = new ObjectController[2];
            //Rigidbody[] body = new Rigidbody[2];
            controllers[0] = c.gameObject[0].GetComponent<ObjectController>();
            states[0] = c.gameObject[0].GetComponent<ObjectController>().nextState;
            masses[0] = states[0].inverseMass;
            inertias[0] =  states[0].inverseInertiaTensorWorld; // new Matrix3();
            //inertias[0].SetDiagonal(states[0].inverseInertiaTensor);

            if (c.gameObject[1] != null)
            {
                controllers[1] = c.gameObject[1].GetComponent<ObjectController>();
                states[1] = c.gameObject[1].GetComponent<ObjectController>().nextState;
                masses[1] = states[1].inverseMass;
                inertias[1] = states[1].inverseInertiaTensorWorld;// new Matrix3();
                //inertias[1].SetDiagonal(states[1].inverseInertiaTensor);
            }

            ContactResolver.ResolveCollision(c, masses, inertias, ref velocityChange, ref rotationChange);

            var controller = c.gameObject[0].GetComponent<ObjectController>();

            if (controller.IsAnimated && states[0].inverseMass > 0f && states[0].mass > 0)
            {
                states[0].momentum += velocityChange[0] * states[0].mass;
                for (int i = 0; i < 3; i++) states[0].angularMomentum[i] += rotationChange[0][i] * states[0].inverseInertiaTensor[i];
                states[0].RecalculatePosition();
                states[0].RecalculateRotation();
            }

            controller = c.gameObject[1].GetComponent<ObjectController>();

            if (controller.IsAnimated && states[1] != null && states[1].inverseMass > 0f && states[1].mass > 0)
            {
                states[1].momentum += velocityChange[1] * states[1].mass;
                for (int i = 0; i < 3; i++) states[1].angularMomentum[i] += rotationChange[1][i] * states[1].inverseInertiaTensor[i];
                states[1].RecalculatePosition();
                states[1].RecalculateRotation();
            }
        }
    }
}