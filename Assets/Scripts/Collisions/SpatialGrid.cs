using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    ///  A 3d Grid to house all collisions
    /// </summary>
    internal class SpatialGrid
    {
        private int W, B, H, total;
        private float cellSizeX, cellSizeY, cellSizeZ, cellSizeXInv, cellSizeYInv, cellSizeZInv;

        private List<GameObject>[,,] allColumns;
        private List<int> occupiedCells;

        private Dictionary<string, bool> knownCollisions;

        private int minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0, cx = 0, cy = 0, cz = 0;

        public SpatialGrid(int w, int b, int h, int pixelW, int pixelB, int pixelH)
        {
            W = w;
            B = b;
            H = h;
            total = w * h * b;

            cellSizeX = pixelW / w;
            cellSizeXInv = 1f / cellSizeX;

            cellSizeZ = pixelB / b;
            cellSizeZInv = 1f / cellSizeZ;

            cellSizeY = pixelH / h;
            cellSizeYInv = 1f / cellSizeY;

            allColumns = new List<GameObject>[w, h, b];
            occupiedCells = new List<int>();
            knownCollisions = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Clears all needed variables from last calculation
        /// </summary>
        public void Setup()
        {
            allColumns = new List<GameObject>[W, H, B];
            occupiedCells = new List<int>();
            knownCollisions = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Inserts an object in the correct cells. An object can be in more than one cell.
        /// </summary>
        /// <param name="entity"></param>
        public void InsertEntity(GameObject entity)
        {
            var collider = entity.GetComponent<ObjectController>().anSimCollider;
            if (collider == null) return;
            var state = entity.GetComponent<ObjectController>().nextState;
            if (state == null) return;

            // Get Min Max values
            minX = (int)(_ProjectPosition(Vector3.right, collider, -1) * cellSizeXInv);
            minY = (int)(_ProjectPosition(Vector3.up, collider, -1) * cellSizeYInv);
            minZ = (int)(_ProjectPosition(Vector3.forward, collider, -1) * cellSizeZInv);
            maxX = (int)(_ProjectPosition(Vector3.right, collider, 1) * cellSizeXInv);
            maxY = (int)(_ProjectPosition(Vector3.up, collider, 1) * cellSizeYInv);
            maxZ = (int)(_ProjectPosition(Vector3.forward, collider, 1) * cellSizeZInv);

            // make sure all is in bounds and min <= max
            if (maxX < minX) { var temp = maxX; maxX = minX; minX = temp; }
            if (maxY < minY) { var temp = maxY; maxY = minY; minY = temp; }
            if (maxZ < minZ) { var temp = maxZ; maxZ = minZ; minZ = temp; }

            minX = minX < 0 ? 0 : minX;
            minZ = minZ < 0 ? 0 : minZ;
            minY = minY < 0 ? 0 : minY;
            maxX = maxX > W - 2 ? W - 2 : maxX;
            maxY = maxY > H - 2 ? H - 2 : maxY;
            maxZ = maxZ > B - 2 ? B - 2 : maxZ;

            for (cx = minX; cx <= maxX; cx++)
            {
                for (cy = minY; cy <= maxY; cy++)
                {
                    for (cz = minZ; cz <= maxZ; cz++)
                    {
                        if (allColumns[cx, cy, cz] == null)
                            allColumns[cx, cy, cz] = new List<GameObject>();
                        allColumns[cx, cy, cz].Add(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if there are possible collision in a cell and returns candidate pairs for collisions.
        /// </summary>
        /// <returns> A List of pairs of enzities that could collide </returns>
        public List<GameObject[]> CollideAll()
        {
            List<GameObject[]> pairs = new List<GameObject[]>();
            GameObject[] body = new GameObject[2];
            List<GameObject> cell;

            for (int x = 0; x < W; x++)
            {
                for (int y = 0; y < H; y++)
                {
                    for (int z = 0; z < B; z++)
                    {
                        if (allColumns[x, y, z] == null || allColumns[x, y, z].Count <= 1) continue;
                        cell = allColumns[x, y, z];

                        for (int k = 0; k < cell.Count; k++)
                        {
                            body[0] = cell[k];
                            for (int l = 0; l < cell.Count; l++)
                            {
                                body[1] = cell[l];
                                if (body[0] == body[1]) continue;
                                var awake0 = body[0].GetComponent<ObjectController>().isAwake;
                                var awake1 = body[1].GetComponent<ObjectController>().isAwake;
                                if (!awake0 && !awake1) continue;
                                if (knownCollisions.ContainsKey(body[0].GetInstanceID() + "_" + body[1].GetInstanceID())) continue;
                                GameObject[] pair = new GameObject[2];
                                pair[0] = body[0];
                                pair[1] = body[1];
                                pairs.Add(pair);
                                knownCollisions.Add(body[0].GetInstanceID() + "_" + body[1].GetInstanceID(), true);
                            }
                        }
                    }
                }
            }
            return pairs;
        }

        /// <summary>
        /// Projects the extends of an object onto a axis in one particular direction.
        /// </summary>
        /// <param name="axis"> Axis to project on </param>
        /// <param name="collider"> objects collider </param>
        /// <param name="sign"> direction(1/-1) </param>
        /// <returns> projected t on axis </returns>
        private float _ProjectPosition(Vector3 axis, OrientedBox3D collider, int sign)
        {
            var x = Vector3.Dot(axis, collider.axis[0] * sign * collider.extents[0]);
            var y = Vector3.Dot(axis, collider.axis[1] * sign * collider.extents[1]);
            var z = Vector3.Dot(axis, collider.axis[2] * sign * collider.extents[2]);

            return x + y + z + Vector3.Dot(axis, collider.center);
        }
    }
}