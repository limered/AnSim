﻿using UnityEngine;

namespace Assets.Scripts.Physics
{
    /// <summary>
    /// Contains position and rotation information for an object. See "gaffer on games", "Ultrapede".
    /// </summary>
    public class State
    {
        public Vector3 position;                // Physical Position of Object
        public Vector3 momentum;                // Current Momentum in kg*m/s

        public Quaternion orientation;          // Current Orientation of Physics Object
        public Vector3 angularMomentum;         // Current angular momentum

        public Vector3 velocity;                // Calculated velocity of object (m/s)
        public Quaternion spin;                 // Quaternion rate of change in orientation.
        public Vector3 angularVelocity;         // Velocity of Angular change

        public float mass;                      // Mass of object
        public float inverseMass;
        public Vector3 inertiaTensor;           // Inertia Tensor of object (We use only cubes in physics sim, so only one value)
        public Vector3 inverseInertiaTensor;

        public State(Vector3 startPosition, Quaternion startOriantation, float mass, Vector3 inertiaTensor)
        {
            position = startPosition;
            orientation = startOriantation;
            this.mass = mass;
            inverseMass = 1.0f / mass;
            this.inertiaTensor = inertiaTensor;
            inverseInertiaTensor = Vector3.zero;
            for (int i = 0; i < 3; i++) inverseInertiaTensor[i] = 1.0f / inertiaTensor[i];
        }

        /// <summary>
        /// Calculates a new velocity from momentum and mass.
        /// </summary>
        public void RecalculatePosition()
        {
            velocity = momentum * inverseMass;
        }

        /// <summary>
        /// Calculates new secondary oriantation variables from angular velocity and inertia tensor.
        /// </summary>
        public void RecalculateRotation()
        {
            angularVelocity = Vector3.Cross(angularMomentum, inverseInertiaTensor);
            AnSimMath.NormalizeQuaternion(orientation);
            spin = AnSimMath.QuatScale(new Quaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0) * orientation, 0.5f);
        }

        /// <summary>
        /// Generates a exact clone of this instance.
        /// </summary>
        /// <returns>A replicated state instance</returns>
        public State Clone()
        {
            var clone = new State(position, orientation, mass, inertiaTensor);
            clone.momentum = momentum;
            clone.angularMomentum = angularMomentum;
            clone.RecalculatePosition();
            clone.RecalculateRotation();
            return clone;
        }
    }
}