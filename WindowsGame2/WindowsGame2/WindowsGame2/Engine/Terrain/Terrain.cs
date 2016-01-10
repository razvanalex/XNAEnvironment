using Engine.Terrain.Collections.Generic;
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

namespace Engine.Terrain
{

    /// <summary>
    /// <para>Instanciate a new <see cref="Terrain"/> object.</para>
    /// </summary>
    public class Terrain
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector<QuadTree> _quadTrees;

        #endregion


        #region Properties

        /// <summary>
        /// <para>Base size of the Vector containing the QuadTrees.</para>
        /// </summary>
        public const int BaseQuadTreesVectorSize = 10;


        /// <summary>
        /// <para>Gets or sets an array of Quad tree that make the terrain.</para>
        /// </summary>
        public Vector<QuadTree> QuadTrees
        {
            get
            {
                if (this._quadTrees == null)
                    this._quadTrees = new Vector<QuadTree>(Terrain.BaseQuadTreesVectorSize);
                return this._quadTrees;
            }
            set
            {
                 this._quadTrees = value;
            }
        }
        #endregion

        #region 3D
        public void TerrainTextures(QuadTree Qtree, Texture2D[] TexturesMap, Texture2D[] Textures, int[] textureTiling, ContentManager Content)
        {
            //TexturesMaps
            for (int i = 0; i < TexturesMap.Length; i++)
                Qtree.TexturesMaps[i] = TexturesMap[i];

            //Textures
            for (int i = 0; i < Textures.Length; i++)
                Qtree.Textures[i] = Textures[i];

            //Tiling
            for (int i = 0; i < Textures.Length; i++)
                Qtree.textureTiling[i] = textureTiling[i];

            //Detail Texture
            Qtree.DetailTexture = Content.Load<Texture2D>("textures//Terrain//noise_texture");
        }
        public void InitializeTerrin(QuadTree Qtree, Camera.Camera camera, Terrain terrain, Texture2D HeightMap, Vector3 WaterPos, ContentManager Content, GraphicsDevice graphicsDevice)
        {

            Camera.Camera.DefaultCamera = camera;
            float quadFrontTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildFrontTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float quadFarTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildFarTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float vertexDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.VertexTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float nodeRelevance = float.Parse(System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.ChildRelevanceThreshold], CultureInfo.GetCultureInfo("en-us"));

            for (int i = 0; i < terrain.QuadTrees.Count; i++)
            {
                terrain.QuadTrees[i].NodeRelevance = nodeRelevance;
                terrain.QuadTrees[i].QuadTreeDetailAtFront = quadFrontTreeDetail;
                terrain.QuadTrees[i].QuadTreeDetailAtFar = quadFarTreeDetail;
                terrain.QuadTrees[i].VertexDetail = vertexDetail;
                LoadGround(terrain.QuadTrees[i], HeightMap, Content);
            }

            Qtree.effect = Content.Load<Effect>("Effects//Terrain");
            Qtree._effect = Content.Load<Effect>("shaders//LPPMainEffect");
            terrain.Initialize();
            terrain.Load(graphicsDevice);
            Qtree.WaterHeight = WaterPos.Y;

        }
        public void LoadGround(QuadTree tree, Texture2D HeightMap, ContentManager Content)
        {
            string heightMapTextureName = System.Configuration.ConfigurationManager.AppSettings[WindowsGame2.Properties.Resources.HeightGrayScaleImage];
            using (Texture2D heightMap = HeightMap)
            {
                LoadHeightData(tree, heightMap);
            }
        }
        private void LoadHeightData(QuadTree tree, Texture2D heightMap)
        {
            Color[] heightMapColors = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData(heightMapColors);

            tree.HeightData = new float[heightMap.Width, heightMap.Height];
            for (int x = 0; x < heightMap.Width; x++)
                for (int y = 0; y < heightMap.Height; y++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapColors[y * heightMap.Width + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Multiply by max height to get final height
                    tree.HeightData[x, y] = amt * BasicWorld.THeight;
                }
        }

        public void Initialize()
        {
            for (int i = 0; i < this._quadTrees.Count; i++)
            {
                this._quadTrees[i].Initialize();
            }
        }

        public void Load(GraphicsDevice device)
        {
            for (int i = 0; i < this._quadTrees.Count; i++)
                this._quadTrees[i].Load(device);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < this._quadTrees.Count; i++)
                this._quadTrees[i].Update(gameTime);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            for (int i = 0; i < this._quadTrees.Count; i++)
                this._quadTrees[i].Draw(View, Projection, CameraPosition);
        }

        #endregion

    }

}
