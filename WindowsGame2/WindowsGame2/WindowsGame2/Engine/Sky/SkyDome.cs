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
using Engine.Water;

namespace Engine.Sky
{
    public class SkyDome
    {
        GraphicsDevice graphicsDevice;

        public SkyDomeSystem sky;
        SkyDomeSystem.Weather weather;

        public SkySphere skySphere;
        public bool LowSky;

        float Time = 0;
        int TIME;

        Random random = new Random();
        Camera.Camera camera;

        Rain rain;
        private KeyboardState previousKeyState;
        private GamePadState previousPadState;

        KeyboardState keyState, oldKeyState;

        private enum Status
        {
            Manual = 0,
            Automatic = 1,
            ActualTime = 2
        }
        Status stat, prevStat;

        object Terrain;
        Vector3 LightColor;
        Vector3 LightDirection;

        public float Theta;
        public float Gr;
        Game game;

        public SkyDome(Game game, Vector3 LightColor, Vector3 LightDirection,
            bool LowSky, Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.camera = camera;
            this.LowSky = LowSky;
            this.game = game;
            this.LightColor = LightColor;
            this.LightDirection = LightDirection;
            
            Initialize();
        }

        public void Initialize()
        {
            if(LowSky)
            {
                skySphere = new SkySphere(game.Content, graphicsDevice, game.Content.Load<TextureCube>("textures//Skybox//skybox"));
            }
            else
            {
                GameTime gt = new GameTime();
                sky = new SkyDomeSystem(game, camera, gt, graphicsDevice);
                Theta = sky.Theta = 2.4f;
                Gr = sky.Gr;
                sky.Parameters.NumSamples = 10;
                TIME = random.Next(5, 10); // 510
             
                sky.prevWeather = SkyDomeSystem.Weather.Clear;
                sky.WeatherChange(ref sky.prevWeather);
                weather = SkyDomeSystem.Weather.Clear;
                sky.WeatherChange(ref weather);   
            }
        }

        public void Update(GameTime gameTime, Vector3 LightDirection, Vector3 LightColor)
        {
            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            if (rain != null)
                rain.Update(camera);

            if ((keyState.IsKeyDown(Keys.D0) && !previousKeyState.IsKeyDown(Keys.D0)))
            {
                rain = new Rain(game, camera.Transform.Translation, true, graphicsDevice);
            }
            if ((keyState.IsKeyDown(Keys.D9) && !previousKeyState.IsKeyDown(Keys.D9)))
            {
                rain = new Rain(game, camera.Transform.Translation, false, graphicsDevice);
            }
            if ((keyState.IsKeyDown(Keys.D8) && !previousKeyState.IsKeyDown(Keys.D8)))
            {
                rain = null;
            }
            this.LightColor = LightColor;
            this.LightDirection = LightDirection;

            if (!LowSky)
            {
                Theta = sky.Theta;
                Gr = sky.Gr;
                SKY(gameTime);
            }
        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                    Terrain = (QuadTree)o;
                if (o is SmallTerrain)
                    Terrain = (SmallTerrain)o;
            }
        }

        Random rndW;

        void Weather()
        {
            if (weather == SkyDomeSystem.Weather.Rain && Time == 0)
            {
                rain = new Rain(game, camera.Transform.Translation, false, graphicsDevice);
                rain.density = 0f;
            }
            if (weather == SkyDomeSystem.Weather.Rain)
            {
                rain.density += 0.02f;
                if (rain.density > 10f)
                    rain.density = 10;
            }
            if (weather != SkyDomeSystem.Weather.Rain && rain != null)
            {
                rain.density -= 0.02f;
                if (rain.density < 0f)
                    rain.density = 0;
                if (Time == 1)
                    rain = null;
            }

            Time += 0.001f;
            if (Time >= TIME)
            {
                sky.prevWeather = weather;
                Time = 0;
                random = new Random();
                TIME = random.Next(6, 10);

                if (sky.prevWeather == SkyDomeSystem.Weather.Rain)
                    weather = SkyDomeSystem.Weather.MoreClouds;
                else if (sky.prevWeather == SkyDomeSystem.Weather.Clear)
                    weather = SkyDomeSystem.Weather.SomeClouds;
                else if (sky.prevWeather == SkyDomeSystem.Weather.Clouds)
                {
                    rndW = new Random();
                    int Ch = rndW.Next(1, 3);
                    if (Ch == 1)
                        weather = SkyDomeSystem.Weather.MoreClouds;
                    else if (Ch == 2)
                        weather = SkyDomeSystem.Weather.SomeClouds;
                }
            }

            sky.WeatherChange(ref weather);
           /* Console.WriteLine("Time=" + Time);
            Console.WriteLine("TIME =" + TIME);
            Console.WriteLine("Weather = " + weather);
            Console.WriteLine("PrevWeather = " + sky.prevWeather);*/
        }

        void SKY(GameTime gameTime)
        {
            sky.Update(gameTime);
            Weather();

            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if ((keyState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space)) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                previousPadState.Buttons.A == ButtonState.Released))
            {
                stat++;
                if ((int)stat == 3)
                    stat = Status.Manual;
            }

            if (Terrain is QuadTree)
            {
                ((QuadTree)Terrain).lightDirection = Vector3.Reflect(LightDirection, Vector3.Up);
                ((QuadTree)Terrain).lightColor = LightColor;
            }
            if (Terrain is SmallTerrain)
            {
                ((SmallTerrain)Terrain).lightDirection = Vector3.Reflect(LightDirection, Vector3.Up);
                ((SmallTerrain)Terrain).lightColor = LightColor;
            }
            
            switch (stat)
            {
                case Status.Manual:
                    sky.RealTime = false;
                    if (keyState.IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
                        sky.Theta -= 0.4f * step;
                    if (keyState.IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
                        sky.Theta += 0.4f * step;
                    if (sky.Theta > (float)Math.PI * 2.0f)
                        sky.Theta = sky.Theta - (float)Math.PI * 2.0f;
                    if (sky.Theta < 0.0f)
                        sky.Theta = (float)Math.PI * 2.0f + sky.Theta;
                    break;
                case Status.Automatic:
                    sky.RealTime = false;
                    sky.Theta += 0.001f * step;
                    if (sky.Theta > (float)Math.PI * 2.0f)
                        sky.Theta = sky.Theta - (float)Math.PI * 2.0f;
                    break;
                case Status.ActualTime:
                    sky.RealTime = true;
                    if (stat != prevStat)
                        sky.ApplyChanges();
                    break;
            }

            previousKeyState = keyState;
            previousPadState = GamePad.GetState(PlayerIndex.One);
            prevStat = stat;
        }

        public void PreDraw(GameTime gameTime)
        {
            sky.PreDraw(gameTime);
        }
        public void Draw()
        {
            if (LowSky) skySphere.Draw(camera.View, camera.Projection, camera.Transform.Translation);
            else sky.Draw(camera.View, camera.Projection, camera.Transform.Translation);
        }

        public void DrawRain(Camera.Camera camera)
        {
            if (rain != null)
                rain.Draw(camera);
        }
    }
}
