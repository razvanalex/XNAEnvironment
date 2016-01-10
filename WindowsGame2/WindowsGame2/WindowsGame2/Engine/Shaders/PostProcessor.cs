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
    public class PostProcessor
    {
        // Pixel shader
        public Effect Effect { get; protected set; }

        // Texture to process
        public Texture2D Input { get; set; }

        // GraphicsDevice and SpriteBatch for drawing
        protected GraphicsDevice graphicsDevice;
        protected static SpriteBatch spriteBatch;

        public PostProcessor(Effect Effect, GraphicsDevice graphicsDevice)
        {
            this.Effect = Effect;

            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphicsDevice);

            this.graphicsDevice = graphicsDevice;
        }

        // Draws the input texture using the pixel shader postprocessor
        public virtual void Draw()
        {
            // Set effect parameters if necessary
            if (Effect.Parameters["ScreenWidth"] != null)
                Effect.Parameters["ScreenWidth"].SetValue(
                    graphicsDevice.Viewport.Width);

            if (Effect.Parameters["ScreenHeight"] != null)
                Effect.Parameters["ScreenHeight"].SetValue(
                    graphicsDevice.Viewport.Height);

            // Initialize the spritebatch and effect
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            Effect.CurrentTechnique.Passes[0].Apply();

            // For reach compatiblity
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

            // Draw the input texture
            spriteBatch.Draw(Input, Vector2.Zero, Color.White);

            // End the spritebatch and effect
            spriteBatch.End();

            // Clean up render states changed by the spritebatch
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;
        }
    }

    public class RenderCapture
    {
        RenderTarget2D renderTarget;
        GraphicsDevice graphicsDevice;

        public RenderCapture(GraphicsDevice GraphicsDevice)
        {
            this.graphicsDevice = GraphicsDevice;
            renderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
                false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public RenderCapture(GraphicsDevice GraphicsDevice, SurfaceFormat Format)
        {
            this.graphicsDevice = GraphicsDevice;
            renderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
                false, Format, DepthFormat.Depth24);
        }

        // Begins capturing from the graphics device
        public void Begin()
        {
            graphicsDevice.SetRenderTarget(renderTarget);
        }

        // Stop capturing
        public void End()
        {
            graphicsDevice.SetRenderTarget(null);
        }

        // Returns what was captured
        public Texture2D GetTexture()
        {
            return renderTarget;
        }
    }
}
