﻿using System.Collections.Generic;
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

        private void _CollideWithWalls(List<GameObject> cubes, GameObject[] walls)
        {
            GameObject cube;
            foreach (GameObject go in cubes)
            {
                cube = go;
                if (cube.GetComponent<ObjectController>().isAwake)
                    wallPhase.CollideWithWalls(ref cube, walls);
            }
        }

        private void _ChangeColor(GameObject cube)
        {
            var script = cube.GetComponent<SmallCubeController>();
            if (script != null)
            {
                script.ChangeColor(Time.realtimeSinceStartup);
            }
        }
    }
}