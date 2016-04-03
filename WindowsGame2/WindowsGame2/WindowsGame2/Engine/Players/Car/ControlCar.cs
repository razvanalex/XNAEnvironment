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


namespace Engine
{
    public class CarControl
    {
        const float friction = 0.020f;
        float WheelBreakes = 0.52f;
        public float UAcceleration = 0.1f;
        public float DAcceleration = 0.1f;
        float NitroAcceleration;

        public Vector3 Rotation;
        public float speed = 0f;
        public float maxspeed;
        float minspeed = -0.08f;

        public float gear = 0;
        float Turn = 0.2f;
        float MaxTurn = MathHelper.Pi / 50;
        float MinTurn = -MathHelper.Pi / 50;
        float MaxTurn1 = MathHelper.Pi / 3000;
        float MinTurn1 = -MathHelper.Pi / 3000;
        public float Turning = 0.2f;

        public float WheelTurn = 0;
        float WheelTurnMax = MathHelper.Pi / 6;
        float WheelTurnMin = -MathHelper.Pi / 6;

        public float CameraInerty = 0;
        float CameraInertyMax = 0.5f;
        float CameraInertyMin = -0.5f;

        public CarControl(Game game, Vector3 Rotation, float speed, float maxSpeed, float Turning, float UAcceleration, float DAcceleration, float WheelTurn, float gear, float CameraInerty)
        {
            this.UAcceleration = UAcceleration;
            this.DAcceleration = DAcceleration;
            this.speed = speed;
            this.Turning = Turning;
            this.Rotation = Rotation;
            this.WheelTurn = WheelTurn;
            this.gear = gear;
            this.CameraInerty = CameraInerty;
            this.maxspeed = maxSpeed;
        }

        public void Control()
        {
#if !XBOX
            KeyboardState keyboardstate = Keyboard.GetState();
            
            //Controls
            //Turning
            if (speed != 0)
            {
                if (speed > 0.020f)
                {
                    Turning = (Turn / speed);
                    if (Turning > 0)
                    {
                        if (Turning >= MaxTurn)
                            Turning = MaxTurn;
                        if (Turning <= MaxTurn1)
                            Turning = MaxTurn1;
                    }
                    if (Turning < 0)
                    {
                        if (Turning <= MinTurn)
                            Turning = MinTurn;
                        if (Turning >= MinTurn1)
                            Turning = MinTurn1;
                    }
                }
                else if (speed <= 0.020f)
                {
                    Turning = speed * 2;
                    if (Turning > 0)
                    {
                        if (Turning >= MaxTurn)
                            Turning = MaxTurn;
                        if (Turning <= MaxTurn1)
                            Turning = MaxTurn1;
                    }
                    if (Turning < 0)
                    {
                        if (Turning <= MinTurn)
                            Turning = MinTurn;
                        if (Turning >= MinTurn1)
                            Turning = MinTurn1;
                    }
                }
            }
            else Turning = 0f;

            // Up
            if (keyboardstate.IsKeyDown(Keys.Up))
            {
                speed += UAcceleration;
                if (keyboardstate.IsKeyDown(Keys.Left))
                    Rotation += new Vector3(Turning, 0, 0);
                if (keyboardstate.IsKeyDown(Keys.Right))
                    Rotation -= new Vector3(Turning, 0, 0);
                if (speed > maxspeed)
                    speed = maxspeed;
            }
            else if (keyboardstate.IsKeyDown(Keys.Left) && (speed > 0))
            {
                Rotation += new Vector3(0, Turning, 0);
                speed -= friction;
                if (speed < 0)
                    speed = 0;
            }
            else if (keyboardstate.IsKeyDown(Keys.Right) && (speed > 0))
            {
                Rotation -= new Vector3(Turning, 0, 0);
                speed -= friction;
                if (speed < 0)
                    speed = 0;
            }
            else
            {
                if (speed > 0)
                {
                    speed -= friction;
                    if (speed < 0)
                        speed = 0;
                }
            }

            //Down
            if (keyboardstate.IsKeyDown(Keys.Down))
            {
                speed -= DAcceleration;
                if (keyboardstate.IsKeyDown(Keys.Left))
                    Rotation += new Vector3(Turning, 0, 0);
                if (keyboardstate.IsKeyDown(Keys.Right))
                    Rotation -= new Vector3(Turning, 0, 0);
                if (speed < minspeed)
                    speed = minspeed;
            }
            else if (keyboardstate.IsKeyDown(Keys.Left) && (speed <= 0))
            {
                Rotation += new Vector3(Turning, 0, 0);
                speed += friction;
                if (speed > 0)
                    speed = 0;
            }
            else if (keyboardstate.IsKeyDown(Keys.Right) && (speed <= 0))
            {
                Rotation -= new Vector3(0, Turning, 0);
                speed += friction;
                if (speed > 0)
                    speed = 0;
            }
            else
            {
                if (speed < 0)
                {
                    speed += friction;
                    if (speed > 0)
                        speed = 0;
                }
            }
            // Breakes
            if (keyboardstate.IsKeyDown(Keys.Space))
            {
                if (speed > 0)
                    speed -= WheelBreakes + friction;
                else if (speed < 0)
                    speed += WheelBreakes + friction;
                else speed = 0;
            }
            //Wheels Turning
            bool lr = false;
            bool FreeCam = false;
            if (keyboardstate.IsKeyDown(Keys.Left))
            {
                if (WheelTurn < WheelTurnMax)
                    WheelTurn += 0.05f;
                else WheelTurn = WheelTurnMax;
                lr = true;
                FreeCam = true;
                if (speed > 0)
                {
                    if ((gear == 1) && (speed <= 0.050f))
                        speed -= 0f;
                    else if ((gear == 1) && (speed > 0.050f))
                        speed -= 0.40f;
                    else if (gear == 2)
                        speed -= 0.35f;
                    else if (gear == 3)
                        speed -= 0.30f;
                    else if (gear == 4)
                        speed -= 0.25f;
                    else if (gear == 5)
                        speed -= 0.20f;
                    else if (gear == 6)
                        speed -= 0.15f;
                }
                else if ((gear == -1) && (speed <= -0.030f))
                    speed += 0.4f;
                CameraInerty += 0.5f;
                if (CameraInerty >= CameraInertyMax)
                    CameraInerty = CameraInertyMax;
            }
            if (keyboardstate.IsKeyDown(Keys.Right))
            {
                if (WheelTurn > WheelTurnMin)
                    WheelTurn -= 0.05f;
                else WheelTurn = WheelTurnMin;
                lr = true;
                FreeCam = true;
                if (speed > 0)
                {
                    if ((gear == 1) && (speed <= 0.050f))
                        speed -= 0f;
                    else if ((gear == 1) && (speed > 0.050f))
                        speed -= 0.40f;
                    else if (gear == 2)
                        speed -= 0.35f;
                    else if (gear == 3)
                        speed -= 0.30f;
                    else if (gear == 4)
                        speed -= 0.25f;
                    else if (gear == 5)
                        speed -= 0.20f;
                    else if (gear == 6)
                        speed -= 0.15f;
                }
                else if ((gear == -1) && (speed <= -0.030f))
                    speed += 0.4f;
                CameraInerty -= 0.05f;
                if (CameraInerty <= CameraInertyMin)
                    CameraInerty = CameraInertyMin;
            }
            if (!lr)
            {
                if (WheelTurn >= 0)
                    WheelTurn -= 0.05f;
                else if (WheelTurn == 0)
                    WheelTurn = 0;
                if (WheelTurn <= 0)
                    WheelTurn += 0.05f;
                else if (WheelTurn == 0)
                    WheelTurn = 0;
            }
            if ((!FreeCam) || (speed == 0))
            {
                if (CameraInerty > 0)
                    CameraInerty -= 0.01f;
                else if (CameraInerty == 0)
                    CameraInerty = 0;
                if (CameraInerty < 0)
                    CameraInerty += 0.01f;
                else if (CameraInerty == 0)
                    CameraInerty = 0;
            }


            if (FreeCam)
            {
                if (keyboardstate.IsKeyDown(Keys.Left))
                {
                    if (CameraInerty < 0)
                        CameraInerty += 0.01f;
                    if (CameraInerty >= CameraInertyMax)
                        CameraInerty = CameraInertyMax;
                }
                if (keyboardstate.IsKeyDown(Keys.Right))
                {
                    if (CameraInerty > 0)
                        CameraInerty -= 0.01f;
                    if (CameraInerty <= CameraInertyMin)
                        CameraInerty = CameraInertyMin;
                }
            }

            //Nitro
            if (keyboardstate.IsKeyDown(Keys.LeftAlt))
            {
                if (speed >= 0.5f)
                {
                    NitroAcceleration = 0.1f;
                    if (speed > 0.10f)
                    {
                        speed += NitroAcceleration;
                        if (speed > maxspeed)
                            speed = maxspeed;
                    }
                }
            }
#endif

            gearbox();
            Acceleration();

        }

        public void gearbox()
        {
            if ((speed > 0) && (speed <= 40f))
                gear = 1;
            if ((speed > 40f) && (speed <= 80f))
                gear = 2;
            if ((speed > 80f) && (speed <= 150f))
                gear = 3;
            if ((speed > 150f) && (speed <= 200f))
                gear = 4;
            if ((speed > 200f) && (speed <= 260f))
                gear = 5;
            if ((speed > 260f) && (speed <= 350f))
                gear = 6;
            if ((speed < 0f) && (speed >= -60f))
                gear = -1;
        }
        public void Acceleration()
        {
            if (gear == -1)
                DAcceleration = 0.36f;
            if (gear == 1)
                UAcceleration = 0.36f;
            if (gear == 2)
                UAcceleration = 0.30f;
            if (gear == 3)
                UAcceleration = 0.24f;
            if (gear == 4)
                UAcceleration = 0.18f;
            if (gear == 5)
                UAcceleration = 0.12f;
            if (gear == 6)
                UAcceleration = 0.06f;
        }
    }
}
