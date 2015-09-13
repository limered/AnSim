using Assets.Scripts.Collisions;
using Assets.Scripts.Physics;
using Assets.Scripts.Renderer;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class MainProgram : MonoBehaviour
    {
        public GameObject BigCubePrefab;
        public Camera MainCamera;
        public GameObject SmallCubePrefab;
        public GameObject[] Walls;

        static public float GravityConstant = -9.8f;

        private float _accumulator = 0;
        private CollisionSystem _collisions = new CollisionSystem();
        private List<GameObject> _cubes = new List<GameObject>();
        private PhysicsSystem _physics = new PhysicsSystem();
        private RenderingSystem _rendering = new RenderingSystem();
        private float _timeStep = 0.02f;

        // Use this for initialization
        private void Start()
        {
            InstantiateSmallCubes(-20, 20, 2);
            //AddWallsToInstancesList();
            _AddPlayer();
        }

        /// <summary>
        /// Instantiates all small cubes.
        /// </summary>
        /// <param name="min">Minimal position (x and z)</param>
        /// <param name="max">Maximal position (x and z)</param>
        /// <param name="stepSize">Sets the desity of small cubes on the ground ((max - min) / step * (max - min) / step, ie: (20 + 20) / 4 * (20 + 20) / 4 = 100 Würfel)</param>
        private void InstantiateSmallCubes(int min, int max, int stepSize)
        {
            for (int i = min; i <= max; i += stepSize)
            {
                for (int j = min; j <= max; j += stepSize)
                {
                    var smallCube = (GameObject)Instantiate(SmallCubePrefab, new Vector3(i, 1, j), Quaternion.identity);//Quaternion.AngleAxis(Random.Range(0, 90), new Vector3(0.5f, 0.5f, 0)));
                    //smallCube.GetComponent<ObjectController>().anSimCollider.UpdateDataFromObject(smallCube);
                    _cubes.Add(smallCube);

                }
            }
        }

        /// <summary>
        /// Adds all the Wall object to list of instances
        /// </summary>
        private void AddWallsToInstancesList()
        {
            for (int i = 0; i < Walls.Length; i++)
            {
                //Walls[i].GetComponent<ObjectController>().anSimCollider.UpdateDataFromObject(Walls[i]);
                //_cubes.Add(Walls[i]);
            }
        }

        private void _AddPlayer()
        {
            var playerCube = GameObject.Find("BigCube");
            //playerCube.GetComponent<ObjectController>().lastState.velocity = new Vector3(5, 0, 0);
            //playerCube.GetComponent<ObjectController>().nextState.velocity = new Vector3(5, 0, 0);
            _cubes.Add(playerCube);

            //var smallCube = GameObject.Find("SmallCube");
            //smallCube.GetComponent<ObjectController>().lastState.velocity = new Vector3(5, 0, 0);
            //smallCube.GetComponent<ObjectController>().nextState.velocity = new Vector3(5, 0, 0);
            //_cubes.Add(smallCube);

        }

        /// <summary>
        /// Main update method for simulation.
        /// </summary>
        private void Update()
        {
            var dt = Time.deltaTime;
            dt = (dt >= 0.0333333333333333f) ? 0.0333333333333333f : dt;
            _accumulator += dt;

            while (_accumulator > _timeStep)
            {
                _collisions.CalculateCollisions(dt, _cubes);
                _physics.IntegratePhysics(dt, _cubes);

                _accumulator -= _timeStep;
            }

            var alpha = _accumulator / _timeStep;
            _rendering.LateUpdate(alpha, _cubes);

            //Eventuell die Kamera hier bewegen um den würfel zu verfolgen? TODO
        }
    }
}