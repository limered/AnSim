using UnityEngine;
using System.Collections;
using Assets.Scripts.Renderer;
using Assets.Scripts.Physics;
using Assets.Scripts.Collisions;

namespace Assets.Scripts
{
    public class MainProgram : MonoBehaviour
    {

        public GameObject BigCubePrefab;
        public Camera MainCamera;
        public GameObject SmallCubePrefab;
        public GameObject[] Walls;

        private float _accumulator = 0;
        private CollisionSystem _collisions = new CollisionSystem();
        private GameObject[] _cubes;
        private PhysicsSystem _physics = new PhysicsSystem();
        private RenderingSystem _rendering = new RenderingSystem();
        private float _timeStep = 0.02f;
        // Use this for initialization
        void Start()
        {
            var player = Instantiate(BigCubePrefab, new Vector3(10, 5, 10), Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
            var dt = Time.deltaTime;
            dt = (dt >= 0.0333333333333333f) ? 0.0333333333333333f : dt;
            _accumulator += dt;

            while (_accumulator > _timeStep) {
                _collisions.CalculateCollisions(dt, _cubes);
                _physics.IntegratePhysics(dt, _cubes);

                _accumulator -= _timeStep;
            }

            var alpha = _accumulator / _timeStep;
            _rendering.Interpolate(alpha, _cubes);
        }

    }
}
