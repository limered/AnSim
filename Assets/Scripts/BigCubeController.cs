using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Player class.
    /// </summary>
    public class BigCubeController : ObjectController
    {
        public float MovementSpeed;
        
        /// <summary>
        /// Overrides base class implementation and adds a input force calculation.
        /// </summary>
        /// <param name="force"></param>
        public override void LinearForces(Vector3 force)
        {
            if (!IsAnimated) return;

            base.LinearForces(force);

            force += InputForce();
        }

        /// <summary>
        /// Calculates the current input force. Based on movement speed(set variable in unity)
        /// </summary>
        /// <returns>THe Movement Force</returns>
        private Vector3 InputForce()
        {
            var result = new Vector3();
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                result.z += MovementSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                result.z -= MovementSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                result.x += MovementSpeed;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                result.x -= MovementSpeed;
            }

            return result;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        void Update() {
            GetComponent<Transform>().position += InputForce();
        }
    }
}