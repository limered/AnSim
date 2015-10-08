using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    ///  A 3d Grid to house all collisions
    /// </summary>
    internal class OctreeGrid
    {
        private int W, B, H, total;

        private const int MaxOctreeDepth = 6;
        private const int MinCubesPerOctree = 3;
        private const int MaxCubesPerOctree = 6;

        private float cellSizeX, cellSizeY, cellSizeZ, cellSizeXInv, cellSizeYInv, cellSizeZInv;

        private List<GameObject>[,,] allColumns;
        private List<int> occupiedCells;

        private Dictionary<string, bool> knownCollisions;

        // determinates which grid-cubes the current Entity is intersecting (contained)
        private int minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0, cx = 0, cy = 0, cz = 0;

        private int counter = 0;

        public OctreeGrid(int w, int b, int h, int pixelW, int pixelB, int pixelH)
        {

            // pixelW/B/H = UnityPixel Volume of our Scene
            // w, b, h = desired amount of grid-cubes in each direction of this container cube inside
            // cellSizeX/Y/Z = the real UnityPixel size of each direction of a single grid-cube inside this volume

             W = w;
            B = b;
            H = h;


            Debug.Log("W: " + W);
            Debug.Log("B: " + B);
            Debug.Log("H: " + H);

            total = w * h * b;

            cellSizeX = pixelW / w;
            //Debug.Log("cellSizeX: " + cellSizeX);
            cellSizeXInv = 1f / cellSizeX;

            cellSizeZ = pixelB / b;
            //Debug.Log("cellSizeZ: " + cellSizeZ);
            cellSizeZInv = 1f / cellSizeZ;

            cellSizeY = pixelH / h;
            //Debug.Log("cellSizeY: " + cellSizeY);
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
            // collider contains all states of object
            var collider = entity.GetComponent<ObjectController>().anSimCollider;
            if (collider == null) return;


            // cellSizeXInv transforms world coordinated projections in grid-cube coordinates
            if (entity.GetComponent<ObjectController>().isPlayer && entity.GetComponent<BigCubeController>().AffectingRadius > 0f)
            {
                var controller = entity.GetComponent<BigCubeController>();
                // neglect the smaller cube size
                minX = Mathf.RoundToInt((collider.center.x - controller.AffectingRadius) * cellSizeXInv);
                maxX = Mathf.RoundToInt(((collider.center.x + controller.AffectingRadius) * cellSizeXInv));
                minY = Mathf.RoundToInt(((collider.center.y - controller.AffectingRadius) * cellSizeYInv));
                maxY = Mathf.RoundToInt(((collider.center.y + controller.AffectingRadius) * cellSizeYInv));
                minZ = Mathf.RoundToInt(((collider.center.z - controller.AffectingRadius) * cellSizeZInv));
                maxZ = Mathf.RoundToInt(((collider.center.z + controller.AffectingRadius) * cellSizeZInv));
            }
            else
            {
                // Get Min Max values
                minX = Mathf.RoundToInt((_ProjectPosition(Vector3.right, collider, -1) * cellSizeXInv));
                minY = Mathf.RoundToInt((_ProjectPosition(Vector3.up, collider, -1) * cellSizeYInv));
                minZ = Mathf.RoundToInt((_ProjectPosition(Vector3.forward, collider, -1) * cellSizeZInv));
                maxX = Mathf.RoundToInt((_ProjectPosition(Vector3.right, collider, 1) * cellSizeXInv));
                maxY = Mathf.RoundToInt((_ProjectPosition(Vector3.up, collider, 1) * cellSizeYInv));
                maxZ = Mathf.RoundToInt((_ProjectPosition(Vector3.forward, collider, 1) * cellSizeZInv));
            }

            // make sure all is in bounds and min <= max
            if (maxX < minX) { var temp = maxX; maxX = minX; minX = temp; }
            if (maxY < minY) { var temp = maxY; maxY = minY; minY = temp; }
            if (maxZ < minZ) { var temp = maxZ; maxZ = minZ; minZ = temp; }

            // fix bounds of grid-cube 
            minX = minX < 0 ? 0 : minX;
            minZ = minZ < 0 ? 0 : minZ;
            minY = minY < 0 ? 0 : minY;
            // 
            maxX = maxX > W - 1 ? W - 1 : maxX;
            maxY = maxY > H - 1 ? H - 1 : maxY;
            maxZ = maxZ > B - 1 ? B - 1 : maxZ;

            counter++;
            //if (counter < 100)
            //{
            //    Debug.Log("minX: " + minX);
            //    Debug.Log("minZ: " + minZ);
            //    Debug.Log("minY: " + minY);
            //    Debug.Log("maxX: " + maxX);
            //    Debug.Log("maxY: " + maxY);
            //    Debug.Log("maxZ: " + maxZ);
            //}

            for (cx = minX; cx <= maxX; cx++)
            {
                for (cy = minY; cy <= maxY; cy++)
                {
                    for (cz = minZ; cz <= maxZ; cz++)
                    {
                        // if no cubes have been insorted in this grid-cell, create a new list of GameObjects
                        if (allColumns[cx, cy, cz] == null)
                            allColumns[cx, cy, cz] = new List<GameObject>();
                        //if (cz == 1 && counter < 100)
                        //{
                        //    Debug.Log("cx: " + cx + " cy: " + cy + " cz: " + cz);
                        //}
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
                        // no collision candidates
                        if (allColumns[x, y, z] == null || allColumns[x, y, z].Count <= 1) continue;

                        cell = allColumns[x, y, z];

                        for (int k = 0; k < cell.Count; k++)
                        {
                            body[0] = cell[k];
                            for (int l = 0; l < cell.Count; l++)
                            {
                                body[1] = cell[l];
                                // same objects
                                if (body[0] == body[1]) continue;

                                var awake0 = body[0].GetComponent<ObjectController>().isAwake;
                                var awake1 = body[1].GetComponent<ObjectController>().isAwake;

                                // both objects need to be awake
                                if (!awake0 && !awake1) continue;
                                
                                // if they already collided for this calculation skip them
                                if (knownCollisions.ContainsKey(body[0].GetInstanceID() + "_" + body[1].GetInstanceID()))
                                    continue;

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
            // project the vector with the given axis to the world axis
            var x = Vector3.Dot(axis, collider.axis[0] * sign * collider.extents[0]);
            var y = Vector3.Dot(axis, collider.axis[1] * sign * collider.extents[1]);
            var z = Vector3.Dot(axis, collider.axis[2] * sign * collider.extents[2]);

            // return the projected value dependent on the world coordinates of the cube
            return x + y + z + Vector3.Dot(axis, collider.center);
        }
    }
}