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

using Engine.Camera;
using Engine.Terrain;
using Engine.Water;

namespace Engine.Billboards
{
    public class Billboard
    {
        #region Declarations and Constants
        ContentManager content;
        GraphicsDevice graphicsDevice;

        public int LOD = 3;
        public int NoLeaves = 30;
        public int NoTrees = 1;

        public List<BillboardsSystem>[] leaves = new List<BillboardsSystem>[30];
        public List<BillboardsSystem>[] trunck = new List<BillboardsSystem>[1];

        //Instancing
        public Matrix[][] instanceTransforms;
        Model[] instancedModel;
        Matrix[][] instancedModelBones;

        public Matrix[][] instanceTransforms1;
        Model instancedModel1;
        Matrix[] instancedModelBones1;

        Vector3 UpLight;// = Vector3.Zero;
        Vector3 RightLight;// = Vector3.Zero;
        Vector3 LightDirection;// = Vector3.Zero;
        Vector3 LightColor;// = Vector3.Zero;
        Vector3 AmbientColor;// = Vector3.Zero;

        Vector3[] PositionL = new Vector3[30];
        Camera.Camera camera;

        object Terrain, Sky;
        int[] no;
        int no_t;
        bool[][] visible_tree;
        private bool[] visible_leaves;

        public bool[] LeavesAreVisible
        {
            get
            {
                return visible_leaves;
            }
        }

        //public variables
        public Vector3[] Position;
        public Vector3[] Rotation;
        public Vector3 Scale = new Vector3(0.1f);
        public Vector3 ScaleL = new Vector3(0.02f);

        public bool Leaves = true; // by default Leaves is true, so be care when you call LoadData() function
        #endregion
        public Billboard(ContentManager content, Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            this.camera = camera;
            this.graphicsDevice = graphicsDevice;
        }
        public void GeneratePositionAndRotation(Texture2D TreeMap, Vector3 Scale)
        {
            Random rnd = new Random();
            Random rotRND = new Random();
            Color[] TreePixels = new Color[TreeMap.Width * TreeMap.Height];
            TreeMap.GetData<Color>(TreePixels);
            float stepness = 0;

            for (int i = 0; i < NoTrees; i++)
            {
                float x = rnd.Next(-256 * 25, 256 * 25);
                float z = rnd.Next(-256 * 25, 256 * 25);

                float RotX = rotRND.Next(0, 64);

                int xCoord = (int)(x / 25) + 256;
                int zCoord = (int)(z / 25) + 256;

                float texVal = TreePixels[zCoord * 512 + xCoord].R / 255f;
                float y = 0;

                float fix = 2.5f;
                x *= Scale.X * fix;
                z *= Scale.Z * fix;

                if (Terrain is QuadTree)
                    y = ((QuadTree)Terrain).GetHeight(x, z);
                else if (Terrain is SmallTerrain)
                    y = ((SmallTerrain)Terrain).GetHeightAtPosition(x, z, out stepness);

                //Check trees collision
                bool Check = true;
                /* It's in working now */

                if ((int)((float)rnd.NextDouble() * texVal * 10) == 1 && Check)
                {
                    Position[i] = new Vector3(x, y, z);
                    Rotation[i] = new Vector3(0, 0, 0);
                }
                else i--;
            }
        }
        /// <summary>
        /// Load textures and models for trees.
        /// If Leaves is true then use LoadData(Model[] Trunck, Model Leaves);
        /// </summary>
        /// <param name="Trunck">Main tree model and also trunck model for maximum detail.</param>
        /// <param name="Leaves">Model for leaves. It will be rendered at maximum detail</param>
        public void LoadData(Model[] Trunck, Model Leaves)
        {
            LOD = Trunck.Length;

            leaves = new List<BillboardsSystem>[NoTrees];
            trunck = new List<BillboardsSystem>[LOD];

            instanceTransforms = new Matrix[LOD + 1][];
            instancedModel = new Model[LOD];
            instancedModelBones = new Matrix[LOD][];

            instanceTransforms1 = new Matrix[NoTrees][];
            Position = new Vector3[NoTrees];
            Rotation = new Vector3[NoTrees];
            PositionL = new Vector3[NoLeaves];
            no = new int[LOD + 1];
            visible_tree = new bool[LOD + 1][];
            visible_leaves = new bool[NoTrees];

            for (int i = 0; i < LOD; i++)
                instancedModel[i] = Trunck[i];

            instancedModel1 = Leaves;
        }
        /// <summary>
        /// Load textures and models for trees.
        /// If Leaves is true then use LoadData(Model[] Trunck, Model Leaves);
        /// </summary>
        /// <param name="Trunck">Main tree model and also trunck model for maximum detail.</param>
        /// <param name="Leaves">Use false if you don't want to use leaves model.</param>
        public void LoadData(Model[] Trunck, bool Leaves)
        {
            LOD = Trunck.Length;

            leaves = new List<BillboardsSystem>[NoTrees];
            trunck = new List<BillboardsSystem>[LOD];

            instanceTransforms = new Matrix[LOD + 1][];
            instancedModel = new Model[LOD];
            instancedModelBones = new Matrix[LOD][];

            instanceTransforms1 = new Matrix[NoTrees][];
            Position = new Vector3[NoTrees];
            Rotation = new Vector3[NoTrees];
            PositionL = new Vector3[0];
            no = new int[LOD + 1];
            visible_tree = new bool[LOD + 1][];
            visible_leaves = new bool[0];

            for (int i = 0; i < LOD; i++)
                instancedModel[i] = Trunck[i];
            this.Leaves = Leaves;
        }
        public void Initialize()
        {
            for (int tree = 0; tree < LOD; tree++)
            {
                instancedModelBones[tree] = new Matrix[instancedModel[tree].Bones.Count];
                instancedModel[tree].CopyAbsoluteBoneTransformsTo(instancedModelBones[tree]);
            }

            for (int lod = 0; lod < LOD; lod++)
            {
                trunck[lod] = new List<BillboardsSystem>();
                for (int tree = 0; tree < NoTrees; tree++)
                {
                    float Heigth = 0;
                    trunck[lod].Add(new BillboardsSystem(content, BillboardsSystem.BillboardMode.None, instancedModel[lod], instancedModelBones[lod], instanceTransforms[lod], new Vector3(Position[tree].X, Heigth, Position[tree].Z), Rotation[tree], Scale, LightDirection, LightColor, AmbientColor, graphicsDevice));
                }
            }

            if (Leaves)
            {
                instancedModelBones1 = new Matrix[instancedModel1.Bones.Count];
                instancedModel1.CopyAbsoluteBoneTransformsTo(instancedModelBones1);

                PositionL[0] = new Vector3(0, 300, 0);
                PositionL[1] = new Vector3(0, 400, 0);
                PositionL[2] = new Vector3(0, 200, 0);
                PositionL[3] = new Vector3(100, 350, 100);
                PositionL[4] = new Vector3(50, 350, 50);
                PositionL[5] = new Vector3(100, 200, -100);
                PositionL[6] = new Vector3(80, 330, -50);
                PositionL[7] = new Vector3(-50, 250, -100);
                PositionL[8] = new Vector3(100, 300, -100);
                PositionL[9] = new Vector3(-100, 300, 100);
                PositionL[10] = new Vector3(-150, 350, 50);
                PositionL[11] = new Vector3(-50, 250, 150);
                PositionL[12] = new Vector3(-100, 450, 100);
                PositionL[13] = new Vector3(100, 450, 0);
                PositionL[14] = new Vector3(100, 450, -100);
                PositionL[15] = new Vector3(-100, 400, -100);
                PositionL[16] = new Vector3(0, 400, -100);
                PositionL[17] = new Vector3(100, 300, 100);
                PositionL[18] = new Vector3(-100, 300, 0);
                PositionL[19] = new Vector3(-100, 300, 100);
                PositionL[20] = new Vector3(0, 300, 100);
                PositionL[21] = new Vector3(0, 300, -200);
                PositionL[22] = new Vector3(100, 200, 100);
                PositionL[23] = new Vector3(80, 210, -75);
                PositionL[24] = new Vector3(-100, 230, 0);
                PositionL[25] = new Vector3(218, 250, 0);
                PositionL[26] = new Vector3(-80, 500, 50);
                PositionL[27] = new Vector3(-30, 400, 90);
                PositionL[28] = new Vector3(0, 450, 0);
                PositionL[29] = new Vector3(0, 150, 100);

                for (int tree = 0; tree < NoTrees; tree++)
                    leaves[tree] = new List<BillboardsSystem>();

                for (int tree = 0; tree < NoTrees; tree++)
                    for (int leave = 0; leave < NoLeaves; leave++)
                        leaves[tree].Add(new BillboardsSystem(content, BillboardsSystem.BillboardMode.Spherical,
                                            instancedModel1, instancedModelBones1, instanceTransforms1[tree],
                                            Position[tree] + PositionL[leave] * Scale, Rotation[tree],
                                            ScaleL, LightDirection, LightColor, AmbientColor, graphicsDevice));
            }
        }
        public void generateTags()
        {
            for (int lod = 0; lod < LOD; lod++)
                for (int tree = 0; tree < NoTrees; tree++)
                    trunck[lod][tree].generateTags();

            if (Leaves)
                for (int i = 0; i < NoTrees; i++)
                    if (leaves[i].Count != 0)
                        for (int j = 0; j < NoLeaves; j++)
                            leaves[i][j].generateTags();
        }
        public void Update(GameTime gameTime)
        {
            UpdateLOD();
            for (int lod = 0; lod < LOD; lod++)
                for (int i = 0; i < no[lod]; i++)
                    if (visible_tree[lod][i])
                        trunck[lod][i].Update(gameTime);

            if (Leaves)
                for (int i = 0; i < NoTrees; i++)
                    if (visible_leaves[i])
                        for (int j = 0; j < NoLeaves; j++)
                            leaves[i][j].Update(gameTime);
        }
        public void UpdateLight(Vector3 LightDirection, Vector3 LightColor, Vector3 AmbientColor)
        {
            this.LightDirection = LightDirection;
            this.LightColor = LightColor;
            this.AmbientColor = AmbientColor;

            for (int lod = 0; lod < LOD; lod++)
                for (int i = 0; i < no[lod]; i++)
                {
                    trunck[lod][i].LightDirection = LightDirection;
                    trunck[lod][i].LightColor = LightColor;
                    trunck[lod][i].AmbientColor = AmbientColor;
                }
            if (Leaves)
                for (int i = 0; i < NoTrees; i++)
                    if (visible_leaves[i])
                        for (int j = 0; j < NoLeaves; j++)
                        {
                            leaves[i][j].LightDirection = LightDirection;
                            leaves[i][j].LightColor = LightColor;
                            leaves[i][j].AmbientColor = AmbientColor;
                        }
        }
        public void GenerateSunVectors(float pitch)
        {
            RightLight = Vector3.Forward;
            UpLight = Vector3.Cross(-LightDirection, RightLight);//new Vector3(LightDirection.X * (float)Math.Sin(pitch), LightDirection.Y * (float)Math.Cos(pitch), LightDirection.Z);     
        }

        public void GetData(object[] obj)
        {
            foreach (object o in obj)
            {
                if (o is QuadTree)
                    Terrain = ((QuadTree)o);
                if (o is SmallTerrain)
                    Terrain = ((SmallTerrain)o);
            }
        }
        void UpdateLOD()
        {
            for (int i = 0; i <= LOD; i++)
            {
                no[i] = 0;
                visible_tree[i] = new bool[NoTrees];
                for (int j = 0; j < NoTrees; j++)
                {
                    visible_tree[i][j] = false;
                    if (Leaves && i == 0)
                        visible_leaves[j] = false;
                }
            }


            for (int tree = 0; tree < NoTrees; tree++)
            {
                int lod = 0;
                double distance = Math.Sqrt(Math.Pow(camera.Transform.Translation.X - Position[tree].X, 2) + Math.Pow(camera.Transform.Translation.Y - Position[tree].Y, 2) + Math.Pow(camera.Transform.Translation.Z - Position[tree].Z, 2));
                float Heigth = 0, Stepness;
                if (Terrain is QuadTree)
                    Heigth = ((QuadTree)Terrain).GetHeight(Position[tree].X, Position[tree].Z);
                else if (Terrain is SmallTerrain)
                    Heigth = ((SmallTerrain)Terrain).GetHeightAtPosition(Position[tree].X, Position[tree].Z, out Stepness);

                if (distance < 500)
                {
                    lod = 0;
                    trunck[lod][no[lod]] = new BillboardsSystem(content, BillboardsSystem.BillboardMode.None,
                                            instancedModel[lod], instancedModelBones[lod], instanceTransforms[lod],
                                            new Vector3(Position[tree].X, Heigth, Position[tree].Z), Rotation[tree],
                                            Scale, LightDirection, LightColor, AmbientColor, graphicsDevice);
                    if (Leaves)
                    {
                        for (int leave = 0; leave < NoLeaves; leave++)
                            leaves[tree][leave] = new BillboardsSystem(content, BillboardsSystem.BillboardMode.Spherical,
                                                instancedModel1, instancedModelBones1, instanceTransforms1[tree],
                                                Position[tree] + PositionL[leave] * Scale, Rotation[tree],
                                                ScaleL, LightDirection, LightColor, AmbientColor, graphicsDevice);
                        visible_leaves[tree] = true;
                    }

                    visible_tree[lod][no[lod]] = true;
                    no[lod]++;
                }
                else if (distance >= 500 && distance <= 3000)
                {
                    lod = 1;
                    trunck[lod][no[lod]] = new BillboardsSystem(content, BillboardsSystem.BillboardMode.None,
                                            instancedModel[lod], instancedModelBones[lod], instanceTransforms[lod],
                                            new Vector3(Position[tree].X, Heigth, Position[tree].Z), Rotation[tree],
                                            Scale, LightDirection, LightColor, AmbientColor, graphicsDevice);

                    visible_tree[lod][no[lod]] = true;
                    no[lod]++;
                }
                else if (distance > 3000 && distance <= 4000)
                {
                    lod = 2;
                    trunck[lod][no[lod]] = new BillboardsSystem(content, BillboardsSystem.BillboardMode.None,
                                            instancedModel[lod], instancedModelBones[lod], instanceTransforms[lod],
                                            new Vector3(Position[tree].X, Heigth, Position[tree].Z), Rotation[tree],
                                            Scale, LightDirection, LightColor, AmbientColor, graphicsDevice);

                    visible_tree[lod][no[lod]] = true;
                    no[lod]++;
                }
                else if (distance > 5000)
                {
                    lod = 3;
                    no[lod]++;
                }
            }

            no_t = 0;
            for (int i = 0; i <= LOD; i++)
                no_t += no[i];
        }
        public void TreePreDraw()
        {
            UpdateLight(LightDirection, LightColor, AmbientColor);
            for (int i = 0; i <= LOD; i++)
                Array.Resize(ref instanceTransforms[i], no[i]);

            int[] n = new int[LOD];
            for (int i = 0; i < LOD; i++)
                n[i] = 0;

            for (int lod = 0; lod < LOD; lod++)
                for (int i = 0; i < no[lod]; i++)
                {
                    if (visible_tree[lod][i])
                    {
                        instanceTransforms[lod][n[lod]] = trunck[lod][i].Transform;
                        trunck[lod][i].UpdateTransformationMatrix(instanceTransforms[lod]);
                        n[lod]++;
                    }
                }

            if (Leaves)
                for (int i = 0; i < NoTrees; i++)
                    if (visible_leaves[i])
                    {
                        Array.Resize(ref instanceTransforms1[i], NoLeaves);
                        leaves[i][0].UpdateCamUpRightVector(camera.Transform.Up, camera.Transform.Right);
                        leaves[i][0].UpdateLightUpRightVector(Vector3.Cross(Vector3.Normalize(LightDirection), Vector3.Forward), Vector3.Forward);
                        for (int j = 0; j < NoLeaves; j++)
                            instanceTransforms1[i][j] = leaves[i][j].Transform;
                    }
                    else Array.Resize(ref instanceTransforms1[i], 0);
        }
        public void Draw(Matrix View, Matrix Projection, Vector3 Position)
        {
            TreePreDraw();

            for (int lod = 0; lod < LOD; lod++)
                if (instanceTransforms[lod].Length != 0)
                    trunck[lod][0].Draw(View, Projection, Position);

            if (Leaves)
                for (int i = 0; i < NoTrees; i++)
                {
                    if (visible_leaves[i])
                        for (int j = 0; j < NoLeaves; j++)
                        {
                            leaves[i][j].UpdateTransformationMatrix(instanceTransforms1[i]);
                            if (j == 0)
                                leaves[i][j].Draw(View, Projection, Position);
                        }
                }
        }
    }
}