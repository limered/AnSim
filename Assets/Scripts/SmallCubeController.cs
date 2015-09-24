﻿using UnityEngine;
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
            if (Time.realtimeSinceStartup - startTime > maxTime) render.material.color = baseCol;
            else
            {
                var time = Time.realtimeSinceStartup - startTime;
                var percent = time * maxTimeInv;

                var diff = baseCol - currentColor;
                diff = diff * percent;

                currentColor += diff;

                render.material.color = currentColor;
            }
        }

        public void ChangeColor(float time)
        {
            startTime = time;
            currentColor = new Color(1, 0, 0);
        }
    }
}
