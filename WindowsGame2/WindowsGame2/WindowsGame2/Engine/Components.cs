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

using WindowsGame2;
using Engine.Terrain;
using Engine.Camera;

namespace Engine
{
    public class Components : Microsoft.Xna.Framework.Game
    {
        GraphicsDevice graphicsDevice;
        int Count = 0;
        public Components(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void TerrainTextures(QuadTree Qtree, ContentManager Content)
        {
            //TexturesMaps
            Qtree.TexturesMaps[0] = Content.Load<Texture2D>("textures//Terrain//grass//GrassMap");
            Qtree.TexturesMaps[1] = Content.Load<Texture2D>("textures//Terrain//rock//RockMap");
            Qtree.TexturesMaps[2] = Content.Load<Texture2D>("textures//Terrain//sand//SandMap");
            Qtree.TexturesMaps[3] = Content.Load<Texture2D>("textures//Terrain//snow//SnowMap");
            Qtree.TexturesMaps[4] = Content.Load<Texture2D>("textures//Terrain//rocks_sand//Rocks_SandMap");
            Qtree.TexturesMaps[5] = Content.Load<Texture2D>("textures//Terrain//beach_sand//Beach_SandMap");
            //Textures
            Qtree.Textures[0] = Content.Load<Texture2D>("textures//Terrain//grass//grass");
            Qtree.Textures[1] = Content.Load<Texture2D>("textures//Terrain//rock//rock");
            Qtree.Textures[2] = Content.Load<Texture2D>("textures//Terrain//sand//sand");
            Qtree.Textures[3] = Content.Load<Texture2D>("textures//Terrain//snow//snow");
            Qtree.Textures[4] = Content.Load<Texture2D>("textures//Terrain//rocks_sand//rocks_sand");
            Qtree.Textures[5] = Content.Load<Texture2D>("textures//Terrain//beach_sand//beach_sand");
            //Detail Texture
            Qtree.DetailTexture = Content.Load<Texture2D>("textures//Terrain//noise_texture");
            //Tiling
            Qtree.textureTiling[0] = 1000;
            Qtree.textureTiling[1] = 100;
            Qtree.textureTiling[2] = 100;
            Qtree.textureTiling[3] = 1000;
            Qtree.textureTiling[4] = 1000;
            Qtree.textureTiling[5] = 500;
        }
        public void InitializeTerrin(QuadTree Qtree, FreeCamera camera, Terrain.Terrain terrain, Vector3 WaterPos, ContentManager Content)
        {

            FreeCamera.DefaultCamera = ((FreeCamera)camera);
            float quadFrontTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildFrontTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float quadFarTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildFarTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float vertexDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.VertexTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float nodeRelevance = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildRelevanceThreshold], CultureInfo.GetCultureInfo("en-us"));

            for (int i = 0; i < terrain.QuadTrees.Count; i++)
            {
                terrain.QuadTrees[i].NodeRelevance = nodeRelevance;
                terrain.QuadTrees[i].QuadTreeDetailAtFront = quadFrontTreeDetail;
                terrain.QuadTrees[i].QuadTreeDetailAtFar = quadFarTreeDetail;
                terrain.QuadTrees[i].VertexDetail = vertexDetail;
                LoadGround(terrain.QuadTrees[i], Content);
            }

            Qtree.effect = Content.Load<Effect>("Effects//Terrain");
            terrain.Initialize();
            terrain.Load(graphicsDevice);
            TerrainTextures(Qtree, Content);
            Qtree.WaterHeight = WaterPos.Y;

        }
        public void LoadGround(QuadTree tree, ContentManager Content)
        {
            string heightMapTextureName = System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.HeightGrayScaleImage];
            using (Texture2D heightMap = Content.Load<Texture2D>(heightMapTextureName))
            {
                LoadHeightData(tree, heightMap);
            }
        }
        private void LoadHeightData(QuadTree tree, Texture2D heightMap)
        {
            Color[] heightMapColors = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData(heightMapColors);

            tree.HeightData = new float[heightMap.Width, heightMap.Height];
            for (int x = 0; x < heightMap.Width; x++)
                for (int y = 0; y < heightMap.Height; y++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapColors[y * heightMap.Width + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Multiply by max height to get final height
                    tree.HeightData[x, y] = amt * Game1.THeight;
                }
        }
        public void updateCamera(GameTime gameTime, float deltaX, float deltaY, FreeCamera camera, MouseState lastMouseState)
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
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
        }
    }
}
