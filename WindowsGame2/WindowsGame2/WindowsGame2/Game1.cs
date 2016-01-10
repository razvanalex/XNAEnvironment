using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Engine;

namespace WindowsGame2
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicWorld basicWorld;
        TestWorld testWorld;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            basicWorld = new BasicWorld(this, Content, graphics, GraphicsDevice);
            Components.Add(basicWorld);
          
           // testWorld = new TestWorld(this, Content, graphics, GraphicsDevice);
           // Components.Add(testWorld);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);      

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }  

        protected override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
        }
    }
}