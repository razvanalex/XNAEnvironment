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


using Engine.Camera;
using Engine.Sky;

namespace Engine
{
    public interface IRenderable
    {
        void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);
        void SetClipPlane(Vector4? Plane);
    }

    // Make Water IRenderable
    public class Water
    {
        Models waterMesh;
        Effect waterEffect;

        ContentManager content;
        GraphicsDevice graphics;

        RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        const float waterHeight = 5.0f;
        RenderTarget2D refractionTarg;

        public float WaterHeight = 0;

        public Water(ContentManager content, GraphicsDevice graphics,
            Vector3 position, Vector2 size, float WaveLength, float WaveHeight, float WaveSpeed, Vector3 LightDirection, Vector3 LightColor, float SunFactor)
        {
            this.content = content;
            this.graphics = graphics;

            waterMesh = new Models(content.Load<Model>("models//plane"), position,
                new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(size.X, 1, size.Y), graphics);

            waterEffect = content.Load<Effect>("Effects//water");
            waterMesh.SetModelEffect(waterEffect, false);
            waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
            waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("textures//Water//wave0"));
            waterEffect.Parameters["WaterNormalMap1"].SetValue(content.Load<Texture2D>("textures//Water//wave1"));
            waterEffect.Parameters["WaveLength"].SetValue(WaveLength);
            waterEffect.Parameters["WaveHeight"].SetValue(WaveHeight);
            waterEffect.Parameters["WaveSpeed"].SetValue(WaveSpeed);
            waterEffect.Parameters["LightDirection"].SetValue(Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)));
            waterEffect.Parameters["LightColor"].SetValue(LightColor);
            waterEffect.Parameters["SunFactor"].SetValue(SunFactor);
            waterEffect.Parameters["WaterHeight"].SetValue(WaterHeight);
            refractionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            
            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void renderReflection(Camera.Camera camera)
        {
            // Reflect the camera's properties across the water plane
            Vector3 reflectedCameraPosition = ((FreeCamera)camera).Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh.Position.Y * 2;

            Vector3 reflectedCameraTarget = ((FreeCamera)camera).Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + waterMesh.Position.Y * 2;

            // Create a temporary camera to render the reflected scene
            Camera.Camera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, ((FreeCamera)camera).nearPlane, ((FreeCamera)camera).farPlane, graphics);
            reflectionCamera.Update();
            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);

            // Set the render target
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(ClearOptions.Target, Color.CornflowerBlue, 1f, 0);  // blacknew Color(122, 144, 255)

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);

                renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);
                renderable.SetClipPlane(null);
            }

            graphics.SetRenderTarget(null);

            // Set the reflected scene to its effect parameter in
            // the water effect
            waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
        }

        public void renderRefraction(Camera.Camera camera)
        {
            // Refract the camera's properties across the water plane
            Vector3 refractedCameraPosition = ((FreeCamera)camera).Position;
            Vector3 refractedCameraTarget = ((FreeCamera)camera).Target;

            // Create a temporary camera to render the rafracted scene
            Camera.Camera refractionCamera = new TargetCamera(refractedCameraPosition, refractedCameraTarget, ((FreeCamera)camera).nearPlane, ((FreeCamera)camera).farPlane, graphics);
            refractionCamera.Update();

            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["RefractedView"].SetValue(refractionCamera.View);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, waterMesh.Position.Y);

            // Set the render target
            graphics.SetRenderTarget(refractionTarg);
            graphics.Clear(new Color(0.10980f, 0.30196f, 0.49412f)); //(122, 144, 255)
           // graphics.Clear(ClearOptions.Target, Color.CornflowerBlue, 1f, 0); 

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);
                renderable.Draw(refractionCamera.View, refractionCamera.Projection, refractedCameraPosition);
                renderable.SetClipPlane(null);
            }

            graphics.SetRenderTarget(null);

            // Set the reflected scene to its effect parameter in the water effect
            waterEffect.Parameters["RefractionMap"].SetValue(refractionTarg);
        }


        public void PreDraw(Camera.Camera camera, GameTime gameTime)
        {
            renderRefraction(camera);
            renderReflection(camera);
            renderRefraction(camera);

            waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            waterMesh.Draw(View, Projection, CameraPosition);
        }
    }

    public class DrawWater : DrawableGameComponent
    {
        public Water water;
        Game game;
        LensFlareComponent lensFlare;
        Camera.Camera camera;
        SmallTerrain terrain;
        SkyDome sky;
        List<Models> models;
        GraphicsDevice graphicsDevice;

        Vector3 LightDirection { get; set; }
        Vector3 LightColor { get; set; }
        Vector3 WaterPos = new Vector3(0, 600, 0);
        Vector2 WaterSize = new Vector2(30000, 30000);
        float WaveLength = 0.003f;
        float WaveHeight = 0.06f;
        float WaveSpeed = 0.02f;
        int Water_Graph, oldWater_Graph, WaterState, oldWaterState;

        public DrawWater(Game game, Camera.Camera Camera, Vector3 LightDirection, Vector3 LightColor, SmallTerrain terrain, SkyDome sky, List<Models> models, LensFlareComponent lensFlare, GraphicsDevice graphicsDevice)
            : base(game)
        {
            this.camera = Camera;
            this.LightDirection = LightDirection;
            this.LightColor = LightColor;
            this.terrain = terrain;
            this.sky = sky;
            this.models = models;
            this.lensFlare = lensFlare;
            this.graphicsDevice = graphicsDevice;
            this.game = game;
        }

        protected override void LoadContent()
        {
            water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);//65
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);//65
            UpdateWater();

            base.Update(gameTime);
        }

        
        void UpdateWater()
        {
            KeyboardState keyState = new KeyboardState();
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

            LightDirection = Vector3.Negate(Vector3.Reflect(lensFlare.LightDirection, Vector3.Up));
            LightColor = Vector3.Normalize(lensFlare.LightColor) * 2;

            if (WaterState != oldWaterState)
                switch (WaterState)
                {
                    case 1:
                        WaveLength = 0.6f;
                        WaveHeight = 0.2f;
                        WaveSpeed = 0.004f;
                        water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 2:
                        WaveLength = 0.6f;
                        WaveHeight = 0.2f;
                        WaveSpeed = 0.04f;
                        water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 3:
                        WaveLength = 0.003f;
                        WaveHeight = 0.01f;
                        WaveSpeed = 0.02f;
                        water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
                        break;
                    case 4:
                        WaveLength = 0.06f;
                        WaveHeight = 0.02f;
                        WaveSpeed = 0.004f;
                        water = new Water(game.Content, graphicsDevice, WaterPos, WaterSize, WaveLength, WaveHeight, WaveSpeed, Vector3.Negate(Vector3.Reflect(LightDirection, Vector3.Up)), LightColor, lensFlare.SunFactor);
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
            for (int i = 0; i < 5; i++)
            {
                water.Objects.Remove(terrain);
                foreach (Models model in models)
                { water.Objects.Remove(model); }
                // water.Objects.Remove(trees[0]); 
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
            //water.Objects.Add(trees[0]);
        }


        public void Draw(GameTime gameTime)
        {
            water.PreDraw(camera, gameTime);
            graphicsDevice.Clear(Color.CornflowerBlue);
            water.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

            foreach (Models model in models)
                model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

           //  base.Draw(gameTime);
        }

    }

}