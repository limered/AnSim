using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class BroadPhase
    {
        SpatialGrid grid = new SpatialGrid(20, 20, 40, 60, 60, 120);

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
