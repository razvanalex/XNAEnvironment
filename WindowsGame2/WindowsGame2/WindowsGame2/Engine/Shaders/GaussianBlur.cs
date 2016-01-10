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
    public class GaussianBlur : PostProcessor
    {
        float blurAmount;

        float[] weightsH, weightsV;
        float[] offsetsH, offsetsV;

        RenderCapture capture;

        public RenderCapture ResultCapture = null;

        public GaussianBlur(GraphicsDevice graphicsDevice, ContentManager Content,
            float BlurAmount)
            : base(Content.Load<Effect>("shaders//GaussianBlur"), graphicsDevice)
        {
            this.blurAmount = BlurAmount;

            // Calculate weights/offsets for horizontal pass
            calcSettings(1.0f / (float)graphicsDevice.Viewport.Width, 0,
                out weightsH, out offsetsH);

            // Calculate weights/offsets for vertical pass
            calcSettings(0, 1.0f / (float)graphicsDevice.Viewport.Height,
                out weightsV, out offsetsV);

            capture = new RenderCapture(graphicsDevice);
        }

        void calcSettings(float w, float h,
            out float[] weights, out float[] offsets)
        {
            // 15 Samples
            weights = new float[15];
            offsets = new float[15];

            // Calulate values for center pixel
            weights[0] = gaussianFn(0);
            offsets[0] = new float();

            float total = weights[0];

            // Calculate samples in pairs
            for (int i = 0; i < 7; i++)
            {
                // Weight each pair of samples according to Gaussian function
                float weight = gaussianFn(i + 1);
                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;
                total += weight * 2;

                // Samples are offset by 1.5 pixels, to make use of
                // filtering halfway between pixels
                float offset = i * 2 + 1.5f;
                Vector2 offsetVec = new Vector2(w, h) * offset;
                offsets[i * 2 + 1] = offsetVec.X;
                offsets[i * 2 + 2] = -offsetVec.X;
            }

            // Divide all weights by total so they will add up to 1
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= total;
        }

        float gaussianFn(float x)
        {
            return (float)((1.0f / Math.Sqrt(2 * Math.PI * blurAmount * blurAmount)) *
                Math.Exp(-(x * x) / (2 * blurAmount * blurAmount)));
        }

        public override void Draw()
        {
            // Set values for horizontal pass
            Effect.Parameters["offsets"].SetValue(offsetsH);
            Effect.Parameters["weights"].SetValue(weightsH);

            // Render this pass into the RenderCapture
            capture.Begin();
            base.Draw();
            capture.End();

            // Get the results of the first pass
            Input = capture.GetTexture();

            if (ResultCapture != null)
                ResultCapture.Begin();

            // Set values for the vertical pass
            Effect.Parameters["offsets"].SetValue(offsetsV);
            Effect.Parameters["weights"].SetValue(weightsV);

            // Render the final pass
            base.Draw();

            if (ResultCapture != null)
                ResultCapture.End();
        }
    }
}
