
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.Terrain.Collections.Generic;
using Engine.Water;
namespace Engine.Terrain
{

    public class QuadTree : QuadNodeCollection, IDisposable, IRenderable
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public Vector<VertexPositionNormalTextureTangentBinormal> Vertices;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public Vector<int> Indices;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public Effect effect;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _size;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _heightFieldSpace;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private byte _depth;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private GraphicsDevice _device;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector2 _location;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _vertexDetail = 17f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _quadTreeDetailAtFront = 10000000f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _quadTreeDetailAtFar = 10000000f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _nodeRelevance = 0.1f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private VertexDeclaration _vertexDeclaration;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _loadUpdateOccurences = 10;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BuffersData _currentBufferData;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector<BuffersData> _disposeDatas;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector<BuffersData> _lastLoadedDatas;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _processIterationId;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float[,] _heightData;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _minimalDepth;
        public Effect _effect;
        protected static QuadRenderer _quadRenderer;

        public const int NoOfTextures = 6;
        public float[] textureTiling = new float[NoOfTextures];
        Vector3 cameraPosition;

        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 AmbientColor { get; set; }

        public Texture2D[] Textures = new Texture2D[NoOfTextures];
        public Texture2D[] TexturesMaps = new Texture2D[NoOfTextures];

        public Texture2D DetailTexture;
        public float DetailDistance = 0;
        public float DetailTextureTiling = 1000;

        public float WaterHeight = 0f;
        #endregion
        private float _scale;

        #region Properties

        /// <summary>
        /// <para>Gets the max number of children that a node can have.</para>
        /// </summary>
        public const int NodeChildsNumber = 4;

        /// <summary>
        /// <para>Gets the max number of children that a node can have.</para>
        /// </summary>
        internal const int ProcessIterationMaxValue = 256;

        /// <summary>
        /// <para>Get and set the scale of terrain.</para>
        /// </summary>
        public float Scale
        {
            get 
            { 
                return this._scale;
            }
            set 
            {
                this._scale = value;
            }
        }
        
        /// <summary>
        /// <para>Gets the current minimal depth upond with node childs are not checked and automatically validated.</para>
        /// </summary>
        internal int MinimalDepth
        {
            get
            {
                return this._minimalDepth;
            }
        }

        private Matrix transform;
        private BoundingSphere boundingSphere;
        public Matrix Transform
        {
            get
            {
                Matrix _scale, _rotation, _position;
                _scale = Matrix.CreateScale(Scale);
                _rotation = Matrix.CreateFromYawPitchRoll(0, 0, 0);
                _position = Matrix.CreateTranslation(new Vector3(0));

               // Matrix.Multiply(ref _scale, ref _rotation, out transform);
                Matrix.Multiply(ref _rotation, ref _position, out transform);

                return transform;
            }
            set { transform = value; }
        }
        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);
            // Merge all the model's built in bounding spheres
            BoundingSphere transformed = BoundingSphere.Transform(Transform);
            sphere = BoundingSphere.CreateMerged(sphere, transformed);
            this.boundingSphere = sphere;
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(0));
                BoundingSphere transformed = boundingSphere;
                transformed = transformed.Transform(worldTransform);
                return transformed;
            }
        }

        /// <summary>
        /// <para>Internal value used to identify the update phase and optimize vertex array searching.</para>
        /// </summary>
        internal int ProcessIterationId
        {
            get
            {
                return this._processIterationId;
            }
        }

        /// <summary>
        /// <para>Gets or sets the heights array.</para>
        /// </summary>
        public float[,] HeightData
        {
            get
            {
                return this._heightData;
            }
            set
            {
                this._heightData = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold relevance for nodes.</para>
        /// </summary>
        public float NodeRelevance
        {
            get
            {
                return this._nodeRelevance;
            }
            set
            {
                this._nodeRelevance = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the vertices.</para>
        /// </summary>
        public float VertexDetail
        {
            get
            {
                return this._vertexDetail;
            }
            set
            {
                this._vertexDetail = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the QuadTree.</para>
        /// </summary>
        public float QuadTreeDetailAtFront
        {
            get
            {
                return this._quadTreeDetailAtFront;
            }
            set
            {
                this._quadTreeDetailAtFront = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the QuadTree.</para>
        /// </summary>
        public float QuadTreeDetailAtFar
        {
            get
            {
                return this._quadTreeDetailAtFar;
            }
            set
            {
                this._quadTreeDetailAtFar = value;
            }
        }

        public Effect Effect
        {
            get
            {
                return this.effect;
            }
        }

        public Effect ShadowEffect
        {
            get
            {
                return this._effect;
            }
        }

        public GraphicsDevice Device
        {
            get
            {
                return this._device;
            }
        }

        /// <summary>
        /// <para>Gets the location of the current tree.</para>
        /// </summary>
        public Vector2 Location
        {
            get
            {
                return this._location;
            }
        }

        /// <summary>
        /// <para>Gets the depth of the current tree.</para>
        /// </summary>
        public int Depth
        {
            get
            {
                return this._depth;
            }
        }

        /// <summary>
        /// <para>Gets the minimal distance between to vertex for the leaf node.</para>
        /// </summary>
        public float HeightFieldSpace
        {
            get
            {
                return this._heightFieldSpace;
            }
        }

        /// <summary>
        /// <para>Gets the size of the tree.</para>
        /// </summary>
        /// <remarks>A tree is a square.</remarks>
        public int Size
        {
            get
            {
                return this._size;
            }
        }

        public override int ChildNumber
        {
            get
            {
                return 1;
            }
        }

        #endregion


        #region Constructors

        public QuadTree(byte depth, int size, float scale, Vector2 location) 
        {
            this.ChildNumber = 1;
            this._location = location;
            this._depth = depth;
            this._size = size;
            this._scale = scale;
            this._heightFieldSpace = (float)(size / System.Math.Pow(2, depth-1));
            this._disposeDatas = new Vector<BuffersData>();
            this._lastLoadedDatas = new Vector<BuffersData>();
            this.Vertices = new Vector<VertexPositionNormalTextureTangentBinormal>(1000);
            this.Indices = new Vector<int>(1000);
            buildBoundingSphere();
        }

        #endregion


        #region 3D

        public void Initialize()
        {
            _quadRenderer = new QuadRenderer();
            
            this.Childs[0] = new QuadNode(null, NodeChild.NorthEast);
            this.Childs[0].Location = this.Location;
            this.Childs[0].ParentTree = this;
            this.Childs[0].Initialize();
        }

        public void Load(GraphicsDevice device)
        {
            this._device = device;           

            this._minimalDepth = 4;

            for (int i = 0; i < this._loadUpdateOccurences; i++)
                UpdateQuadVertices();

            this._minimalDepth = 1;

            this.BuildQuadVerticesList();

            this._currentBufferData = this._lastLoadedDatas[0];

            //this._vertexDeclaration = new VertexDeclaration(null);
            //this.Device.VertexDeclaration = this._vertexDeclaration;
            //this.Device.Vertices[0].SetSource(this._currentBufferData.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);

            this.Device.Indices = this._currentBufferData.IndexBuffer;
        }

        void device_DeviceReset(object sender, EventArgs e)
        {
            //this.Device.`..VertexDeclaration = this._vertexDeclaration;
            //this.Device.Vertices[0].SetSource(this._currentBufferData.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);

            this.Device.Indices = this._currentBufferData.IndexBuffer;
        }

        public void Update(GameTime time)
        {
            //this._effect.FogEnabled = false;
            System.Threading.Thread.Sleep(250);
            //return;
            this._processIterationId += 1;
            if (this._processIterationId > ProcessIterationMaxValue)
                this._processIterationId = 0;

            this.UpdateQuadVertices();
            this.BuildQuadVerticesList();
            this.ClearLastLoaded();
        }

        private void ClearLastLoaded()
        {
            while (this._disposeDatas.Count > 0)
            {
                BuffersData data = this._disposeDatas[0];
                this._disposeDatas.RemoveAt(0);
                data.VertexBuffer.Dispose();
                data.VertexBuffer = null;
                data.IndexBuffer.Dispose();
                data.IndexBuffer = null;
            }
        }

        /// <summary>
        /// <para>First step in quad tree update.</para>
        /// <para>This method update the quads of each</para>
        /// </summary>
        private void UpdateQuadVertices()
        {
            for (int i = 0; i < this.Childs.Length; i++)
                this.Childs[i].Update();
        }

        /// <summary>
        /// <para>Second stepin quad tree update.</para>
        /// <para>This method get all enabled vertices for all sub quad node and build two lists of vertices and and indices.</para>
        /// </summary>
        private void BuildQuadVerticesList()
        {
            this.Vertices.Clear(1000);
            this.Indices.Clear(1000);

            for (int i = 0; i < this.Childs.Length; i++)
                this.Childs[i].GetEnabledVertices();

            /*if (Indices.Count == 0)
                return;*/

            if (!this.Device.IsDisposed)
            {
                IndexBuffer indexBuffer;
                VertexBuffer vertexBuffer;


                vertexBuffer = new VertexBuffer(this.Device, typeof(VertexPositionNormalTextureTangentBinormal), Vertices.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPositionNormalTextureTangentBinormal>(Vertices.ToArray());

                indexBuffer = new IndexBuffer(this.Device, typeof(int), Indices.Count, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(Indices.ToArray());

                BuffersData data = new BuffersData();
                data.IndexBuffer = indexBuffer;
                data.VertexBuffer = vertexBuffer;
                data.NumberOfIndices = Indices.Count / 3;
                data.NumberOfVertices = Vertices.Count;

                this._lastLoadedDatas.Add(data);
            }
         }

        private void SetVertecesIndices()
        {           
            if (_lastLoadedDatas.Count > 0)
            {
                this._disposeDatas.Add(this._currentBufferData);
                this._currentBufferData = _lastLoadedDatas[0];
                this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);
                this.Device.Indices = this._currentBufferData.IndexBuffer;
                _lastLoadedDatas.RemoveAt(0);
            }
            else
            {
                this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);
                this.Device.Indices = this._currentBufferData.IndexBuffer;
            }
        }
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            cameraPosition = CameraPosition;
            SetVertecesIndices();

            effect.Parameters["World"].SetValue(Transform);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);

            effect.Parameters["LightDirection"].SetValue(LightDirection);
            effect.Parameters["LightColor"].SetValue(LightColor);
            effect.Parameters["TextureTiling"].SetValue(textureTiling);

            for (int i = 1; i <= NoOfTextures; i++)
            {
                effect.Parameters[("Texture" + i).ToString()].SetValue(Textures[i - 1]);
                effect.Parameters[("TexturesMaps" + i).ToString()].SetValue(TexturesMaps[i - 1]);
            }

            effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);
            effect.Parameters["CameraPosition"].SetValue(CameraPosition);
            effect.Parameters["WaterHeight"].SetValue(WaterHeight);

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            Device.RasterizerState = rs;
          
            this.Effect.CurrentTechnique.Passes[0].Apply();
            this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._currentBufferData.NumberOfVertices, 0, this._currentBufferData.NumberOfIndices);
        }

        public void Draw(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            SetVertecesIndices();

            Effect effect = _effect;
            effect.CurrentTechnique = effect.Techniques[3];

            effect.Parameters["LightBuffer"].SetValue(lightBuffer);
            effect.Parameters["LightBufferPixelSize"].SetValue(new Vector2(0.5f / lightBuffer.Width, 0.5f / lightBuffer.Height));

            effect.Parameters["World"].SetValue(Transform);
            effect.Parameters["WorldView"].SetValue(Transform * camera.View);
            effect.Parameters["WorldViewProjection"].SetValue(Transform * camera.View * camera.Projection);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);
           
            effect.Parameters["AmbientColor"].SetValue(LightColor);
            effect.Parameters["LightColor"].SetValue(LightColor);
            effect.Parameters["AmbientColor"].SetValue(AmbientColor);

            effect.Parameters["TextureTiling"].SetValue(textureTiling);

            for (int i = 1; i <= NoOfTextures; i++)
            {
                effect.Parameters[("Texture" + i).ToString()].SetValue(Textures[i - 1]);
                effect.Parameters[("TexturesMaps" + i).ToString()].SetValue(TexturesMaps[i - 1]);
            }

            effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);
            effect.Parameters["CameraPosition"].SetValue(camera.Transform.Translation);
            effect.Parameters["WaterHeight"].SetValue(WaterHeight);
            effect.CurrentTechnique.Passes[0].Apply();
        
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            Device.RasterizerState = rs;

            this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._currentBufferData.NumberOfVertices, 0, this._currentBufferData.NumberOfIndices);
        }
      

        public void RenderToGBuffer(Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            SetVertecesIndices();

            Effect effect = _effect;
            effect.CurrentTechnique = effect.Techniques[0];
            //our first pass is responsible for rendering into GBuffer
            effect.Parameters["World"].SetValue(Transform);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);
            effect.Parameters["WorldView"].SetValue(Transform * camera.View);
            effect.Parameters["WorldViewProjection"].SetValue(Transform * camera.View * camera.Projection);
            effect.Parameters["FarClip"].SetValue(camera.FarPlane);
            effect.CurrentTechnique.Passes[0].Apply();
            RasterizerState  rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = rs;
            this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._currentBufferData.NumberOfVertices, 0, this._currentBufferData.NumberOfIndices);
        }

        public void RenderShadowMap(ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            SetVertecesIndices();
            Effect effect = _effect;

            //render to shadow map
            effect.CurrentTechnique = effect.Techniques[2];
            effect.Parameters["World"].SetValue(Transform);
            effect.Parameters["LightViewProj"].SetValue(viewProj);
            effect.Parameters["TextureEnabled"].SetValue(false);

            effect.CurrentTechnique.Passes[0].Apply();
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = rs;
            this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._currentBufferData.NumberOfVertices, 0, this._currentBufferData.NumberOfIndices);
        }

        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
        #endregion


        #region Public methods

        /// <summary>
        /// <para>Gets the height of the ground at the specified position.</para>
        /// </summary>
        private float CubicPolate(float v0, float v1, float v2, float v3, float fracy)
        {
            float A = (v3 - v2) - (v0 - v1);
            float B = (v0 - v1) - A;
            float C = v2 - v0;
            float D = v1;

            return (float)(A * (float)Math.Pow(fracy, 3) + (float)B * Math.Pow(fracy, 2) + C * fracy + D);
        }

        public float GetHeightAt(float X, float Z)
        {
            int left, top;

            left = (int)((X - this.Size / 2) / this.Scale);
            top = (int)((Z - this.Size / 2) / this.Scale);

            float xNormalized = ((X - this.Size / 2) % this.Scale) / this.Scale;
            float zNormalized = ((Z - this.Size / 2) % this.Scale) / this.Scale;

            float topHeight = MathHelper.Lerp(
                _heightData[left, top],
                _heightData[left + 1, top],
                xNormalized);

            float bottomHeight = MathHelper.Lerp(
                _heightData[left, top + 1],
                _heightData[left + 1, top + 1],
                xNormalized);

            return MathHelper.Lerp(topHeight, bottomHeight, zNormalized);
        }

        public float GetHeight(float x, float z)
        {
            float X = Math.Abs(((x + this.Size / 2) / 64) % this.Size);
            float Z = Math.Abs(((z + this.Size / 2) / 64) % this.Size);
            Vector3 positionOnMap = new Vector3(X, 0, Z);

            int left, top;
            left = (int)(positionOnMap.X / this.Scale);
            top = (int)(positionOnMap.Z / this.Scale);

            // Clamp coordinates
            left = (left <= 0) ? 0 : left;
            top = (top <= 0) ? 0 : top;
            left = (left > 513 - 2) ? 513 - 2 : left;
            top = (top > 513 - 2) ? 513 - 2 : top;

            // Use modulus to find out how far away we are from the upper
            // left corner of the cell, then normalize it with the scale.
            float xNormalized = (positionOnMap.X % this.Scale) / this.Scale;
            float zNormalized = (positionOnMap.Z % this.Scale) / this.Scale;

            // normalize the height positions and interpolate them.
            float topHeight = MathHelper.Lerp(
                _heightData[left, top],
                _heightData[left + 1, top], xNormalized);

            float bottomHeight = MathHelper.Lerp(
                _heightData[left, top + 1],
                _heightData[left + 1, top + 1], xNormalized);

            float height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);

            height *= this.Scale;
            return height * 1.575f;
        }
     

        public float GetHeightAT(float x, float y)
        {
            float X = Math.Abs(((x + this.Size / 2) / 64) % this.Size);
            float Y = Math.Abs(((y + this.Size / 2) / 64) % this.Size);
            //return _heightData[(int)(X / Scale), (int)(Y / Scale)];

            // Map to cell coordinates
            X /= this.Scale;
            Y /= this.Scale;

            // Truncate coordinates to get coordinates of top left cell vertex
            int x0 = (int)X;
            int y0 = (int)Y;

            // Try to get coordinates of bottom right cell vertex
            int x1 = (x0 - 1 == this.Size ? x0 - 1 : x0);
            int y1 = y0;

            // Try to get coordinates of bottom right cell vertex
            int x2 = x0;
            int y2 = (y0 - 1 == this.Size ? y0 - 1 : y0);

            // Try to get coordinates of bottom right cell vertex
            int x3 = (x0 - 1 == this.Size ? x0 - 1 : x0);
            int y3 = (y0 - 1 == this.Size ? y0 - 1 : y0);
           
            // Get the heights at the two corners of the cell
            float h0 = _heightData[x0, y0];
            float h1 = _heightData[x1, y1];
            float h2 = _heightData[x2, y2];
            float h3 = _heightData[x3, y3];

          //  float leftOver = ((X - x1) + (Y - y1)) / 2f;
            return CubicPolate(h0, h1, h2, h3, 1f);// MathHelper.Lerp(h1, h2, leftOver);
        }

        /// <summary>
        /// <para>Gets the sub node size at the specified level depth.</para>
        /// </summary>
        public float GetNodeSizeAtLevel(int depth)
        {
            int diff = (int)((this.Depth-1) - depth);
            double result = System.Math.Pow(2, diff);
            return this._heightFieldSpace * (float)result;
        }

        #endregion


        #region IDisposable Members

        public override void Dispose()
        {
            base.Dispose();

            for(int i = 0 ; i < this._disposeDatas.Count ; i++)
            {
                BuffersData data = this._disposeDatas[i];
                data.VertexBuffer.Dispose();
                data.VertexBuffer = null;
                data.IndexBuffer.Dispose();
                data.IndexBuffer = null;
            }
            this._disposeDatas.Clear();
            this._disposeDatas = null;

            for (int i = 0; i < this._lastLoadedDatas.Count; i++)
            {
                BuffersData data = this._lastLoadedDatas[i];
                data.VertexBuffer.Dispose();
                data.VertexBuffer = null;
                data.IndexBuffer.Dispose();
                data.IndexBuffer = null;
                this._lastLoadedDatas.RemoveAt(0);
            }
            this._lastLoadedDatas.Clear();
            this._lastLoadedDatas = null;

        }

        #endregion
    }
}
