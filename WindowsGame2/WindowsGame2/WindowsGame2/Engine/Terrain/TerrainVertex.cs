using Engine.Terrain.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Engine.Terrain
{

    /// <summary>
    /// <para>Define a terrain vertex.</para>
    /// </summary>
    public class TerrainVertex
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _bufferIndice;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _lastUsedIteration;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector<QuadNode> _references;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private VertexPositionNormalTextureTangentBinormal _value;

        #endregion


        #region Properties

        /// <summary>
        /// <para>Gets or sets the indice of the current vertex inside the vertex buffer since the last iteration.</para>
        /// </summary>
        public int BufferIndice
        {
            get
            {
                return this._bufferIndice;
            }
            set
            {
                this._bufferIndice = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the last iteration identifier where the current vertex was used.</para>
        /// </summary>
        public int LastUsedIteration
        {
            get
            {
                return this._lastUsedIteration;
            }
            set
            {
                this._lastUsedIteration = value;
            }
        }

        /// <summary>
        /// <para>Gets a value indicating if the current vertex is enabled.</para>
        /// </summary>
        /// <remarks>A vertex is enabled wjen referenced by one node minimum.</remarks>
        public bool Enabled
        {
            get
            {
                return this.References.Count > 0;
            }
        }

        /// <summary>
        /// <para>Gets or sets the back vertex value.</para>
        /// </summary>
        public VertexPositionNormalTextureTangentBinormal Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }

        /// <summary>
        /// <para>Gets the list of node that reference the current <see cref="TerrainVertex"/>.</para>
        /// </summary>
        public Vector<QuadNode> References
        {
            get
            {
                return this._references;
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// <para>Instanciate a new <see cref="TerrainVertex"/>.</para>
        /// </summary>
        /// <param name="value"></param>
        public TerrainVertex(VertexPositionNormalTextureTangentBinormal value)
        {
            this._bufferIndice = 0;
            this._lastUsedIteration = -1;
            this._references = new Vector<QuadNode>();
            this._value = value;
        }

        #endregion


        #region Public methods

        public void AddReferenceTo(QuadNode node)
        {
            if (this._references.IndexOf(node) >= 0)
                return;
            this._references.Add(node);
        }

        public void RemoveReferenceFrom(QuadNode node)
        {
            this._references.Remove(node);
         }

        #endregion

    }
}
