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

        public bool hasGravity;

        public Vector3 accumulatedLinearForce;
        public Vector3 accumulatedAngularForce;
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
                hasGravity = stat.PlayerHasGravity;
                LinearDamping = stat.PlayerLinearDamping;
                AngularDamping = stat.PlayerAngularDamping;
                Restitution = stat.PlayerBounce;
                Friction = stat.PlayerFriction;
                canSleep = stat.PlayerCanSleep;
                ((BigCubeController)this).MovementSpeed = stat.PlayerSpeed;
                ((BigCubeController)this).AffectingRadius = stat.PlayerAffectingRadius;
                ((BigCubeController)this).PushForce = stat.PlayerPushForce;
                ((BigCubeController)this).Collision = stat.PlayerCollision;
            }
            else
            {
                var stat = program.GetComponent<Statics>();
                Mass = stat.BoxesMass;
                hasGravity = stat.BoxesHaveGravity;
                LinearDamping = stat.BoxesLinearDamping;
                AngularDamping = stat.BoxesAngularDamping;
                Restitution = stat.BoxesBounce;
                Friction = stat.BoxesFriction;
                canSleep = stat.BoxesCanSleep;
            }

            Mass = (Mass <= 0) ? 1 : Mass;

            var transform = GetComponent<Transform>();
            Vector3 inertiaTensor = new Vector3(Mass * ((transform.localScale.y * transform.localScale.y + transform.localScale.z * transform.localScale.z) / 12),
                Mass * ((transform.localScale.x * transform.localScale.x + transform.localScale.z * transform.localScale.z) / 12),
                Mass * ((transform.localScale.x * transform.localScale.x + transform.localScale.y * transform.localScale.y) / 12));

            inertiaTensor *= 5;

            lastState = new State(transform.position, transform.rotation, Mass, inertiaTensor);
            nextState = lastState.Clone();
            anSimCollider = new OrientedBox3D();
            anSimCollider.UpdateDataFromObject(gameObject);
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
            if (hasGravity)
                force.y += MainProgram.GRAVITY * nextState.mass * 0.6f;
        }

        /// <summary>
        /// Adds a force for damping of linear movement of this object (i.e. Reibung der Luft)
        /// </summary>
        /// <param name="force"> Container for force calculation </param>
        public virtual void MovementDamping(ref Vector3 force)
        {
            force -= LinearDamping * nextState.velocity;
        }

        /// <summary>
        /// Adds a damping force on rotation.
        /// </summary>
        /// <param name="torque"> Container for torque calculation </param>
        public virtual void RotationalDamping(ref Vector3 torque)
        {
            torque -= AngularDamping * nextState.angularVelocity;
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
                // Set motion to 2*Sleep epsilos, so it doesn't sleep again instantly
                motion = MainProgram.SLEEP_EPSILON * 3f;
            }
            else if (canSleep)
            {
                isAwake = false;
                //Clear states, so it doesn't move anymore
                nextState.velocity = Vector3.zero;
                nextState.angularVelocity = Vector3.zero;

                nextState.RecalculatePosition();
                nextState.RecalculateRotation();

                lastState = nextState.Clone();

                ClearForces();
            }
        }

        /// <summary>
        /// Updates the motion variable and determines if this object should sleep
        /// </summary>
        public void UpdateMotion()
        {
            var body = nextState;
            var currentMotion = Vector3.Dot(body.velocity, body.velocity) + Vector3.Dot(body.angularVelocity, body.angularVelocity);

            var bias = Mathf.Pow(0.5f, MainProgram.TIMESTEP);

            motion = bias * motion + (1f - bias) * currentMotion;   // Durchschnitt über die letzten bewegungszajlen

            if (motion > 10 * MainProgram.SLEEP_EPSILON) motion = 10 * MainProgram.SLEEP_EPSILON;   // Begrenze bewegung nach oben

            // CHeck if should sleep
            if (motion < MainProgram.SLEEP_EPSILON)
                SetAwake(false);
        }

        /// <summary>
        /// Adds a force to the next calculation
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            accumulatedLinearForce += force;
        }

        /// <summary>
        /// Adds a torque to the next calculation
        /// </summary>
        /// <param name="torque"></param>
        public void AddTorque(Vector3 torque)
        {
            accumulatedAngularForce += torque;
        }

        /// <summary>
        /// Clears all accumulated forces
        /// </summary>
        public void  ClearForces()
        {
            lastFrameAcceleration = accumulatedLinearForce * nextState.inverseMass;

            accumulatedLinearForce = Vector3.zero;
            accumulatedAngularForce = Vector3.zero;
        }
    }
}