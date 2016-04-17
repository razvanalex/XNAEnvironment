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

using Engine;
using Engine.Camera;
using Engine.Terrain;

namespace Engine
{
    public class PlayerManager
    {
        Game game;
        GraphicsDevice graphicsDevice;
        Camera.Camera camera;
        QuadTree terrain;

        public CarPlayer carPlayer;       

        public PlayerManager(Game game, GraphicsDevice graphicsDevice)
        {
            this.game = game;
            this.graphicsDevice = graphicsDevice;
        }

        public void Initialize()
        {
            carPlayer = new CarPlayer(game, graphicsDevice);
            carPlayer.GetData(new object[] { terrain, camera });
            carPlayer.Initialize();
            carPlayer.GenerateTags();           
        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                    terrain = (QuadTree)o;
                if (o is Camera.Camera)
                    camera = (Camera.Camera)o;
            }
        }

        public void Update(GameTime gameTime)
        {
            carPlayer.Update(gameTime);
        }

        public void Draw(Camera.Camera camera)
        {
            carPlayer.Draw(camera);
        }
    }
}
