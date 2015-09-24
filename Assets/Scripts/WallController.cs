using UnityEngine;

namespace Assets.Scripts
{
    internal class WallController : ObjectController
    {
        public Vector3 normal;
        public float wallConstant;

        private void Start()
        {
            _CalculateWallConstant();
        }

        private void _CalculateWallConstant()
        {
            var transform = GetComponent<Transform>();
            Vector3 position = transform.position;

            position.x += normal.x * transform.localScale.x / 2f;
            position.y += normal.y * transform.localScale.y / 2f;
            position.z += normal.z * transform.localScale.z / 2f;

            wallConstant = Vector3.Dot(normal, position);
        }
    }
}