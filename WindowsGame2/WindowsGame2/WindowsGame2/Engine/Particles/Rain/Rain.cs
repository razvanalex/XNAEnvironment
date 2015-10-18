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


namespace Engine.Particles
{
    public class Rain : Microsoft.Xna.Framework.GameComponent
    {
        List<RainSystem> RainParticle = new List<RainSystem>();
        Game game;

        GraphicsDevice graphicsDevice;

        Matrix[] instanceTransformsSM;
        Matrix[] instanceTransformsPS;

        bool snow;

        public Rain(Game game, Vector3 Position, bool snow, GraphicsDevice graphicsDevice)
            : base(game)
        {
            this.game = game;
            this.graphicsDevice = graphicsDevice;
            this.snow = snow;

            if(!snow) RainParticle.Add(new RainSystem(game, Position, new Vector2(250, 50), 20000, new Vector2(0.2f, 10), 10, new Vector3(0), 0.5f, snow));
            else RainParticle.Add(new RainSystem(game, Position, new Vector2(250, 20), 20000, new Vector2(0.5f, 0.5f), 10, new Vector3(0), 0.5f, snow));
        }

        public void Update(Camera.Camera camera)
        {
            if(!snow)
                RainParticle[0].Position = new Vector3(0, 500, 0) + ((Camera.FreeCamera)camera).Position;
            else RainParticle[0].Position = new Vector3(0, 300, 0) + ((Camera.FreeCamera)camera).Position; 
            foreach (RainSystem rain in RainParticle)
            {            
                rain.Update(camera);
            }
        }

        public void Draw(Camera.Camera camera)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rs;
       
            foreach (RainSystem rain in RainParticle)
                rain.Draw(instanceTransformsPS, instanceTransformsSM, camera);

            rs = new RasterizerState();
            rs.CullMode = CullMode.CullCounterClockwiseFace;
            graphicsDevice.RasterizerState = rs;
        }

    }
}
