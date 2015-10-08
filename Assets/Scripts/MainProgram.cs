using Assets.Scripts.Collisions;
using Assets.Scripts.Physics;
using Assets.Scripts.Renderer;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class MainProgram : MonoBehaviour
    {
        public static float TIMESTEP = 0.02f;
        static public float GRAVITY = -9.8f;

        public static float POSITION_EPSOLON = 0f;
        public static float SLEEP_EPSILON = 0.7f;
        public static float VELOCITY_EPSILON = 0f;

        public GameObject BigCubePrefab;
        public Camera MainCamera;
        public GameObject SmallCubePrefab;
        public GameObject[] Walls;
        
        // unitsX/Y/Z: size of the game Scene, depth: start octree depth
        public Octree octreeGrid = new Octree(60, 60, 60, 0);

        private float _accumulator = 0;
        private CollisionSystem _collisions = new CollisionSystem();
        private List<GameObject> _cubes = new List<GameObject>();
        private PhysicsSystem _physics = new PhysicsSystem();
        private RenderingSystem _rendering = new RenderingSystem();

        /// <summary>
        /// Adds the Player Cube to the cubes list
        /// </summary>
        private void _AddPlayer()
        {
            BigCubePrefab = GameObject.Find("BigCube");
            _cubes.Add(BigCubePrefab);
        }

        /// <summary>
        /// Adds all the Wall object to list of instances
        /// </summary>
        private void AddWallsToInstancesList()
        {
            for (int i = 0; i < Walls.Length; i++)
            {
                _cubes.Add(Walls[i]);
            }
        }

        /// <summary>
        /// Builds all small cube objects
        /// </summary>
        /// <param name="xStart"> start position in x direction </param>
        /// <param name="xCount"> count in x direction </param>
        /// <param name="yStart"></param>
        /// <param name="yCout"></param>
        /// <param name="zStart"></param>
        /// <param name="zCount"></param>
        /// <param name="step"> distance between cube centers </param>
        /// <param name="startAwake"> if the cubes should start awake </param>
        private void InstantiateSmallCubes(float xStart, int xCount, float yStart, int yCout, float zStart, int zCount, float step, bool startAwake)
        {
            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCout; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        var smallCube = (GameObject)Instantiate(SmallCubePrefab, new Vector3(xStart + x * step, yStart + y * step, zStart + z * step), Quaternion.identity);
                        smallCube.GetComponent<ObjectController>().program = gameObject;
                        if (startAwake)
                            smallCube.GetComponent<ObjectController>().SetAwake(true);
                        _cubes.Add(smallCube);
                    }
                }
            }
        }

        /// <summary>
        /// Starts simulation
        /// </summary>
        private void Start()
        {
            var statics = GetComponent<Statics>();
            _AddPlayer();
            InstantiateSmallCubes(
                statics.BoxesX.x, (int)statics.BoxesX.y,
                statics.BoxesY.x, (int)statics.BoxesY.y,
                statics.BoxesZ.x, (int)statics.BoxesZ.y,
                statics.BoxesDistance,
                statics.BoxesStartAwake);

            //for (int i = 0; i < _cubes.Count; i++)
            //{
            //    octreeGrid.addCube(_cubes[i]);
            //}
            //var test = 1 + 2;
        }

        public float LockedY = 0;
        public float LockedZ = 0;

        /// <summary>
        /// Main update method for simulation.
        /// </summary>
        private void Update()
        {
            var dt = Time.deltaTime;
            dt = (dt >= TIMESTEP) ? TIMESTEP : dt;
            _accumulator += dt;

            while (_accumulator > TIMESTEP)
            {
                _collisions.CalculateCollisions(TIMESTEP, _cubes, Walls, ref octreeGrid);
                _physics.IntegratePhysics(TIMESTEP, _cubes);

                _accumulator -= TIMESTEP;
            }

            var alpha = _accumulator / TIMESTEP;
            _rendering.LateUpdate(alpha, _cubes);
            
            //transform.position = new Vector3(_accumulator, LockedY, LockedZ);
        }
    }
}