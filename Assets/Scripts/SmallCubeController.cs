using UnityEngine;
using System.Collections;
using Assets.Scripts.Physics;
using Assets.Scripts.Collisions;

namespace Assets.Scripts
{
    internal class SmallCubeController : ObjectController
    {
        Color baseCol = new Color(0.833f, 0.872f, 1);
        Color currentColor;
        float startTime = -20f;
        float maxTime = 10f; //sec
        float maxTimeInv = 1f / 10f;

        void Update() {
            var render = GetComponent<MeshRenderer>();
            if (!isAwake) render.material.color = baseCol;//Time.realtimeSinceStartup - startTime > maxTime)
            else
            {
                //var time = Time.realtimeSinceStartup - startTime;
                //var percent = time * maxTimeInv;

                //var diff = baseCol - currentColor;
                //diff = diff * percent;

                //currentColor += diff;

                render.material.color = currentColor;
            }

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
            startTime = time;
            currentColor = new Color(1, 0, 0);
        }
    }
}
