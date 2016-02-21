using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Engine.Shaders
{
    public class PostProcessingComponent
    {
        private Light _mainLight;
        private LightShaftEffect _lightShaftEffect;

        private float _intensity = 1f;
        private float _saturation = 1.3f;//1.5f;
        private float _contrast = 1.8f;
        private float _exposure = 0f;
        private Color _colorBalance = Color.White * 0.5f;
        
        private Color _shaftTint = Color.White;
        private float _shaftBlend = 0.8f;
        private float _scale = 1;
        private float _shaftDecay = 0f;
        private float _spread = 0.2f;
        private float MAXshaftBlend = 0.8f;

        public float Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Saturation = value;
            }
        }

        public float Contrast
        {
            get { return _contrast; }
            set
            {
                _contrast = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Contrast = value;
            }
        }

        public float Exposure
        {
            get { return _exposure; }
            set
            {
                _exposure = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Exposure = value;
            }
        }

        public Color ColorBalance
        {
            get { return _colorBalance; }
            set
            {
                _colorBalance = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.ColorBalance = value;
            }
        }

        public Color ShaftTint
        {
            get { return _shaftTint; }
            set
            {
                _shaftTint = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.ShaftTint = value;
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Scale = value;
            }
        }

        public float Intensity
        {
            get { return _intensity; }
            set
            {
                _intensity = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Intensity = value;
            }
        }

        public float Spread
        {
            get { return _spread; }
            set
            {
                _spread = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Spread = value;
            }
        }


        public float ShaftBlend
        {
            get { return _shaftBlend; }
            set
            {
                _shaftBlend = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Blend = value;
            }
        }

        public float ShaftDecay
        {
            get { return _shaftDecay; }
            set
            {
                _shaftDecay = value;
                if (_lightShaftEffect != null)
                    _lightShaftEffect.Decay = value;
            }
        }

        [ContentSerializer(SharedResource = true)]
        public Light MainLight
        {
            get { return _mainLight; }
            set { _mainLight = value; }
        }


        public void InitComponent(ContentManager contentManager, Renderer renderer)
        {
            _lightShaftEffect = new LightShaftEffect();
            if (renderer != null)
            {
                renderer.LightShaftEffect = _lightShaftEffect;

                _lightShaftEffect.Init(contentManager, renderer);
                _lightShaftEffect.Scale = _scale;
                _lightShaftEffect.Intensity = _intensity;
                _lightShaftEffect.Spread = _spread;
                _lightShaftEffect.ShaftTint = _shaftTint;
                _lightShaftEffect.Decay = _shaftDecay;
                _lightShaftEffect.ColorBalance = _colorBalance;
                _lightShaftEffect.Contrast = _contrast;
                _lightShaftEffect.Exposure = _exposure;
                _lightShaftEffect.Saturation = _saturation;
                _lightShaftEffect.Blend = _shaftBlend;
            }
        }

        public void PreRender(Renderer renderPipeline, Camera.Camera camera)
        {
            if (_mainLight != null && _lightShaftEffect != null)
            {
                ShaftBlend = _mainLight.Intensity * 3;
                ShaftBlend = Math.Min(Math.Abs(Math.Max(ShaftBlend, 0f)), MAXshaftBlend);

                // we need to convert the light position to screen space. This algorithm works only 
                // for directional lights
                Vector3 pos = camera.Transform.Translation -_mainLight.Transform.Forward * 10;
                Console.WriteLine(pos);
                Vector4 pos4 = new Vector4(pos, 1);
                pos4 = Vector4.Transform(pos4, camera.View * camera.Projection);

                pos.X = pos4.X / pos4.W;
                // flip Y 
                pos.Y = -pos4.Y / pos4.W;
                pos.Z = pos4.Z / pos4.W;

                // do some hacks to make the intensity goes to zero when the light is outside the screen
                float intensity = 1 - (float)Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y) * 0.15f;
                intensity = Math.Min(1, intensity);
                intensity = Math.Max(0, intensity);
                if (pos.Z < 0 || pos.Z > 1)
                    _lightShaftEffect.Intensity = 0;
                else
                {
                    // make the intensity function more narrow (intensity^3)
                    _lightShaftEffect.Intensity = _intensity * (intensity * intensity * intensity);
                }
                _lightShaftEffect.LightCenter = new Vector2(pos.X, pos.Y);
            }
        }
    }
}