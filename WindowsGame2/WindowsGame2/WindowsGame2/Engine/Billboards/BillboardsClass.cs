using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Engine.Camera;
using Engine.Terrain;
using Engine.Water;

namespace Engine.Billboards
{

    public class BillboardsClass
    {
        ContentManager content;
        GraphicsDevice graphicsDevice;

        Camera.Camera camera;
        object Terrain;

        public Vector3 LightDirection;
        public Vector3 LightColor;
        public Vector3 AmbientColor;
        public float SunPitch = 0;

        public Billboard Fir;
        public Billboard Linden;
        public Billboard Palm;

        public Vector3 Scale;

        public BillboardsClass(ContentManager content, Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            this.camera = camera;
            this.graphicsDevice = graphicsDevice;
        }

        public void Initialize()
        {
            //Generate Firs
            Fir = new Billboard(content, camera, graphicsDevice);
            
            Fir.NoTrees = 2000;
            Fir.Scale = new Vector3(0.5f);
            
            Fir.GetData(new object[] { Terrain });
            Fir.LoadData(new Model[] {  content.Load<Model>("models//Trees//Fir//Fir_Tree"), 
                                        content.Load<Model>("models//Trees//Fir//FarFir_Tree"),
                                        content.Load<Model>("models//Trees//Fir//Far_FarFir_Tree") 
                                     }, false);
            Fir.GeneratePositionAndRotation(content.Load<Texture2D>("models//Trees//Fir//Fir_TreeMAP"), Scale);
            Fir.Initialize();
            Fir.generateTags();

            //Generate Lindens
            Linden = new Billboard(content, camera, graphicsDevice);

            Linden.NoLeaves = 30;
            Linden.NoTrees = 2000;
            Linden.Scale = new Vector3(0.2f);
            Linden.ScaleL = new Vector3(0.002f);

            Linden.GetData(new object[] { Terrain });
            Linden.LoadData(new Model[] {  content.Load<Model>("models//Trees//Linden//Linden"),
                                        content.Load<Model>("models//Trees//Linden//FarLinden"),
                                        content.Load<Model>("models//Trees//Linden//Far_FarLinden") 
                                     }, content.Load<Model>("models//Trees//Linden//leaves"));
            Linden.GeneratePositionAndRotation(content.Load<Texture2D>("models//Trees//Linden//Linden_TreeMAP"), Scale);
            Linden.Initialize();
            Linden.generateTags();

            //Generate Palms
            Palm = new Billboard(content, camera, graphicsDevice);

            Palm.NoTrees = 1000;
            Palm.Scale = new Vector3(0.2f);

            Palm.GetData(new object[] { Terrain });
            Palm.LoadData(new Model[] {  content.Load<Model>("models//Trees//Palms//Palm"), 
                                        content.Load<Model>("models//Trees//Palms//Far_FarPalm"),
                                        content.Load<Model>("models//Trees//Palms//Far_FarPalm") 
                                     }, false);
            Palm.GeneratePositionAndRotation(content.Load<Texture2D>("models//Trees//Palms//Palm_TreeMAP"), Scale);
            Palm.Initialize();
            Palm.generateTags();
        }

        public void Update(GameTime gameTime)
        {
            //Update Firs          
            Fir.UpdateLight(LightDirection, LightColor, AmbientColor);
            Fir.GenerateSunVectors(SunPitch);
            Fir.Update(gameTime);         

            //Update Lindens         
            Linden.UpdateLight(LightDirection, LightColor, AmbientColor);
            Linden.GenerateSunVectors(SunPitch);
            Linden.Update(gameTime);

            //Update Palms   
            Palm.UpdateLight(LightDirection, LightColor, AmbientColor);
            Palm.GenerateSunVectors(SunPitch);
            Palm.Update(gameTime);
        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                {
                    Terrain = o;
                    Scale = new Vector3(((QuadTree)Terrain).Scale);
                }
                if (o is SmallTerrain)
                {
                    Terrain = o;
                    Scale = new Vector3(1);
                }
            }
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 Position)
        {
            //Draw Firs
            Fir.Draw(View, Projection, Position);

            //Draw Lindens
            Linden.Draw(View, Projection, Position);

            //Draw Palms
            Palm.Draw(View, Projection, Position);
        }
    }
}