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

        public Microsoft.Xna.Framework.Plane GetPlane(Engine.Terrain.Collections.Generic.Vector<Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture> vertices)
        {
            return new Microsoft.Xna.Framework.Plane(vertices[this.Indice1].Position, vertices[this.Indice2].Position, vertices[this.Indice3].Position);
        }

        public Microsoft.Xna.Framework.Vector3 GetNormal(Engine.Terrain.Collections.Generic.Vector<Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture> vertices)
        {
            return new Microsoft.Xna.Framework.Plane(vertices[this.Indice1].Position, vertices[this.Indice2].Position, vertices[this.Indice3].Position).Normal;
        }

        public void SetNormal(Engine.Terrain.Collections.Generic.Vector<Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture> vertices)
        {
            Microsoft.Xna.Framework.Vector3 normal = this.GetNormal(vertices);

            Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture vertex = vertices[this.Indice1];
            vertex.Normal += normal;
            vertex.Normal.Normalize();
            vertices[this.Indice1] = vertex;

            vertex = vertices[this.Indice2];
            vertex.Normal += normal;
            vertex.Normal.Normalize();
            vertices[this.Indice2] = vertex;

            vertex = vertices[this.Indice3];
            vertex.Normal += normal;
            vertex.Normal.Normalize();
            vertices[this.Indice3] = vertex;
        }

        #endregion

    }
}
