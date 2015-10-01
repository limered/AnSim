using UnityEngine;

namespace Assets.Scripts
{
    internal class SmallCubeController : ObjectController
    {
        private Color baseCol = new Color(0.833f, 0.872f, 1);
        private Color red = new Color(1, 0, 0);
        private Color currentColor = new Color(0.833f, 0.872f, 1);
        private float startTime = -20f;
        private float maxTime = 10f; //sec
        private float maxTimeInv = 1f / 10f;

        private bool _lastAwakeState;

        private void Update()
        {
            var render = GetComponent<MeshRenderer>();
            if (!isAwake)
            {
                if (_lastAwakeState != isAwake)
                {
                    startTime = Time.realtimeSinceStartup;
                }
                var time = Time.realtimeSinceStartup - startTime;
                var percent = time * maxTimeInv;

                var diff = baseCol - currentColor;
                diff = diff * percent;

                currentColor += diff;

                render.material.color = currentColor;
            }
            else
            {
                if (_lastAwakeState != isAwake)
                {
                    currentColor = red;
                }
                render.material.color = currentColor;
            }

            _lastAwakeState = isAwake;

            // Update sleep state
            
        }

        /// <summary>
        /// Function to call on collision
        /// </summary>
        /// <param name="timeStamp"></param>
        public void OnCollision(float timeStamp)
        {
            
        }
    }
}