using UnityEngine;

namespace Assets.Scripts.Physics
{
    /// <summary>
    /// Contains position and rotation information for an object. See "gaffer on games", "Ultrapede".
    /// </summary>
    internal class State
    {
        public Vector3 position;                // Physical Position of Object
        public Vector3 momentum;                // Current Momentum in kg*m/s

        public Quaternion orientation;          // Current Orientation of Physics Object
        public Vector3 angularMomentum;         // Current angular momentum

        public Vector3 velocity;                // Calculated velocity of object (m/s)
        public Quaternion spin;                 // Quaternion rate of change in orientation.
        public Vector3 angularVelocity;         // Velocity of Angular change

        public float mass;                      // Mass of object
        public float inverseMass;
        public Vector3 inertiaTensor;           // Inertia Tensor of object (We use only cubes in physics sim, so only one value)
        public Vector3 inverseInertiaTensor;

        public Matrix4x4 transform = new Matrix4x4();
        public Matrix3 inverseInertiaTensorLocal = new Matrix3();
        public Matrix3 inverseInertiaTensorWorld = new Matrix3();

        public State(Vector3 startPosition, Quaternion startOriantation, float mass, Vector3 inertiaTensor)
        {
            position = startPosition;
            orientation = startOriantation;
            this.mass = mass;
            inverseMass = 1.0f / mass;
            this.inertiaTensor = inertiaTensor;
            inverseInertiaTensor = Vector3.zero;
            for (int i = 0; i < 3; i++) inverseInertiaTensor[i] = 1.0f / inertiaTensor[i];
            inverseInertiaTensorLocal.SetDiagonal(inverseInertiaTensor);
        }

        /// <summary>
        /// Calculates the transform matrix in world coords
        /// </summary>
        /// <param name="transformMatrix"></param>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        private void CalculateTransformMatrix(ref Matrix4x4 transformMatrix, Vector3 position, Quaternion orientation)
        {
            transformMatrix[0] = 1 - 2 * orientation.y * orientation.y - 2 * orientation.z * orientation.z;
            transformMatrix[1] = 2 * orientation.x * orientation.y - 2 * orientation.w * orientation.z;
            transformMatrix[2] = 2 * orientation.x * orientation.z + 2 * orientation.w * orientation.y;
            transformMatrix[3] = position.x;

            transformMatrix[4] = 2 * orientation.x * orientation.y + 2 * orientation.w * orientation.z;
            transformMatrix[5] = 1 - 2 * orientation.x * orientation.x - 2 * orientation.z * orientation.z;
            transformMatrix[6] = 2 * orientation.y * orientation.z - 2 * orientation.w * orientation.x;
            transformMatrix[7] = position.y;

            transformMatrix[8] = 2 * orientation.x * orientation.z - 2 * orientation.w * orientation.y;
            transformMatrix[9] = 2 * orientation.y * orientation.z + 2 * orientation.w * orientation.x;
            transformMatrix[10] = 1 - 2 * orientation.x * orientation.x - 2 * orientation.y * orientation.y;
            transformMatrix[11] = position.z;
        }

        /// <summary>
        /// Calculates the inertia tensor in world coords
        /// </summary>
        /// <param name="iitWorld"></param>
        /// <param name="iitBody"></param>
        /// <param name="rotmat"></param>
        private void TransformInertiaTensor(ref Matrix3 iitWorld, Matrix3 iitBody, Matrix4x4 rotmat)
        {
            float t4 = rotmat[0] * iitBody[0] + rotmat[1] * iitBody[3] + rotmat[2] * iitBody[6];
            float t9 = rotmat[0] * iitBody[1] + rotmat[1] * iitBody[4] + rotmat[2] * iitBody[7];
            float t14 = rotmat[0] * iitBody[2] + rotmat[1] * iitBody[5] + rotmat[2] * iitBody[8];
            float t28 = rotmat[4] * iitBody[0] + rotmat[5] * iitBody[3] + rotmat[6] * iitBody[6];
            float t33 = rotmat[4] * iitBody[1] + rotmat[5] * iitBody[4] + rotmat[6] * iitBody[7];
            float t38 = rotmat[4] * iitBody[2] + rotmat[5] * iitBody[5] + rotmat[6] * iitBody[8];
            float t52 = rotmat[8] * iitBody[0] + rotmat[9] * iitBody[3] + rotmat[10] * iitBody[6];
            float t57 = rotmat[8] * iitBody[1] + rotmat[9] * iitBody[4] + rotmat[10] * iitBody[7];
            float t62 = rotmat[8] * iitBody[2] + rotmat[9] * iitBody[5] + rotmat[10] * iitBody[8];

            iitWorld[0] = t4 * rotmat[0] + t9 * rotmat[1] + t14 * rotmat[2];
            iitWorld[1] = t4 * rotmat[4] + t9 * rotmat[5] + t14 * rotmat[6];
            iitWorld[2] = t4 * rotmat[8] + t9 * rotmat[9] + t14 * rotmat[10];
            iitWorld[3] = t28 * rotmat[0] + t33 * rotmat[1] + t38 * rotmat[2];
            iitWorld[4] = t28 * rotmat[4] + t33 * rotmat[5] + t38 * rotmat[6];
            iitWorld[5] = t28 * rotmat[8] + t33 * rotmat[9] + t38 * rotmat[10];
            iitWorld[6] = t52 * rotmat[0] + t57 * rotmat[1] + t62 * rotmat[2];
            iitWorld[7] = t52 * rotmat[4] + t57 * rotmat[5] + t62 * rotmat[6];
            iitWorld[8] = t52 * rotmat[8] + t57 * rotmat[9] + t62 * rotmat[10];
        }

        /// <summary>
        /// Calculates variables to world coords
        /// </summary>
        public void CalculateDerivedData()
        {
            CalculateTransformMatrix(ref transform, position, orientation);
            TransformInertiaTensor(ref inverseInertiaTensorWorld, inverseInertiaTensorLocal, transform);
        }

        /// <summary>
        /// Calculates a new velocity from momentum and mass.
        /// </summary>
        public void RecalculatePosition()
        {
            velocity = momentum * inverseMass;
        }

        /// <summary>
        /// Calculates new secondary oriantation variables from angular velocity and inertia tensor.
        /// </summary>
        public void RecalculateRotation()
        {
            for (int i = 0; i < 3; i++) angularVelocity[i] = angularMomentum[i] * inverseInertiaTensor[i];
            orientation = AnSimMath.NormalizeQuaternion(orientation);
            spin = AnSimMath.QuatScale(new Quaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0) * orientation, 0.5f);
        }

        /// <summary>
        /// Generates a exact clone of this instance.
        /// </summary>
        /// <returns>A replicated state instance</returns>
        public State Clone()
        {
            var clone = new State(position, orientation, mass, inertiaTensor);
            clone.momentum = momentum;
            clone.angularMomentum = angularMomentum;
            clone.RecalculatePosition();
            clone.RecalculateRotation();
            clone.CalculateDerivedData();
            return clone;
        }
    }
}