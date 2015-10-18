using Engine.Terrain.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
