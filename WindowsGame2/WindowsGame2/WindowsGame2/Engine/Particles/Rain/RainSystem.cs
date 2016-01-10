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

using Engine.Camera;

namespace Engine.Particles
{
    public class RainSystem : Microsoft.Xna.Framework.GameComponent
    {
        ParticleSystem rp;
        Random r = new Random();
        GraphicsDevice graphicsDevice;

        public Vector3 Position;
        Vector2 scale;
        int nParticle;
        Vector2 ParticleSize;
        float lifeSpan;
        Vector3 wind;
        float FadeInTime;
        public static float density = 10;
        bool snow;

        Matrix transformR;

        public Matrix TransformR
        {
            get { return transformR; }
        }

        public RainSystem(Game game, Vector3 Position, Vector2 scale, int nParticle,
            Vector2 ParticleSize, float lifeSpan, Vector3 wind, float FadeInTime, bool snow)
            : base(game)
        {
            this.graphicsDevice = game.GraphicsDevice;
            this.Position = Position;
            this.scale = scale;
            this.nParticle = nParticle;
            this.ParticleSize = ParticleSize;
            this.lifeSpan = lifeSpan;
            this.wind = wind;
            this.FadeInTime = FadeInTime;
            this.snow = snow;
            if(!snow)
                rp = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/rain"), nParticle, ParticleSize, lifeSpan, wind, FadeInTime);
            else rp = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/snow"), nParticle, ParticleSize, lifeSpan, wind, FadeInTime);     
        }

        // Returns a random Vector3 between min and max
        Vector3 randVec3(Vector3 min, Vector3 max)
        {
            return new Vector3(
                min.X + (float)r.NextDouble() * (max.X - min.X),
                min.Y + (float)r.NextDouble() * (max.Y - min.Y),
                min.Z + (float)r.NextDouble() * (max.Z - min.Z));
        }

        void Generateparticle()
        {
            for (int i = 1; i <= density; i++)
            {
                Vector3 offset = new Vector3(MathHelper.ToRadians(15.0f));
                Vector3 randAngle = Vector3.Zero;
                float randSpeed = 0;

                if (!snow) //rain
                {
                    randAngle = Vector3.Down;
                    randSpeed = ((float)r.NextDouble() + 2) * scale.Y;
                }
                else //snow
                {
                    randAngle = Vector3.Down + randVec3(-offset, offset);
                    randSpeed = ((float)r.NextDouble() + 2) * scale.Y;
                }

                Vector3 randPosition = randVec3(new Vector3(-scale.X, 0, -scale.X), new Vector3(scale.X, 0, scale.X));               
                rp.AddParticle(randPosition + Position, randAngle, randSpeed);
            }
        }

        public void Update(Camera.Camera camera)
        {
            Generateparticle();
            rp.Update();
          
        }

        void TransformMatrix(ref Matrix transform, Vector3 Position, Vector3 Rotation)
        {
            Matrix _scale, _rotation, _position;
            _scale = Matrix.CreateScale(new Vector3(scale, scale.X));
            _rotation = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            _position = Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);

            Matrix.Multiply(ref _scale, ref _rotation, out transform);
            Matrix.Multiply(ref transform, ref _position, out transform);
        }

        public void Draw(Matrix[] instanceTransformsPS, Camera.Camera camera)
        {
            rp.Draw(camera.View, camera.Projection, new Vector3(0, 1, 0), camera.Transform.Right);
            //rp.DrawHardwareInstancing(instanceTransformsPS, ((FreeCamera)camera), Position);
        }

    }
}
