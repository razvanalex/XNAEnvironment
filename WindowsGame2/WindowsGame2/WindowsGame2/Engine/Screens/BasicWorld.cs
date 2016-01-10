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
using Engine.Camera;
using Engine.Sky;
using Engine.Terrain;
using Engine.Particles;
using Engine.Water;
using Engine.Billboards;
using Engine.Shaders;

namespace Engine
{
    public class BasicWorld : Microsoft.Xna.Framework.DrawableGameComponent
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice graphicsDevice;

        ContentManager Content;
        SpriteBatch spriteBatch;

        Components components;

        List<Models> models = new List<Models>();
        List<Models> models1 = new List<Models>();

        //Camera
        Camera.Camera camera;
        MouseState lastMouseState;

        #region Sky
        SkyDome sky;
        #endregion

        #region Water
        //Water
        Water.Water water;

        float WaveLength = 0.003f;
        float WaveHeight = 0.06f;
        float WaveSpeed = -0.02f;
        Vector3 WaterPos2 = new Vector3(0, 1000f, 0);
        Vector2 WaterSize2 = new Vector2(100, 100);

        Vector3 WaterPos = new Vector3(0, 470, 0);
        Vector2 WaterSize = new Vector2(30000, 30000);
        #endregion

        //Terrain
        Terrain.Terrain terrain;
        QuadTree Qtree;

        #region Blur
        //Blur   
        RenderCapture renderCapture;
        PostProcessor postprocessor;

        RenderCapture depthCapture;
        Effect depthEffect;

        RenderCapture blurCapture;
        DepthOfField dof;

        bool blur = false;
        #endregion

        bool terrain_ = true;
        float deltaX = 0, deltaY = 0;
        private BackgroundWorker worker;
        public static float THeight = 3500;
        BillboardsClass tree;

        //Sun
        public Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));
        public Vector3 LightColor = new Vector3(2, 2, 2);
        public Vector3 AmbientColor = new Vector3(2, 2, 2);

        LensFlareComponent lensFlare;
        public GameTime gametime;

        //ScreenShot
        int ScreenShotTime = 0;

        //Shadow
        KeyboardState keyState, oldKeyState;

        bool fullScreen;

        float timer = 0;

        Fire fire;
        Game game;

        private List<Light> _lights;
        private List<Light> _visibleLights;
        private Renderer renderer;

        Texture2D _lightDebugTexture;
        //font
        SpriteFont spriteFont;

        //FPS Counter
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        public BasicWorld(Game game, ContentManager Content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
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
            // TODO: Add your initialization logic here
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            lastMouseState = Mouse.GetState();

            lensFlare = new LensFlareComponent(game);
            game.Components.Add(lensFlare);

            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphicsDevice);

            //models.Add(new Models(Content.Load<Model>("models//model1"), new Vector3(1000, 1000, 1000), new Vector3(0), new Vector3(500f), graphicsDevice));//new Vector3(60, WaterPos.Y - 100f, 150)        


            Effect cubeMapEffect = Content.Load<Effect>("Effects//AlphaBlending");
            cubeMapEffect.Parameters["LightDirection"].SetValue(LightDirection);
            CubeMapReflectMaterial cubeMat = new CubeMapReflectMaterial(Content.Load<TextureCube>("textures//Skybox//SkyBoxTex"));

            //models[0].SetModelEffect(cubeMapEffect, true);
            //    models[0].SetModelMaterial(cubeMat);           

            #region Terrain
            //  smallTerrain = new SmallTerrain(Content.Load<Texture2D>("textures//Terrain//terrain"), 100f, THeight, 100, LightDirection, LightColor, graphicsDevice, Content);
            models.Add(new Models(Content.Load<Model>("models//model1"), new Vector3(0, 3000, 0), Vector3.Zero, new Vector3(20), graphicsDevice));
            models1.Add(new Models(Content.Load<Model>("models//model1"), new Vector3(0, 3000, 0), Vector3.Zero, new Vector3(10), graphicsDevice));

            camera = new FreeCamera(new Vector3(0, 5000, 0), 0, 0, 1f, 100000.0f, graphicsDevice);

            AmbientColor = lensFlare.AmbientColor;

            components = new Components(graphicsDevice);
            terrain.InitializeTerrin(Qtree, (FreeCamera)camera, terrain, Content.Load<Texture2D>("textures//Terrain//terrain513"), WaterPos, Content, graphicsDevice);
            terrain.TerrainTextures(Qtree,
              new Texture2D[] {
                    Content.Load<Texture2D>("textures//Terrain//grass//GrassMap"),
                    Content.Load<Texture2D>("textures//Terrain//rock//RockMap"),
                    Content.Load<Texture2D>("textures//Terrain//sand//SandMap"),
                    Content.Load<Texture2D>("textures//Terrain//snow//SnowMap"),
                    Content.Load<Texture2D>("textures//Terrain//rocks_sand//Rocks_SandMap"),
                    Content.Load<Texture2D>("textures//Terrain//beach_sand//Beach_SandMap")
                }, new Texture2D[] {
                    Content.Load<Texture2D>("textures//Terrain//grass//grass"),
                    Content.Load<Texture2D>("textures//Terrain//rock//rock"),
                    Content.Load<Texture2D>("textures//Terrain//sand//sand"),
                    Content.Load<Texture2D>("textures//Terrain//snow//snow"),
                    Content.Load<Texture2D>("textures//Terrain//rocks_sand//rocks_sand"),
                    Content.Load<Texture2D>("textures//Terrain//beach_sand//beach_sand")
                }, new int[] { 1000, 100, 100, 1000, 1000, 500 }, Content);

            #endregion

            #region BillBoards

            tree = new BillboardsClass(Content, camera, graphicsDevice);
            tree.GetData(new object[] { Qtree });
            tree.Initialize();

            #endregion

            fire = new Fire(game);
            //  fire.AddFire(new Vector3(4500, Qtree.GetHeight(4500, -250), -250), new Vector2(10, 10), 200, new Vector2(20), 1, new Vector3(0), 1);
            //fire.AddFire(new Vector3(543, Qtree.GetHeight(4000, -250), -250), new Vector2(10, 10), 200, new Vector2(20), 1, new Vector3(0), 1);

            //Sky
            sky = new SkyDome(game, LightColor, LightDirection, false, camera, graphicsDevice);
            sky.GetData(new object[] { Qtree });

            //Water
            water = new Water.Water(WaveLength, WaveHeight, WaveSpeed, WaterPos, WaterSize, Content, graphicsDevice, LightDirection, LightColor, lensFlare.SunFactor);

            worker.RunWorkerAsync();

            _visibleLights = new List<Light>();
            _lights = new List<Light>();
            renderer = new Renderer(GraphicsDevice, Content, 800, 600);

            //add a directional light
            Light dirLight = new Light();
            dirLight.LightType = Light.Type.Directional;
            dirLight.Transform = Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver4, -MathHelper.PiOver4, 0);
            dirLight.Color = Color.White;
            dirLight.Intensity = 0.5f;
            dirLight.ShadowDistance = 10000;
            dirLight.ShadowDepthBias = 0.01f;
            dirLight.CastShadows = true;
            _lights.Add(dirLight);
            /*
            //create some point lights
            const int numLights = 10;
            Random random = new Random();
            for (int i = 0; i < numLights; i++)
            {
                Light light = new Light();
                light.Radius = 10000.0f + 0.5f * (float)random.NextDouble();
                light.Intensity = 1 + 0.5f * (float)random.NextDouble();
                light.Color = new Color((float)random.NextDouble() + 0.25f, (float)random.NextDouble() + 0.25f, (float)random.NextDouble() + 0.25f, 1);

                _lights.Add(light);
                const int randomRadiusDistribution = 5000;
                Vector3 endPoint = new Vector3((float)(random.NextDouble() * 2 - 1) * randomRadiusDistribution, (float)(random.NextDouble() * 0.5f - 0.2f), (float)(random.NextDouble() * 2 - 1) * randomRadiusDistribution);
                Vector3 startPoint = new Vector3((float)(random.NextDouble() * 2 - 1) * randomRadiusDistribution, 2000, (float)(random.NextDouble() * 2 - 1) * randomRadiusDistribution);
                light.Transform = Matrix.CreateTranslation(startPoint);
            }
            //create some spot lights
            Light spot;

            spot = new Light();
            spot.LightType = Light.Type.Spot;
            spot.ShadowDepthBias = 0.0001f;
            spot.Radius = 80;
            spot.SpotAngle = 40;
            spot.Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            spot.CastShadows = true;
            float dist = 15000f;
            float h = 40000f;
            Matrix m;
            Vector3 pos = new Vector3(-dist, h, -3);
            m = Matrix.CreateFromYawPitchRoll(5 * MathHelper.PiOver4, -MathHelper.PiOver4, 0);
            m.Translation = pos;
            spot.Transform = m;
            _lights.Add(spot);
            */
            _lightDebugTexture = Content.Load<Texture2D>("textures//lightTexture");
            spriteFont = Content.Load<SpriteFont>("SpriteFont1");

            renderCapture = new RenderCapture(graphicsDevice);
            postprocessor = new GaussianBlur(graphicsDevice, Content, 2f);

            depthEffect = Content.Load<Effect>("shaders//DepthEffect");
            depthCapture = new RenderCapture(graphicsDevice, SurfaceFormat.HalfSingle);

            blurCapture = new RenderCapture(graphicsDevice, SurfaceFormat.Color);
            dof = new DepthOfField(graphicsDevice, Content);

            base.LoadContent();
        }
        private void InitializeTerrain()
        {
            //create terrain object
            this.terrain = new Terrain.Terrain();
            //set the depth of the tree
            byte treeDepth = 8;
            //set the scale of the terrain
            float scale = 0.63f;
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

            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) || (Keyboard.GetState().IsKeyDown(Keys.Q)) || (Keyboard.GetState().IsKeyDown(Keys.Escape)))
                game.Exit();
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

            fullScreen = graphics.IsFullScreen;

            if ((keyState.IsKeyDown(Keys.B) && oldKeyState.IsKeyUp(Keys.B)) && !blur)
                blur = true;
            else if ((keyState.IsKeyDown(Keys.B) && oldKeyState.IsKeyUp(Keys.B)) && blur)
                blur = false;

            /*  if ((keyState.IsKeyDown(Keys.U)))
                  models[0].Position += new Vector3(0, 3f, 0);
              if ((keyState.IsKeyDown(Keys.J)))
                  models[0].Position -= new Vector3(0, 3f, 0);
              */
            if (keyState.IsKeyDown(Keys.PageUp))
            {
                THeight += 10;
                // smallTerrain = new SmallTerrain(Content.Load<Texture2D>("textures//Terrain//terrain"), 100f, THeight, 1, new Vector3(1, -1, 0), LightColor, graphicsDevice, Content);
                // components.TerrainTextures(Qtree, Content);
            }
            else if (keyState.IsKeyDown(Keys.PageDown))
            {
                THeight -= 10;
                //  smallTerrain = new SmallTerrain(Content.Load<Texture2D>("textures//Terrain//terrain"), 100f, THeight, 1, new Vector3(1, -1, 0), LightColor, graphicsDevice, Content);
                // components.TerrainTextures(Qtree, Content);
            }


            if ((keyState.IsKeyDown(Keys.T) && oldKeyState.IsKeyUp(Keys.T)) && !terrain_)
                terrain_ = true;
            else if ((keyState.IsKeyDown(Keys.T) && oldKeyState.IsKeyUp(Keys.T)) && terrain_)
                terrain_ = false;

            if ((keyState.IsKeyDown(Keys.F) && oldKeyState.IsKeyUp(Keys.F)))
            {
                graphics.ToggleFullScreen();
                deltaX = 0;
                deltaY = 0;
                Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                lastMouseState = Mouse.GetState();
            }
            else if ((keyState.IsKeyDown(Keys.F) && oldKeyState.IsKeyUp(Keys.F)))
            {
                graphics.ToggleFullScreen();
                deltaX = 0;
                deltaY = 0;
                Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
                lastMouseState = Mouse.GetState();
            }


            lensFlare.Light_anim = sky.Theta;
            LightDirection = Vector3.Negate(Vector3.Reflect(lensFlare.LightDirection, Vector3.Up));
            AmbientColor = lensFlare.AmbientColor;
            LightColor = Vector3.Lerp(lensFlare.LightColor, AmbientColor, sky.Gr);
            _lights[0].Transform = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, sky.Theta + MathHelper.PiOver2, 0);
            tree.LightColor = LightColor;
            tree.AmbientColor = AmbientColor;
            tree.LightDirection = LightDirection;
            tree.Update(gametime);

            sky.Update(gameTime, LightDirection, LightColor);
            sky.GetData(new object[] { Qtree });

           // water.UpdateLight(LightColor, -LightDirection, lensFlare.SunFactor);
           // water.GetData(new object[] { Qtree, sky, new Billboard[] { tree.Linden, tree.Fir, tree.Palm } });
          //  water.ChangeGraphics();

            //  Thread t1 = new Thread(delegate()
            // {

            //terrain = new Terrain(Content.Load<Texture2D>("textures//Terrain//Terain"), 100f, THeight, Content.Load<Texture2D>("textures//Terrain//grass"), 1, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, graphicsDevice, Content);
            //  });
            // t1.Start();          

            // foreach (Billboards tree in trees[1])
            // {
            //  if (tree.model == instancedModel[1][0])
            //  tree.model.Meshes[0].MeshParts[2].Transform = Matrix.CreateBillboard(tree.position, ((FreeCamera)camera).Position, ((FreeCamera)camera).Up, ((FreeCamera)camera).Forward);
            //  }

            //  Console.WriteLine(trees[1][0].model.Meshes[0].MeshParts[1].ToString());

            //ScreenShot
            ScreenShotTime++;
            if (ScreenShotTime == 2)
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                    ScreenShot();
            if (Keyboard.GetState().IsKeyUp(Keys.P))
                ScreenShotTime = 0;

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

        int Count = 0;
        public void ScreenShot()
        {
#if WINDOWS
            int w = graphicsDevice.PresentationParameters.BackBufferWidth;
            int h = graphicsDevice.PresentationParameters.BackBufferHeight;

            Count++;

            //force a frame to be drawn (otherwise back buffer is empty)
            Draw(new GameTime());

            //pull the picture from the buffer
            int[] backBuffer = new int[w * h];
            graphicsDevice.GetBackBufferData(backBuffer);

            //copy into a texture
            Texture2D texture = new Texture2D(graphicsDevice, w, h, false, graphicsDevice.PresentationParameters.BackBufferFormat);
            texture.SetData(backBuffer);

            //save to disk
            string path = @"ScreenShots";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (Directory.Exists(path))
            {
                while (File.Exists(path + @"/ScreenShot_" + Count + ".png"))
                    Count++;

                Stream stream = File.OpenWrite(path + @"/ScreenShot_" + Count + ".png");
                texture.SaveAsPng(stream, w, h);
                stream.Close();
            }

#elif XBOX
    throw new NotSupportedException();
#endif
        }


        public override void Draw(GameTime gameTime)
        {
            RasterizerState rs = new RasterizerState();
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (blur)
            {
                #region BLUR
                water.PreDraw(camera, gameTime);
                depthCapture.Begin();

                // Clear to white (max depth)
                graphicsDevice.Clear(Color.White);

                /* foreach (Models model in models)
                {
                    model.CacheEffects(); // Cache effect
                    model.SetModelEffect(depthEffect, false); // Set depth effect
                    model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                    model.RestoreEffects(); // Restore effects
                }*/

                for (int i = 0; i < tree.Linden.LOD; i++)
                {
                    if (tree.Fir.instanceTransforms[i].Length != 0)
                    {
                        tree.Linden.trunck[i][0].CacheEffects();
                        tree.Linden.trunck[i][0].SetModelEffect(depthEffect, false);
                        tree.Linden.trunck[i][0].Draw(camera.View, camera.Projection, camera.Transform.Translation);
                        tree.Linden.trunck[i][0].RestoreEffects();
                    }
                }

                // Finish rendering to depth map
                depthCapture.End();

                // Begin rendering the main render

                renderCapture.Begin();
                graphicsDevice.Clear(Color.CornflowerBlue);

                // sky.Draw();
                // Draw all models         
                if (terrain_)
                    terrain.Draw(camera.View, camera.Projection, camera.Transform.Translation);

                lensFlare.View = camera.View;
                lensFlare.Projection = camera.Projection;

                tree.Draw(camera.View, camera.Projection, camera.Transform.Translation);

                // Draw all of the models
                graphicsDevice.BlendState = BlendState.AlphaBlend;
                // foreach (Models model in models)
                //      model.Draw(camera.View, camera.Projection, camera.Transform.Translation);           

                rs.CullMode = CullMode.None;
                graphicsDevice.RasterizerState = rs;

                rs = new RasterizerState();
                rs.CullMode = CullMode.CullCounterClockwiseFace;
                graphicsDevice.RasterizerState = rs;

                water.Draw(camera);

                graphicsDevice.BlendState = BlendState.Opaque;

                // Finish the main render
                renderCapture.End();

                // Prepare to blur results of main render
                postprocessor.Input = renderCapture.GetTexture();
                // Output blur to our RenderCapture
                ((GaussianBlur)postprocessor).ResultCapture = blurCapture;
                // Perform blur
                postprocessor.Draw();

                // Set the three images to the DOF class
                dof.DepthMap = depthCapture.GetTexture();
                dof.Unblurred = renderCapture.GetTexture();
                dof.Input = ((GaussianBlur)postprocessor).ResultCapture.GetTexture();

                // Combine the images into the final result
                dof.Draw();
                #endregion
            }
            else if (!blur)
            {
                #region NotBlur
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.Clear(Color.CornflowerBlue);

                _visibleLights.Clear();
                foreach (Light light in _lights)
                {
                    if (light.LightType == Light.Type.Directional)
                    {
                        _visibleLights.Add(light);
                    }
                    else if (light.LightType == Light.Type.Spot)
                    {
                        if (camera.Frustum.Intersects(light.Frustum))
                            _visibleLights.Add(light);
                    }
                    else if (light.LightType == Light.Type.Point)
                    {
                        if (camera.Frustum.Intersects(light.BoundingSphere))
                            _visibleLights.Add(light);
                    }
                }

                rs = new RasterizerState();
                rs.CullMode = CullMode.None;
                rs.FillMode = FillMode.Solid;
                graphicsDevice.RasterizerState = rs;

                renderer.RenderScene(((FreeCamera)camera), gameTime, _visibleLights, new object[] { sky, terrain, water }, new object[] { tree.Fir, tree.Linden, tree.Palm });
                
                //tree.Draw(camera.View, camera.Projection, camera.Transform.Translation);
                //  water.PreDraw(camera, gameTime);       
               // if (terrain_)
               //     terrain.Draw(camera.View, camera.Projection, camera.Transform.Translation);

                // Draw all of the models
                //  foreach (Models model in models)
                //      model.Draw(camera.View, camera.Projection, camera.Transform.Translation);

                //sky.DrawRain(camera);
                //fire.Draw(camera);

                rs = new RasterizerState();
                rs.CullMode = CullMode.CullCounterClockwiseFace;
                graphicsDevice.RasterizerState = rs;
                graphicsDevice.BlendState = BlendState.Opaque;


                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                //spriteBatch.Draw(renderer.NormalBuffer, new Rectangle(0, 0, 200, 200), Color.White);
                //spriteBatch.Draw(renderer.DepthBuffer, new Rectangle(200, 0, 200, 200), Color.White);
                // spriteBatch.Draw(renderer._outputTexture, new Rectangle(0, 0, 600, 200), Color.White);            
                 spriteBatch.End();

                //FPS
                frameCounter++;
                spriteBatch.Begin();
              //  spriteBatch.DrawString(spriteFont, "FPS: " + frameRate, new Vector2(20, 20), Color.Black);
                spriteBatch.End();


                lensFlare.View = camera.View;
                lensFlare.Projection = camera.Projection;
                #endregion
            }


            base.Draw(gameTime);
        }
    }
}