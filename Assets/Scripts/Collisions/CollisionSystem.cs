using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Calculates collisions for each object. Siehe "gaffer on games" und "Ultrapede"
    /// </summary>
    internal class CollisionSystem
    {
        public static float overlapElasticity = 0.2f;

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

            //GameObject cube0;
            //GameObject cube1;
            //for (var i = 0; i < cubes.Count; i++)
            //{
            //    cube0 = cubes[i];

            //    if(cube0.GetComponent<ObjectController>().isAwake)
            //        WallCollisionSolver.CollideWithWalls(ref cube0, walls);

            //    for (var j = 0; j < cubes.Count; j++)
            //    {
            //        cube1 = cubes[j];
            //        if (cube0 == cube1) continue;
            //        if (!cube0.GetComponent<ObjectController>().isAwake && !cube1.GetComponent<ObjectController>().isAwake) continue;
            //        var collision = _Collide(cube0, cube1, dt);
            //        if (collision)
            //        {
            //            ContactGenerator.ComputeCollisionInfo(ref coll);
            //            _CalculateCollisionResponse(cube0, cube1);

            //            _ChangeColor(cube0, cube1);
            //        }
            //    }
            //}
        }

        private void _CollideWithWalls(List<GameObject> cubes, GameObject[] walls)
        {
            GameObject cube;
            foreach (GameObject go in cubes)
            {
                cube = go;
                if (cube.GetComponent<ObjectController>().isAwake)
                    WallCollisionSolver.CollideWithWalls(ref cube, walls);
            }
        }

        private void _ChangeColor(GameObject cube)
        {
            var script = cube.GetComponent<SmallCubeController>();
            if (script != null)
            {
                script.ChangeColor(Time.realtimeSinceStartup);
            }

            //// Awake state
            //var script2 = cube0.GetComponent<ObjectController>();
            //var script3 = cube1.GetComponent<ObjectController>();

            //bool body0Awake = script2.isAwake;
            //bool body1Awake = script3.isAwake;

            //if (body0Awake ^ body1Awake)
            //{
            //    if (body0Awake) script3.SetAwake(true);
            //    else script2.SetAwake(true);
            //}
        }
    }
}