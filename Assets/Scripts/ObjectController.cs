using UnityEngine;
using System.Collections;
using Assets.Scripts.Physics;
using Assets.Scripts.Collisions;

namespace Assets.Scripts
{
    /// <summary>
    /// Base class for all physics / collision based objects in simulation.
    /// </summary>
    public abstract class ObjectController : MonoBehaviour
    {

        public OrientedBox3D anSimCollider;
        public State lastState;
        public State nextState;

        public float Mass;
        public bool IsAnimated;
        public float LinearDamping;
        public float AngularDamping;

        public Vector3 CollisionForce;  // public und in unity zu sehen für testing
        public Vector3 CollisionTorque; // public und in unity zu sehen für testing

        /// <summary>
        /// Instantiates physics and collision components of this object.
        /// </summary>
        void Start()
        {
            Mass = (Mass <= 0) ? 1 : Mass;
            var transform = GetComponent<Transform>();
            lastState = new State(transform.position, transform.rotation, Mass, 0f); //TODO Inertia Tensor aus Größe/Scale berechnen
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
        public virtual void LinearForces(Vector3 force)
        {
            if (!IsAnimated) return;

            Gravity(force);
            MovementDamping(force);
        }

        /// <summary>
        /// Calculates rotational forces for this object;
        /// </summary>
        /// <param name="torque"> Container for force calculation </param>
        public virtual void RotationForces(Vector3 torque)
        {
            if (!IsAnimated) return;

            RotationalDamping(torque);
        }

        /// <summary>
        /// Adds gravity to force container for calculation.
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public void Gravity(Vector3 force)
        {
            force.y += MainProgram.GravityConstant;
        }

        /// <summary>
        /// Adds a force for damping of linear movement of this object (i.e. Reibung der Luft)
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public virtual void MovementDamping(Vector3 force) {
            force -= LinearDamping * nextState.velocity;
        }

        /// <summary>
        /// Adds a damping force on rotation.
        /// </summary>
        /// <param name="torque"> Container for torque calculation </param>
        public virtual void RotationalDamping(Vector3 torque)
        {
            torque -= AngularDamping * nextState.angularVelocity;
        }
    }
}
