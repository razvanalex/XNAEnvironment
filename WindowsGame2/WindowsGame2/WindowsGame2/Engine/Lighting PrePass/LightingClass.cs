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


namespace Engine.Shaders
{
    /// <summary>
    /// This class sets and draws lights in the game
    /// </summary>
    public class LightingClass
    {
        public List<Light> lights;
        public List<Light> visibleLights;
        public List<MovingLight>  moveLight;
        public float MaxAmbientIntensity = 0.4f;
        public float MinAmbientIntensity = 0.17f;

        public LightingClass()
        {
            visibleLights = new List<Light>();
            lights = new List<Light>();
            moveLight = new List<MovingLight>();
            Sky.SkyDomeSystem.MaxAmbientIntensity = MaxAmbientIntensity;
            Sky.SkyDomeSystem.MinAmbientIntensity = MinAmbientIntensity;
        }

        /// <summary>
        /// Add directional light. Only one call. Use this method if you want to draw with shadow mapping.
        /// </summary>
        /// <param name="DirectionalYaw">Yaw angle of light</param>
        /// <param name="DirectionalPitch">Pitch angle of light</param>
        /// <param name="DirectionalRoll">Roll angle of light</param>
        /// <param name="LightColor">The ambinet color light</param> 
        /// <param name="ShadowDistance">Maximum distance of shadow to be rendered</param>
        /// <param name="ShadowDepthBias">Set the depth bias</param>      
        public void AddDirectionalLight(float DirectionalYaw, float DirectionalPitch, float DirectionalRoll, Color LightColor, float Intensity, float ShadowDistance, float ShadowDepthBias)
        {
            CheckDirectionalLight();

            Light dirLight = new Light();
            dirLight.LightType = Light.Type.Directional;
            dirLight.Transform = Matrix.CreateFromYawPitchRoll(DirectionalYaw, DirectionalPitch, DirectionalRoll);
            dirLight.Color = LightColor;
            dirLight.Intensity = Intensity;
            dirLight.ShadowDistance = ShadowDistance;
            dirLight.ShadowDepthBias = ShadowDepthBias;
            dirLight.CastShadows = true;
            lights.Add(dirLight);
        }

        /// <summary>
        /// Add directional light. Only one call. Use this method if you want to draw without shadow mapping.
        /// </summary>
        /// <param name="DirectionalYaw">Yaw angle of light</param>
        /// <param name="DirectionalPitch">Pitch angle of light</param>
        /// <param name="DirectionalRoll">Roll angle of light</param>
        /// <param name="LightColor">The ambinet color light</param>
        /// <param name="Intensity">The intensity of light</param>
        /// <param name="CastShadows">Set false if you don't want shadow mapping. Othewise set (float)ShadowDistance and (float)ShadowDepthBias</param> 
        public void AddDirectionalLight(float DirectionalYaw, float DirectionalPitch, float DirectionalRoll, Color LightColor, float Intensity, bool CastShadows)
        {
            CheckDirectionalLight();

            Light dirLight = new Light();
            dirLight.LightType = Light.Type.Directional;
            dirLight.Transform = Matrix.CreateFromYawPitchRoll(DirectionalYaw, DirectionalPitch, DirectionalRoll);
            dirLight.Color = LightColor;
            dirLight.Intensity = Intensity;
            dirLight.CastShadows = CastShadows;
            if (CastShadows)
                throw new InvalidOperationException("Use AddDirectionalLight(float DirectionalYaw, float DirectionalPitch, float DirectionalRoll, Color LightColor, float Intensity, float ShadowDistance, float ShadowDepthBias) method for shadow mapping!");

            lights.Add(dirLight);

        }

        private void CheckDirectionalLight()
        {
            int noDirLight = 0;
            foreach (Light light in lights)
                if (light.LightType == Light.Type.Directional)
                {
                    noDirLight++;
                    if (noDirLight > 1)
                        throw new InvalidOperationException("Add only one directional light!");
                }
        }
        public void AddPointLight(float Radius, float Intensity, Color LightColor, Vector3 LightPosition)
        {
            Light Plight = new Light();
            Plight.LightType = Light.Type.Point;
            Plight.Radius = Radius;
            Plight.Intensity = Intensity;
            Plight.Color = LightColor;
            Plight.Transform = Matrix.CreateTranslation(LightPosition);
            lights.Add(Plight);
        }

        public void AddMovingPointLight(float Radius, float Intensity, Color LightColor, Vector3 startPoint, Vector3 endPoint, float speed)
        {
            Light Plight = new Light();
            Plight.LightType = Light.Type.Point;
            Plight.Radius = Radius;
            Plight.Intensity = Intensity;
            Plight.Color = LightColor;
            Plight.Transform = Matrix.CreateTranslation(startPoint);
            lights.Add(Plight);
            moveLight.Add(new MovingLight(Plight, startPoint, endPoint, speed));
        }

        public void AddSpotLight(float Radius, float Intensity, float SpotAngle, float SpotYaw, float SpotPitch, float SpotRoll, Color LightColor, Vector3 LightPosition, bool CastShadows)
        {
            Light spot = new Light();
            spot.LightType = Light.Type.Spot;
            spot.ShadowDepthBias = 0.0001f;
            spot.Radius = Radius;
            spot.Intensity = Intensity;
            spot.Color = LightColor;
            spot.CastShadows = CastShadows;
            spot.SpotAngle = SpotAngle;
        
            Matrix rotation = Matrix.CreateFromYawPitchRoll(SpotYaw, SpotPitch, SpotRoll);
            Matrix transform = rotation;
            transform.Translation = LightPosition;        

            float tan = (float)Math.Tan(MathHelper.ToRadians(SpotAngle));
            Matrix scale = Matrix.CreateScale(Radius * tan, Radius * tan, Radius);
            transform = scale * transform;
            
            spot.Transform = transform;
            lights.Add(spot);

        }

        public void AddSpotLight(float Radius, float Intensity, float SpotAngle, float SpotYaw, float SpotPitch, float SpotRoll, Color LightColor, Vector3 LightPosition, float ShadowDepthBias)
        {
            Light spot = new Light();
            spot.LightType = Light.Type.Spot;
            spot.ShadowDepthBias = ShadowDepthBias;
            spot.Radius = Radius;
            spot.Intensity = Intensity;
            spot.Color = LightColor;
            spot.CastShadows = true;
            spot.SpotAngle = SpotAngle;

            Matrix rotation = Matrix.CreateFromYawPitchRoll(SpotYaw, SpotPitch, SpotRoll);
            Matrix transform = rotation;
            transform.Translation = LightPosition;

            spot.Transform = transform;
            lights.Add(spot);
        }

        public void UpdatDirectionalLight(float DirectionalYaw, float DirectionalPitch, float DirectionalRoll, float Intensity)
        {
            foreach (Light light in lights)
                if (light.LightType == Light.Type.Directional)
                {
                    light.Transform = Matrix.CreateFromYawPitchRoll(DirectionalYaw, DirectionalPitch, DirectionalRoll);
                    light.Intensity = Intensity;
                }
        }

        public void UpdateMovingPoint(GameTime gameTime)
        {
            if (moveLight != null || moveLight.Count != 0)
                foreach (MovingLight light in moveLight)
                    light.Update((float)(gameTime.ElapsedGameTime.TotalSeconds));
        }
        public void SetLights()
        {
            visibleLights.Clear();

            foreach (Light light in lights)
            {
                if (light.LightType == Light.Type.Directional)
                {
                    visibleLights.Add(light);
                }
                else if (light.LightType == Light.Type.Spot)
                {
                 //   if (Camera.Camera.DefaultCamera.Frustum.Intersects(light.Frustum))
                  //  {
                        visibleLights.Add(light);
                     //   DebugShapeRenderer.AddBoundingFrustum(light.Frustum, Color.Red);
                  //  }
                }
                else if (light.LightType == Light.Type.Point)
                {
                    //if (Camera.Camera.DefaultCamera.Frustum.Intersects(light.BoundingSphere))
                  //  {
                        visibleLights.Add(light);
                        // DebugShapeRenderer.AddBoundingSphere(light.BoundingSphere, light.Color);
                  //  }
                }

            }
        }

        public Light DirLight()
        {
            Light DL = new Light();

            foreach (Light light in lights)
            {
                if (light.LightType == Light.Type.Directional)
                {
                    DL = light;
                }             
            }
            return DL;
        }
   
    }
}