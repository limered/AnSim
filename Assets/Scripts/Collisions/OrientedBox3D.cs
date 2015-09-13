using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// (Cube Collider) Generates positions of vertices from a state object and saves them for colision calculation.
    /// </summary>
    public class OrientedBox3D
    {
        public Vector3 center { get; private set; }

        public Vector3[] axis { get; private set; }

        public float[] extents { get; private set; }

        public Vector3 velocity { get; private set; }
        public Quaternion rotation { get; private set; }
        public Matrix4x4 rotMat { get; private set; }

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
            var state = o.GetComponent<ObjectController>().nextState;
            var transform = o.GetComponent<Transform>();

            center = transform.position;//.Set(transform.position.x, transform.position.y, transform.position.z); //temp
            //center = state.position;

            rotation = transform.rotation;   //temp
            //var orientation = state.orientation;

            axis[0] = rotation * Vector3.right;
            axis[1] = rotation * Vector3.up;
            axis[2] = rotation * Vector3.forward;

            rotMat = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

            extents[0] = transform.localScale.x * 0.5f;
            extents[1] = transform.localScale.y * 0.5f;
            extents[2] = transform.localScale.z * 0.5f;

            velocity = state.velocity;
        }
    }
}