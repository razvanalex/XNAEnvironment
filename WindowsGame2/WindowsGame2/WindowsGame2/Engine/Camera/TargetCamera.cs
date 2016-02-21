using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Camera
{

    public class TargetCamera : Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public float nearPlane { get; set; }
        public float farPlane { get; set; }
        public TargetCamera(Vector3 Position, Vector3 Target, float nearPlane, float farPlane,
        GraphicsDevice graphicsDevice)
            : base(graphicsDevice, nearPlane, farPlane)
        {
            this.Position = Position;
            this.Target = Target;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
        }
        public override void Update()
        {
            Vector3 forward = Target - Position;
            Vector3 side = Vector3.Cross(forward, Vector3.Up);
            
            Vector3 up = Vector3.Cross(forward, side);
            this.View = Matrix.CreateLookAt(Position, Target, up);

            base.Update();
        }
    }
}
