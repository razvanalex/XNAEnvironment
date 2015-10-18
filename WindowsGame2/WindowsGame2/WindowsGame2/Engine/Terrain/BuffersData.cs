using Microsoft.Xna.Framework.Graphics;

namespace Engine.Terrain
{

    /// <summary>
    /// <para>Data buffers.</para>
    /// </summary>
    public struct BuffersData
    {

        #region Fields

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private int _numberOfIndices;
        private int _numberOfVertices;

        #endregion


        #region Properties


        /// <summary>
        /// <para>The associated vertex buffer.</para>
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get
            {
                return this._vertexBuffer;
            }
            set
            {
                this._vertexBuffer = value;
            }
        }

        /// <summary>
        /// <para>The associated index buffer.</para>
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get
            {
                return this._indexBuffer;
            }
            set
            {
                this._indexBuffer = value;
            }
        }

        /// <summary>
        /// <para>The number of indices.</para>
        /// </summary>
        public int NumberOfIndices
        {
            get
            {
                return this._numberOfIndices;
            }
            set
            {
                this._numberOfIndices = value;
            }
        }

        /// <summary>
        /// <para>The number of vertices.</para>
        /// </summary>
        public int NumberOfVertices
        {
            get
            {
                return this._numberOfVertices;
            }
            set
            {
                this._numberOfVertices = value;
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// <para>Dispose the current loaded data.</para>
        /// </summary>
        public void Dispose()
        {
            this.VertexBuffer.Dispose();
            this.IndexBuffer.Dispose();
        }

        #endregion

    }

}
