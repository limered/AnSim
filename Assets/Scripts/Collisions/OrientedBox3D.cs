using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// (Cube Collider) Generates positions of vertices from a state object and saves them for colision calculation.
    /// </summary>
    internal class OrientedBox3D
    {
        public Vector3 center { get; private set; }
        public Vector3[] axis { get; private set; }
        public float[] extents { get; private set; }

        public Vector3 velocity { get; private set; }
        public Quaternion orientation { get; private set; }
        public Matrix4x4 transform = new Matrix4x4();
        public Matrix3 inverseInertiaTensorLocal = new Matrix3();
        public Matrix3 inverseInertiaTensorWorld = new Matrix3();

        public OrientedBox3D()
        {
            center = new Vector3();
            axis = new Vector3[3];
            extents = new float[3];
        }

        /// <summary>
        /// Updates this collision info from object data
        /// </summary>
        /// <param name="o"> GameObject to update from </param>
        public void UpdateDataFromObject(GameObject o)
        {
            var controller = o.GetComponent<ObjectController>();
            var state = controller.nextState;
            var transform = o.GetComponent<Transform>();

            center = state.position;
            orientation = state.orientation;

            axis[0] = orientation * Vector3.right;
            axis[1] = orientation * Vector3.up;
            axis[2] = orientation * Vector3.forward;

            extents[0] = transform.localScale.x * 0.5f;
            extents[1] = transform.localScale.y * 0.5f;
            extents[2] = transform.localScale.z * 0.5f;

            velocity = state.velocity;

            inverseInertiaTensorLocal.SetDiagonal(state.inverseInertiaTensor);

            // Last frame acceleration for collision handling
            controller.lastFrameAcceleration = controller.accumulatedLinearForce * controller.nextState.inverseMass;

            _CalculateDerivedData();
        }

        /// <summary>
        /// Calculates the transform matrix in wirld coords
        /// </summary>
        /// <param name="transformMatrix"></param>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        public void CalculateTransformMatrix(ref Matrix4x4 transformMatrix, Vector3 position, Quaternion orientation)
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
        public void TransformInertiaTensor(ref Matrix3 iitWorld, Matrix3 iitBody, Matrix4x4 rotmat)
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
        private void _CalculateDerivedData()
        {
            CalculateTransformMatrix(ref transform, center, orientation);
            TransformInertiaTensor(ref inverseInertiaTensorWorld, inverseInertiaTensorLocal, transform);
        }
    }
}