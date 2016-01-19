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
using Engine.Billboards;
using Engine.Camera;
using Engine.Particles;
using Engine.Shaders;
using Engine.Sky;
using Engine.Terrain;
using Engine.Water;

namespace Engine
{
    public class TestWorld : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game game;
        GraphicsDeviceManager graphics;
        GraphicsDevice graphicsDevice;
        ContentManager Content;
        MouseState lastMouseState;
        SpriteBatch spriteBatch;

        Camera.Camera camera;
        SkyDome sky;
        LensFlareComponent lensFlare;
        Components components;

        Vector3 LightColor;
        Vector3 LightDirection;
        Vector3 AmbientColor;

        float timer = 0;
        float deltaX = 0, deltaY = 0;
        KeyboardState keyState, oldKeyState;

        //font
        SpriteFont spriteFont;

        //FPS Counter
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        Terrain.Terrain terrain;
        QuadTree Qtree;
        private BackgroundWorker worker;

        RenderTarget2D rt;

        Fire fire;
        Effect depthEffect;

        public TestWorld(Game game, ContentManager Content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
            : base(game)
        {
            this.game = game;
            this.Content = Content;
            this.graphicsDevice = graphicsDevice;
            this.graphics = graphics;
         
            InitializeThread();
            InitializeTerrain();
        }
        public override void Initialize()
        {
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            lastMouseState = Mouse.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice); 
            spriteFont = Content.Load<SpriteFont>("SpriteFont1");
            camera = new FreeCamera(new Vector3(0, 200, 0), 0, 0, 1f, 1000000.0f, graphicsDevice);
            sky = new SkyDome(game, false, camera, graphicsDevice);
            lensFlare = new LensFlareComponent(game);
            components = new Components(graphicsDevice);
            game.Components.Add(lensFlare);

            terrain.InitializeTerrin(Qtree, (FreeCamera)camera, terrain, Content.Load<Texture2D>("textures//Terrain//BasicTerrain//heightmap"), Vector3.Zero, Content, graphicsDevice);

            terrain.TerrainTextures(Qtree, 
                new Texture2D[] {
                    Content.Load<Texture2D>("textures//Terrain//BasicTerrain//GroundMap"),
                }, new Texture2D[] {
                    Content.Load<Texture2D>("textures//Terrain//grass//grass"),
                }, new int[] { 1000, 100, 100, 1000, 1000, 500 }, Content);

            fire = new Fire(game);
            fire.AddFire(new Vector3(450, Qtree.GetHeight(0, 300), 300), new Vector2(10, 50), 100, new Vector2(50), 1f, new Vector3(0), 1);

            base.LoadContent();
        }
        private void InitializeTerrain()
        {
            //create terrain object
            this.terrain = new Terrain.Terrain();
            //set the depth of the tree
            byte treeDepth = 8;
            //set the scale of the terrain
            float scale = 0.39f;
            //set the size of the terrain part represented by the root quad tree node.
            int landSize = (int)(32768 * scale);
            //create a new quadtree with the specified depth, land size and at location (0,0)
            Qtree = new QuadTree(treeDepth, landSize, scale, new Vector2(-landSize / 2, -landSize / 2));

            this.terrain.QuadTrees.Add(Qtree);
        }
        private void InitializeThread()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.WorkerSupportsCancellation = true;
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {
                terrain.Update(null);
            }
        }

        public override void Update(GameTime gameTime)
        {
            fire.Update(camera);

            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyUp(Keys.LeftControl))
            {
                timer++;
                if (timer == 1)
                    Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                components.updateCamera(gameTime, deltaX, deltaY, (FreeCamera)camera, lastMouseState);
                game.IsMouseVisible = false;
            }
            else
            {
                timer = 0;
                if (timer > 100)
                    timer = 100;
                game.IsMouseVisible = true;
            }



            sky.Update(gameTime);
            sky.GetData(new object[] { Qtree });

            //FPS Counter
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
            //sky.Draw(gameTime);
            lensFlare.View = camera.View;
            lensFlare.Projection = camera.Projection;
            terrain.Draw(camera.View, camera.Projection, camera.Transform.Translation);
            //fire.Draw(camera);

            //FPS
            frameCounter++;
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "FPS: " + frameRate, new Vector2(20, 20), Color.Black);
            graphicsDevice.SetRenderTarget(rt);   
            spriteBatch.Draw(rt, new Rectangle(0, 10, 100, 100), Color.White);
            graphicsDevice.SetRenderTarget(null);   
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
