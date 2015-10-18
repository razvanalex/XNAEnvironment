namespace Engine.Terrain
{

    /// <summary>
    /// <para>Positions of the vertices positionned on the side of a <see cref="QuadNode"/>.</para>
    /// </summary>
    public enum NodeSideVertex : byte
    {

        /// <summary>
        /// <para>The East Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        East = 0,

        /// <summary>
        /// <para>The North Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        North = 1,

        /// <summary>
        /// <para>The West Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        West = 2,

        /// <summary>
        /// <para>The South Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        South = 3,

        /// <summary>
        /// <para>The Center Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        Center = 4,

    }
}
