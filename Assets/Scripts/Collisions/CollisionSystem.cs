using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Calculates collisions for each object. Siehe "gaffer on games" und "Ultrapede"
    /// </summary>
    internal class CollisionSystem
    {
        private WallCollisionSolver wallPhase = new WallCollisionSolver();
        private BroadPhase broadPhase = new BroadPhase();
        private NarrowPhase narrowPhase = new NarrowPhase();

        /// <summary>
        /// Starts the collision calculation process
        /// </summary>
        /// <param name="dt"> timestep </param>
        /// <param name="cubes"> array containing all cubes </param>
        /// <param name="walls"> array containing all walls </param>
        public void CalculateCollisions(float dt, List<GameObject> cubes, GameObject[] walls)
        {
            _CollideWithWalls(cubes, walls);

            var collisions = broadPhase.PerformPhase(cubes);

            var moved = narrowPhase.PerformPhase(collisions);

            foreach (KeyValuePair<int, GameObject> kv in moved)
            {
                _ChangeColor(kv.Value);
            }
        }

        /// <summary>
        /// Collides all awake object with walls
        /// </summary>
        /// <param name="cubes"></param>
        /// <param name="walls"></param>
        private void _CollideWithWalls(List<GameObject> cubes, GameObject[] walls)
        {
            GameObject cube;
            ObjectController objectControl;
            foreach (GameObject go in cubes)
            {
                cube = go;
                objectControl = cube.GetComponent<ObjectController>();
                objectControl.ClearForces();
                if (objectControl.isAwake)
                {
                    UpdateCollider(objectControl, cube);
                    objectControl.lastState = objectControl.nextState.Clone();
                    objectControl.nextState.CalculateDerivedData();
                    objectControl.UpdateMotion();

                    wallPhase.CollideWithWalls(ref cube, walls);
                }
            }
        }

        /// <summary>
        /// Calls the collider update method
        /// </summary>
        /// <param name="objectControl"></param>
        /// <param name="cube"> GameObject instance </param>
        private void UpdateCollider(ObjectController objectControl, GameObject cube)
        {
            objectControl.anSimCollider.UpdateDataFromObject(cube);
        }

        /// <summary>
        /// Changes the color of object, who were in a collision
        /// </summary>
        /// <param name="cube"></param>
        private void _ChangeColor(GameObject cube)
        {
            var script = cube.GetComponent<SmallCubeController>();
            if (script != null)
            {
                script.OnCollision(Time.realtimeSinceStartup);
            }
        }
    }
}