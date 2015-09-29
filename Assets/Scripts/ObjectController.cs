using Assets.Scripts.Collisions;
using Assets.Scripts.Physics;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Base class for all physics / collision based objects in simulation.
    /// </summary>
    internal abstract class ObjectController : MonoBehaviour
    {
        public GameObject program;

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

        public bool isPlayer;

        /// <summary>
        /// Instantiates physics and collision components of this object.
        /// </summary>
        private void Start()
        {
            if (isPlayer)
            {
                var stat = program.GetComponent<Statics>();
                Mass = stat.PlayerMass;
                LinearDamping = stat.PlayerLinearDamping;
                AngularDamping = stat.PlayerAngularDamping;
                Restitution = stat.PlayerBounce;
                Friction = stat.PlayerFriction;
                canSleep = stat.PlayerCanSleep;
                ((BigCubeController)this).MovementSpeed = stat.PlayerSpeed;
            }
            else
            {
                var stat = program.GetComponent<Statics>();
                Mass = stat.BoxesMass;
                LinearDamping = stat.BoxesLinearDamping;
                AngularDamping = stat.BoxesAngularDamping;
                Restitution = stat.BoxesBounce;
                Friction = stat.BoxesFriction;
                canSleep = stat.BoxesCanSleep;
            }

            Mass = (Mass <= 0) ? 1 : Mass;

            var transform = GetComponent<Transform>();
            Vector3 inertiaTensor = new Vector3(Mass * (transform.localScale.y * transform.localScale.y + transform.localScale.z * transform.localScale.z / 12),
                Mass * (transform.localScale.x * transform.localScale.x + transform.localScale.z * transform.localScale.z / 12),
                Mass * (transform.localScale.x * transform.localScale.x + transform.localScale.y * transform.localScale.y / 12));

            lastState = new State(transform.position, transform.rotation, Mass, inertiaTensor);
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
            force.y += MainProgram.GRAVITY * nextState.mass;
        }

        /// <summary>
        /// Adds a force for damping of linear movement of this object (i.e. Reibung der Luft)
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public virtual void MovementDamping(ref Vector3 force)
        {
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

        /// <summary>
        /// Sets this object alive or not.
        /// </summary>
        /// <param name="awake"> awake or not </param>
        public void SetAwake(bool awake)
        {
            if (awake)
            {
                isAwake = true;
                GetComponent<Rigidbody>().WakeUp(); //Temp
                motion = MainProgram.SLEEP_EPSILON * 2f;    // Set motion to 2*Sleep epsilos, so it doesn't sleep again instantly
            }
            else if (canSleep)
            {
                isAwake = false;
                // Clear states, so it doesn't move anymore
                nextState.velocity = Vector3.zero;
                nextState.angularVelocity = Vector3.zero;
                lastState.velocity = Vector3.zero;
                lastState.angularVelocity = Vector3.zero;

                // Temp for uniry
                GetComponent<Rigidbody>().Sleep(); //temp //  .velocity = Vector3.zero;
                                                   //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Updates the motion variable and determines if this object should sleep
        /// </summary>
        public void UpdateMotion()
        {
            var body = GetComponent<Rigidbody>();   // Temp
            var currentMotion = Vector3.Dot(body.velocity, body.velocity) + Vector3.Dot(body.angularVelocity, body.angularVelocity);

            var bias = Mathf.Pow(0.5f, MainProgram.TIMESTEP);

            motion = bias * motion + (1f - bias) * currentMotion;   // Durchschnitt über die letzten bewegungszahnel

            if (motion > 10 * MainProgram.SLEEP_EPSILON) motion = 10 * MainProgram.SLEEP_EPSILON;   // Begrenze bewegung nach oben

            // CHeck if should sleep
            if (motion < MainProgram.SLEEP_EPSILON)
                SetAwake(false);
        }
    }
}