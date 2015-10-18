namespace Engine.Terrain
{

    /// <summary>
    /// <para>Enumerates the childs of a <see cref="QuadNode"/>.</para>
    /// </summary>
    public enum NodeChild : byte
    {

        /// <summary>
        /// <para>The NorthWest Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthWest = 0,

        /// <summary>
        /// <para>The North East Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthEast = 1,

        /// <summary>
        /// <para>The South West Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthWest = 2,

        /// <summary>
        /// <para>The South East Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthEast = 3,

    }
}
