using System;
namespace Engine.Terrain
{
    public class QuadNodeCollection : IDisposable
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private QuadNode[] _childs;

        #endregion


        #region Properties

        public virtual int ChildNumber
        {
            get
            {
                return 4;
            }
            set
            {

            }
        }

        public QuadNode[] Childs
        {
            get
            {
                return this._childs;
            }
        }

        #endregion


        #region Constructors


        public QuadNodeCollection()
        {
            this._childs = new QuadNode[this.ChildNumber];
        }

        #endregion


        #region IDisposable Members

        public virtual void Dispose()
        {
            for (int i = 0; i < QuadTree.NodeChildsNumber; i++)
                if (this.Childs[i] != null)
                    this.Childs[i].Dispose();
        }

        #endregion

    }
}
