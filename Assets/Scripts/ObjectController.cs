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
        public GameObject statics;

        public OrientedBox3D anSimCollider;
        public State lastState;
        public State nextState;

        public float Mass;
        public bool IsAnimated;
        public float LinearDamping;
        public float AngularDamping;

        public float Restitution;
        public float Friction;

        public Vector3 CollisionForce;  // public und in unity zu sehen für testing
        public Vector3 CollisionTorque; // public und in unity zu sehen für testing

        public Vector3 accumulatedForce;
        public Vector3 lastFrameAcceleration;

        public bool isAwake;
        public bool canSleep;
        public float motion;


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

            force += -LinearDamping * tempVel;//nextState.velocity;
        }

        /// <summary>
        /// Adds a damping force on rotation.
        /// </summary>
        /// <param name="torque"> Container for torque calculation </param>
        public virtual void RotationalDamping(ref Vector3 torque)
        {
            var tempVel = GetComponent<Rigidbody>().angularVelocity;

            torque += -AngularDamping * tempVel;//nextState.angularVelocity * nextState.mass;
        }

        public void SetAwake(bool awake)
        {
            if (awake)
            {
                isAwake = true;
                GetComponent<Rigidbody>().WakeUp();
                motion = MainProgram.SLEEP_EPSILON * 2f;
            }
            else if(canSleep)
            {
                isAwake = false;
                nextState.velocity = Vector3.zero;
                nextState.angularVelocity = Vector3.zero;

                lastState.velocity = Vector3.zero;
                lastState.angularVelocity = Vector3.zero;

                GetComponent<Rigidbody>().Sleep();//  .velocity = Vector3.zero;
                //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                
            }
        }

        public void UpdateMotion()
        {
            var body = GetComponent<Rigidbody>();
            var currentMotion = Vector3.Dot(body.velocity, body.velocity) + Vector3.Dot(body.angularVelocity, body.angularVelocity);

            var bias = Mathf.Pow(0.5f, MainProgram._timeStep);

            motion = bias * motion + (1f - bias) * currentMotion;
            if (motion > 10 * MainProgram.SLEEP_EPSILON) motion = 10 * MainProgram.SLEEP_EPSILON;

            if (motion < MainProgram.SLEEP_EPSILON)
                SetAwake(false);
        }
    }
}
