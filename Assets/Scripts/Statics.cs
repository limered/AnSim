using UnityEngine;

namespace Assets.Scripts
{
    internal class Statics : MonoBehaviour
    {
        public float WallBounce;                // Bounciness of walls/floor
        public float WallFriction;              // Friction of Walls Floor
        public float WallPenetrationPenalty;    // Spring parameter
        public float WallPenetrationDamping;    // Spring parameter
        public float WallElastic;               // Overlap elasticity of walls
        public bool WallIsWater;                // Turns walls into water ( turns off overlap correction )

        public Vector2 BoxesX;                  // X: start position, Y: count in direction
        public Vector2 BoxesY;
        public Vector2 BoxesZ;
        public float BoxesDistance;             // Distance between boxes
        public bool BoxesStartAwake;            // If boxes are awake on start

        public float BoxesMass;
        public float BoxesLinearDamping;
        public float BoxesAngularDamping;
        public float BoxesBounce;
        public float BoxesFriction;
        public bool BoxesCanSleep;

        public float PlayerMass;
        public float PlayerLinearDamping;
        public float PlayerAngularDamping;
        public float PlayerBounce;
        public float PlayerFriction;
        public bool PlayerCanSleep;
        public float PlayerSpeed;
        public float PlayerAffectingRadius;
        public float PlayerPushForce;
        public bool PlayerCollision;
    }
}