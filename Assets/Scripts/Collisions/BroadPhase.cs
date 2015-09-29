using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class BroadPhase
    {
        private SpatialGrid grid = new SpatialGrid(20, 20, 10, 60, 60, 60);

        public List<GameObject[]> PerformPhase(List<GameObject> cubes)
        {
            grid.Setup();
            foreach (var cube in cubes)
            {
                grid.InsertEntity(cube);
            }
            return grid.CollideAll();
        }
    }
}