using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class ContactResolver
    {
        public static void CalculateContactBasis(Vector3 contactNormal, out Matrix3 basis)
        {
            Vector3[] contactTangent = new Vector3[2];
            basis = new Matrix3();
            if (Mathf.Abs(contactNormal.x) > Mathf.Abs(contactNormal.y))
            {
                float s = 1.0f / Mathf.Sqrt(contactNormal.z * contactNormal.z + contactNormal.x * contactNormal.x);

                contactTangent[0].x = contactNormal.z * s;
                contactTangent[0].y = 0;
                contactTangent[0].z = -contactNormal.x * s;

                contactTangent[1].x = contactNormal.y * contactTangent[0].x;
                contactTangent[1].y = contactNormal.z * contactTangent[0].x - contactNormal.x * contactTangent[0].z;
                contactTangent[1].z = -contactNormal.y * contactTangent[0].x;
            }
            else
            {
                float s = 1.0f / Mathf.Sqrt(contactNormal.z * contactNormal.z + contactNormal.y * contactNormal.y);

                contactTangent[0].x = 0;
                contactTangent[0].y = -contactNormal.z * s;
                contactTangent[0].z = contactNormal.y * s;

                contactTangent[1].x = contactNormal.y * contactTangent[0].z - contactNormal.z * contactTangent[0].y;
                contactTangent[1].y = -contactNormal.x * contactTangent[0].z;
                contactTangent[1].z = contactNormal.x * contactTangent[0].y;
            }

            basis.SetColumns(contactNormal, contactTangent[0], contactTangent[1]);
        }

        private static Vector3 _CalculateLocalVelocity(Vector3 rot, Vector3 vel, Vector3 relativeContactPos)
        {
            Vector3 velocity = Vector3.Cross(rot, relativeContactPos);
            velocity += vel;

            return velocity;
        }

        public static void ResolveCollision(Vector3 contactNormal, float contactDepth, Vector3 P,
            Vector3 C0, Vector3 V0, Vector3 W0, float m0, float mass0, Vector3 i0, ref Vector3 force0, ref Vector3 torque0,
            Vector3 C1, Vector3 V1, Vector3 W1, float m1, float mass1, Vector3 i1, ref Vector3 force1, ref Vector3 torque1)
        {
            Matrix3 contactToWorld;
            CalculateContactBasis(contactNormal, out contactToWorld);

            Vector3[] relativeContactPosition = new Vector3[2];
            relativeContactPosition[0] = P - C0;
            relativeContactPosition[1] = P - C1;

            Vector3 deltaVelWorld = Vector3.Cross(relativeContactPosition[0], contactNormal);
            for (int i = 0; i < 3; i++) deltaVelWorld[i] *= i0[i];
            deltaVelWorld = Vector3.Cross(deltaVelWorld, relativeContactPosition[0]);

            float deltaVelocity = Vector3.Dot(deltaVelWorld, contactNormal);

            deltaVelocity += m0;

            deltaVelWorld = Vector3.Cross(relativeContactPosition[1], contactNormal);
            for (int i = 0; i < 3; i++) deltaVelWorld[i] *= i1[i];
            deltaVelWorld = Vector3.Cross(deltaVelWorld, relativeContactPosition[1]);

            deltaVelocity += Vector3.Dot(deltaVelWorld, contactNormal);

            deltaVelocity += m1;

            Vector3 contactVelocity = _CalculateLocalVelocity(W0, V0, relativeContactPosition[0]);
            contactVelocity -= _CalculateLocalVelocity(W1, V1, relativeContactPosition[1]);

            contactVelocity = contactToWorld.TransformTranspose(contactVelocity);

            // Add restitution
            float desiredDeltaVelocity = -contactVelocity.x * (1f + 0.4f);

            // Calculate Impulse
            var impulseContact = new Vector3(desiredDeltaVelocity / deltaVelocity, 0, 0);
            Vector3 impulse = contactToWorld.Transform(impulseContact);

            force0 = impulse * m0;
            force1 = -impulse * m1;

            Vector3 impulsiveTorque0 = Vector3.Cross(-impulse, relativeContactPosition[0]);
            Vector3 impulsiveTorque1 = Vector3.Cross(impulse, relativeContactPosition[1]);

            for (int i = 0; i < 3; i++)
            {
                torque0[i] = i0[i] * impulsiveTorque0[i];
                torque1[i] = i1[i] * impulsiveTorque1[i];
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
                angularMove[b] = (0 - b) * contact.depth * angularInertia[b] * invTotalInertia;
                linearMove[b] = (0 - b) * contact.depth * m[b] * invTotalInertia;

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