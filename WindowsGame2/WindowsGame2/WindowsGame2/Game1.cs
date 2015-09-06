using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
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

namespace WindowsGame2
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Models> models = new List<Models>();

        //Camera
        Camera camera;
        MouseState lastMouseState;

        // DrawWater water;

        #region Sky
        //SkySphere
        //SkySphere sky;

        //SkyDome
        SkyDome sky;
        SkyDome.Weather weather;
        float Time = 0;
        Random random = new Random();
        int TIME;
        #endregion

        #region Water
        //Water
        Water water;
        float WaveLength = 0.003f;
        float WaveHeight = 0.06f;
        float WaveSpeed = 0.02f;
        Vector3 WaterPos = new Vector3(0, 600, 0);
        Vector2 WaterSize = new Vector2(30000, 30000);
        #endregion

        //FPS Counter
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        //font
        SpriteFont spriteFont;

        //Terrain
        Terrain terrain;

        Effect effect;

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

        const int noTypeOfTrees = 3;
        List<Billboards>[] trees = new List<Billboards>[noTypeOfTrees];
        float steepness;
        int[] noTree = new int[noTypeOfTrees];
        Vector3[][] PositionTree = new Vector3[noTypeOfTrees][];
        Vector3[][] RotationTree = new Vector3[noTypeOfTrees][];

        //Thread t1;
        float THeight = 2500;

        //Instancing

        Matrix[][][] instanceTransforms = new Matrix[noTypeOfTrees][][];
        Model[][] instancedModel = new Model[noTypeOfTrees][];
        Matrix[][][] instancedModelBones = new Matrix[noTypeOfTrees][][];

        //Sun
        Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));
        Vector3 LightColor = new Vector3(0, 0, 0);
        Vector3 AmbinetColor;

        LensFlareComponent lensFlare;

        public GameTime gametime;

        //ScreenShot
        int ScreenShotTime = 0;

        //Shadow
        //    PrelightingRenderer renderer;

        //Update
        int Water_Graph, oldWater_Graph, WaterState, oldWaterState;
        KeyboardState keyState, oldKeyState;

        int[] no_1 = new int[noTypeOfTrees];
        int[] no_2 = new int[noTypeOfTrees];
        int[] no_3 = new int[noTypeOfTrees];
        int[] no_4 = new int[noTypeOfTrees];
        int[] no_t = new int[noTypeOfTrees];

        bool fullScreen;
        bool[,][] visible_tree = new bool[noTypeOfTrees, 4][];
        double distance;
        float timer = 0;

        private enum Status
        {
            Manual = 0,
            Automatic = 1,
            ActualTime = 2
        }
        Status stat, prevStat;
        private KeyboardState previousKeyState;
        private GamePadState previousPadState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // graphics.IsFullScreen = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            lastMouseState = Mouse.GetState();
            camera = new FreeCamera(new Vector3(150, 75, 180), MathHelper.PiOver2 - 0.3f, 0, 1f, 1000000.0f, GraphicsDevice);

            IsFixedTimeStep = false;

            graphics.SynchronizeWithVerticalRetrace = true;

            lensFlare = new LensFlareComponent(this);
            Components.Add(lensFlare);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

          //  models.Add(new Models(Content.Load<Model>("models//ALan Tree//AlanTree"), new Vector3(0, 0, 0), new Vector3(0), new Vector3(5f), GraphicsDevice));//new Vector3(60, WaterPos.Y - 100f, 150)

            #region Sky

            //Old Sky
            // sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("textures//skyboxtexture"));
            GameTime gt = new GameTime();
            //New Sky
            sky = new SkyDome(this, (FreeCamera)camera, gt, GraphicsDevice);
            // Set skydome parameters here
            sky.Theta = 2.4f;// (float)Math.PI / 2.0f - 0.3f;
            sky.Parameters.NumSamples = 10;
            TIME = random.Next(5, 15);
            //TIME = 1;
            AmbinetColor = lensFlare.AmbientColor;

            weather = SkyDome.Weather.Clear;
            sky.WeatherChange(ref weather);

            #endregion

            //Effect cubeMapEffect = Content.Load<Effect>("shaders//AlphaBlending");
           // cubeMapEffect.Parameters["LightDirection"].SetValue(LightDirection);
            //     CubeMapReflectMaterial cubeMat = new CubeMapReflectMaterial(Content.Load<TextureCube>("textures//SkyBoxTex"));
           // models[0].SetModelEffect(cubeMapEffect, true);
            //    models[0].SetModelMaterial(cubeMat);           

            spriteFont = Content.Load<SpriteFont>("SpriteFont1");

            #region Terrain
            terrain = new Terrain(Content.Load<Texture2D>("textures//Terrain//terain"), 100f, THeight, 100, LightDirection, LightColor, GraphicsDevice, Content);
            TerrainTextures();
            #endregion

            water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);//65
            #region BillBoards
            Random rnd = new Random();
            Random rotRND = new Random();
            effect = Content.Load<Effect>("models//InstancedModel");
            Texture2D TreeMap = Content.Load<Texture2D>("textures//Terrain//Tree_Map");
            Color[] TreePixels = new Color[TreeMap.Width * TreeMap.Height];
            TreeMap.GetData<Color>(TreePixels);

            noTree[0] = 2500; //firs
            noTree[1] = 2500; //lindens
            noTree[2] = 2500; //palms

            int noTotalBillBoards = 0;

            for (int tree = 0; tree < noTypeOfTrees; tree++)
            {
                noTotalBillBoards += noTree[tree];
                PositionTree[tree] = new Vector3[noTree[tree]];
                RotationTree[tree] = new Vector3[noTree[tree]];
            }

            for (int tree = 0; tree < noTypeOfTrees; tree++)
                for (int i = 0; i < noTree[tree]; i++)
                {
                    float x = rnd.Next(-256 * 25, 256 * 25);
                    float z = rnd.Next(-256 * 25, 256 * 25);
                    float RotX = rotRND.Next(0, 64);

                    int xCoord = (int)(x / 25) + 256;
                    int zCoord = (int)(z / 25) + 256;

                    float texVal;
                    switch(tree)
                    {
                        case 0: texVal = TreePixels[zCoord * 512 + xCoord].R / 255f; break;
                        case 1: texVal = TreePixels[zCoord * 512 + xCoord].G / 255f; break;
                        case 2: texVal = TreePixels[zCoord * 512 + xCoord].B / 255f; break;
                        default: texVal = 0; break;
                    }                      

                    float y = terrain.GetHeightAtPosition(x, z, out steepness);

                    if ((int)((float)rnd.NextDouble() * texVal * 10) == 1)
                    {
                        PositionTree[tree][i] = new Vector3(x, y, z);
                        RotationTree[tree][i] = new Vector3(RotX, 0, 0);
                    }
                    else i--;
                }

            #region Instancing

            //Instancing
            for (int i = 0; i < 3; i++)
                instancedModel[i] = new Model[3];

            instancedModel[0][0] = Content.Load<Model>("models//Trees//Fir//fir_tree");
            instancedModel[0][1] = Content.Load<Model>("models//Trees//Fir//FarFir_Tree");
            instancedModel[0][2] = Content.Load<Model>("models//Trees//Fir//Far_FarFir_Tree");

            instancedModel[1][0] = Content.Load<Model>("models//Trees//Linden//Linden");
            instancedModel[1][1] = Content.Load<Model>("models//Trees//Linden//Far_FarLinden");
            instancedModel[1][2] = Content.Load<Model>("models//Trees//Linden//Far_FarLinden");

            instancedModel[2][0] = Content.Load<Model>("models//Trees//Palms//Palm");
            instancedModel[2][1] = Content.Load<Model>("models//Trees//Palms//Palm");
            instancedModel[2][2] = Content.Load<Model>("models//Trees//Palms//Palm");

            for (int tree = 0; tree < noTypeOfTrees; tree++)
                instancedModelBones[tree] = new Matrix[3][];
           
            for (int tree = 0; tree < noTypeOfTrees; tree++)
                instanceTransforms[tree] = new Matrix[3][];
            
            for (int tree = 0; tree < noTypeOfTrees; tree++)
                for (int i = 0; i < 3; i++ )
                    instancedModelBones[tree][i] = new Matrix[instancedModel[tree][i].Bones.Count];

            for (int tree = 0; tree < noTypeOfTrees; tree++)
                for (int i = 0; i < 3; i++)
                    instancedModel[tree][i].CopyAbsoluteBoneTransformsTo(instancedModelBones[tree][i]);

            #endregion

            for (int tree = 0; tree < noTypeOfTrees; tree++) 
                trees[tree] = new List<Billboards>();

            for (int tree = 0; tree < noTypeOfTrees; tree++)
                for (int i = 0; i < noTree[tree]; i++)
                    trees[tree].Add(new Billboards(Content, instancedModel[tree][2], instancedModel[tree], instancedModelBones[tree], instanceTransforms[tree], PositionTree[tree][i], RotationTree[tree][i], new Vector3(0.1f), LightDirection, LightColor,AmbinetColor, GraphicsDevice));

            #endregion

            //Effect shadowEffect = Content.Load<Effect>("shaders//VSM");
            // models[0].SetModelEffect(shadowEffect, true);
            // models[1].SetModelEffect(effect, true);
            /*
            renderer = new PrelightingRenderer(GraphicsDevice, Content);
            renderer.Models = models;
            renderer.Camera = camera;
            renderer.Lights = new List<PPPointLight>() {
                new PPPointLight(new Vector3(0, 1000, 0), Color.White, 20000),
            };
            renderer.ShadowLightPosition = new Vector3(0, 1000, 0);
            renderer.ShadowLightTarget = Vector3.Zero;
            renderer.DoShadowMapping = true;
            renderer.ShadowMult = 0.3f;
            */

            renderCapture = new RenderCapture(GraphicsDevice);
            postprocessor = new GaussianBlur(GraphicsDevice, Content, 2f);

            depthEffect = Content.Load<Effect>("shaders//DepthEffect");
            depthCapture = new RenderCapture(GraphicsDevice, SurfaceFormat.HalfSingle);

            blurCapture = new RenderCapture(GraphicsDevice, SurfaceFormat.Color);
            dof = new DepthOfField(GraphicsDevice, Content);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void TerrainTextures()
        {
            //weightMaps
            terrain.WeightMap[0] = Content.Load<Texture2D>("textures//Terrain//grass//weightMap");
            terrain.WeightMap[1] = Content.Load<Texture2D>("textures//Terrain//rock//weightMap");
            terrain.WeightMap[2] = Content.Load<Texture2D>("textures//Terrain//sand//weightMap");
            terrain.WeightMap[3] = Content.Load<Texture2D>("textures//Terrain//snow//weightMap");
            terrain.WeightMap[4] = Content.Load<Texture2D>("textures//Terrain//rocks_sand//weightMap");
            //textures  rocks_sand
            terrain.Textures[0] = Content.Load<Texture2D>("textures//Terrain//grass//grass");
            terrain.Textures[1] = Content.Load<Texture2D>("textures//Terrain//rock//rock");
            terrain.Textures[2] = Content.Load<Texture2D>("textures//Terrain//sand//sand");
            terrain.Textures[3] = Content.Load<Texture2D>("textures//Terrain//snow//snow");
            terrain.Textures[4] = Content.Load<Texture2D>("textures//Terrain//rocks_sand//rocks_sand");
            //detail
            terrain.DetailTexture = Content.Load<Texture2D>("textures//Terrain//noise_texture");
            //tiling
            terrain.textureTiling[0] = 1000;
            terrain.textureTiling[1] = 1000;
            terrain.textureTiling[2] = 100;
            terrain.textureTiling[3] = 100;
            terrain.textureTiling[4] = 1000;
        }

        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) || (Keyboard.GetState().IsKeyDown(Keys.Q)) || (Keyboard.GetState().IsKeyDown(Keys.Escape)))
                this.Exit();
            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyUp(Keys.LeftControl))
            {
                timer++;
                if (timer == 1)
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                updateCamera(gameTime);
                IsMouseVisible = false;
            }
            else
            {
                timer = 0;
                if (timer > 100)
                    timer = 100;
                IsMouseVisible = true;
            }

            fullScreen = graphics.IsFullScreen;

            if ((keyState.IsKeyDown(Keys.B) && oldKeyState.IsKeyUp(Keys.B)) && !blur)
                blur = true;
            else if ((keyState.IsKeyDown(Keys.B) && oldKeyState.IsKeyUp(Keys.B)) && blur)
                blur = false;

            if ((keyState.IsKeyDown(Keys.U)))
                models[0].Position += new Vector3(0, 3f, 0);
            if ((keyState.IsKeyDown(Keys.J)))
                models[0].Position -= new Vector3(0, 3f, 0);

            if (keyState.IsKeyDown(Keys.PageUp))
            {
                THeight += 10;
                terrain = new Terrain(Content.Load<Texture2D>("textures//Terrain//terain"), 100f, THeight, 1, new Vector3(1, -1, 0), LightColor, GraphicsDevice, Content);
                TerrainTextures();
            }
            else if (keyState.IsKeyDown(Keys.PageDown))
            {
                THeight -= 10;
                terrain = new Terrain(Content.Load<Texture2D>("textures//Terrain//terain"), 100f, THeight, 1, new Vector3(1, -1, 0), LightColor, GraphicsDevice, Content);
                TerrainTextures();
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
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                lastMouseState = Mouse.GetState();
            }
            else if ((keyState.IsKeyDown(Keys.F) && oldKeyState.IsKeyUp(Keys.F)))
            {
                graphics.ToggleFullScreen();
                deltaX = 0;
                deltaY = 0;
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                lastMouseState = Mouse.GetState();
            }
            oldWaterState = WaterState;
            if (keyState.IsKeyDown(Keys.NumPad1))
                WaterState = 1;
            else if (keyState.IsKeyDown(Keys.NumPad2))
                WaterState = 2;
            else if (keyState.IsKeyDown(Keys.NumPad3))
                WaterState = 3;
            else if (keyState.IsKeyDown(Keys.NumPad4))
                WaterState = 4;
            else if (keyState.IsKeyDown(Keys.NumPad5))
                WaterState = 5;

            if (WaterState != oldWaterState)
                switch (WaterState)
                {
                    case 1:
                        WaveLength = 0.6f;
                        WaveHeight = 0.2f;
                        WaveSpeed = 0.004f;
                        water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 2:
                        WaveLength = 0.6f;
                        WaveHeight = 0.2f;
                        WaveSpeed = 0.04f;
                        water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 3:
                        WaveLength = 0.003f;
                        WaveHeight = 0.01f;
                        WaveSpeed = 0.02f;
                        water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 4:
                        WaveLength = 0.06f;
                        WaveHeight = 0.02f;
                        WaveSpeed = 0.004f;
                        water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                }

            WaterState = 0;

            oldWater_Graph = Water_Graph;
            if (keyState.IsKeyDown(Keys.D1))
                Water_Graph = 1;
            else if (keyState.IsKeyDown(Keys.D2))
                Water_Graph = 2;
            else if (keyState.IsKeyDown(Keys.D3))
                Water_Graph = 3;
            else if (keyState.IsKeyDown(Keys.D4))
                Water_Graph = 4;
            else if (keyState.IsKeyDown(Keys.D5))
                Water_Graph = 5;

            lensFlare.Light_anim = sky.Theta;
            LightDirection = Vector3.Negate(Vector3.Reflect(lensFlare.LightDirection, Vector3.Up));
            LightColor = Vector3.Normalize(lensFlare.LightColor) * 2;
            AmbinetColor = lensFlare.AmbientColor;
       
            //  Thread t1 = new Thread(delegate()
           // {
            water = new Water(Content, GraphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
            //terrain = new Terrain(Content.Load<Texture2D>("textures//Terrain//Terain"), 100f, THeight, Content.Load<Texture2D>("textures//Terrain//grass"), 1, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, GraphicsDevice, Content);
          //  });
           // t1.Start();          
            
            for (int tree = 0; tree < noTypeOfTrees; tree++)
                trees[tree][0] = new Billboards(Content, instancedModel[tree][2], instancedModel[tree], instancedModelBones[tree], instanceTransforms[tree], PositionTree[tree][0], RotationTree[tree][0], new Vector3(0.1f), LightDirection, LightColor,AmbinetColor, GraphicsDevice);
            
            for (int tree = 0; tree < noTypeOfTrees; tree++)
                no_1[tree] = no_2[tree] = no_3[tree] = no_4[tree] = 0;

            for (int j = 0; j < 4; j++)
                for (int tree = 0; tree < noTypeOfTrees; tree++)
                {
                    visible_tree[tree, j] = new bool[noTree[tree]];
                    for (int i = 0; i < noTree[tree]; i++)
                        visible_tree[tree, j][i] = false;
                }

            for (int tree = 0; tree < noTypeOfTrees; tree++)
                for (int i = 0; i < noTree[tree]; i++)
                {
                    distance = Math.Sqrt(Math.Pow(((FreeCamera)camera).Position.X - trees[tree][i].position.X, 2) + Math.Pow(((FreeCamera)camera).Position.Y - trees[tree][i].position.Y, 2) + Math.Pow(((FreeCamera)camera).Position.Z - trees[tree][i].position.Z, 2));
                    if (distance < 100)
                    {
                        trees[tree][i] = new Billboards(Content, instancedModel[tree][0], instancedModel[tree], instancedModelBones[tree], instanceTransforms[tree], new Vector3(PositionTree[tree][i].X, terrain.GetHeightAtPosition(PositionTree[tree][i].X, PositionTree[tree][i].Z, out steepness), PositionTree[tree][i].Z), RotationTree[tree][i], new Vector3(0.1f), LightDirection, LightColor, AmbinetColor, GraphicsDevice);
                        no_1[tree]++; visible_tree[tree, 0][i] = true;
                    }
                    else if (distance >= 100 && distance <= 1000)
                    {
                        trees[tree][i] = new Billboards(Content, instancedModel[tree][1], instancedModel[tree], instancedModelBones[tree], instanceTransforms[tree], new Vector3(PositionTree[tree][i].X, terrain.GetHeightAtPosition(PositionTree[tree][i].X, PositionTree[tree][i].Z, out steepness), PositionTree[tree][i].Z), RotationTree[tree][i], new Vector3(0.1f), LightDirection, LightColor, AmbinetColor, GraphicsDevice);
                        no_2[tree]++; visible_tree[tree, 1][i] = true;
                    }
                    else if (distance > 1000 && distance <= 2000)
                    {
                        trees[tree][i] = new Billboards(Content, instancedModel[tree][2], instancedModel[tree], instancedModelBones[tree], instanceTransforms[tree], new Vector3(PositionTree[tree][i].X, terrain.GetHeightAtPosition(PositionTree[tree][i].X, PositionTree[tree][i].Z, out steepness), PositionTree[tree][i].Z), RotationTree[tree][i], new Vector3(0.1f), LightDirection, LightColor,AmbinetColor, GraphicsDevice);
                        no_3[tree]++; visible_tree[tree, 2][i] = true;
                    }
                    else if (distance > 2000)
                    {
                        no_4[tree]++; visible_tree[tree, 3][i] = true;
                    }                
                }
                        
            SKY(gameTime);
            
            for (int tree = 0; tree < noTypeOfTrees; tree++)
                no_t[tree] = no_1[tree] + no_2[tree] + no_3[tree] + no_4[tree];

            switch (Water_Graph)
            {
                case 1: RemoveWaterModels(); break;
                case 2: RemoveWaterModels(); LowWater(); break;
                case 3: RemoveWaterModels(); MediumWater(); break;
                case 4: RemoveWaterModels(); HighWater(); break;
            }

            //FPS Counter
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            for (int i = 0; i < noTypeOfTrees; i++)
                foreach (Billboards instance in trees[i])
                    instance.Update(gameTime);

            //ScreenShot
            ScreenShotTime++;
            if (ScreenShotTime == 2)
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                    ScreenShot();
            if (Keyboard.GetState().IsKeyUp(Keys.P))
                ScreenShotTime = 0;

            base.Update(gameTime);
        }

        void Weather()
        {
            Time += 0.001f;
            if (Time >= TIME)
            {
                sky.prevWeather = weather;
                Time = 0;
                random = new Random();
                TIME = random.Next(8, 16);
                weather = SkyDome.Weather.SomeClouds;
            }
            sky.WeatherChange(ref weather);
            Console.WriteLine("Time=" + Time);
            Console.WriteLine("TIME =" + TIME);
            Console.WriteLine("Weather = " + weather);
        }

        void SKY(GameTime gameTime)
        {
            sky.Update(gameTime);
            Weather();

            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if ((keyState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space)) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                previousPadState.Buttons.A == ButtonState.Released))
            {
                stat++;
                if ((int)stat == 3)
                    stat = Status.Manual;
            }

            terrain.lightDirection = lensFlare.LightDirection;
            terrain.lightColor = lensFlare.LightColor;
            switch (stat)
            {
                case Status.Manual:
                    sky.RealTime = false;
                    if (keyState.IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
                        sky.Theta -= 0.4f * step;
                    if (keyState.IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
                        sky.Theta += 0.4f * step;
                    if (sky.Theta > (float)Math.PI * 2.0f)
                        sky.Theta = sky.Theta - (float)Math.PI * 2.0f;
                    if (sky.Theta < 0.0f)
                        sky.Theta = (float)Math.PI * 2.0f + sky.Theta;
                    break;
                case Status.Automatic:
                    sky.RealTime = false;
                    sky.Theta += 0.001f * step;
                    if (sky.Theta > (float)Math.PI * 2.0f)
                        sky.Theta = sky.Theta - (float)Math.PI * 2.0f;
                    break;
                case Status.ActualTime:
                    sky.RealTime = true;
                    if (stat != prevStat)
                        sky.ApplyChanges();
                    break;
            }

            previousKeyState = keyState;
            previousPadState = GamePad.GetState(PlayerIndex.One);
            prevStat = stat;
        }

        void RemoveWaterModels()
        {
            for (int i = 0; i < 5; i++)
            {
                water.Objects.Remove(terrain);
                foreach (Models model in models)
                { water.Objects.Remove(model); }
                water.Objects.Remove(trees[0][0]);
                water.Objects.Remove(trees[1][0]);
                water.Objects.Remove(trees[2][0]);
            }
        }

        void LowWater()
        {
            water.Objects.Add(sky);
        }
        void MediumWater()
        {
            water.Objects.Add(sky);
            water.Objects.Add(terrain);
        }
        void HighWater()
        {
            water.Objects.Add(sky);
            water.Objects.Add(terrain);
            foreach (Models model in models)
                water.Objects.Add(model);
            water.Objects.Add(trees[0][0]);
            water.Objects.Add(trees[1][0]);
            water.Objects.Add(trees[2][0]);
        }

        void updateCamera(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            deltaX += (float)lastMouseState.X - (float)mouseState.X;
            deltaY += (float)lastMouseState.Y - (float)mouseState.Y;

            ((FreeCamera)camera).Rotate(deltaX * .01f, deltaY * .01f);
            Vector3 translation = Vector3.Zero;

            if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
            if (keyState.IsKeyDown(Keys.LeftShift))
                translation *= 3f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            else translation *= 0.3f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            ((FreeCamera)camera).Move(translation);

            camera.Update();
            lastMouseState = mouseState;
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }


        //ScreenShot
        int Count = 0;
        public void ScreenShot()
        {
#if WINDOWS
            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

            Count++;

            //force a frame to be drawn (otherwise back buffer is empty)
            Draw(new GameTime());

            //pull the picture from the buffer
            int[] backBuffer = new int[w * h];
            GraphicsDevice.GetBackBufferData(backBuffer);

            //copy into a texture
            Texture2D texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
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

        protected override void Draw(GameTime gameTime)
        {
            //  renderer.Draw();

            RasterizerState rs = new RasterizerState();
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (blur)
            {
                #region BLUR

                // sky.PreDraw(gameTime);
                // water.PreDraw(camera, gameTime);
                depthCapture.Begin();

                // Clear to white (max depth)
                GraphicsDevice.Clear(Color.White);

                foreach (Models model in models)
                {
                    model.CacheEffects(); // Cache effect
                    model.SetModelEffect(depthEffect, false); // Set depth effect
                    model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                    model.RestoreEffects(); // Restore effects
                }
                /*
                              foreach (Billboards tree in trees)
                                {
                                    tree.CacheEffects(); // Cache effect
                                    tree.SetModelEffect(depthEffect, false); // Set depth effect
                                    GraphicsDevice.BlendState = BlendState.Opaque;
                                    tree.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                                    tree.RestoreEffects(); // Restore effects

                                }*/

                sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                //  terrain.CacheEffects();
                //  terrain.SetModelEffect(depthEffect, false); 
                //  terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                //  terrain.RestoreEffects();

                // Finish rendering to depth map
                depthCapture.End();

                // Begin rendering the main render

                renderCapture.Begin();
                GraphicsDevice.Clear(Color.CornflowerBlue);

                // sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                // Draw all models         
                if (terrain_)
                    terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                // Draw all of the models
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                foreach (Models model in models)
                    model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                rs.CullMode = CullMode.None;
                GraphicsDevice.RasterizerState = rs;

                for (int tree = 0; tree < noTypeOfTrees; tree++)
                {
                    Array.Resize(ref instanceTransforms[tree][0], no_1[tree]);
                    Array.Resize(ref instanceTransforms[tree][1], no_2[tree]);
                    Array.Resize(ref instanceTransforms[tree][2], no_3[tree]);
                }

                int[] n1 = new int[noTypeOfTrees];
                int[] n2 = new int[noTypeOfTrees];
                int[] n3 = new int[noTypeOfTrees];

                for (int tree = 0; tree < noTypeOfTrees; tree++)
                    n1[tree] = n2[tree] = n3[tree] = 0;

                for (int tree = 0; tree < noTypeOfTrees; tree++)
                    for (int i = 0; i < noTree[tree]; i++)
                    {
                        if (visible_tree[tree, 0][i])
                        {
                            instanceTransforms[tree][0][n1[tree]] = trees[tree][i].Transform; n1[tree]++;
                        }
                        else if (visible_tree[tree, 1][i])
                        {
                            instanceTransforms[tree][1][n2[tree]] = trees[tree][i].Transform; n2[tree]++;
                        }
                        else if (visible_tree[tree, 2][i])
                        {
                            instanceTransforms[tree][2][n3[tree]] = trees[tree][i].Transform; n3[tree]++;
                        }
                    }

                ((Billboards)trees[0][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                ((Billboards)trees[1][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                ((Billboards)trees[2][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                rs = new RasterizerState();
                rs.CullMode = CullMode.CullCounterClockwiseFace;
                GraphicsDevice.RasterizerState = rs;

                //   water.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                GraphicsDevice.BlendState = BlendState.Opaque;

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

                water.PreDraw(camera, gameTime);
                rs.CullMode = CullMode.None;
                GraphicsDevice.RasterizerState = rs;
                sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                GraphicsDevice.Clear(Color.CornflowerBlue);
                sky.PreDraw(gameTime);

                sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                for (int tree = 0; tree < noTypeOfTrees; tree++)
                {
                    Array.Resize(ref instanceTransforms[tree][0], no_1[tree]);
                    Array.Resize(ref instanceTransforms[tree][1], no_2[tree]);
                    Array.Resize(ref instanceTransforms[tree][2], no_3[tree]);
                }
              
                int[] n1 = new int[noTypeOfTrees];
                int[] n2 = new int[noTypeOfTrees];
                int[] n3 = new int[noTypeOfTrees];
             
                for (int tree = 0; tree < noTypeOfTrees; tree++)
                    n1[tree] = n2[tree] = n3[tree] = 0;

                for (int tree = 0; tree < noTypeOfTrees; tree++)
                    for (int i = 0; i < noTree[tree]; i++)
                    {
                        if (visible_tree[tree, 0][i])
                        {
                            instanceTransforms[tree][0][n1[tree]] = trees[tree][i].Transform; n1[tree]++;
                        }
                        else if (visible_tree[tree, 1][i])
                        {
                            instanceTransforms[tree][1][n2[tree]] = trees[tree][i].Transform; n2[tree]++;
                        }
                        else if (visible_tree[tree, 2][i])
                        {
                            instanceTransforms[tree][2][n3[tree]] = trees[tree][i].Transform; n3[tree]++;
                        }
                    }

                ((Billboards)trees[0][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                ((Billboards)trees[1][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                ((Billboards)trees[2][0]).Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                water.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                // Draw all of the models
                foreach (Models model in models)
                    model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                rs = new RasterizerState();
                rs.CullMode = CullMode.CullCounterClockwiseFace;
                GraphicsDevice.RasterizerState = rs;
                GraphicsDevice.BlendState = BlendState.Opaque;

                if (terrain_)
                    terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                lensFlare.View = camera.View;
                lensFlare.Projection = camera.Projection;
                #endregion
            }

            //FPS
            frameCounter++;
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "FPS: " + frameRate, new Vector2(20, 20), Color.Black);
            spriteBatch.DrawString(spriteFont, "Light_anim: " + lensFlare.LightColor.ToString(), new Vector2(20, 40), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}