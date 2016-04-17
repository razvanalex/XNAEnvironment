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
    public class GroundHitSystem : Microsoft.Xna.Framework.GameComponent
    {
        ParticleSystem dust;
        ParticleSystem rocks;

        Random r = new Random();
        GraphicsDevice graphicsDevice;

        public Vector3 Position;
        public Vector2 scale;
        int nParticle;
        Vector2 ParticleSize;
        float lifeSpan;
        Vector3 wind;
        float FadeInTime;

        Matrix transformDust;
        Matrix transformRocks;

        public Matrix TransformDust
        {
            get { return transformDust; }
        }
        public Matrix TransformRocks
        {
            get { return transformRocks; }
        }

        public GroundHitSystem(Game game, Vector3 Position, Vector2 scale, int nParticle,
            Vector2 ParticleSize, float ParticleScaleSpeed, float lifeSpan, Vector3 wind, float FadeInTime)
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

           // dust = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/smoke"), 
           //                             nParticle, lifeSpan, wind, FadeInTime);
           // rocks = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/fire"), 
          //                              nParticle, lifeSpan, wind, FadeInTime);
        }

        // Returns a random Vector3 between min and max
        Vector3 randVec3(Vector3 min, Vector3 max)
        {
            return new Vector3(
                min.X + (float)r.NextDouble() * (max.X - min.X),
                min.Y + (float)r.NextDouble() * (max.Y - min.Y),
                min.Z + (float)r.NextDouble() * (max.Z - min.Z));
        }

        public void Update(Camera.Camera camera)
        {
            // Generate a direction within 15 degrees of (0, 1, 0)
            Vector3 offset = new Vector3(MathHelper.ToRadians(10.0f));
            Vector3 randAngle = Vector3.Up + randVec3(-offset, offset);

            // Generate a position between (-400, 0, -400) and (400, 0, 400)
            Vector3 randPosition = randVec3(new Vector3(-scale.X, 0, -scale.X), new Vector3(scale.X, 0, scale.X));

            float randSpeed = ((float)r.NextDouble() + 2) * scale.Y;

           // rocks.AddParticle(randPosition + Position, randAngle, randSpeed, ParticleSize);
            rocks.Update();

            //dust.AddParticle(randPosition + Position + new Vector3(0, 4 * scale.Y, 0), randAngle, randSpeed, ParticleSize);
            dust.Update();

            TransformMatrix(ref transformRocks, Position, new Vector3(((FreeCamera)camera).Yaw, ((FreeCamera)camera).Pitch, 0));
            TransformMatrix(ref transformDust, Position, new Vector3(((FreeCamera)camera).Yaw, ((FreeCamera)camera).Pitch, 0)); 
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

        public void Draw(Camera.Camera camera)
        {
            dust.Draw(camera.View, camera.Projection, camera.Transform.Up, camera.Transform.Right);
            rocks.Draw(camera.View, camera.Projection, camera.Transform.Up, camera.Transform.Right);
        }        
    }
}
