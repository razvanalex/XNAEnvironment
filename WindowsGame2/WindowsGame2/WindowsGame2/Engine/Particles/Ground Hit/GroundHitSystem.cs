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
        public ParticleSystem dust;
        public ParticleSystem rocks;

        Random r = new Random();
        GraphicsDevice graphicsDevice;

        public Vector3 Position;
        public Vector2 scale;
        int nParticle;
        Vector2 ParticleSize;
        Vector2 ParticleScaleSpeed;
        float lifeSpan;
        Vector3 wind;
        float FadeInTime;
        int factor = 10;

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
            Vector2 ParticleSize, Vector2 ParticleScaleSpeed, float lifeSpan, Vector3 wind, float FadeInTime)
            : base(game)
        {
            this.graphicsDevice = game.GraphicsDevice;
            this.Position = Position;
            this.scale = scale;
            this.nParticle = nParticle;
            this.ParticleSize = ParticleSize;
            this.ParticleScaleSpeed = ParticleScaleSpeed;
            this.lifeSpan = lifeSpan;
            this.wind = wind;
            this.FadeInTime = FadeInTime;
            
            dust = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/smoke"),
                                        nParticle, ParticleSize, lifeSpan * 2, wind, FadeInTime);
            rocks = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/soil-rock"),
                                        nParticle * factor, ParticleSize / 8, lifeSpan, wind, FadeInTime);
        }

        // Returns a random Vector3 between min and max
        Vector3 randVec3(Vector3 min, Vector3 max)
        {
            return new Vector3(
                min.X + (float)r.NextDouble() * (max.X - min.X),
                min.Y + (float)r.NextDouble() * (max.Y - min.Y),
                min.Z + (float)r.NextDouble() * (max.Z - min.Z));
        }
        float t = 0;
        public void Update(Camera.Camera camera)
        {
            //Add rocks
            for (int i = 0; i < nParticle * factor; i++)
            {
                // Generate a direction within 15 degrees of (0, 1, 0)
                Vector3 offset = new Vector3(MathHelper.ToRadians(20.0f));

                Vector3 randAngle = Vector3.Up + randVec3(-offset, offset);

                // Generate a position between (-scale.X, 0, -scale.X) and (scale.X, 0, scale.X)
                Vector3 randPosition = randVec3(new Vector3(-scale.X, 0, -scale.X), new Vector3(scale.X, 0, scale.X));

                float randSpeed = ((float)r.NextDouble() + 2) * scale.Y;
                rocks.AddParticle(randPosition + Position, randAngle, randSpeed * 2, new Vector2(0), 5f);
                rocks.Update();
            }

            //Add smoke
            for (int i = 0; i < nParticle; i++)
            {
                // Generate a direction within 15 degrees of (0, 1, 0)
                Vector3 offset = new Vector3(MathHelper.ToRadians(50.0f));

                Vector3 randAngle = Vector3.Up + randVec3(-offset, offset);

                // Generate a position between (-scale.X, 0, -scale.X) and (scale.X, 0, scale.X)
                Vector3 randPosition = randVec3(new Vector3(-scale.X, 0, -scale.X), new Vector3(scale.X, 0, scale.X));

                float randSpeed = ((float)r.NextDouble() + 2) * scale.Y;

                dust.AddParticle(randPosition + Position, randAngle, randSpeed, ParticleScaleSpeed, 0);
                dust.Update();         
            }
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
