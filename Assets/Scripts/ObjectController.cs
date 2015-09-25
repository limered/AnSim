using UnityEngine;
using System.Collections;
using Assets.Scripts.Physics;
using Assets.Scripts.Collisions;

namespace Assets.Scripts
{
    /// <summary>
    /// Base class for all physics / collision based objects in simulation.
    /// </summary>
    internal abstract class ObjectController : MonoBehaviour
    {

        public OrientedBox3D anSimCollider;
        public State lastState;
        public State nextState;

        public float Mass;
        public bool IsAnimated;
        public float LinearDamping;
        public float AngularDamping;

        public float Restitution;

        public Vector3 CollisionForce;  // public und in unity zu sehen für testing
        public Vector3 CollisionTorque; // public und in unity zu sehen für testing

        public Vector3 accumulatedForce;
        public Vector3 lastFrameAcceleration;


        /// <summary>
        /// Instantiates physics and collision components of this object.
        /// </summary>
        void Start()
        {
            Mass = (Mass <= 0) ? 1 : Mass;
            var transform = GetComponent<Transform>();
            Vector3 inertiaTensor = new Vector3(Mass * (transform.localScale.y * transform.localScale.y + transform.localScale.z * transform.localScale.z / 12),
                Mass * (transform.localScale.x * transform.localScale.x + transform.localScale.z * transform.localScale.z / 12),
                Mass * (transform.localScale.x * transform.localScale.x + transform.localScale.y * transform.localScale.y / 12));
            lastState = new State(transform.position, transform.rotation, Mass, inertiaTensor); //TODO Inertia Tensor aus Größe/Scale berechnen
            nextState = lastState.Clone();
            anSimCollider = new OrientedBox3D();
            anSimCollider.UpdateDataFromObject(gameObject);

            CollisionForce = new Vector3();
            CollisionTorque = new Vector3();
        }

        /// <summary>
        /// Calculates movement forces for this object.
        /// </summary>
        /// /// <param name="force"> Container for force calculation </param>
        public virtual void LinearForces(ref Vector3 force)
        {
            if (!IsAnimated) return;

            Gravity(ref force);
            MovementDamping(ref force);
        }

        /// <summary>
        /// Calculates rotational forces for this object;
        /// </summary>
        /// <param name="torque"> Container for force calculation </param>
        public virtual void RotationForces(ref Vector3 torque)
        {
            if (!IsAnimated) return;

            RotationalDamping(ref torque);
        }

        /// <summary>
        /// Adds gravity to force container for calculation.
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public void Gravity(ref Vector3 force)
        {
            force.y += MainProgram.GravityConstant * nextState.mass;
        }

        /// <summary>
        /// Adds a force for damping of linear movement of this object (i.e. Reibung der Luft)
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public virtual void MovementDamping(ref Vector3 force) {

            var tempVel = GetComponent<Rigidbody>().velocity;

            force -= LinearDamping * tempVel * nextState.mass;//nextState.velocity;
        }

        /// <summary>
        /// Adds a damping force on rotation.
        /// </summary>
        /// <param name="torque"> Container for torque calculation </param>
        public virtual void RotationalDamping(ref Vector3 torque)
        {
            torque -= AngularDamping * nextState.angularVelocity * nextState.mass;
        }
    }
}
