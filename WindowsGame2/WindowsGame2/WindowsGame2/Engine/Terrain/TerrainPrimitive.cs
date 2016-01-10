namespace Engine.Terrain
{

    /// <summary>
    /// <para>Define a terrain primitive (ie : a triangle).</para>
    /// </summary>
    public struct TerrainPrimitive
    {

        #region Fields

        private int _indice1;
        private int _indice2;
        private int _indice3;

        #endregion


        #region Properties

        public int Indice1
        {
            get
            {
                return this._indice1;
            }
            set
            {
                this._indice1 = value;
            }
        }

        public int Indice2
        {
            get
            {
                return this._indice2;
            }
            set
            {
                this._indice2 = value;
            }
        }

        public int Indice3
        {
            get
            {
                return this._indice3;
            }
            set
            {
                this._indice3 = value;
            }
        }

        #endregion


        #region Constructors

       /* internal TerrainPrimitive()
        {

        }*/

        /// <summary>
        /// <para>Initialize a new <see cref="TerrainPrimitive"/> value.</para>
        /// </summary>
        /// <param name="indice1">First indice of the used vertex for this primitive.</param>
        /// <param name="indice2">Second indice of the used vertex for this primitive.</param>
        /// <param name="indice3">Third indice of the used vertex for this primitive.</param>
        public TerrainPrimitive(int indice1, int indice2, int indice3)
        {
            this._indice3 = indice3;
            this._indice2 = indice2;
            this._indice1 = indice1;
        }

        #endregion


        #region Public methods

        public Microsoft.Xna.Framework.Plane GetPlane(Engine.Terrain.Collections.Generic.Vector<VertexPositionNormalTextureTangentBinormal> vertices)
        {
            return new Microsoft.Xna.Framework.Plane(vertices[this.Indice1].Position, vertices[this.Indice2].Position, vertices[this.Indice3].Position);
        }
        public Microsoft.Xna.Framework.Vector3 GetNormal(Engine.Terrain.Collections.Generic.Vector<VertexPositionNormalTextureTangentBinormal> vertices)
        {
            return new Microsoft.Xna.Framework.Plane(vertices[this.Indice1].Position, vertices[this.Indice2].Position, vertices[this.Indice3].Position).Normal;
        }
        public void SetNormalTangentBinormal(Engine.Terrain.Collections.Generic.Vector<VertexPositionNormalTextureTangentBinormal> vertices)
        {
            Microsoft.Xna.Framework.Vector3 normal = this.GetNormal(vertices);
           
            // This is a triangle from vertices 
            Microsoft.Xna.Framework.Vector3 v1 = vertices[this.Indice1].Position;
            Microsoft.Xna.Framework.Vector3 v2 = vertices[this.Indice2].Position;
            Microsoft.Xna.Framework.Vector3 v3 = vertices[this.Indice3].Position;
          
            // These are the texture coordinate of the triangle  
            Microsoft.Xna.Framework.Vector2 w1 = vertices[this.Indice1].TextureCoordinate;
            Microsoft.Xna.Framework.Vector2 w2 = vertices[this.Indice2].TextureCoordinate;
            Microsoft.Xna.Framework.Vector2 w3 = vertices[this.Indice3].TextureCoordinate;
          
            float x1 = v2.X - v1.X;
            float x2 = v3.X - v1.X;
            float y1 = v2.Y - v1.Y;
            float y2 = v3.Y - v1.Y;
            float z1 = v2.Z - v1.Z;
            float z2 = v3.Z - v1.Z;

            float s1 = w2.X - w1.X;
            float s2 = w3.X - w1.X;
            float t1 = w2.Y - w1.Y;
            float t2 = w3.Y - w1.Y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            Microsoft.Xna.Framework.Vector3 sdir = new Microsoft.Xna.Framework.Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Microsoft.Xna.Framework.Vector3 tdir = new Microsoft.Xna.Framework.Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r); 

            VertexPositionNormalTextureTangentBinormal vertex = vertices[this.Indice1];
            vertex.Normal += normal;
            vertex.Normal.Normalize();      
            // Gram-Schmidt orthogonalize  
            Microsoft.Xna.Framework.Vector3 tangent = sdir - vertex.Normal * Microsoft.Xna.Framework.Vector3.Dot(vertex.Normal, sdir);
            Microsoft.Xna.Framework.Vector3 binormal = Microsoft.Xna.Framework.Vector3.Cross(vertex.Tangent, vertex.Normal);
            tangent.Normalize();
            vertex.Tangent = tangent;
            vertex.Binormal = Microsoft.Xna.Framework.Vector3.Cross(tangent, vertex.Normal);
            vertices[this.Indice1] = vertex;

            vertex = vertices[this.Indice2];
            vertex.Normal += normal;
            vertex.Normal.Normalize();
            // Gram-Schmidt orthogonalize  
            tangent = sdir - vertex.Normal * Microsoft.Xna.Framework.Vector3.Dot(vertex.Normal, sdir);
            binormal = Microsoft.Xna.Framework.Vector3.Cross(vertex.Tangent, vertex.Normal);
            tangent.Normalize();
            vertex.Tangent = Microsoft.Xna.Framework.Vector3.Zero;
            vertex.Binormal = Microsoft.Xna.Framework.Vector3.Cross(tangent, vertex.Normal);
            vertices[this.Indice2] = vertex;

            vertex = vertices[this.Indice3];
            vertex.Normal += normal;
            vertex.Normal.Normalize();
            // Gram-Schmidt orthogonalize  
            tangent = sdir - vertex.Normal * Microsoft.Xna.Framework.Vector3.Dot(vertex.Normal, sdir);
            binormal = Microsoft.Xna.Framework.Vector3.Cross(vertex.Tangent, vertex.Normal);
            tangent.Normalize();
            vertex.Tangent = tangent;
            vertex.Binormal = Microsoft.Xna.Framework.Vector3.Cross(tangent, vertex.Normal);
            vertices[this.Indice3] = vertex;
        }
        #endregion

    }
}
