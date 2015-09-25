using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Contains all important information on a collision between two objects
    /// </summary>
    internal class Contact
    {
        public GameObject[] gameObject = new GameObject[2]; // The two objects

        public Vector3 point;                               // Collision point in world coords
        public float depth;                                 // Collision depth along the normal

        public Vector3 normal;                              // Normal of collision
        public int code;                                    // Code from collision registration (mortly for debugging)

        public Matrix3 contactToWorld = new Matrix3();      // Transformation Matrix from world to contact space
        public Vector3[] relativeContactPosition = new Vector3[2]; // Relative position of the two objects in contact space
        public Vector3 contactVelocity;                            // Relative Velocity in Contact point space
        public float desiredDeltaVelocity;                         // Velocity needet to push the two objects apart

        public Vector3 lastFrameAcc = Vector3.zero;

        public Contact(GameObject A, GameObject B, Vector3 point, Vector3 normal, float depth, int code)
        {
            gameObject[0] = A;
            gameObject[1] = B;
            this.point = point;
            this.normal = normal;
            this.depth = depth;
            this.code = code;

            CalculateInternals();
        }

        /// <summary>
        /// Recalculates all important collision information.
        /// </summary>
        public void CalculateInternals()
        {
            SwapObjects();
            CalculateContactBasis();
            CalculateRelativePosition();
            CalculateContactVelocity();
            CalculateDesiredDeltaVelocity();
        }

        /// <summary>
        /// Checks if the collision body is in first position. This could happen if the second body isn't movable object i.e. isAnimated is turned off
        /// </summary>
        public void SwapObjects()
        {
            if (gameObject[0] == null)
            {
                gameObject[0] = gameObject[1];
                gameObject[1] = null;
                normal *= -1;
            }
        }

        /// <summary>
        /// Calculates a transformation matrix for transformation of points into contact space.
        /// </summary>
        public void CalculateContactBasis()
        {
            Vector3[] contactTangent = new Vector3[2];
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                float s = 1.0f / Mathf.Sqrt(normal.z * normal.z + normal.x * normal.x);

                contactTangent[0].x = normal.z * s;
                contactTangent[0].y = 0;
                contactTangent[0].z = -normal.x * s;

                contactTangent[1].x = normal.y * contactTangent[0].x;
                contactTangent[1].y = normal.z * contactTangent[0].x - normal.x * contactTangent[0].z;
                contactTangent[1].z = -normal.y * contactTangent[0].x;
            }
            else
            {
                float s = 1.0f / Mathf.Sqrt(normal.z * normal.z + normal.y * normal.y);

                contactTangent[0].x = 0;
                contactTangent[0].y = -normal.z * s;
                contactTangent[0].z = normal.y * s;

                contactTangent[1].x = normal.y * contactTangent[0].z - normal.z * contactTangent[0].y;
                contactTangent[1].y = -normal.x * contactTangent[0].z;
                contactTangent[1].z = normal.x * contactTangent[0].y;
            }

            contactToWorld.SetColumns(normal, contactTangent[0], contactTangent[1]);
        }

        /// <summary>
        /// Sets the relative position of this contact point to both bodies
        /// </summary>
        public void CalculateRelativePosition()
        {
            relativeContactPosition[0] = point - gameObject[0].GetComponent<Transform>().position;
            if (gameObject[1])
                relativeContactPosition[1] = point - gameObject[1].GetComponent<Transform>().position;
        }

        /// <summary>
        /// Calculates the relative velocity in this point
        /// </summary>
        public void CalculateContactVelocity()
        {
            var body = gameObject[0].GetComponent<Rigidbody>();
            contactVelocity = _CalculateLocalVelocity(body.angularVelocity, body.velocity, relativeContactPosition[0]);
            if (gameObject[1])
            {
                body = gameObject[1].GetComponent<Rigidbody>();
                contactVelocity -= _CalculateLocalVelocity(body.angularVelocity, body.velocity, relativeContactPosition[0]);
            }
        }

        /// <summary>
        /// Calculates the local velocity of an object relative to a point
        /// </summary>
        /// <param name="rot"> Angular velocity of body. </param>
        /// <param name="vel"> Linear velocity of body </param>
        /// <param name="relativeContactPos"> Relative position to point. </param>
        /// <returns> Local velocity in a certain point </returns>
        private Vector3 _CalculateLocalVelocity(Vector3 rot, Vector3 vel, Vector3 relativeContactPos)
        {
            return contactToWorld.TransformTranspose(Vector3.Cross(rot, relativeContactPos) + vel);
        }

        /// <summary>
        /// Calculates the min velocity that will push the two objects apart.
        /// </summary>
        public void CalculateDesiredDeltaVelocity()
        {
            float velocityLimit = 0.25f;

            var controller = gameObject[0].GetComponent<ObjectController>();
            float velocityFromAcc = Vector3.Dot(controller.lastFrameAcceleration, normal) * MainProgram._timeStep;

            var restitution = controller.Restitution;
            if (gameObject[1] != null)
            {
                controller = gameObject[1].GetComponent<ObjectController>();
                velocityFromAcc -= Vector3.Dot(controller.lastFrameAcceleration, normal) * MainProgram._timeStep;
                restitution += controller.Restitution;
            }

            restitution *= 0.5f;

            //if (Mathf.Abs(contactVelocity.x) < velocityLimit)
            //    restitution = 0f;

            desiredDeltaVelocity = -contactVelocity.x * (1f + restitution);// * (contactVelocity.x - velocityFromAcc);
        }

        /************************ not used atm. *************************/

        public void Update(Vector3 point, float depth)
        {
            this.point = point;
            this.depth = depth;
        }

        public void Update(Vector3 point, float depth, Vector3 normal)
        {
            this.point = point;
            this.depth = depth;
            this.normal = normal;
        }

        public static bool operator ==(Contact a, Contact b)
        {
            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(Contact a, Contact b)
        {
            return a.GetHashCode() != b.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (GetHashCode() == obj.GetHashCode());
        }

        public override int GetHashCode()
        {
            return code.GetHashCode() + depth.GetHashCode() + point.GetHashCode() + normal.GetHashCode();
        }
    }
}