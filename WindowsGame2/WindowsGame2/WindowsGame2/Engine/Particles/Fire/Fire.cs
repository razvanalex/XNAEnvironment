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

using Engine.Shaders;

namespace Engine.Particles
{
    public class Fire : Microsoft.Xna.Framework.GameComponent
    {
        List<FireSystem> FireParticle = new List<FireSystem>();
        Game game;

        Matrix[] instanceTransformsSM;
        Matrix[] instanceTransformsPS;

        public Fire(Game game)
            : base(game)
        {
            this.game = game;
        }

        public void AddFire(Vector3 Position, Vector2 Scale, int NoParticles, Vector2 ParticleSize, float LifeSpan, Vector3 Wind, float FadeInTime)
        {
            FireParticle.Add(new FireSystem(game, Position, Scale, NoParticles, ParticleSize, LifeSpan, Wind, FadeInTime));
        }

        public void AddLight(ref LightingClass light)
        {
            for (int i = 0; i < FireParticle.Count(); i++)
                light.AddPointLight(FireParticle[0].scale.X * 25, 2, Color.Orange, new Vector3(FireParticle[i].Position.X, FireParticle[i].Position.Y + FireParticle[0].scale.Y / 2, FireParticle[i].Position.Z));
        }

        public void Update(Camera.Camera camera)
        {
            foreach (FireSystem fire in FireParticle)
                fire.Update(camera);
        }

        public void Draw(Camera.Camera camera)
        {
            Array.Resize(ref instanceTransformsPS, FireParticle.Count);
            Array.Resize(ref instanceTransformsSM, FireParticle.Count);

            for (int i = 0; i < FireParticle.Count; i++)
            {
                instanceTransformsPS[i] = FireParticle[i].TransformPS;
                instanceTransformsSM[i] = FireParticle[i].TransformSM;
            }

            foreach (FireSystem fire in FireParticle)
                fire.Draw(instanceTransformsPS, instanceTransformsSM, camera);
        }

    }
}