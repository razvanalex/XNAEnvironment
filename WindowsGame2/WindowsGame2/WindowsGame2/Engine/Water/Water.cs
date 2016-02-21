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
using Engine.Sky;
using Engine.Terrain;
using Engine.Particles;
using Engine.Billboards;

namespace Engine.Water
{
    public class Water
    {
        public WaterSystem water;
        float WaveLength;
        float WaveHeight;
        float WaveSpeed;
        Vector3 WaterPos;
        Vector2 WaterSize;
        int Water_Graph = 2;
        int oldWater_Graph, WaterState, oldWaterState;
        KeyboardState keyState, oldKeyState;
        ContentManager Content;
        GraphicsDevice graphicsDevice;
        Vector3 LightDirection;
        Vector3 LightColor;
        float SunFactor;

        SkyDome sky;
        object Terrain;
        public List<Models> models = new List<Models>();
        public const int noTypeOfTrees = 3;
        Billboard[] trees;

        public Water(float WaveLength, float WaveHeight, float WaveSpeed,
            Vector3 WaterPos, Vector2 WaterSize, ContentManager Content,
            GraphicsDevice graphicsDevice, Vector3 LightDirection,
            Vector3 LightColor, float SunFactor)
        {
            this.WaveLength = WaveLength;
            this.WaveHeight = WaveHeight;
            this.WaveSpeed = WaveSpeed;
            this.WaterPos = WaterPos;
            this.WaterSize = WaterSize;
            this.Content = Content;
            this.graphicsDevice = graphicsDevice;
            this.LightDirection = LightDirection;
            this.LightColor = LightColor;
            this.SunFactor = SunFactor;
            Initialize();
        }

        void Initialize()
        {
            water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
            water.WaterHeight = WaterPos.Y;
        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                    Terrain = o;
                if (o is SmallTerrain)
                    Terrain = o;
                if (o is SkyDome)
                    sky = (SkyDome)o;
                if (o is List<Models>)
                    models = (List<Models>)o;
                if (o is Billboard[])
                    trees = (Billboard[])o;
            }
        }
        public void ChangeGraphics()
        {
            oldKeyState = keyState;
            keyState = Keyboard.GetState();

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
                        water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
                        break;
                    case 2:
                        WaveLength = 0.6f;
                        WaveHeight = 0.2f;
                        WaveSpeed = 0.04f;
                        water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
                        break;
                    case 3:
                        WaveLength = 0.003f;
                        WaveHeight = 0.01f;
                        WaveSpeed = 0.02f;
                        water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
                        break;
                    case 4:
                        WaveLength = 0.06f;
                        WaveHeight = 0.02f;
                        WaveSpeed = 0.004f;
                        water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
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

            switch (Water_Graph)
            {
                case 1: RemoveWaterModels(); break;
                case 2: RemoveWaterModels(); LowWater(); break;
                case 3: RemoveWaterModels(); MediumWater(); break;
                case 4: RemoveWaterModels(); HighWater(); break;
            }
        }
        void RemoveWaterModels()
        {
            water.Objects.RemoveAll(sky => true);
            water.Objects.RemoveAll(models => true);
            water.Objects.RemoveAll(trees => true);
            water.Objects.RemoveAll(Qtree => true);
        }
        void LowWater()
        {
            if (!sky.LowSky)
                water.Objects.Add(sky.sky);
            else water.Objects.Add(sky.skySphere);
        }
        void MediumWater()
        {
            if (!sky.LowSky)
                water.Objects.Add(sky.sky);
            else water.Objects.Add(sky.skySphere);

            if (Terrain is QuadTree)
                water.Objects.Add(((QuadTree)Terrain));
            else if (Terrain is SmallTerrain)
                water.Objects.Add(((SmallTerrain)Terrain));
        }
        void HighWater()
        {
            if (!sky.LowSky)
                water.Objects.Add(sky.sky);
            else water.Objects.Add(sky.skySphere);

            if (Terrain is QuadTree)
                water.Objects.Add(((QuadTree)Terrain));
            else if (Terrain is SmallTerrain)
                water.Objects.Add(((SmallTerrain)Terrain));

            foreach (Models model in models)
                water.Objects.Add(model);

            foreach (Billboard tree in trees)
                for (int lod = 0; lod < 3; lod++)
                    if (tree.instanceTransforms[lod].Length != 0)
                    {
                        water.Objects.Add(tree.trunck[lod][0]);
                        if (tree.Leaves)
                            for (int i = 0; i < tree.NoTrees; i++)
                            {
                                if (tree.LeavesAreVisible[i])
                                    for (int j = 0; j < tree.NoLeaves; j++)
                                        water.Objects.Add(tree.leaves[i][j]);
                            }
                    }

        }

        public void UpdateLight(Vector3 LightColor, Vector3 LightDirection, float SunFactor)
        {
            this.LightColor = LightColor;
            this.LightDirection = LightDirection;
            this.SunFactor = SunFactor;
            water = new WaterSystem(Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, SunFactor);
        }

        public void Draw(Camera.Camera camera)
        {
            water.Draw(camera.View, camera.Projection, camera.Transform.Translation);
        }
        public void PreDraw(Camera.Camera camera, GameTime gameTime)
        {
            water.PreDraw(camera, gameTime);
        }
    }
}