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


namespace Engine.Sky
{
    public class SkySphere : IRenderable 
    {
        Models model;
        
        Effect effect;
        GraphicsDevice graphics;
       
        public SkySphere(ContentManager Content, GraphicsDevice graphicsDevice, TextureCube Texture)
        {
            model = new Models(Content.Load<Model>("models//skySphere"), Vector3.Zero, Vector3.Zero, new Vector3(-100000), graphicsDevice);
            effect = Content.Load<Effect>("shaders//SkyBox");
            effect.Parameters["CubeMap"].SetValue(Texture);          
            model.SetModelEffect(effect, false);

            this.graphics = graphicsDevice;
        }
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            // Disable the depth buffer
            graphics.DepthStencilState = DepthStencilState.None;
            
            model.Position = CameraPosition;
            model.Draw(View, Projection, CameraPosition);

            graphics.DepthStencilState = DepthStencilState.Default;
        }
        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
    }
}
