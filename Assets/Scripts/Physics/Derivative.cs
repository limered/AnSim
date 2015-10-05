using UnityEngine;

namespace Assets.Scripts.Physics
{
    internal class Derivative
    {
        public Vector3 velocity;
        public Vector3 force;
        public Quaternion spin;
        public Vector3 torque;

        public Derivative(Vector3 velocity, Vector3 force, Quaternion spin, Vector3 torque)
        {
            this.velocity = velocity;
            this.force = force;
            this.spin = spin;
            this.torque = torque;
        }

        public Derivative(Derivative old)
        {
            velocity = old.velocity;
            force = old.force;
            spin = old.spin;
            torque = old.torque;
        }
        public Derivative() {
            velocity = Vector3.zero;
            force = Vector3.zero;
            spin = Quaternion.identity;
            torque = Vector3.zero;
        }
    }
}