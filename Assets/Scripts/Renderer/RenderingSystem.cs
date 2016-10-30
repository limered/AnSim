using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Renderer
{
    /// <summary>
    /// Interpolates between last State and next State and sends the interpolated position to unity.
    /// </summary>
    internal class RenderingSystem
    {
        public void LateUpdate(float alpha, List<GameObject> cubes)
        {
            for (var i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                var objectControl = cube.GetComponent<ObjectController>();

                var position = _InterpolatePosition(alpha, objectControl);
                var rotation = _InterpolateOrientation(alpha, objectControl);

                if(objectControl.isAwake)
                    _UpdateRendering(position, rotation, cube.GetComponent<Transform>());
            }
        }

        /// <summary>
        /// Interpolates the state position of an object.
        /// </summary>
        /// <param name="alpha">[0, 1] alpha from last state to next state</param>
        /// <param name="objectControl"></param>
        private Vector3 _InterpolatePosition(float alpha, ObjectController objectControl)
        {
            return Vector3.Lerp(objectControl.lastState.position, objectControl.nextState.position, alpha);
        }

        /// <summary>
        /// Interpolates oriantation of object.
        /// </summary>
        /// <param name="alpha">[0, 1] alpha from last state to next state</param>
        /// <param name="objectControl"></param>
        /// <returns></returns>
        private Quaternion _InterpolateOrientation(float alpha, ObjectController objectControl)
        {
            return Quaternion.Slerp(objectControl.lastState.orientation, objectControl.nextState.orientation, alpha);
        }

        /// <summary>
        /// Pushes state chenges to the simulation.
        /// </summary>
        /// <param name="position"> New, interpolated position of object </param>
        /// <param name="rotation"> New, interpolated orientation of object </param>
        /// <param name="transformComponent"> Transform component of object </param>
        private void _UpdateRendering(Vector3 position, Quaternion rotation, Transform transformComponent)
        {
            for (int i = 0; i < 3; i++) if (float.IsNaN(position[i])) return;
            for (int i = 0; i < 4; i++) if (float.IsNaN(rotation[i])) return;

            transformComponent.position = position;
            transformComponent.rotation = rotation;
        }
    }
}