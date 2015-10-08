using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class BroadPhase
    {
        //private SpatialGrid grid = new SpatialGrid(20, 20, 10, 60, 60, 60);
        //private Octree grid = new Octree(60, 60, 60, 0);
        
        public void PerformPhase(List<GameObject> cubes, ref List<GameObject[]> pairs, ref Octree grid)
        {
            grid.Setup();
            foreach (var cube in cubes)
            {
                //grid.InsertEntity(cube);
                grid.cubeHasMoved(cube);
                //grid.addCube(cube);
            }
            grid.CollideAll(ref pairs);
        }
    }
}