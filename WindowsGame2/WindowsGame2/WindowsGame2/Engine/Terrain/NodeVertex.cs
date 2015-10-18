namespace Engine.Terrain
{

    /// <summary>
    /// <para>Positions of the vertices on a Node.</para>
    /// </summary>
    public enum NodeVertex : byte
    {

        /// <summary>
        /// <para>The Center Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        Center = 0,
        /// <summary>
        /// <para>The West Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        West = 1,
        /// <summary>
        /// <para>The North Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        North = 2,
        /// <summary>
        /// <para>The East Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        East = 3,
        /// <summary>
        /// <para>The South Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        South = 4,
        /// <summary>
        /// <para>The NorthWest Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthWest = 5,
        /// <summary>
        /// <para>The NorthEast Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthEast = 6,
        /// <summary>
        /// <para>The SouthEast Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthEast = 7,
        /// <summary>
        /// <para>The SouthWest Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthWest = 8,

    }

}
