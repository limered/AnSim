using UnityEngine;

namespace Assets.Scripts
{
    internal class SmallCubeController : ObjectController
    {
        private Color baseCol = new Color(0.833f, 0.872f, 1);
        private Color red = new Color(1, 0, 0);
        private Color currentColor;
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

            if (!isAwake) return;

            Vector3 force = Vector3.zero;
            LinearForces(ref force);

            Vector3 torque = Vector3.zero;
            RotationForces(ref torque);

            GetComponent<Rigidbody>().AddForce(force);
            GetComponent<Rigidbody>().AddTorque(torque);

            lastFrameAcceleration = force * nextState.inverseMass;

            UpdateMotion();
        }

        public void ChangeColor(float time)
        {
            //startTime = time;
            //currentColor = red;
        }
    }
}