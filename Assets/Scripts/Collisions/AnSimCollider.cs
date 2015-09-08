using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// (Cube Collider) Generates positions of vertices from a state object and saves them for colision calculation.
    /// </summary>
    public class AnSimCollider
    {
        private Vector3 center;     // Position of OBB

        private Vector3 axis0;      // Rotatated x-axis
        private Vector3 axis1;      // Rotatated y-axis
        private Vector3 axis2;      // Rotatated z-axis

        private float extent0;      // Scale in x-axis
        private float extent1;      // Scale in y-axis
        private float extent2;      // Scale in z-axis

        /// <summary>
        /// Updates this collision info from object data
        /// </summary>
        /// <param name="o"> GameObject to update from </param>
        public void UpdateDataFromObject(GameObject o)
        {
            var state = o.GetComponent<ObjectController>().nextState;
            var transform = o.GetComponent<Transform>();

            center = transform.position; //temp
            //center = state.position;

            var orientation = transform.rotation;   //temp
            //var orientation = state.orientation;

            axis0 = orientation * Vector3.right;
            axis1 = orientation * Vector3.up;
            axis2 = orientation * Vector3.forward;

            extent0 = transform.localScale.x;
            extent1 = transform.localScale.y;
            extent2 = transform.localScale.z;
        }
    }
}