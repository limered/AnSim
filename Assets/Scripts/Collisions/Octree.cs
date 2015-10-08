using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Collisions;

public class Octree
{
    private const int MaxOctreeDepth = 6;
    private const int MinCubesPerOctree = 3;
    private const int MaxCubesPerOctree = 6;

    // cell size of octree
    private int AmountOfCellsInX, AmountOfCellsInY, AmountOfCellsInZ;

    // corners of octree
    private int minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0, cx = 0, cy = 0, cz = 0;

    // cellSizeX/Y/Z = the real UnityPixel size of each of the 8 grid-cubes inside the octree
    private float cellSizeX, cellSizeY, cellSizeZ, cellSizeXInv, cellSizeYInv, cellSizeZInv;
    private int centerX, centerY, centerZ;

    // the stored cubes if this node has no node children
    private List<GameObject> storedCubes;

    // all cubes, also from children
    private int countAllCubes;

    // depth of the node
    int depth = 0;

    // for logging
    private int counter = 0;

    // if node has node children
    private bool hasNodeChildren;

    //private List<GameObject>[,,] allColumns;
    // childNodes are the 8 Members of the Octree
    private Octree[,,] childNodes;
    //private List<GameObject>[,,] children;

    // only available if there are no childNodes
    public List<GameObject> allCubes;

    // dictionary of all Collision cube pairs
    private Dictionary<string, bool> knownCollisions;

    /// <summary>
    /// 
    /// unitsX/B/H = UnityUnits available for this octree
    /// </summary>
    /// <param name="unitsX"></param>
    /// <param name="unitsY"></param>
    /// <param name="unitsZ"></param>
    /// <param name="depth"></param>
    public Octree(int unitsX, int unitsY, int unitsZ, int depth)
    {
        this.depth = depth;
        hasNodeChildren = false;
        countAllCubes = 0;

        // 2 cells in each direction = 8 cells = octree
        AmountOfCellsInX = 2;
        AmountOfCellsInY = 2;
        AmountOfCellsInZ = 2;

        cellSizeX = unitsX / AmountOfCellsInX;
        //Debug.Log("cellSizeX: " + cellSizeX);
        cellSizeXInv = 1f / cellSizeX;

        cellSizeZ = unitsY / AmountOfCellsInY;
        //Debug.Log("cellSizeZ: " + cellSizeZ);
        cellSizeZInv = 1f / cellSizeZ;

        cellSizeY = unitsZ / AmountOfCellsInZ;
        //Debug.Log("cellSizeY: " + cellSizeY);
        cellSizeYInv = 1f / cellSizeY;

        //allColumns = new List<GameObject>[AmountOfCellsInX, AmountOfCellsInY, AmountOfCellsInZ];
        //children = new List<GameObject>[AmountOfCellsInX, AmountOfCellsInY, AmountOfCellsInZ];
        childNodes = new Octree[AmountOfCellsInX, AmountOfCellsInY, AmountOfCellsInZ];

        allCubes = new List<GameObject>();

        //knownCollisions = new Dictionary<string, bool>();

        //Debug.Log("create Depth of Octree: " + depth);
        //Debug.Log("allCubes: " + allCubes.Count);
        //Debug.Log("countAllCubes: " + countAllCubes);
    }

    /// <summary>
    /// Clears all needed variables from last calculation
    /// </summary>
    public void Setup()
    {
        knownCollisions = new Dictionary<string, bool>();
    }

    /// <summary>
    /// Either adds a Cube or removes it from its children
    /// </summary>
    public void InsertEntity(GameObject entity, bool addCube)
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
        maxX = maxX > AmountOfCellsInX - 1 ? AmountOfCellsInX - 1 : maxX;
        maxY = maxY > AmountOfCellsInZ - 1 ? AmountOfCellsInZ - 1 : maxY;
        maxZ = maxZ > AmountOfCellsInY - 1 ? AmountOfCellsInY - 1 : maxZ;

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
                    //if (childNodes[cx, cy, cz] == null)
                    //    children[cx, cy, cz] = new List<GameObject>();

                    if (addCube)
                    {
                        //children[cx, cy, cz].Add(entity);
                        childNodes[cx, cy, cz].addCube(entity);
                    }
                    else
                    {
                        //children[cx, cy, cz].Remove(entity);
                        childNodes[cx, cy, cz].removeCube(entity);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates new childNodes (Octrees) and sort the current cubes into their leafes.
    /// </summary>
    public void addNewChildNodes()
    {
        for (int x = 0; x < AmountOfCellsInX; x++)
        {
            for (int y = 0; y < AmountOfCellsInY; y++)
            {
                for (int z = 0; z < AmountOfCellsInZ; z++)
                {
                    // create new Octree
                    // TODO: is the Dimension right?
                    var newDepth = depth+1;
                    //Debug.Log("NEW Depth is called again: " + newDepth);
                    childNodes[x, y, z] = new Octree(Mathf.RoundToInt(cellSizeX), Mathf.RoundToInt(cellSizeY), Mathf.RoundToInt(cellSizeZ), newDepth);

                    // Remove all cubes from the children List for this node and add them to the new Octree Node
                    //List<GameObject> cell = children[x, y, z];
                    //for (int i = 0; i < all.Count; i++)
                    //{
                    //    childNodes[x, y, z].InsertEntity(cell[i], true);
                    //}
                    // Remove all cubes from the allCubes List for this node and add them to the new Octree Node
                    // delete all children from current root Node
                    //children[x, y, z].Clear();
                }
            }
        }


        // Remove all cubes from the allCubes List for this node and add them to the new Octree Node
        for (int i = 0; i < allCubes.Count; i++)
        {
            InsertEntity(allCubes[i], true);
        }
        //Debug.Log("#### delete all cubes! " + allCubes.Count);
        // delete all children from current root Node
        allCubes.Clear();
        //Debug.Log("#### delete all cubes 2! " + allCubes.Count);
        hasNodeChildren = true;
    }
    /// <summary>
    /// Gets all cubes for all nodes of this octree and add them to this or one of its descendants
    /// </summary>
    public void getAllChildrenNodeCubes(ref List<GameObject> allChildren)
    {
        if (hasNodeChildren)
        {
            for (int x = 0; x < AmountOfCellsInX; x++)
            {
                for (int y = 0; y < AmountOfCellsInY; y++)
                {
                    for (int z = 0; z < AmountOfCellsInZ; z++)
                    {
                        childNodes[x, y, z].getAllChildrenNodeCubes(ref allChildren);
                    }
                }
            }
        }
        else
        {
            // if leaf is reached, take all node Elements and add them to the allChildren List
            //for (int x = 0; x < AmountOfCellsInX; x++)
            //{
            //    for (int y = 0; y < AmountOfCellsInY; y++)
            //    {
            //        for (int z = 0; z < AmountOfCellsInZ; z++)
            //        {
            //List<GameObject> cell = children[x, y, z];

            //for (int i = 0; i < cell.Count; i++)
            //{
            //    allChildren.Add(cell[i]);
            //}

            for (int i = 0; i < allCubes.Count; i++)
            {
                allChildren.Add(allCubes[i]);
            }
            //}
            //        }
            //    }
        }
    }

    /// <summary>
    /// Destroys all Child Nodes of this Octree and moves its children (cubes) to itself
    /// </summary>
    public void destroyAllChildNodes()
    {
        // reorganizes all cubes from the childNodes to this root Node
        getAllChildrenNodeCubes(ref allCubes);

        // delete all OCtree Children
        //for (int x = 0; x < AmountOfCellsInX; x++)
        //{
        //    for (int y = 0; y < AmountOfCellsInY; y++)
        //    {
        //        for (int z = 0; z < AmountOfCellsInZ; z++)
        //        {
        //            // TODO: Necessary?
        //            //children[x, y, z] = null;
        //            childNodes[x, y, z].allCubes.Clear();
        //            childNodes[x, y, z] = null;
        //        }
        //    }
        //}

        // now this node doesn't have any child nodes anymore
        hasNodeChildren = false;
    }

    public void removeCube(GameObject entity)
    {
        countAllCubes = countAllCubes - 1;

        if (hasNodeChildren && countAllCubes < MinCubesPerOctree)
        {
            destroyAllChildNodes();
        }

        if (hasNodeChildren)
        {
            InsertEntity(entity, false);
        }
        else
        {
            allCubes.Remove(entity);
        }
    }

    public void addCube(GameObject entity)
    {
        countAllCubes = countAllCubes + 1;

        if (!hasNodeChildren && depth < MaxOctreeDepth && countAllCubes > MaxCubesPerOctree)
        {
            Debug.Log("addNewChildNodes: " + depth);
            addNewChildNodes();
        }

        if (hasNodeChildren)
        {
            InsertEntity(entity, true);
        }
        else
        {
            allCubes.Add(entity);
        }
    }

    public void cubeHasMoved(GameObject cube)
    {
        removeCube(cube);
        // TODO: higher complexity for even better results: check here in which tree it was before and is now respectively
        addCube(cube);
    }

    /// <summary>
    /// Checks if there are possible collision in a Node/Leaf and returns candidate pairs for collisions.
    /// </summary>
    /// <returns> A List of pairs of enzities that could collide </returns>
    public void CollideAll(ref List<GameObject[]> pairs)
    {
        //List<GameObject[]> pairs = new List<GameObject[]>();
        GameObject[] body = new GameObject[2];

        if (hasNodeChildren)
        {
            for (int x = 0; x < AmountOfCellsInX; x++)
            {
                for (int y = 0; y < AmountOfCellsInY; y++)
                {
                    for (int z = 0; z < AmountOfCellsInZ; z++)
                    {
                        childNodes[x, y, z].CollideAll(ref pairs);
                    }
                }
            }
        }
        else
        {
            //for (int x = 0; x < AmountOfCellsInX; x++)
            //{
            //    for (int y = 0; y < AmountOfCellsInY; y++)
            //    {
            //        for (int z = 0; z < AmountOfCellsInZ; z++)
            //        {

            for (int k = 0; k < allCubes.Count; k++)
            {
                body[0] = allCubes[k];
                for (int l = 0; l < allCubes.Count; l++)
                {
                    body[1] = allCubes[l];
                    // same objects
                    if (body[0] == body[1]) continue;

                    var awake0 = body[0].GetComponent<ObjectController>().isAwake;
                    var awake1 = body[1].GetComponent<ObjectController>().isAwake;

                    // both objects need to be awake
                    if (!awake0 && !awake1) continue;

                    // if they already collided for this calculation skip them
                    //if (knownCollisions.ContainsKey(body[0].GetInstanceID() + "_" + body[1].GetInstanceID()))
                    //    continue;
                    if (k > l)
                    {
                        continue;
                    }

                    GameObject[] pair = new GameObject[2];
                    pair[0] = body[0];
                    pair[1] = body[1];
                    pairs.Add(pair);
                    //knownCollisions.Add(body[0].GetInstanceID() + "_" + body[1].GetInstanceID(), true);
                }
            }
            //}
            //    }
            //}
        }
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
