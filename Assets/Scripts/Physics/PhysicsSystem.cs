using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Physics
{
    /// <summary>
    /// Calls PositionPhysics and RotationPhysics.
    /// </summary>
    internal class PhysicsSystem
    {
        public void IntegratePhysics(float dt, List<GameObject> cubes)
        {
            for (int i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                var controller = cube.GetComponent<ObjectController>();

                if (!controller.isAwake) continue;

                // Generate forces for next frame
                Vector3 force = Vector3.zero;
                controller.LinearForces(ref force);

                // Generate torque for next frame
                Vector3 torque = Vector3.zero;
                controller.RotationForces(ref torque);

                // Add forces to accumulator
                controller.AddForce(force);
                controller.AddTorque(torque);

                PositionPhysics.IntegrateRK4(controller.accumulatedLinearForce, controller.accumulatedAngularForce, controller.nextState, dt);
            }
        }
    }
}