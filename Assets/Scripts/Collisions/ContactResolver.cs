using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class ContactResolver
    {
        /// <summary>
        /// Resolves collision velocities of two onjects in a collision point
        /// </summary>
        /// <param name="c"> Collision point </param>
        /// <param name="m"> inverse masses of objects </param>
        /// <param name="invInertia"> inverse inertia of objects </param>
        /// <param name="velocityChange"></param>
        /// <param name="rotationChange"></param>
        public static void ResolveCollision(Contact c, float[] m, Matrix3[] invInertia, ref Vector3[] velocityChange, ref Vector3[] rotationChange)
        {
            var objectCount = (c.gameObject[1] != null) ? 2 : 1;
            Vector3 impulseContact = Vector3.zero;

            // Check Friction
            float friction = 0f;
            for (int i = 0; i < 2; i++)
                friction += c.gameObject[i].GetComponent<ObjectController>().Friction;
            friction *= 0.5f;

            if (friction <= 0)  // Frictionless contact
            {
                float deltaVelocity = 0;
                for (int b = 0; b < objectCount; b++)
                {
                    Vector3 deltaVelWorld = Vector3.Cross(c.relativeContactPosition[b], c.normal);
                    deltaVelWorld = invInertia[b].TransformTranspose(deltaVelWorld);
                    deltaVelWorld = Vector3.Cross(deltaVelWorld, c.relativeContactPosition[b]);

                    deltaVelocity += Vector3.Dot(deltaVelWorld, c.normal);

                    deltaVelocity += m[b];
                }

                impulseContact = new Vector3(c.desiredDeltaVelocity / deltaVelocity, 0, 0);
            }
            else
            {
                // Take friction into account
                float inverseMass = 0;
                Matrix3 deltaVelWorld = new Matrix3(),
                    impulseToTorque = new Matrix3(),
                    deltaVelWorld_temp = new Matrix3();
                for (int i = 0; i < 2; i++)
                {
                    inverseMass += m[i];

                    impulseToTorque.SetSkewSymmetric(c.relativeContactPosition[i]);

                    deltaVelWorld_temp = impulseToTorque * invInertia[i];
                    deltaVelWorld_temp *= impulseToTorque;
                    deltaVelWorld_temp *= -1;

                    deltaVelWorld += deltaVelWorld_temp;
                }

                Matrix3 deltaVelocity = c.contactToWorld.Transposed();
                deltaVelocity *= deltaVelWorld;
                deltaVelocity *= c.contactToWorld;

                deltaVelocity[0] += inverseMass;
                deltaVelocity[4] += inverseMass;
                deltaVelocity[8] += inverseMass;

                Matrix3 impulseMatrix = deltaVelocity.Inverse();

                Vector3 velKill = new Vector3(c.desiredDeltaVelocity,
                    -c.contactVelocity.y,
                    -c.contactVelocity.z);

                impulseContact = impulseMatrix.Transform(velKill);

                float planarImpulse = Mathf.Sqrt(impulseContact.y * impulseContact.y + impulseContact.z * impulseContact.z);
                if (planarImpulse > impulseContact.x * friction)
                {
                    planarImpulse = 1f / planarImpulse;
                    impulseContact.y *= planarImpulse;
                    impulseContact.z *= planarImpulse;

                    impulseContact.x = deltaVelocity[0, 0] +
                        deltaVelocity[1, 0] * friction * impulseContact.y +
                        deltaVelocity[2, 0] * friction * impulseContact.z;

                    impulseContact.x = c.desiredDeltaVelocity / impulseContact.x;
                    impulseContact.y *= friction * impulseContact.x;
                    impulseContact.z *= friction * impulseContact.x;
                }
            }

            Vector3 impulse = c.contactToWorld.Transform(impulseContact);

            Vector3 impulsiveTorque = Vector3.Cross(c.relativeContactPosition[0], impulse);
            rotationChange[0] = invInertia[0].TransformTranspose(impulsiveTorque);
            velocityChange[0] = impulse * m[0];

            if (objectCount > 1)
            {
                velocityChange[1] = -impulse * m[1];
                impulsiveTorque = Vector3.Cross(impulse, c.relativeContactPosition[1]);
                rotationChange[1] = invInertia[1].TransformTranspose(impulsiveTorque);
            }
        }

        /// <summary>
        /// Calculates the changes in position and rotation of two overlapping objects in a collision point
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="m"> inverse masses of the two objects </param>
        /// <param name="invInertiaWorld"> inverse inertias of the two objects </param>
        /// <param name="positionChange"> Vector to save the position change to </param>
        /// <param name="rotationDir"> Vector to save the direction of the rotation change into </param>
        /// <param name="rotationAng"> Amount to ritate in rotation dir </param>
        public static void ResolveOverlap(Contact contact, float[] m, Matrix3[] invInertiaWorld, ref Vector3[] positionChange, ref Vector3[] rotationDir, ref float[] rotationAng)
        {
            float angularLimit = 5f,
                totalInertia = 0;
            float[] angularMove = new float[2],
                linearMove = new float[2],
                linearInertia = new float[2],
                angularInertia = new float[2];

            var objectCount = (contact.gameObject[1] == null) ? 1 : 2;

            for (uint b = 0; b < objectCount; b++)
            {
                Vector3 angularInertiaWorld = Vector3.Cross(contact.relativeContactPosition[b], contact.normal);
                angularInertiaWorld = invInertiaWorld[b].TransformTranspose(angularInertiaWorld);
                angularInertiaWorld = Vector3.Cross(angularInertiaWorld, contact.relativeContactPosition[b]);
                angularInertia[b] = Vector3.Dot(angularInertiaWorld, contact.normal);

                linearInertia[b] = m[b];

                totalInertia += linearInertia[b] + angularInertia[b];
            }

            float[] inverseMass = new float[2];

            totalInertia = angularInertia[0] + m[0];
            if (objectCount > 1)
            {
                inverseMass[1] = angularInertia[1] + m[1];
                totalInertia += inverseMass[1];
            }

            float invTotalInertia = 1f / totalInertia;
            for (int b = objectCount - 1; b >= 0; b--)
            {
                angularMove[b] = (1 - 2 * b) * contact.depth * angularInertia[b] * invTotalInertia;
                linearMove[b] = (1 - 2 * b) * contact.depth * m[b] * invTotalInertia;

                Vector3 projection1 = contact.relativeContactPosition[b];
                projection1 += contact.normal * Vector3.Dot(-contact.relativeContactPosition[b], contact.normal);
                float max1 = angularLimit * contact.relativeContactPosition[b].magnitude;
                if (Mathf.Abs(angularMove[b]) > max1)
                {
                    float pp = angularMove[b] + linearMove[b];
                    angularMove[b] = angularMove[b] > 0 ? max1 : -max1;
                    linearMove[b] = pp - angularMove[b];
                }
            }

            Vector3[] rotationDirection = new Vector3[2];
            float[] rotationAmount = new float[2];

            // Both final chenges
            for (int b = 0; b < objectCount; b++)
            {
                Vector3 t;
                if (angularMove[b] != 0f)
                {
                    t = Vector3.Cross(contact.relativeContactPosition[b], contact.normal);
                    rotationDirection[b] = invInertiaWorld[b].TransformTranspose(t);

                    rotationAmount[b] = angularMove[b] / angularInertia[b];
                }
                else
                {
                    rotationDirection[b] = Vector3.zero;
                    rotationAmount[b] = 1f;
                }

                positionChange[b] = contact.normal * linearMove[b];

                rotationDir[b] = rotationDirection[b];
                rotationAng[b] = rotationAmount[b] * 0.5f;
            }
        }
    }
}