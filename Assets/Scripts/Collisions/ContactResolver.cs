using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class ContactResolver
    {

        public static void ResolveCollision(Contact c, float[] m, Matrix3[] invInertia, ref Vector3[] velocityChange, ref Vector3[] rotationChange)
        {
            var objectCount = (c.gameObject[1] != null) ? 2 : 1;

            float deltaVelocity = 0;

            for (int b = 0; b < objectCount; b++)
            {
                Vector3 deltaVelWorld = Vector3.Cross(c.relativeContactPosition[b], c.normal);
                deltaVelWorld = invInertia[b].Transform(deltaVelWorld);
                deltaVelWorld = Vector3.Cross(deltaVelWorld, c.relativeContactPosition[b]);

                deltaVelocity += Vector3.Dot(deltaVelWorld, c.normal);

                deltaVelocity += m[b];
            }

            var impulseContact = new Vector3(c.desiredDeltaVelocity / deltaVelocity, 0, 0);
            Vector3 impulse = c.contactToWorld.Transform(impulseContact);

            for (int b = 0; b < objectCount; b++)
            {
                velocityChange[b] = (1 - 2 * b) * impulse * m[b];
                Vector3 impulsiveTorque = Vector3.Cross((-1 + 2 * b) * impulse, c.relativeContactPosition[b]);
                rotationChange[b] = invInertia[b].Transform(impulsiveTorque);
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
                angularInertiaWorld = invInertiaWorld[b].Transform(angularInertiaWorld);
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
                    rotationDirection[b] = invInertiaWorld[b].Transform(t);

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