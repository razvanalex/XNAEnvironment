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
using System.IO;

using Engine;
using Engine.Camera;
using Engine.Terrain;

namespace Engine
{
    public class CarPlayer
    {
        SpriteBatch spriteBatch;

        public List<Models> models = new List<Models>();
        Camera.Camera camera;

        public CarControl carControl;
        QuadTree terrain;

        CarsData carData;
        Model CarModel;
        Model WheelModel;

        string CarPath = "Models/Cars/";
        string CarModelName = "";
        public Vector3 Wheel_Scale;
        public Vector3 Car_Scale;

        const float friction = 0.00020f;
        float UAcceleration = 0.0001f;
        float DAcceleration = 0.0001f;
        public float speed = 0f;
        public float gear = 0;
        float Turning = 0;
        float WheelTurn = 0;
        float CameraInerty;
        float OldSpeed = 0f;
        float MaxRadiusCam = 100f;
        float MinRadiusCam = 80f;

        Game game;
        GraphicsDevice graphicsDevice;

        //Camera
        public Vector3 CameraTarget = Vector3.Zero;
        public Vector3 CameraPosition = Vector3.Zero;
        public Vector3 CameraRotation = Vector3.Zero;
        float CamRotKey = 0f;
        float RadiusCam = 100f;
        bool changeCar;


        float HPosWDL, HPosWDR, HPosWUR, HPosWUL;
        float L1, L2, L3, L4;
        float Angle1, Angle2, Angle3, Angle4;
        float Angle12 = 0, Angle34 = 0;
        Vector3 Point_Right_Left = Vector3.Zero;
        Vector3 Point_Up_Down = Vector3.Zero;
        float SemiRadiusL12 = 24.5f;
        float SemiRadiusL34 = 15.2f;
        Vector3 heighPoint = Vector3.Zero;
        float CarHeigh = 6.4f;

        float test = 0;
        float t1, t2, t3, t4;

        int no_points = 0;
        Vector3 Point_On_Wheel = Vector3.Zero;
        Vector3 Point_On_Ground = Vector3.Zero;

        Vector3 WheelRotationVector = Vector3.Zero;

        public CarPlayer(Game game, GraphicsDevice graphicsDevice)
        {
            this.game = game;
            this.graphicsDevice = graphicsDevice;
        }

        public void Initialize()
        {
            //Load Car by its name...
            CarModelName = "Audi R8";
            carData = new CarsData(game, CarModelName);
            Car_Scale = carData.Scale_Car;
            Wheel_Scale = carData.Scale_Wheel;
            CarModel = game.Content.Load<Model>(CarPath + CarModelName + carData.CarModelName);
            WheelModel = game.Content.Load<Model>(carData.Model_Wheel);

            //Load Car Components
            models.Add(new Models(game.Content, CarModel, new Vector3(1000f, 0, 10f), new Vector3(0, 0, 0), Car_Scale, graphicsDevice));

            models.Add(new Models(game.Content, WheelModel, new Vector3(0.152f, 0.065f, 0.245f), new Vector3(0, 0, 0), Wheel_Scale, graphicsDevice));//UR
            models.Add(new Models(game.Content, WheelModel, new Vector3(-0.152f, 0.065f, 0.245f), new Vector3(0, MathHelper.Pi, 0), Wheel_Scale, graphicsDevice));//UL
            models.Add(new Models(game.Content, WheelModel, new Vector3(0.152f, 0.065f, -0.245f), new Vector3(0, 0, 0), Wheel_Scale, graphicsDevice));//DR
            models.Add(new Models(game.Content, WheelModel, new Vector3(-0.152f, 0.065f, -0.245f), new Vector3(0, MathHelper.Pi, 0), Wheel_Scale, graphicsDevice));//DL       

        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                    terrain = (QuadTree)o;
                if (o is Camera.Camera)
                    camera = (Camera.Camera)o;
            }
        }

        void UpdateLight()
        {
            foreach (Models model in models)
            {
                model.AmbientColor = terrain.AmbientColor;
                model.LightColor = terrain.LightColor;
            }
        }
        public void GenerateTags()
        {
            foreach (Models model in models)
                model.generateTags();
        }

        public void Update(GameTime gameTime)
        {
            carControl = new CarControl(game, models[0].Rotation, speed, carData.MaxSpeed, Turning, UAcceleration, DAcceleration, WheelTurn, gear, CameraInerty);
            carControl.Control();
            models[0].Rotation = carControl.Rotation;
            speed = carControl.speed;
            Turning = carControl.Turning;
            UAcceleration = carControl.UAcceleration;
            DAcceleration = carControl.DAcceleration;
            WheelTurn = carControl.WheelTurn;
            gear = carControl.gear;
            CameraInerty = carControl.CameraInerty;
            CarMoving(gameTime);
            KeyboardState keyboardstate = Keyboard.GetState();

            //Change car;
            if (keyboardstate.IsKeyDown(Keys.D1))
            {
                CarModelName = "Lamborghini Veneno";
                changeCar = true;
            }
            if (keyboardstate.IsKeyDown(Keys.D2))
            {
                CarModelName = "Lamborghini Aventador 2012";
                changeCar = true;
            }
            if (keyboardstate.IsKeyDown(Keys.D3))
            {
                CarModelName = "Audi R8";
                changeCar = true;
            }

            carData = new CarsData(game, CarModelName);
            Car_Scale = carData.Scale_Car;
            Wheel_Scale = carData.Scale_Wheel;
            CarModel = game.Content.Load<Model>(CarPath + CarModelName + carData.CarModelName);
            WheelModel = game.Content.Load<Model>(carData.Model_Wheel);

            models[0] = new Models(game.Content, CarModel, models[0].Position, models[0].Rotation, Car_Scale, graphicsDevice);
            models[1] = new Models(game.Content, WheelModel, models[1].Position, models[1].Rotation, Wheel_Scale, graphicsDevice);
            models[2] = new Models(game.Content, WheelModel, models[2].Position, models[2].Rotation, Wheel_Scale, graphicsDevice);
            models[3] = new Models(game.Content, WheelModel, models[3].Position, models[3].Rotation, Wheel_Scale, graphicsDevice);
            models[4] = new Models(game.Content, WheelModel, models[4].Position, models[4].Rotation, Wheel_Scale, graphicsDevice);

            if (changeCar)
            {
                speed = 0;
                changeCar = false;
            }
            OldSpeed = speed;
            UpdateLight();
        }


        void CarMoving(GameTime gameTime)
        {
            //Rotation Fix
            models[0].Rotation -= new Vector3(MathHelper.TwoPi * (float)Math.Truncate(models[0].Rotation.X / MathHelper.TwoPi), 0, 0);

            //Calculate some lenghts
            L1 = (float)Math.Sqrt((float)Math.Pow(models[1].Position.X - models[3].Position.X, 2) + (float)Math.Pow(models[1].Position.Y - models[3].Position.Y, 2) + (float)Math.Pow(models[1].Position.Z - models[3].Position.Z, 2));
            L2 = (float)Math.Sqrt((float)Math.Pow(models[2].Position.X - models[4].Position.X, 2) + (float)Math.Pow(models[2].Position.Y - models[4].Position.Y, 2) + (float)Math.Pow(models[2].Position.Z - models[4].Position.Z, 2));
            L3 = (float)Math.Sqrt((float)Math.Pow(models[1].Position.X - models[2].Position.X, 2) + (float)Math.Pow(models[1].Position.Y - models[2].Position.Y, 2) + (float)Math.Pow(models[1].Position.Z - models[2].Position.Z, 2));
            L4 = (float)Math.Sqrt((float)Math.Pow(models[3].Position.X - models[4].Position.X, 2) + (float)Math.Pow(models[3].Position.Y - models[4].Position.Y, 2) + (float)Math.Pow(models[3].Position.Z - models[4].Position.Z, 2));

            //Get Height from Wheels
            HPosWUR = terrain.GetHeight(models[1].Position.X - CarHeigh * (float)Math.Sin(Angle12) * (float)Math.Cos(models[0].Rotation.Y), models[1].Position.Z - CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.Y));// + CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34);
            HPosWUL = terrain.GetHeight(models[2].Position.X - CarHeigh * (float)Math.Sin(Angle12) * (float)Math.Cos(models[0].Rotation.Y), models[2].Position.Z - CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.Y));// + CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34);
            HPosWDR = terrain.GetHeight(models[3].Position.X - CarHeigh * (float)Math.Sin(Angle12) * (float)Math.Cos(models[0].Rotation.Y), models[3].Position.Z - CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.Y));// + CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34);
            HPosWDL = terrain.GetHeight(models[4].Position.X - CarHeigh * (float)Math.Sin(Angle12) * (float)Math.Cos(models[0].Rotation.Y), models[4].Position.Z - CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.Y));// + CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34);

            t1 = terrain.GetHeight(models[1].Position.X, models[1].Position.Z);
            t2 = terrain.GetHeight(models[2].Position.X, models[2].Position.Z);
            t3 = terrain.GetHeight(models[3].Position.X, models[3].Position.Z);
            t4 = terrain.GetHeight(models[4].Position.X, models[4].Position.Z);

            //Calculate Car's Angles
            Angle1 = (float)Math.Atan((HPosWUR - HPosWDR) / (2 * SemiRadiusL12));
            Angle2 = (float)Math.Atan((HPosWUL - HPosWDL) / (2 * SemiRadiusL12));
            Angle3 = (float)Math.Atan((HPosWDR - HPosWDL) / (2 * SemiRadiusL34));
            Angle4 = (float)Math.Atan((HPosWUR - HPosWUL) / (2 * SemiRadiusL34));
            Angle12 = (Angle1 + Angle2) / 2;
            Angle34 = (Angle3 + Angle4) / 2;

            //Car
            Vector3 NewCarPosition = new Vector3(0, 0, speed * (float)Math.Cos(Angle12));
            models[0].Position += Vector3.Transform(NewCarPosition, Matrix.CreateFromYawPitchRoll(models[0].Rotation.X, models[0].Rotation.Y, models[0].Rotation.Z));
            models[0].Position = new Vector3(models[0].Position.X, (float)Math.Abs(Math.Sin(models[0].Rotation.Y) * (L2 / 2 - CarHeigh)) + (float)Math.Abs(Math.Sin(models[0].Rotation.Z) * L1 / 2), models[0].Position.Z);
            models[0].Rotation = new Vector3(models[0].Rotation.X, -Angle12, Angle34);

            //Wheels  
            heighPoint = new Vector3(
                -CarHeigh * ((float)Math.Sin(Angle12) * (float)Math.Cos(Angle34) * (float)Math.Sin(models[0].Rotation.X) + (float)Math.Sin(Angle34) * (float)Math.Cos(models[0].Rotation.X)),
                CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34), 
                -CarHeigh * ((float)Math.Sin(Angle12) * (float)Math.Cos(Angle34) * (float)Math.Cos(models[0].Rotation.X) - (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.X)));
          
            //heighPoint = new Vector3(-CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Cos(models[0].Rotation.X) - CarHeigh * (float)Math.Sin(Angle34 + models[0].Rotation.X), CarHeigh * (float)Math.Cos(Angle12) * (float)Math.Cos(Angle34), CarHeigh * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.X) - CarHeigh * (float)Math.Cos(Angle34 + models[0].Rotation.X));

            models[0].Position = new Vector3(models[0].Position.X, terrain.GetHeight(models[0].Position.X, models[0].Position.Z), models[0].Position.Z);

            Point_Right_Left = new Vector3(SemiRadiusL34 * ((float)Math.Cos(Angle34) * (float)Math.Cos(models[0].Rotation.X) - (float)Math.Sin(Angle12) * (float)Math.Sin(Angle34) * (float)Math.Sin(models[0].Rotation.X)), SemiRadiusL34 * (float)Math.Sin(Angle34) * (float)Math.Cos(Angle12), -SemiRadiusL34 * ((float)Math.Cos(Angle34) * (float)Math.Sin(models[0].Rotation.X) + (float)Math.Sin(Angle12) * (float)Math.Sin(Angle34) * (float)Math.Cos(models[0].Rotation.X)));
            Point_Up_Down = new Vector3(-SemiRadiusL12 * (float)Math.Cos(Angle12) * (float)Math.Cos(models[0].Rotation.X + MathHelper.PiOver2), SemiRadiusL12 * (float)Math.Sin(Angle12), SemiRadiusL12 * (float)Math.Cos(Angle12) * (float)Math.Sin(models[0].Rotation.X + MathHelper.PiOver2));

            models[1].Position = models[0].Position + heighPoint + Point_Right_Left + Point_Up_Down;  //UR
            models[2].Position = models[0].Position + heighPoint - Point_Right_Left + Point_Up_Down;  //UL
            models[3].Position = models[0].Position + heighPoint + Point_Right_Left - Point_Up_Down;  //DR
            models[4].Position = models[0].Position + heighPoint - Point_Right_Left - Point_Up_Down;  //DL

            Vector3 LeftWheelRot = new Vector3(
                models[0].Rotation.X, 
                models[0].Rotation.Y,
                (float)Math.PI - models[0].Rotation.Z);
            Vector3 RighttWheelRot = models[0].Rotation;

            WheelRotationVector += new Vector3(0, speed / 10, 0);

            models[1].Rotation = RighttWheelRot + WheelRotationVector;
            models[2].Rotation = LeftWheelRot + WheelRotationVector;
            models[3].Rotation = RighttWheelRot + WheelRotationVector;
            models[4].Rotation = LeftWheelRot + WheelRotationVector;

            /*models[1].Rotation += new Vector3(0, speed / 10, 0);
            models[3].Rotation += new Vector3(0, speed / 10, 0);
            models[2].Rotation += new Vector3(0, speed / 10, 0);
            models[4].Rotation += new Vector3(0, speed / 10, 0);*/
            
            //Acceleration and Gear
            carControl.gearbox();
            carControl.Acceleration();

        } 

        public void CameraView(ref Camera.Camera camera)
        {
            float InitialAngleCam = (float)Math.Asin(1f);
            float CamRotSpeed = 0.05f; // 0.03f
            float returnSpeed = 0.08f; // 0.05f
            float error = CamRotSpeed;
            float returnError = returnSpeed;

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (CamRotKey < MathHelper.Pi && CamRotKey > 0)
                    CamRotKey += CamRotSpeed;
                else if ((CamRotKey > -MathHelper.Pi && CamRotKey < 0) || (CamRotKey >= MathHelper.Pi + error && CamRotKey <= 3 * MathHelper.PiOver2 + error))
                    CamRotKey -= CamRotSpeed;
                else CamRotKey = MathHelper.Pi;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if ((CamRotKey <= MathHelper.PiOver2) || (CamRotKey >= MathHelper.Pi && CamRotKey <= 3 * MathHelper.PiOver2) || (CamRotKey <= MathHelper.Pi && CamRotKey > MathHelper.PiOver2 + error))
                    CamRotKey += CamRotSpeed;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if ((CamRotKey <= 0 && CamRotKey >= -MathHelper.PiOver2 + error) || (CamRotKey >= 0 && CamRotKey < MathHelper.PiOver2 + error) || (CamRotKey >= MathHelper.Pi && CamRotKey <= 3 * MathHelper.PiOver2 + error))
                    CamRotKey -= CamRotSpeed;
                else if (CamRotKey <= MathHelper.Pi && CamRotKey > MathHelper.PiOver2)
                {
                    CamRotKey -= CamRotSpeed;
                    if (CamRotKey > MathHelper.PiOver2 - error && CamRotKey < MathHelper.PiOver2 + error)
                        CamRotKey += CamRotSpeed;
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.D) && Keyboard.GetState().IsKeyUp(Keys.A) && Keyboard.GetState().IsKeyUp(Keys.S))
            {
                if (CamRotKey < -returnError || CamRotKey >= 3 * MathHelper.PiOver2 - returnError)
                    CamRotKey += returnSpeed;
                else if (CamRotKey > returnError)
                    CamRotKey -= returnSpeed;
                else CamRotKey = 0f;
                if ((CamRotKey > MathHelper.PiOver2 && CamRotKey < 3 * MathHelper.PiOver2) || CamRotKey < -MathHelper.PiOver2)
                    CamRotKey = 0;
            }
            if (CamRotKey >= MathHelper.Pi * 2)
                CamRotKey = 0;

            //Up-Down Inerty
            if (OldSpeed > speed)
            {
                RadiusCam += speed * 0.003f;
                if (RadiusCam >= MaxRadiusCam)
                    RadiusCam = MaxRadiusCam;
            }
            else if (OldSpeed < speed)
            {
                RadiusCam -= speed * 0.004f;
                if (RadiusCam <= MinRadiusCam)
                    RadiusCam = MinRadiusCam;
            }
            //Fix Up-Down Camera's Inerty
            if ((speed >= 0) && (speed <= 0.050f) && (OldSpeed >= speed) && (Keyboard.GetState().IsKeyUp(Keys.Up)))
            {
                if (RadiusCam < 1 - 0.001f)
                    RadiusCam += 0.001f;
                else if (RadiusCam > 1 + 0.001f)
                    RadiusCam -= 0.001f;
                else RadiusCam = 1;
            }
            test += 0.01f;
            CameraRotation.X = models[0].Rotation.Y;

            //CameraRotation.Z = test;
            CameraTarget.Y = models[0].Position.Y + 10;
            CameraTarget.X = models[0].Position.X + (float)Math.Cos(models[0].Rotation.Y - InitialAngleCam - CameraInerty - CamRotKey) * RadiusCam;
            CameraTarget.Z = models[0].Position.Z + (float)Math.Sin(-models[0].Rotation.Y + InitialAngleCam + CameraInerty + CamRotKey) * RadiusCam;
            float cameraRot = CameraRotation.X - CameraInerty - CamRotKey;

            camera = new FreeCamera(CameraTarget, cameraRot, 0, 1, 10000, graphicsDevice);
            camera.Update();
            //camera = new Camera(this, CameraTarget, 0f, CameraRotation.X - CameraInerty, -2.5f, -1.5f, -2f, GraphicsDevice.Viewport.AspectRatio, 0.0002f, 1000f);      
        }

        public void RenderToGBuffer(Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            for (int i = 0; i < models.Count; i++)
                models[i].RenderToGBuffer(camera, graphicsDevice);
        }

        public void RenderShadowMap(ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            for (int i = 0; i < models.Count; i++)
                models[i].RenderShadowMap(ref viewProj, graphicsDevice);
        }

        public void Draw(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            for (int i = 0; i < models.Count; i++)
                models[i].Draw(camera, graphicsDevice, lightBuffer);
        }

        public void Draw(Camera.Camera camera)
        {
            for (int i = 0; i < models.Count; i++)
                models[i].Draw(camera.View, camera.Projection, camera.Transform.Translation);
        }
    }
}