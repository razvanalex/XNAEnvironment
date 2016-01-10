#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Engine.Camera;
using Engine.Water;
#endregion

namespace Engine.Sky
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SkyDomeSystem : IRenderable
    {

        #region Properties

        private float fTheta;
        private float fPhi;

        private float previousTheta, previousPhi;

        private bool realTime;
        Camera.Camera camera;

        Game game;

        Texture2D mieTex, rayleighTex;
        RenderTarget2D mieRT, rayleighRT;
      //  DepthStencilBuffer oldDepthBuffer, newDepthBuffer;

        Texture2D moonTex, glowTex, starsTex;

        Texture2D permTex, gradTex;

        Effect scatterEffect, texturedEffect, noiseEffect;

        QuadRenderComponent quad;

        SkyDomeParameters parameters;

        VertexDeclaration vertexDecl;
        VertexPositionTexture[] domeVerts, quadVerts;
        short[] ib, quadIb;

        int DomeN;
        int DVSize;
        int DISize;

        Vector4 sunColor;

        private float inverseCloudVelocity;
        private float cloudCover;
        private float cloudSharpness;
        private float numTiles;
        GraphicsDevice graphicsDevice;

        GameTime gameTime;

        #endregion

        #region Gets/Sets
        /// <summary>
        /// Gets/Sets Theta value
        /// </summary>
        public float Theta { get { return fTheta; } set { fTheta = value; } }

        /// <summary>
        /// Gets/Sets Phi value
        /// </summary>
        public float Phi { get { return fPhi; } set { fPhi = value; } }
            
        /// <summary>
        /// Gets/Sets actual time computation
        /// </summary>
        public bool RealTime 
        { 
            get { return realTime; } 
            set { realTime = value; } 
        }

        /// <summary>
        /// Gets/Sets the SkyDome parameters
        /// </summary>
        public SkyDomeParameters Parameters { get { return parameters; } set { parameters = value; } }

        /// <summary>
        /// Gets the Sun color
        /// </summary>
        public Vector4 SunColor { get { return sunColor; } set { sunColor = value; } }

        /// <summary>
        /// Gets/Sets InverseCloudVelocity value
        /// </summary>
        public float InverseCloudVelocity { get { return inverseCloudVelocity; } set { inverseCloudVelocity = value; } }

        /// <summary>
        /// Gets/Sets CloudCover value
        /// </summary>
        public float CloudCover { get { return cloudCover; } set { cloudCover = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float CloudSharpness { get { return cloudSharpness; } set { cloudSharpness = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float NumTiles { get { return numTiles; } set { numTiles = value; } }

        public float Gr;
        #endregion

        #region Contructor

        public SkyDomeSystem(Game game, Camera.Camera camera, GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            this.game = game;
            this.camera = camera;
            this.graphicsDevice = graphicsDevice;
            this.gameTime = gameTime;
            realTime = false;
            
            parameters = new SkyDomeParameters();

            quad = new QuadRenderComponent(game);
            game.Components.Add(quad);

            fTheta = 0.0f;
            fPhi = 0.0f;

            DomeN = 32;

            GeneratePermTex();
            Initialize();
            LoadContent();

        }

        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
#if XBOX
            this.mieRT = new RenderTarget2D(game.GraphicsDevice, 256, 128, 1,
                SurfaceFormat.Color, MultiSampleType.None, 0);
            this.rayleighRT = new RenderTarget2D(game.GraphicsDevice, 256, 128, 1,
                SurfaceFormat.Color, MultiSampleType.None, 0);
#else
            // You can use SurfaceFormat.Color to increase performance / reduce quality
            this.mieRT = new RenderTarget2D(game.GraphicsDevice, 256, 128, true,
                SurfaceFormat.HalfVector4, DepthFormat.None);
            this.rayleighRT = new RenderTarget2D(game.GraphicsDevice, 256, 128, true,
                SurfaceFormat.HalfVector4, DepthFormat.None);
#endif

            // Clouds constantes
            inverseCloudVelocity = 16.0f;
            CloudCover = -0.1f;
            CloudSharpness = 0.5f;
            numTiles = 16.0f;
            sunColor = this.GetSunColor(-this.fTheta, 2);
           // base.Initialize();
        }
        #endregion

        #region Load

        protected void LoadContent()
        {
            scatterEffect = game.Content.Load<Effect>("Effects/Sky/scatter");
            texturedEffect = game.Content.Load<Effect>("Effects/Sky/Textured");
            noiseEffect = game.Content.Load<Effect>("Effects/Sky/SNoise");

            moonTex = game.Content.Load<Texture2D>("textures/Sky/moon");
            glowTex = game.Content.Load<Texture2D>("textures/Sky/moonglow");
            starsTex = game.Content.Load<Texture2D>("textures/Sky/starfield");

            GenerateDome();
            GenerateMoon();

          //  base.LoadContent();

        }

        #endregion

        #region Weather
        public enum Weather
        {
            Clear = 0,
            SomeClouds = 1,
            Clouds = 2,
            MoreClouds = 3,
            Rain = 4,
            MoreRain = 5,
            Thunder = 6
        }
        public Weather prevWeather;

        void GoTo(ref float Value, float ToValue, float step)
        {
            if (step < (float)Math.Abs(ToValue - Value))
            {
                if (Value < ToValue)
                {
                    Value += step;
                }
                else if (Value > ToValue)
                {
                    Value -= step;
                }
            }
            else if (step > (float)Math.Abs(ToValue - Value))
            {
                Value = ToValue;
            }
        }

        public void WeatherChange(ref Weather weather)
        {
            sunColor = this.GetSunColor(-this.fTheta, 2);
            switch (weather)
            {
                case Weather.Clear:
                    inverseCloudVelocity = 64.0f;
                    numTiles = 16f;
                    CloudCover = -1f;
                    CloudSharpness = 1f;
                    Gr = 0;
                    break;
                case Weather.SomeClouds:
                    inverseCloudVelocity = 64.0f;
                    numTiles = 32.0f;
                    Gr = 0;
                    if (prevWeather == Weather.Clear)
                    {
                        GoTo(ref cloudCover, -0.1f, 0.0001f);
                        GoTo(ref cloudSharpness, 0.5f, 0.0001f);
                        if (CloudCover == -0.1f && CloudSharpness == 0.5f)
                            weather = Weather.Clouds;
                    }
                    else if (prevWeather == Weather.Clouds)
                    {
                        GoTo(ref cloudCover, -1, 0.0002f);
                        GoTo(ref cloudSharpness, 1, 0.0001f);
                        if (CloudCover == -1 && CloudSharpness == 1)
                            weather = Weather.Clear;
                    }
                    break;
                case Weather.Clouds:
                    Gr = 0;
                    inverseCloudVelocity = 64.0f;
                    CloudCover = -0.1f;
                    CloudSharpness = 0.5f;
                    numTiles = 32.0f;
                    break;
                case Weather.MoreClouds:
                    if (prevWeather == Weather.Clouds)
                    {
                        CloudSharpness = 0.5f;
                        numTiles = 32;

                        GoTo(ref inverseCloudVelocity, 4, 0.012f);
                        GoTo(ref Gr, 0.9f, 0.0001f);
                        GoTo(ref cloudCover, 4, 0.001f);
                        Console.WriteLine("CloudCover " + CloudCover + " Gr = " + Gr + " inverseCloudVelocity = " + inverseCloudVelocity);
                        if (CloudCover == 4.0f && Gr == 0.9f && inverseCloudVelocity == 4.0f)
                        {
                            weather = Weather.Rain;
                        }
                        
                    }
                    else if (prevWeather == Weather.Rain)
                    {
                        CloudSharpness = 0.5f;
                        numTiles = 32;   
                  
                        GoTo(ref inverseCloudVelocity, 64, 0.012f);
                        GoTo(ref Gr, 0, 0.0001f);
                        GoTo(ref cloudCover, -0.1f, 0.001f);

                        //Console.WriteLine("CloudCover " + CloudCover + " Gr = " + Gr + " inverseCloudVelocity = " + inverseCloudVelocity);
                        if (CloudCover == -0.1f && Gr == 0f && inverseCloudVelocity == 64)
                            weather = Weather.Clouds;
                    }
                    // if (prevWeather == Weather.MoreClouds)
                   // {
                    //    weather = Weather.Rain;
                   // }
                    break;
                case Weather.Rain:
                    Gr = 0.9f;
                    inverseCloudVelocity = 4.0f;                    
                    CloudCover = 4f;
                    CloudSharpness = 0.5f;
                    numTiles = 32;
                    break;
                case Weather.MoreRain: break;
                case Weather.Thunder: break;
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (realTime)
            {
                int minutos = DateTime.Now.Hour*60 + DateTime.Now.Minute ;
                this.fTheta = (float)minutos * (float)(Math.PI) / 12.0f / 60.0f;
            }

            parameters.LightDirection = this.GetDirection();
            parameters.LightDirection.Normalize(); 
          //  base.Update(gameTime);
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public  void Draw(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {

        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            graphicsDevice.SetRenderTarget(null);
            Matrix World = Matrix.CreateTranslation(CameraPosition.X, CameraPosition.Y, CameraPosition.Z);

            if (previousTheta != fTheta || previousPhi != fPhi)
                UpdateMieRayleighTextures();

            game.GraphicsDevice.Clear(new Color(sunColor));//new Color(0.10980f, 0.30196f, 0.49412f)));//sunColor

             RasterizerState rs = new RasterizerState();
             rs.CullMode = CullMode.None;            
             game.GraphicsDevice.RasterizerState = rs;
             
            DepthStencilState depthState = new DepthStencilState();
            depthState.DepthBufferEnable = false; 
            depthState.DepthBufferWriteEnable = false;
            graphicsDevice.DepthStencilState = depthState;

            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Render"];
            scatterEffect.Parameters["txMie"].SetValue(this.mieTex);
            scatterEffect.Parameters["txRayleigh"].SetValue(this.rayleighTex);
            scatterEffect.Parameters["World"].SetValue(World);
            scatterEffect.Parameters["WorldViewProjection"].SetValue(World * View * Projection);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X,
                -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            scatterEffect.Parameters["NumSamples"].SetValue(parameters.NumSamples);
            scatterEffect.Parameters["fExposure"].SetValue(parameters.Exposure);
            scatterEffect.Parameters["SunColor"].SetValue(sunColor);
            scatterEffect.Parameters["gr"].SetValue(this.Gr);
            scatterEffect.Parameters["StarsTex"].SetValue(starsTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                scatterEffect.Parameters["starIntensity"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                scatterEffect.Parameters["starIntensity"].SetValue(0.0f);
           
            foreach (EffectPass pass in scatterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, domeVerts, 0, this.DVSize, ib, 0, this.DISize);
            }

            DrawGlow(View, Projection, CameraPosition);
            DrawMoon(View, Projection, CameraPosition);
            DrawClouds(View, Projection, CameraPosition);
            graphicsDevice.SetRenderTarget(null);
          
            depthState = new DepthStencilState();
            depthState.DepthBufferEnable = true; 
            depthState.DepthBufferWriteEnable = true; 
            graphicsDevice.DepthStencilState = depthState;      

            previousTheta = this.fTheta;
            previousPhi = this.fPhi;           
        }

        #region DrawMoon

        private void DrawMoon(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];

            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(this.Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-this.Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 15,
                parameters.LightDirection.Y * 15,
                parameters.LightDirection.Z * 15) *
                Matrix.CreateTranslation(CameraPosition));
            texturedEffect.Parameters["View"].SetValue(View);
            texturedEffect.Parameters["Projection"].SetValue(Projection);
            texturedEffect.Parameters["Texture"].SetValue(this.moonTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }
            graphicsDevice.BlendState = BlendState.Opaque;  
        }

        #endregion

        #region DrawGlow

        private void DrawGlow(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];
           // texturedEffect.Begin();
            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(this.Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-this.Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 5,
                parameters.LightDirection.Y * 5,
                parameters.LightDirection.Z * 5) *
                Matrix.CreateTranslation(CameraPosition));//*
            texturedEffect.Parameters["View"].SetValue(View);
            texturedEffect.Parameters["Projection"].SetValue(Projection);
            texturedEffect.Parameters["Texture"].SetValue(this.glowTex);
            if (fTheta < Math.PI / 2.0f || fTheta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(
                    Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
               // pass.Begin();
                pass.Apply();
                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);

             //   pass.End();
            }
       //     texturedEffect.End();

          //  game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            graphicsDevice.BlendState = BlendState.Opaque;
        }

        #endregion

        #region DrawClouds

        public void PreDraw(GameTime gameTime)
        {
            noiseEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds / inverseCloudVelocity);
        }

        public void DrawClouds(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            noiseEffect.CurrentTechnique = noiseEffect.Techniques["Noise"];
            //noiseEffect.Begin();
            noiseEffect.Parameters["World"].SetValue(Matrix.CreateScale(20000.0f)*
                Matrix.CreateTranslation(new Vector3(0,0,-900)) *
                Matrix.CreateRotationX((float)Math.PI/2.0f) *
                Matrix.CreateTranslation(CameraPosition.X, CameraPosition.Y, CameraPosition.Z));
          
            noiseEffect.Parameters["View"].SetValue(View);
            noiseEffect.Parameters["Projection"].SetValue(Projection);
            noiseEffect.Parameters["permTexture"].SetValue(this.permTex);
            noiseEffect.Parameters["SunColor"].SetValue(sunColor);
            noiseEffect.Parameters["numTiles"].SetValue(numTiles);
            noiseEffect.Parameters["CloudCover"].SetValue(cloudCover);
            noiseEffect.Parameters["CloudSharpness"].SetValue(cloudSharpness);
            
            foreach (EffectPass pass in noiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }
            graphicsDevice.BlendState = BlendState.Opaque;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Get Light Direction
        Vector4 GetDirection()
        {
	        
	        float y = (float)Math.Cos((double)this.fTheta);
            float x = (float)(Math.Sin((double)this.fTheta) * Math.Cos(this.fPhi));
            float z = (float)(Math.Sin((double)this.fTheta) * Math.Sin(this.fPhi));
            float w = 1.0f;

	        return new Vector4(x,y,z,w);
        }
        #endregion

        #region UpdateMieRayleighTextures

        void UpdateMieRayleighTextures()
        {
            game.GraphicsDevice.SetRenderTargets(rayleighRT, mieRT);
            game.GraphicsDevice.Clear(Color.CornflowerBlue);

            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Update"];
            scatterEffect.Parameters["InvWavelength"].SetValue(parameters.InvWaveLengths);
            scatterEffect.Parameters["WavelengthMie"].SetValue(parameters.WaveLengthsMie);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X,
                -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            EffectPass pass = scatterEffect.CurrentTechnique.Passes[0];
            pass.Apply();
            graphicsDevice.BlendState = BlendState.Opaque;
            quad.Render(Vector2.One * -1, Vector2.One);
            game.GraphicsDevice.SetRenderTargets(null); 
            this.mieTex = mieRT;
            this.rayleighTex = rayleighRT;
        }

        #endregion

        #region GenerateDome

        private void GenerateDome()
        {
            int Latitude = DomeN / 2;
            int Longitude = DomeN;
            DVSize = Longitude * Latitude;
            DISize = (Longitude - 1) * (Latitude - 1) * 2;
            DVSize *= 2;
            DISize *= 2;

            vertexDecl = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

            domeVerts = new VertexPositionTexture[DVSize];

            // Fill Vertex Buffer
            int DomeIndex = 0;
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)Longitude - 1.0f)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = MathHelper.Pi * j / (Latitude - 1);

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(Longitude - 1)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = (MathHelper.Pi * 2.0) - (MathHelper.Pi * j / (Latitude - 1));

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }

            // Fill index buffer
            ib = new short[DISize * 3];
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(i * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j + 1);

                    ib[index++] = (short)((i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j);
                }
            }
            short Offset = (short)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(Offset + i * Latitude + j);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j);

                    ib[index++] = (short)(Offset + i * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + i * Latitude + j);
                }
            }
        }

        #endregion

        #region GenerateMoon

        private void GenerateMoon()
        {
            quadVerts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(1,-1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,-1,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,1,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,0))
                        };

            quadIb = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        #endregion

        #region GetSunColor

        Vector4 GetSunColor(float fTheta, int nTurbidity)
        {
            float fBeta = 0.04608365822050f * nTurbidity - 0.04586025928522f;
            float fTauR, fTauA;
            float[] fTau = new float[3];

            float coseno = (float)Math.Cos((double)fTheta + Math.PI);
            double factor = (double)fTheta / Math.PI * 180.0;
            double jarl = Math.Pow(93.885 - factor, -1.253);
            float potencia = (float)jarl;
            float m = 1.0f / (coseno + 0.15f * potencia);

            int i;
            float[] fLambda = new float[3];
            fLambda[0] = parameters.WaveLengths.X;
            fLambda[1] = parameters.WaveLengths.Y;
            fLambda[2] = parameters.WaveLengths.Z;


            for (i = 0; i < 3; i++)
            {
                potencia = (float)Math.Pow((double)fLambda[i], 4.0);
                fTauR = (float)Math.Exp((double)(-m * 0.008735f * potencia));

                const float fAlpha = 1.3f;
                potencia = (float)Math.Pow((double)fLambda[i], (double)-fAlpha);
                if (m < 0.0f)
                    fTau[i] = 0.0f;
                else
                {
                    fTauA = (float)Math.Exp((double)(-m * fBeta * potencia));
                    fTau[i] = fTauR * fTauA;
                }

            }

            Vector4 vAttenuation = new Vector4(fTau[0], fTau[1], fTau[2], 1.0f);
            return vAttenuation;
        }

        #endregion

        #region GeneratePermTex

        private void GeneratePermTex()
        {
            int[] perm = { 151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

            int[] gradValues = { 1,1,0,    
                -1,1,0, 1,-1,0, 
                -1,-1,0, 1,0,1,
                -1,0,1, 1,0,-1,
                -1,0,-1, 0,1,1,
                0,-1,1, 0,1,-1,
                0,-1,-1, 1,1,0,
                0,-1,1, -1,1,0, 
                0,-1,-1
            };

            permTex = new Texture2D(game.GraphicsDevice, 256, 256, true, SurfaceFormat.Color);

            byte[] pixels;
            pixels = new byte[256 * 256 * 4];
            for(int i = 0; i<256; i++)
            {
                for(int j = 0; j<256; j++) 
                {
                  int offset = (i*256+j)*4;
                  byte value = (byte)perm[(j + perm[i]) & 0xFF];
                  pixels[offset + 1] = (byte)(gradValues[value & 0x0F] * 64 + 64);
                  pixels[offset + 2] = (byte)(gradValues[value & 0x0F + 1] * 64 + 64);
                  pixels[offset + 3] = (byte)(gradValues[value & 0x0F + 2] * 64 + 64);
                  pixels[offset] = value;
                }
            }

            permTex.SetData<byte>(pixels);
        }

        #endregion

        #endregion

        #region Public Methods

        public void ApplyChanges()
        {
            this.UpdateMieRayleighTextures();
        }
        public void SetClipPlane(Vector4? Plane)
        {
            scatterEffect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
            if (Plane.HasValue)
                scatterEffect.Parameters["ClipPlane"].SetValue(Plane.Value);

            texturedEffect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
            if (Plane.HasValue)
                texturedEffect.Parameters["ClipPlane"].SetValue(Plane.Value);

            noiseEffect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
            if (Plane.HasValue)
                noiseEffect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
        #endregion

    }
}