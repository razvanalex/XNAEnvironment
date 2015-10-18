namespace Engine.Terrain
{
    /// <summary>
    /// <para>Enumerated the vertices of a <see cref="QuadNode"/>.</para>
    /// </summary>
    [System.Flags]
    public enum NodeContent : ushort
    {

        /// <summary>
        /// <para>Default value.</para>
        /// </summary>
        None = 0,

        /// <summary>
        /// <para>The North West Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthWestVertex = 1,
 
        /// <summary>
        /// <para>The North East Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthEastVertex = 2,
 
        /// <summary>
        /// <para>The South East Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthEastVertex = 4,
 
        /// <summary>
        /// <para>The South West Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthWestVertex = 8,
 
        /// <summary>
        /// <para>The Center Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        CenterVertex = 16,
  
        /// <summary>
        /// <para>The West Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        WestVertex = 32,

        /// <summary>
        /// <para>The North Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthVertex = 64,
 
        /// <summary>
        /// <para>The East Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        EastVertex = 128,
 
        /// <summary>
        /// <para>The South Vertex of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthVertex = 256,
 
        /// <summary>
        /// <para>The North West Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthWestChild = 512,
 
        /// <summary>
        /// <para>The North East Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        NorthEastChild = 1024,

        /// <summary>
        /// <para>The South West Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthWestChild = 2048,
 
        /// <summary>
        /// <para>The South East Child of the <see cref="QuadNode"/>.</para>
        /// </summary>
        SouthEastChild = 4096,

    }

}
