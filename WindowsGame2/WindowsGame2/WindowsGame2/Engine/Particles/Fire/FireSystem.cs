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
    public class FireSystem : Microsoft.Xna.Framework.GameComponent
    {
        ParticleSystem ps;
        ParticleSystem smoke;
        Random r = new Random();
        GraphicsDevice graphicsDevice;

        Vector3 Position;
        Vector2 scale;
        int nParticle;
        Vector2 ParticleSize;
        float lifeSpan;
        Vector3 wind;
        float FadeInTime;

        Matrix transformPS;
        Matrix transformSM;

        public Matrix TransformPS
        {
            get { return transformPS; }
        }
        public Matrix TransformSM
        {
            get { return transformSM; }
        }

        public FireSystem(Game game, Vector3 Position, Vector2 scale, int nParticle,
            Vector2 ParticleSize, float lifeSpan, Vector3 wind, float FadeInTime)
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

            ps = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/fire"), nParticle, ParticleSize, lifeSpan, wind, FadeInTime);
            smoke = new ParticleSystem(graphicsDevice, game.Content, game.Content.Load<Texture2D>("textures/Particles/smoke"), nParticle, ParticleSize * 2, lifeSpan * 5, wind * 2, FadeInTime * 5);
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

            ps.AddParticle(randPosition + Position, randAngle, randSpeed);
            ps.Update();

            smoke.AddParticle(randPosition + Position + new Vector3(0, 4 * scale.Y, 0), randAngle, randSpeed);
            smoke.Update();

            TransformMatrix(ref transformPS, Position, new Vector3(((FreeCamera)camera).Yaw, ((FreeCamera)camera).Pitch, 0));
            TransformMatrix(ref transformSM, Position + new Vector3(0, 4 * scale.Y, 0), new Vector3(((FreeCamera)camera).Yaw, ((FreeCamera)camera).Pitch, 0));
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

        public void Draw(Matrix[] instanceTransformsPS, Matrix[] instanceTransformsSM, Camera.Camera camera)
        {
         //   ps.DrawHardwareInstancing(instanceTransformsPS, ((FreeCamera)camera), Position);
         //   smoke.DrawHardwareInstancing(instanceTransformsSM, ((FreeCamera)camera), Position);         
            ps.Draw(camera.View, camera.Projection, camera.Transform.Up, camera.Transform.Right);
            smoke.Draw(camera.View, camera.Projection, camera.Transform.Up, camera.Transform.Right);
        }

    }
}
