//-----------------------------------------------------------------------------
// MovingLight.fx
//
// Jorge Adriano Luna 2011
// http://jcoluna.wordpress.com
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;

namespace Engine.Shaders
{
    /// <summary>
    /// Simple class to move a point light between 2 points
    /// </summary>
    public class MovingLight
    {
        private Vector3 _startPoint;
        private Vector3 _endPoint;
        private float _speed;
        private float _currentTime;
        private Light _light;

        public MovingLight(Light light, Vector3 startPoint, Vector3 endPoint, float speed)
        {
            _light = light;
            _startPoint = startPoint;
            _endPoint = endPoint;
            _speed = speed;
            _currentTime = 0;
        }

        /// <summary>
        /// Move our light between start and end point, using a cosine function
        /// </summary>
        /// <param name="deltaTimeSeconds"></param>
        public void Update(float deltaTimeSeconds)
        {
            _currentTime += deltaTimeSeconds*_speed;
            float t = (float) Math.Cos(_currentTime)*0.5f + 0.5f;
            Vector3 p = Vector3.Lerp(_startPoint, _endPoint, t);
            Matrix m = _light.Transform;
            m.Translation = p;
            _light.Transform = m;
        }

    }
}
