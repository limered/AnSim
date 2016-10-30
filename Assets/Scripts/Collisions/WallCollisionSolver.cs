using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Lets cube collide with all walls
    /// </summary>
    internal class WallCollisionSolver
    {
        private float c;
        private float f;
        private float k;
        private float b;
        private float elasticity;
        private bool isWater;

        /// <summary>
        /// Starts collision detection with all five walls.
        /// </summary>
        /// <param name="cube"> Cube to collide </param>
        /// <param name="walls"> Array containing all walls </param>
        public void CollideWithWalls(ref GameObject cube, GameObject[] walls)
        {
            _SetVariables(walls[0]);

            for (int i = 0; i < walls.Length; i++)
            {
                _CollideCubeWithWall(ref cube, walls[i]);
            }
        }

        /// <summary>
        /// Sets the needed variables for calculation
        /// </summary>
        /// <param name="wall"></param>
        private void _SetVariables(GameObject wall)
        {
            var statics = wall.GetComponent<ObjectController>().program.GetComponent<Statics>();
            c = statics.WallBounce;
            f = statics.WallFriction;
            k = statics.WallPenetrationPenalty;
            b = statics.WallPenetrationDamping;
            elasticity = statics.WallElastic;
            isWater = statics.WallIsWater;
        }

        /// <summary>
        /// Lets a cube collide with a wall/floor
        /// </summary>
        /// <param name="cube"></param>
        /// <param name="wall"></param>
        private void _CollideCubeWithWall(ref GameObject cube, GameObject wall)
        {
            var position = cube.GetComponent<Transform>().position;
            var controller = cube.GetComponent<ObjectController>();
            var collision = controller.anSimCollider;
            var rigidbody = controller.nextState;

            var wallController = wall.GetComponent<WallController>();

            Matrix3 inertia = new Matrix3();
            inertia.SetDiagonal(rigidbody.inertiaTensor);

            Vector3 force = Vector3.zero;
            Vector3 torque = Vector3.zero;
            float maxPenetration = float.MinValue;

            for (int z = -1; z <= 1; z += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int x = -1; x <= 1; x += 2)
                    {
                        Vector3 point = position + collision.axis[0] * x * collision.extents[0] +
                            collision.axis[1] * y * collision.extents[1] +
                            collision.axis[2] * z * collision.extents[2];
                        _CollidePointWithPlane(point, position, rigidbody.angularVelocity, rigidbody.velocity, wallController, ref force, ref torque, ref maxPenetration);
                    }
                }
            }

            if (!isWater && maxPenetration > 0)
            {
                rigidbody.position += wallController.normal * maxPenetration * elasticity;
                controller.AddForce(force, false);
                controller.AddTorque(torque);
            }
            else
            {
                controller.AddForce(force, false);
                controller.AddTorque(torque);
            }
        }

        /// <summary>
        /// Walls are infinite planes with a position (coded in wallConstant) and a normal.
        /// </summary>
        /// <param name="p"> collision point (a vertex of the cube) </param>
        /// <param name="center">Center of cube object</param>
        /// <param name="aV"> angular velocity of cube </param>
        /// <param name="v"> velocity of cube </param>
        /// <param name="plane"> the wall </param>
        /// <param name="force">parameter to accumulate forces </param>
        /// <param name="torque">parameter to accumulate torque</param>
        /// <param name="maxPenetration"> maximal penetration depth to force the cube out of the plane </param>
        private void _CollidePointWithPlane(Vector3 p, Vector3 center, Vector3 aV, Vector3 v, WallController plane, ref Vector3 force, ref Vector3 torque, ref float maxPenetration)
        {
            //Penetration of point into plane
            float penetration = plane.wallConstant - Vector3.Dot(p, plane.normal);

            if (penetration <= 0) return;

            //save maximum penetration depth
            if (penetration > maxPenetration)
                maxPenetration = penetration;

            // velocity in point
            Vector3 velocity = Vector3.Cross(aV, p - center) + v;
            // relative velocity between point and plane
            float relativeSpeed = Vector3.Dot(plane.normal, velocity);

            if (relativeSpeed > 0)
            {
                // bouncyness
                Vector3 collisionForce = plane.normal * (relativeSpeed * c);
                force += collisionForce;
                torque += Vector3.Cross(p - center, collisionForce);
            }

            // friction
            Vector3 tangentialVelocity = velocity + (plane.normal * relativeSpeed);
            Vector3 frictionForce = -tangentialVelocity * f;
            force += frictionForce;
            torque += Vector3.Cross(p - center, frictionForce);

            // stiffness of spring
            Vector3 penaltyForce = plane.normal * (penetration * k);
            force += penaltyForce;
            torque += Vector3.Cross(p - center, penaltyForce);

            Vector3 dampingForce = plane.normal * (relativeSpeed * penetration * b);
            force += dampingForce;
            torque += Vector3.Cross(p - center, dampingForce);
        }
    }
}