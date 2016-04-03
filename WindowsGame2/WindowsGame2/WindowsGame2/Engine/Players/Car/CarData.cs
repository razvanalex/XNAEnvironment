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
    public class CarsData
    {
        public float MaxSpeed;
        string modelCar;
        public string CarModelName = "";
        public string Model_Wheel = "";
        public Vector3 Scale_Car;
        public Vector3 Scale_Wheel;

        public CarsData(Game game, string CarModel)
        {
            modelCar = CarModel;
            CarData();
        }
        public void CarData()
        {
            if (modelCar == "Lamborghini Aventador 2012")
            {
                CarModelName = "/Lamborghini_Aventador_2012";
                Model_Wheel = @"models/Cars/Lamborghini Aventador 2012/Wheel";
                Scale_Car = new Vector3(.39f);
                Scale_Wheel = new Vector3(.38f);
                MaxSpeed = 350f;
            }
            if (modelCar == "Lamborghini Veneno")
            {
                CarModelName = "/Lamborghini_Veneno";
                Model_Wheel = @"models/Cars/Lamborghini Veneno/Wheel1";
                Scale_Car = new Vector3(.39f);
                Scale_Wheel = new Vector3(.38f);
                MaxSpeed = 400f;
            }
            if (modelCar == "Audi R8")
            {
                CarModelName = "/AudiR8";
                Model_Wheel = @"models/Cars/Audi R8/Wheel2";
                Scale_Car = new Vector3(.39f);
                Scale_Wheel = new Vector3(.38f);
                MaxSpeed = 300f;
            }

        }
    }
}
