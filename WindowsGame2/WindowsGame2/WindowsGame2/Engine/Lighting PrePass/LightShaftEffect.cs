//-----------------------------------------------------------------------------
// LightPrePass.cs
//
// Jorge Adriano Luna 2012
// http://jcoluna.wordpress.com
//-----------------------------------------------------------------------------
#region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Engine.Shaders
{
    public class LightShaftEffect
    {
        /// <summary>
        /// Effect used
        /// </summary>
        protected Effect _effect;

        /// <summary>
        /// Effect parameters, cached for efficiency
        /// </summary>
        protected EffectParameter _parameterColorBuffer;
        protected EffectParameter _parameterScreenRes;
        protected EffectParameter _parameterSaturation;
        protected EffectParameter _parameterLinearColorBalance;
        protected EffectParameter _parameterLinearExposure;
        protected EffectParameter _parameterContrast;

        protected EffectParameter _parameterPixelSize;
        protected EffectParameter _parameterHalfPixel;
        protected EffectParameter _parameterHalfDepthTexture;
        protected EffectParameter _parameterRGBShaftTexture;
        protected EffectParameter _parameterLightCenter;
        protected EffectParameter _parameterScale;
        protected EffectParameter _parameterIntensity;
        protected EffectParameter _parameterSpread;
        protected EffectParameter _parameterTint;
        protected EffectParameter _parameterDecay;
        protected EffectParameter _parameterBlend;
        protected EffectParameter _parameterTextureAspectRatio;
        /// <summary>
        /// QuadRenderer used to do the fullscreen pass and the internal steps
        /// </summary>
        protected static QuadRenderer _quadRenderer;

        /// <summary>
        /// Parameters used to control this effect
        /// </summary>
        private float _blend = 0.5f;
        private float _scale = 4;
        private float _intensity = 1;
        private float _spread = 0.005f;
        private float _decay = 0.5f;
        private Color _shaftTint = Color.White;
        private Vector2 _lightCenter = Vector2.Zero;

        private float _saturation = 1;
        private float _contrast = 1;
        private float _exposure = 1;

        private Color _colorBalance = Color.White;
        protected Vector4 _linearColorBalance = Vector4.One;

        public float Intensity
        {
            get { return _intensity; }
            set
            {
                _intensity = value;
                if (_parameterIntensity != null)
                    _parameterIntensity.SetValue(Intensity);
            }
        }

        public Vector2 LightCenter
        {
            get { return _lightCenter; }
            set
            {
                _lightCenter = value;
                if (_parameterLightCenter != null)
                    _parameterLightCenter.SetValue(value);
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (_parameterScale != null)
                    _parameterScale.SetValue(value);
            }
        }

        public float Spread
        {
            get { return _spread; }
            set
            {
                _spread = value;
                if (_parameterSpread != null)
                    _parameterSpread.SetValue(value);
            }
        }
        public float Decay
        {
            get { return _decay; }
            set
            {
                _decay = value;
                if (_parameterDecay != null)
                    _parameterDecay.SetValue(value);
            }
        }

        public Color ShaftTint
        {
            get { return _shaftTint; }
            set
            {
                _shaftTint = value;
                if (_parameterTint != null)
                    _parameterTint.SetValue(value.ToVector4());
            }
        }
        public float Blend
        {
            get { return _blend; }
            set
            {
                _blend = value;
                if (_parameterBlend != null)
                    _parameterBlend.SetValue(value);
            }
        }

        public float Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                if (_parameterSaturation != null)
                    _parameterSaturation.SetValue(value);
            }
        }

        public float Contrast
        {
            get { return _contrast; }
            set
            {
                _contrast = value;
                if (_parameterContrast != null)
                    _parameterContrast.SetValue(value);
            }
        }

        public float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                if (_parameterLinearExposure != null)
                    _parameterLinearExposure.SetValue((float)Math.Pow(2, _exposure));
            }
        }

        public Color ColorBalance
        {
            get { return _colorBalance; }
            set
            {
                _colorBalance = value;
                _linearColorBalance.X = (float)Math.Pow(_colorBalance.R / 255.0f, 2.2f);
                _linearColorBalance.Y = (float)Math.Pow(_colorBalance.G / 255.0f, 2.2f);
                _linearColorBalance.Z = (float)Math.Pow(_colorBalance.B / 255.0f, 2.2f);
                if (_parameterLinearColorBalance != null)
                    _parameterLinearColorBalance.SetValue(_linearColorBalance);
            }
        }

        /// <summary>
        /// Initialize the effect. Loads the shader file, extract the parameters, apply
        /// the default values
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="renderer"></param>
        public void Init(ContentManager contentManager, Renderer renderer)
        {
            _quadRenderer = new QuadRenderer();
            try
            {
                _effect = contentManager.Load<Effect>("Shaders/LightShaft");
                ExtractParameters();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading bloom depth effect: " + ex.ToString());
            }
        }



        protected void ExtractParameters()
        {
            _parameterColorBuffer = _effect.Parameters["ColorBuffer"];
            _parameterScreenRes = _effect.Parameters["ScreenRes"];
            _parameterSaturation = _effect.Parameters["Saturation"];
            _parameterLinearColorBalance = _effect.Parameters["LinearColorBalance"];
            _parameterLinearExposure = _effect.Parameters["LinearExposure"];
            _parameterContrast = _effect.Parameters["Contrast"];

            _parameterLinearColorBalance.SetValue(_linearColorBalance);
            _parameterSaturation.SetValue(_saturation);
            _parameterContrast.SetValue(_contrast);
            _parameterLinearExposure.SetValue((float)Math.Pow(2, _exposure));

            _parameterPixelSize = _effect.Parameters["PixelSize"];
            _parameterHalfPixel = _effect.Parameters["HalfPixel"];
            _parameterHalfDepthTexture = _effect.Parameters["DepthBuffer"];
            _parameterRGBShaftTexture = _effect.Parameters["ShaftBuffer"];

            _parameterScale = _effect.Parameters["Scale"];
            _parameterIntensity = _effect.Parameters["Intensity"];
            _parameterSpread = _effect.Parameters["Spread"];
            _parameterTint = _effect.Parameters["ShaftTint"];
            _parameterDecay = _effect.Parameters["Decay"];
            _parameterLightCenter = _effect.Parameters["LightCenter"];
            _parameterBlend = _effect.Parameters["Blend"];

            _parameterScale.SetValue(Scale);
            _parameterIntensity.SetValue(Intensity);
            _parameterSpread.SetValue(Spread);
            _parameterTint.SetValue(_shaftTint.ToVector4());
            _parameterDecay.SetValue(_decay);

            _parameterTextureAspectRatio = _effect.Parameters["TextureAspectRatio"];

            if (_parameterBlend != null)
                _parameterBlend.SetValue(_blend);
        }
        public void RenderPostFx(Renderer renderer, GraphicsDevice device, RenderTarget2D srcTarget, RenderTarget2D dstTarget)
        {
            RenderTarget2D quarter0 = renderer.QuarterBuffer0;
            RenderTarget2D quarter1 = renderer.QuarterBuffer1;
            RenderTarget2D halfDepth = renderer.GetDownsampledDepth();

            _effect.CurrentTechnique = _effect.Techniques[0];
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            //render to a half-res buffer
            device.SetRenderTarget(quarter0);
            _parameterColorBuffer.SetValue(srcTarget);
            _parameterHalfDepthTexture.SetValue(halfDepth);
            _parameterTextureAspectRatio.SetValue(srcTarget.Height / (float)srcTarget.Width);
            // Convert to rgb first, so we have linear filtering
            _effect.CurrentTechnique = _effect.Techniques[0];

            Vector2 pixelSize = new Vector2(1.0f / (float)srcTarget.Width, 1.0f / (float)srcTarget.Height);
            _parameterPixelSize.SetValue(pixelSize);
            _parameterHalfPixel.SetValue(pixelSize * 0.5f);

            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);

            pixelSize = new Vector2(1.0f / (float)quarter0.Width, 1.0f / (float)quarter0.Height);
            _parameterPixelSize.SetValue(pixelSize);
            _parameterHalfPixel.SetValue(pixelSize * 0.5f);
            _effect.CurrentTechnique = _effect.Techniques[1];

            device.SetRenderTarget(quarter1);
            _parameterRGBShaftTexture.SetValue(quarter0);

            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);


            device.SetRenderTarget(dstTarget);

            pixelSize = new Vector2(1.0f / (float)srcTarget.Width, 1.0f / (float)srcTarget.Height);
            _parameterPixelSize.SetValue(pixelSize);
            _parameterHalfPixel.SetValue(pixelSize * 0.5f);

            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;

            device.BlendState = BlendState.Opaque;

            _parameterRGBShaftTexture.SetValue(quarter1);
            _effect.CurrentTechnique = _effect.Techniques[2];
            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);
            device.SetRenderTarget(null);
        }
    }
}