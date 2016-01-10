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

    public class FreeCamera : Camera
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        private Vector3 translation;
        public override float NearPlane { get; set; }
        public override float FarPlane { get; set; }

        public FreeCamera(Vector3 Position, float Yaw, float Pitch, float nearPlane, float farPlane, GraphicsDevice graphicsDevide)
            : base(graphicsDevide, nearPlane, farPlane)
        {
            this.Position = Position;
            this.Yaw = Yaw;
            this.Pitch = Pitch;
            this.FarPlane = farPlane;
            this.NearPlane = nearPlane;
            
            translation = Vector3.Zero;
        }

        public void Rotate(float YawChange, float PitchChange)
        {
            this.Yaw += YawChange;
            this.Pitch += PitchChange;
        }

        public void Move(Vector3 Translation)
        {
            this.translation += Translation;
        }

        Vector3 up;
        Vector3 forward;
        Vector3 right;
        public Vector3 Forward
        {
            get
            {
                return forward;
            }
            set
            {
                forward = value;
            }
        }
        public Vector3 Up
        {
            get
            {
                return up;
            }
            set
            {
                up = value;
            }
        }
        public Vector3 Right
        {
            get
            {
                return right;
            }
            set
            {
               right = value;
            }
        }

        public override void Update()
        {
            Matrix rotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0);

            translation = Vector3.Transform(translation, rotation);
            Position += translation;
            translation = Vector3.Zero;

            forward = Vector3.Transform(Vector3.Forward, rotation);
            Target = Position + forward;           

            up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, Target, up);

            right = Vector3.Cross(-forward, up);
            base.Update();
        }
    }
}
