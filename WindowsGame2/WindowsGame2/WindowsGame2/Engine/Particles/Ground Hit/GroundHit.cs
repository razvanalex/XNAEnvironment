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
using Engine.Shaders;

namespace Engine.Particles
{
    public class GroundHit : Microsoft.Xna.Framework.GameComponent
    {
        List<GroundHitSystem> DustParticle = new List<GroundHitSystem>();
        Game game;
        float timer = 0;

        public GroundHit(Game game)
            : base(game)
        {
            this.game = game;
        }

        public void AddDust(Vector3 Position, Vector2 Scale, int NoParticles, Vector2 ParticleSize, Vector2 ParticleScaleSpeed, float LifeSpan, Vector3 Wind, float FadeInTime)
        {
            DustParticle.Add(new GroundHitSystem(game, Position, Scale, NoParticles, ParticleSize, ParticleScaleSpeed, LifeSpan, Wind, FadeInTime));
        }

        public void Update(Camera.Camera camera)
        {
            timer++;
            if(timer % 100 == 0)
                foreach (GroundHitSystem dust in DustParticle)
                    dust.Update(camera);
        }

        public void Draw(Camera.Camera camera)
        {
            foreach (GroundHitSystem dust in DustParticle)
                dust.Draw(camera);
        }
    }
}