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

namespace Engine.Water
{
    public interface IRenderable
    {
        /// <summary>
        /// Draw without lighting. Simple drawing.
        /// </summary>
        /// <param name="View"></param>
        /// <param name="Projection"></param>
        /// <param name="CameraPosition"></param>
        void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);
        void SetClipPlane(Vector4? Plane);
    }

    // Make Water IRenderable
    public class WaterSystem :IRenderable
    {
        public Models waterMesh;
        Effect waterEffect;

        ContentManager content;
        GraphicsDevice graphics;

        RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        const float waterHeight = 5.0f;
        RenderTarget2D refractionTarg;

        public float WaterHeight = 0;

        public WaterSystem(ContentManager content, GraphicsDevice graphics,
            Vector3 position, Vector2 size, float WaveLength, float WaveHeight, float WaveSpeed, Vector3 LightDirection, Vector3 LightColor, float SunFactor)
        {
            this.content = content;
            this.graphics = graphics;

            waterMesh = new Models(content, content.Load<Model>("models//plane"), position, new Vector3(0, -MathHelper.PiOver2, 0), new Vector3(size.X, 1, size.Y), graphics);
            waterEffect = content.Load<Effect>("Effects//water");

            waterMesh.SetModelEffect(waterEffect, false);
            waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
            waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("textures//Water//wave0"));
            waterEffect.Parameters["WaterNormalMap1"].SetValue(content.Load<Texture2D>("textures//Water//wave1"));
            waterEffect.Parameters["WaveLength"].SetValue(WaveLength);
            waterEffect.Parameters["WaveHeight"].SetValue(WaveHeight);
            waterEffect.Parameters["WaveSpeed"].SetValue(WaveSpeed);
            waterEffect.Parameters["LightDirection"].SetValue(LightDirection);
            waterEffect.Parameters["LightColor"].SetValue(LightColor);
            waterEffect.Parameters["SunFactor"].SetValue(SunFactor);
            waterEffect.Parameters["WaterHeight"].SetValue(waterMesh.Position.Y);
            refractionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            
            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void renderReflection(Camera.Camera camera)
        {
            // Reflect the camera's properties across the water plane
            Vector3 reflectedCameraPosition = camera.Transform.Translation;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh.Position.Y * 2;
            
            Vector3 reflectedCameraTarget = ((FreeCamera)camera).Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + waterMesh.Position.Y * 2;

            // Create a temporary camera to render the reflected scene
            Camera.Camera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, camera.NearPlane, camera.FarPlane, graphics);
            reflectionCamera.Update();
            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);

            // Set the render target
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(ClearOptions.Target, Color.CornflowerBlue, 1f, 0);

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
            Vector3 refractedCameraPosition = camera.Transform.Translation;
            Vector3 refractedCameraTarget = camera.Target;

            // Create a temporary camera to render the rafracted scene
           // Camera.Camera refractionCamera = new TargetCamera(refractedCameraPosition, refractedCameraTarget, camera.NearPlane, camera.FarPlane, graphics);
           // refractionCamera.Update();

            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["RefractedView"].SetValue(camera.View);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, waterMesh.Position.Y);

            // Set the render target
            graphics.SetRenderTarget(refractionTarg);
            graphics.Clear(new Color(0.10980f, 0.30196f, 0.49412f)); //(122, 144, 255)

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);
                renderable.Draw(camera.View, camera.Projection, refractedCameraPosition);
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
           
            waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            waterMesh.Draw(View, Projection, CameraPosition);            
        }

        public void RenderToGBuffer(Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            waterMesh.RenderToGBuffer(camera, graphicsDevice);
        }
        public void RenderShadowMap(ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            waterMesh.RenderShadowMap(ref viewProj, graphicsDevice);
        }
        public void Draw(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            waterMesh.Draw(camera, graphicsDevice, lightBuffer); 
        }

        public void SetClipPlane(Vector4? Plane)
        {
            foreach (ModelMesh mesh in waterMesh.Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect.Parameters["ClipPlaneEnabled"] != null)
                        part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

                    if (Plane.HasValue)
                        if (part.Effect.Parameters["ClipPlane"] != null)
                            part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
                }
        }
    }
}