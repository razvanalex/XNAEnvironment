using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.Terrain.Collections.Generic;
using Engine.Camera;

namespace Engine.Terrain
{

    /// <summary>
    /// <para>Defines a QuadNode. A QuadNode is a Leaf/Branch inside the <see cref="QuadTree"/>. It share Vertices with its <see cref="QuadNode"/> neighbor.</para>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Depth : {Depth} at {Location}, {Position} of parent and {EnabledVertices} enabled.")]
    public class QuadNode : QuadNodeCollection, IDisposable
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private QuadNode _parent;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private QuadTree _parentTree;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector2 _location;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private byte _depth;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private NodeContent _enabledContent;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BoundingBox _boundingBoxNorthWestChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BoundingBox _boundingBoxNorthEastChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BoundingBox _boundingBoxSouthWestChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BoundingBox _boundingBoxSouthEastChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private NodeChild _position;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float[] _realToInterpolatedVertexHeight;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private QuadNode[] _neighbors;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private TerrainVertex[] _vertices;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _dotNorthWestChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _dotSouthWestChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _dotSouthEastChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _dotNorthEastChild;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private bool _isFullRelevant;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private NodeContent _relevantChild;
      //  public Vector3 CameraPosition = new Vector3(20000, 5000, 20000);
        #endregion


        #region Properties

        /// <summary>
        /// <para>Gets the number of sides for a <see cref="QuadNode"/>.</para>
        /// </summary>
        /// <remarks>Sides are Center, West, North, East, South.</remarks>
        public const int SidesNumber = 5;

        /// <summary>
        /// <para>Gets the maximum number of vertices for a <see cref="QuadNode"/>.</para>
        /// </summary>
        /// <remarks>Vertices are located on Center, West, North West, North, North East, East, South East, South, South West.</remarks>
        public const int VerticesNumber = 9;

        /// <summary>
        /// <para>Gets the enabled vertices.</para>
        /// </summary>
        public NodeContent EnabledContent
        {
            get
            {
                return this._enabledContent;
            }
        }

        /// <summary>
        /// <para>Gets a value indicating if the current <see cref="QuadNode"/> is full relevant and need not to be splitted.</para>
        /// </summary>
        public bool IsFullRelevant
        {
            get
            {
                return this._isFullRelevant;
            }
            internal set
            {
                this._isFullRelevant = value;
            }
        }
        /// <summary>
        /// <para>Gets a named position for the current <see cref="QuadNode"/>.</para>
        /// </summary>
        public NodeChild Position
        {
            get
            {
                return this._position;
            }
            internal set
            {
                this._position = value;
            }
        }

        /// <summary>
        /// <para>Gets all the current <see cref="QuadNode"/>'s neighbor.</para>
        /// </summary>
        public QuadNode[] Neighbor
        {
            get
            {
                return this._neighbors;
            }
        }

        /// <summary>
        /// <para>Gets all the vertices for the current <see cref="QuadNode"/>.</para>
        /// </summary>
        public TerrainVertex[] Vertices
        {
            get
            {
                return this._vertices;
            }
        }

        /// <summary>
        /// <para>Gets the Depth of the current <see cref="QuadNode"/> inside the <see cref="QuadTree"/>.</para>
        /// </summary>
        /// <remarks>0 indicates the root, and this.ParentTree.Depth the value for the Leaf.</remarks>
        public byte Depth
        {
            get
            {
                return this._depth;
            }
        }

        /// <summary>
        /// <para>Gets the location of the <see cref="QuadNode"/>.</para>
        /// </summary>
        /// <remarks>Vector2 for X and Z coordinates components. The Y value is computed with a call to the GetHeight method of the associated <see cref="QuadTree"/>.</remarks>
        public Vector2 Location
        {
            get
            {
                return this._location;
            }
            internal set
            {
                this._location = value;
            }
        }

        /// <summary>
        /// <para>Gets the <see cref="QuadNode"/> parent of the current node <see cref="QuadNode"/>.</para>
        /// </summary>
        public QuadNode Parent
        {
            get
            {
                return this._parent;
            }
        }

        /// <summary>
        /// <para>Gets the owner <see cref="QuadTree"/>.</para>
        /// </summary>
        public QuadTree ParentTree
        {
            get
            {
                return this._parentTree;
            }
            internal set
            {
                this._parentTree = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the east neighbor.</para>
        /// </summary>
        public QuadNode EastNeighbor
        {
            get
            {
                return this._neighbors[(int)NodeSideVertex.East];
            }
            set
            {
                this._neighbors[(int)NodeSideVertex.East] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the west neighbor.</para>
        /// </summary>
        public QuadNode WestNeighbor
        {
            get
            {
                return this._neighbors[(int)NodeSideVertex.West];
            }
            set
            {
                this._neighbors[(int)NodeSideVertex.West] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north neighbor.</para>
        /// </summary>
        public QuadNode NorthNeighbor
        {
            get
            {
                return this._neighbors[(int)NodeSideVertex.North];
            }
            set
            {
                this._neighbors[(int)NodeSideVertex.North] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south neighbor.</para>
        /// </summary>
        public QuadNode SouthNeighbor
        {
            get
            {
                return this._neighbors[(int)NodeSideVertex.South];
            }
            set
            {
                this._neighbors[(int)NodeSideVertex.South] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north west vertex.</para>
        /// </summary>
        public TerrainVertex NorthWestVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.NorthWest];
            }
            set
            {
                this.Vertices[(int)NodeVertex.NorthWest] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north east vertex.</para>
        /// </summary>
        public TerrainVertex NorthEastVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.NorthEast];
            }
            set
            {
                this.Vertices[(int)NodeVertex.NorthEast] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south west vertex.</para>
        /// </summary>
        public TerrainVertex SouthWestVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.SouthWest];
            }
            set
            {
                this.Vertices[(int)NodeVertex.SouthWest] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south east vertex.</para>
        /// </summary>
        public TerrainVertex SouthEastVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.SouthEast];
            }
            set
            {
                this.Vertices[(int)NodeVertex.SouthEast] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the east vertex.</para>
        /// </summary>
        public TerrainVertex EastVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.East];
            }
            set
            {
                this.Vertices[(int)NodeVertex.East] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the west vertex.</para>
        /// </summary>
        public TerrainVertex WestVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.West];
            }
            set
            {
                this.Vertices[(int)NodeVertex.West] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north vertex.</para>
        /// </summary>
        public TerrainVertex NorthVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.North];
            }
            set
            {
                this.Vertices[(int)NodeVertex.North] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south vertex.</para>
        /// </summary>
        public TerrainVertex SouthVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.South];
            }
            set
            {
                this.Vertices[(int)NodeVertex.South] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the center vertex.</para>
        /// </summary>
        public TerrainVertex CenterVertex
        {
            get
            {
                return this.Vertices[(int)NodeVertex.Center];
            }
            set
            {
                this.Vertices[(int)NodeVertex.Center] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north west child.</para>
        /// </summary>
        public QuadNode NorthWestChild
        {
            get
            {
                return this.Childs[(int)NodeChild.NorthWest];
            }
            set
            {
                this.Childs[(int)NodeChild.NorthWest] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the north east child.</para>
        /// </summary>
        public QuadNode NorthEastChild
        {
            get
            {
                return this.Childs[(int)NodeChild.NorthEast];
            }
            set
            {
                this.Childs[(int)NodeChild.NorthEast] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south west child.</para>
        /// </summary>
        public QuadNode SouthWestChild
        {
            get
            {
                return this.Childs[(int)NodeChild.SouthWest];
            }
            set
            {
                this.Childs[(int)NodeChild.SouthWest] = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the south east child.</para>
        /// </summary>
        public QuadNode SouthEastChild
        {
            get
            {
                return this.Childs[(int)NodeChild.SouthEast];
            }
            set
            {
                this.Childs[(int)NodeChild.SouthEast] = value;
            }
        }

        #endregion


        #region Constructors

        public QuadNode(QuadNode parent, NodeChild position)
        {
            //array for neighbors at each sides
            this._neighbors = new QuadNode[4];
            //array for all the nine vertices of the current node
            this._vertices = new TerrainVertex[QuadNode.VerticesNumber];
            //the interpolated position difference with the real position
            this._realToInterpolatedVertexHeight = new float[QuadNode.SidesNumber];
            this._parent = parent;
            this._position = position;
            if (parent == null)
                this._depth = 0;
            else
            {
                this._depth = Convert.ToByte(parent.Depth + 1);
                this._parentTree = parent._parentTree;
            }
        }

        /// <summary>
        /// <para>Link the current node to its neighbor.</para>
        /// </summary>
        private void InitializeNeighbors()
        {
            if (this.Parent == null)
                return;

            switch (this.Position)
            {
                case NodeChild.NorthWest:

                    //East/West
                    {
                        this.EastNeighbor = this.Parent.NorthEastChild;
                        if (this.Parent.NorthEastChild != null)
                            this.Parent.NorthEastChild.WestNeighbor = this;
                    }
                    //South/North
                    {
                        this.SouthNeighbor = this.Parent.SouthWestChild;
                        if (this.Parent.SouthWestChild != null)
                            this.Parent.SouthWestChild.NorthNeighbor = this;
                    }
                    //West/East
                    if (this.Parent.WestNeighbor != null)
                    {
                        this.WestNeighbor = this.Parent.WestNeighbor.NorthEastChild;
                        if (this.Parent.WestNeighbor.NorthEastChild != null)
                            this.Parent.WestNeighbor.NorthEastChild.EastNeighbor = this;
                    }
                    //North/South
                    if (this.Parent.NorthNeighbor != null)
                    {
                        this.NorthNeighbor = this.Parent.NorthNeighbor.SouthWestChild;
                        if (this.Parent.NorthNeighbor.SouthWestChild != null)
                            this.Parent.NorthNeighbor.SouthWestChild.SouthNeighbor = this;
                    }
                    break;

                case NodeChild.NorthEast:
                    //West/East
                    this.WestNeighbor = this.Parent.NorthWestChild;
                    if (this.Parent.NorthWestChild != null)
                        this.Parent.NorthWestChild.EastNeighbor = this;
                    //South/North
                    this.SouthNeighbor = this.Parent.SouthEastChild;
                    if (this.Parent.SouthEastChild != null)
                        this.Parent.SouthEastChild.NorthNeighbor = this;
                    //East/West
                    if (this.Parent.EastNeighbor != null)
                    {
                        this.EastNeighbor = this.Parent.EastNeighbor.NorthWestChild;
                        if (this.Parent.EastNeighbor.NorthWestChild != null)
                            this.Parent.EastNeighbor.NorthWestChild.WestNeighbor = this;
                    }
                    //North/South
                    if (this.Parent.NorthNeighbor != null)
                    {
                        this.NorthNeighbor = this.Parent.NorthNeighbor.SouthEastChild;
 
                        if (this.Parent.NorthNeighbor.SouthEastChild != null)
                            this.Parent.NorthNeighbor.SouthEastChild.SouthNeighbor = this;
                    }
                    break;

                case NodeChild.SouthWest:
                    //East/West
                    this.EastNeighbor = this.Parent.SouthEastChild;
                    if (this.Parent.SouthEastChild != null)
                        this.Parent.SouthEastChild.WestNeighbor = this;
                    //North/South
                    this.NorthNeighbor = this.Parent.NorthWestChild;
                    if (this.Parent.NorthWestChild != null)
                        this.Parent.NorthWestChild.SouthNeighbor = this;
                    //West/East
                    if (this.Parent.WestNeighbor != null)
                    {
                        this.WestNeighbor = this.Parent.WestNeighbor.SouthEastChild;
                        if (this.Parent.WestNeighbor.SouthEastChild != null)
                            this.Parent.WestNeighbor.SouthEastChild.EastNeighbor = this;
                    }
                    //South/North
                    if (this.Parent.SouthNeighbor != null)
                    {
                        this.SouthNeighbor = this.Parent.SouthNeighbor.NorthWestChild;
                        if (this.Parent.SouthNeighbor.NorthWestChild != null)
                            this.Parent.SouthNeighbor.NorthWestChild.NorthNeighbor = this;
                    }
                    break;

                case NodeChild.SouthEast:
                    //West/East
                    this.WestNeighbor = this.Parent.SouthWestChild;
                    if (this.Parent.SouthWestChild != null)
                        this.Parent.SouthWestChild.EastNeighbor = this;
                    //North/South
                    this.NorthNeighbor = this.Parent.NorthEastChild;
                    if (this.Parent.NorthEastChild != null)
                        this.Parent.NorthEastChild.SouthNeighbor = this;
                    //East/West
                    if (this.Parent.EastNeighbor != null)
                    {
                        this.EastNeighbor = this.Parent.EastNeighbor.SouthWestChild;
                        if (this.Parent.EastNeighbor.SouthWestChild != null)
                            this.Parent.EastNeighbor.SouthWestChild.WestNeighbor = this;
                    }
                    //South/North
                    if (this.Parent.SouthNeighbor != null)
                    {
                        this.SouthNeighbor = this.Parent.SouthNeighbor.NorthEastChild;
                        if (this.Parent.SouthNeighbor.NorthEastChild != null)
                            this.Parent.SouthNeighbor.NorthEastChild.NorthNeighbor = this;
                    }

                    break;

                default:
                    break;
            }

        }

        #endregion


        #region 3D

        public void Initialize()
        {
            float size = GetNodeSize();
            float x = this.Location.X;
            float z = this.Location.Y;
            float xWhole = x + size;
            float zWhole = z + size;
            float zHalf = z + size / 2;
            float xHalf = x + size / 2;
            float heightXZWhole = this.ParentTree.GetHeight(x, zWhole);
            float heightXWholeZWhole = this.ParentTree.GetHeight(xWhole, zWhole);
            float heightXWholeZ = this.ParentTree.GetHeight(xWhole, z); ;
            float heightXZ = this.ParentTree.GetHeight(x, z);


            //normal of the current quad
            Vector3 normal = //Vector3.Multiply(
                new Plane(
                new Vector3(x, heightXZWhole, zWhole),
                new Vector3(xWhole, heightXWholeZ, z),
                new Vector3(x, heightXZ, z)).Normal
                *
                new Plane(
                new Vector3(x, heightXZWhole, zWhole),
                new Vector3(xWhole, heightXWholeZWhole, zWhole),
                new Vector3(xWhole, heightXWholeZ, z)).Normal
                ;
            normal.Normalize();

            //first compute the 4 egdes of the current square
            //here we know all : position and height.
            {
                if (this.Parent != null)
                {
                    //if we have a parent maybe we can use its vertices
                    switch (this.Position)
                    {
                        case NodeChild.NorthWest:
                            this.NorthWestVertex = this.Parent.NorthWestVertex;
                            this.NorthEastVertex = this.Parent.NorthVertex;
                            this.SouthEastVertex = this.Parent.CenterVertex;
                            this.SouthWestVertex = this.Parent.WestVertex;
                            break;
                        case NodeChild.NorthEast:
                            this.NorthWestVertex = this.Parent.NorthVertex;
                            this.NorthEastVertex = this.Parent.NorthEastVertex;
                            this.SouthWestVertex = this.Parent.CenterVertex;
                            this.SouthEastVertex = this.Parent.EastVertex;
                            break;
                        case NodeChild.SouthWest:
                            this.NorthWestVertex = this.Parent.WestVertex;
                            this.NorthEastVertex = this.Parent.CenterVertex;
                            this.SouthWestVertex = this.Parent.SouthWestVertex;
                            this.SouthEastVertex = this.Parent.SouthVertex;
                            break;
                        case NodeChild.SouthEast:
                            this.NorthWestVertex = this.Parent.CenterVertex;
                            this.NorthEastVertex = this.Parent.EastVertex;
                            this.SouthWestVertex = this.Parent.SouthVertex;
                            this.SouthEastVertex = this.Parent.SouthEastVertex;
                            break;
                        default:
                            break;
                    }

                    //only the sides can be new
                    this.CenterVertex = new TerrainVertex(new VertexPositionNormalTexture(new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, zHalf), zHalf), normal, GetTextureCoordinates(xHalf, zHalf)));
                    this.WestVertex = (this.WestNeighbor != null ? this.WestNeighbor.EastVertex : new TerrainVertex(new VertexPositionNormalTexture(new Vector3(x, this.ParentTree.GetHeight(x, zHalf), zHalf), normal, GetTextureCoordinates(x, zHalf))));
                    this.NorthVertex = (this.NorthNeighbor != null ? this.NorthNeighbor.SouthVertex : new TerrainVertex(new VertexPositionNormalTexture(new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, zWhole), zWhole), normal, GetTextureCoordinates(xHalf, zWhole))));
                    this.EastVertex = (this.EastNeighbor != null ? this.EastNeighbor.WestVertex : new TerrainVertex(new VertexPositionNormalTexture(new Vector3(xWhole, this.ParentTree.GetHeight(xWhole, zHalf), zHalf), normal, GetTextureCoordinates(xWhole, zHalf))));
                    this.SouthVertex = (this.SouthNeighbor != null ? this.SouthNeighbor.NorthVertex : new TerrainVertex(new VertexPositionNormalTexture(new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, z), z), normal, GetTextureCoordinates(xHalf, z))));
                }
                else //this occurs only on depth = 0
                {
                   
                    Vector3 northWest = new Vector3(x, heightXZWhole, zWhole);
                    Vector3 northEast = new Vector3(xWhole, heightXWholeZWhole, zWhole);
                    Vector3 southEast = new Vector3(xWhole, heightXWholeZ, z);
                    Vector3 southWest = new Vector3(x, heightXZ, z);
                    Vector3 west = new Vector3(x, this.ParentTree.GetHeight(x, zHalf), zHalf);
                    Vector3 north = new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, zWhole), zWhole);
                    Vector3 east = new Vector3(xWhole, this.ParentTree.GetHeight(xWhole, zHalf), zHalf);
                    Vector3 south = new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, z), z);

                    this.NorthWestVertex = new TerrainVertex(new VertexPositionNormalTexture(northWest, normal, GetTextureCoordinates(x, zWhole)));
                    this.NorthEastVertex = new TerrainVertex(new VertexPositionNormalTexture(northEast, normal, GetTextureCoordinates(xWhole, zWhole)));
                    this.SouthEastVertex = new TerrainVertex(new VertexPositionNormalTexture(southEast, normal, GetTextureCoordinates(xWhole, z)));
                    this.SouthWestVertex = new TerrainVertex(new VertexPositionNormalTexture(southWest, normal, GetTextureCoordinates(x, z)));
                    this.CenterVertex = new TerrainVertex(new VertexPositionNormalTexture(new Vector3(xHalf, this.ParentTree.GetHeight(xHalf, zHalf), zHalf), normal, GetTextureCoordinates(xHalf, zHalf)));
                    this.WestVertex = new TerrainVertex(new VertexPositionNormalTexture(west, normal, GetTextureCoordinates(x, zHalf)));
                    this.NorthVertex = new TerrainVertex(new VertexPositionNormalTexture(north, normal, GetTextureCoordinates(xHalf, zWhole)));
                    this.EastVertex = new TerrainVertex(new VertexPositionNormalTexture(east, normal, GetTextureCoordinates(xWhole, zHalf)));
                    this.SouthVertex = new TerrainVertex(new VertexPositionNormalTexture(south, normal, GetTextureCoordinates(xHalf, z)));
                }
            }
            //At te beginning the four edges are enabled
            {
                this.EnableVertex(NodeContent.NorthWestVertex, NodeVertex.NorthWest);
                this.EnableVertex(NodeContent.NorthEastVertex, NodeVertex.NorthEast);
                this.EnableVertex(NodeContent.SouthEastVertex, NodeVertex.SouthEast);
                this.EnableVertex(NodeContent.SouthWestVertex, NodeVertex.SouthWest);
            }

            //then interpolate the 4 sides and the center.
            //when can easily deduce the position x and z because we know the size of the
            //current square and its location.
            //for the height we have to interpolate from the two neighbor egdes. 
            {
                float centerHeight = (float)(0.25 * (this.NorthEastVertex.Value.Position.Y + this.NorthWestVertex.Value.Position.Y + this.SouthWestVertex.Value.Position.Y + this.SouthEastVertex.Value.Position.Y));
                float eastSideHeight = (float)(0.5 * (this.SouthEastVertex.Value.Position.Y + this.NorthEastVertex.Value.Position.Y));
                float northSideHeight = (float)(0.5 * (this.NorthEastVertex.Value.Position.Y + this.NorthWestVertex.Value.Position.Y));
                float westSideStory = (float)(0.5 * (this.NorthWestVertex.Value.Position.Y + this.SouthWestVertex.Value.Position.Y));//sorry for westsidestory instead of westsideheight but it's too fun :)
                float southSideHeight = (float)(0.5 * (this.SouthWestVertex.Value.Position.Y + this.SouthEastVertex.Value.Position.Y));

                this._realToInterpolatedVertexHeight[(int)NodeSideVertex.Center] = Math.Abs(centerHeight - this.CenterVertex.Value.Position.Y);
                this._realToInterpolatedVertexHeight[(int)NodeSideVertex.East] = Math.Abs(eastSideHeight - this.EastVertex.Value.Position.Y);
                this._realToInterpolatedVertexHeight[(int)NodeSideVertex.North] = Math.Abs(northSideHeight - this.NorthVertex.Value.Position.Y);
                this._realToInterpolatedVertexHeight[(int)NodeSideVertex.West] = Math.Abs(westSideStory - this.WestVertex.Value.Position.Y);
                this._realToInterpolatedVertexHeight[(int)NodeSideVertex.South] = Math.Abs(southSideHeight - this.SouthVertex.Value.Position.Y);
            }

            this._boundingBoxNorthWestChild = BoundingBox.CreateFromPoints(new Vector3[] { this.CenterVertex.Value.Position, this.NorthWestVertex.Value.Position, this.WestVertex.Value.Position, this.NorthVertex.Value.Position });
            this._boundingBoxNorthEastChild = BoundingBox.CreateFromPoints(new Vector3[] { this.CenterVertex.Value.Position, this.NorthEastVertex.Value.Position, this.EastVertex.Value.Position, this.NorthVertex.Value.Position });
            this._boundingBoxSouthWestChild = BoundingBox.CreateFromPoints(new Vector3[] { this.CenterVertex.Value.Position, this.SouthWestVertex.Value.Position, this.WestVertex.Value.Position, this.SouthVertex.Value.Position });
            this._boundingBoxSouthEastChild = BoundingBox.CreateFromPoints(new Vector3[] { this.CenterVertex.Value.Position, this.SouthEastVertex.Value.Position, this.EastVertex.Value.Position, this.SouthVertex.Value.Position });

            Vector3 childnormal = Vector3.Multiply(
                new Plane(this.NorthVertex.Value.Position,this.NorthEastVertex.Value.Position,this.EastVertex.Value.Position).Normal,
                new Plane(this.NorthVertex.Value.Position,this.EastVertex.Value.Position,this.CenterVertex.Value.Position).Normal);
            normal.Normalize();
            this._dotNorthEastChild = 1 - Vector3.Dot(childnormal, normal);

            childnormal = Vector3.Multiply(
                new Plane(this.NorthWestVertex.Value.Position,this.NorthVertex.Value.Position,this.CenterVertex.Value.Position).Normal,
                new Plane(this.NorthWestVertex.Value.Position,this.CenterVertex.Value.Position,this.WestVertex.Value.Position).Normal);
            childnormal.Normalize();
            this._dotNorthWestChild = 1 - Vector3.Dot(childnormal, normal);

            childnormal = Vector3.Multiply(
                new Plane(this.CenterVertex.Value.Position,this.EastVertex.Value.Position,this.SouthEastVertex.Value.Position).Normal,
                new Plane(this.CenterVertex.Value.Position,this.SouthEastVertex.Value.Position,this.SouthVertex.Value.Position).Normal);
            childnormal.Normalize();
            this._dotSouthEastChild = 1 - Vector3.Dot(childnormal, normal);

            childnormal = Vector3.Multiply(
                new Plane(this.WestVertex.Value.Position,this.CenterVertex.Value.Position,this.SouthVertex.Value.Position).Normal,
                new Plane(this.WestVertex.Value.Position,this.SouthVertex.Value.Position,this.SouthWestVertex.Value.Position).Normal);
            childnormal.Normalize();
            this._dotSouthWestChild = 1 - Vector3.Dot(childnormal, normal);
        }

        public Vector2 GetTextureCoordinates(float x, float z)
        {
            x = x - this.ParentTree.Location.X;
            z = z - this.ParentTree.Location.Y;
            float textureRepeat = 1;
            return new Vector2(x / ((float)ParentTree.Size / textureRepeat), z / ((float)ParentTree.Size / textureRepeat));
        }

        public void Update()
        {
            if (this.Depth < this.ParentTree.Depth)
            {
                //check vertices
                {
                    //the vertices are shared so whe dont have to check all the sides vertex only two not opposite are enough
                    //it is possible to stop all vertices checks and use only childs checks
                    //you will have the same result bith few less details.
                    //CheckVertexAt(VertexPosition.West, EnabledVertex.West, Sides.West);
                    //CheckVertexAt(VertexPosition.North, EnabledVertex.North, Sides.North);
                    CheckVertexAt(NodeVertex.East, NodeContent.EastVertex, NodeSideVertex.East);
                    CheckVertexAt(NodeVertex.South, NodeContent.SouthVertex, NodeSideVertex.South);
                
                }
                //check childs if we are not on leaf
                {
                        this.CheckChildAt(NodeChild.NorthWest, NodeContent.NorthWestChild, this._dotNorthWestChild, this._boundingBoxNorthWestChild);
                        this.CheckChildAt(NodeChild.NorthEast, NodeContent.NorthEastChild, this._dotNorthEastChild, this._boundingBoxNorthEastChild);
                        this.CheckChildAt(NodeChild.SouthEast, NodeContent.SouthEastChild, this._dotSouthEastChild, this._boundingBoxSouthEastChild);
                        this.CheckChildAt(NodeChild.SouthWest, NodeContent.SouthWestChild, this._dotSouthWestChild, this._boundingBoxSouthWestChild);
                }

                for (int i = 0; i < QuadTree.NodeChildsNumber; i++)
                    if ((this.Childs[i] != null) && (!this.Childs[i].IsFullRelevant))
                        this.Childs[i].Update();
            }

        }

        #endregion


        #region Vertex Manipulation methods

        /// <summary>
        /// <para>Enable the specified flag and the specified vertex.</para>
        /// </summary>
         public void EnableVertex(NodeContent flag, NodeVertex vertex)
        {
            this.EnableVertex(flag);
            this._vertices[(int)vertex].AddReferenceTo(this);
        }

         /// <summary>
         /// <para>Enable the specified flag.</para>
         /// </summary>
        public void EnableVertex(NodeContent flag)
        {
            this._enabledContent |= flag;

            if (((int)this._enabledContent >> 5) != 0)
            {
                this._enabledContent |= NodeContent.CenterVertex;
                this.CenterVertex.AddReferenceTo(this);
            }

            //if an edge is enable must enable neighbor to share the edge
            if (((int)flag >> 5) != 0 && (this.Parent != null))
            {
                switch (flag)
                {
                    case NodeContent.WestVertex:

                        if (this.WestNeighbor == null)
                        {
                            if (this.Position == NodeChild.NorthWest)
                            {
                                if (this.Parent.WestNeighbor != null)
                                    if (this.Parent.WestNeighbor.NorthEastChild == null)
                                        this.Parent.WestNeighbor.AddChild(NodeChild.NorthEast, NodeContent.NorthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthWest)
                            {
                                if (this.Parent.WestNeighbor != null)
                                    if (this.Parent.WestNeighbor.SouthEastChild == null)
                                        this.Parent.WestNeighbor.AddChild(NodeChild.SouthEast, NodeContent.SouthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthEast)
                            {
                                if (this.Parent.SouthWestChild == null)
                                    this.Parent.AddChild(NodeChild.SouthWest, NodeContent.SouthWestChild);
                            }
                            else if (this.Position == NodeChild.NorthEast)
                            {
                                if (this.Parent.NorthWestChild == null)
                                    this.Parent.AddChild(NodeChild.NorthWest, NodeContent.NorthWestChild);
                            }
                        }
                        break;
                    case NodeContent.NorthVertex:

                        if (this.NorthNeighbor == null)
                        {
                            if (this.Position == NodeChild.NorthWest)
                            {
                                if (this.Parent.NorthNeighbor != null)
                                    if (this.Parent.NorthNeighbor.SouthWestChild == null)
                                        this.Parent.NorthNeighbor.AddChild(NodeChild.SouthWest, NodeContent.SouthWestChild);
                            }
                            else if (this.Position == NodeChild.NorthEast)
                            {
                                if (this.Parent.NorthNeighbor != null)
                                    if (this.Parent.NorthNeighbor.SouthEastChild == null)
                                        this.Parent.NorthNeighbor.AddChild(NodeChild.SouthEast, NodeContent.SouthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthEast)
                            {
                                if (this.Parent.NorthEastChild == null)
                                    this.Parent.AddChild(NodeChild.NorthEast, NodeContent.NorthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthWest)
                            {
                                if (this.Parent.NorthWestChild == null)
                                    this.Parent.AddChild(NodeChild.NorthWest, NodeContent.NorthWestChild);
                            }
                        }
                        break;
                    case NodeContent.EastVertex:
                        if (this.EastNeighbor == null)
                        {
                            if (this.Position == NodeChild.NorthEast)
                            {
                                if (this.Parent.EastNeighbor != null)
                                    if (this.Parent.EastNeighbor.NorthWestChild == null)
                                        this.Parent.EastNeighbor.AddChild(NodeChild.NorthWest, NodeContent.NorthWestChild);
                            }
                            else if (this.Position == NodeChild.SouthEast)
                            {
                                if (this.Parent.EastNeighbor != null)
                                    if (this.Parent.EastNeighbor.SouthWestChild == null)
                                        this.Parent.EastNeighbor.AddChild(NodeChild.SouthWest, NodeContent.SouthWestChild);
                            }
                            else if (this.Position == NodeChild.NorthWest)
                            {
                                if (this.Parent.NorthEastChild == null)
                                    this.Parent.AddChild(NodeChild.NorthEast, NodeContent.NorthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthWest)
                            {
                                if (this.Parent.SouthEastChild == null)
                                    this.Parent.AddChild(NodeChild.SouthEast, NodeContent.SouthEastChild);
                            }
                        }
                        break;
                    case NodeContent.SouthVertex:
                        if (this.SouthNeighbor == null)
                        {
                            if (this.Position == NodeChild.SouthEast)
                            {
                                if (this.Parent.SouthNeighbor != null)
                                    if (this.Parent.SouthNeighbor.NorthEastChild == null)
                                        this.Parent.SouthNeighbor.AddChild(NodeChild.NorthEast, NodeContent.NorthEastChild);
                            }
                            else if (this.Position == NodeChild.SouthWest)
                            {
                                if (this.Parent.SouthNeighbor != null)
                                    if (this.Parent.SouthNeighbor.NorthWestChild == null)
                                        this.Parent.SouthNeighbor.AddChild(NodeChild.NorthWest, NodeContent.NorthWestChild);
                            }
                            else if (this.Position == NodeChild.NorthEast)
                            {
                                if (this.Parent.SouthEastChild == null)
                                    this.Parent.AddChild(NodeChild.SouthEast, NodeContent.SouthEastChild);
                            }
                            else if (this.Position == NodeChild.NorthWest)
                            {
                                if (this.Parent.SouthWestChild == null)
                                    this.Parent.AddChild(NodeChild.SouthWest, NodeContent.SouthWestChild);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// <para>Disable the specified flag and the specified vertex.</para>
        /// </summary>
        public void DisableVertex(NodeContent flag, NodeVertex vertex)
        {
            this.DisableVertex(flag);
            this._vertices[(int)vertex].RemoveReferenceFrom(this);
        }

        /// <summary>
        /// <para>Disable the specified flag.</para>
        /// </summary>
        public void DisableVertex(NodeContent flag)
        {
            this._enabledContent &= ~flag;

            if (((int)this._enabledContent >> 5) == 0)
            {
                this._enabledContent &= ~NodeContent.CenterVertex;
                this.CenterVertex.RemoveReferenceFrom(this);
            }
        }

        /// <summary>
        /// <para>Gets a value indicating is the spécified vertex is used by a child.</para>
        /// </summary>
        /// <returns></returns>
        private bool EdgeUsed(NodeVertex position)
        {
            return this.Vertices[(int)position].Enabled;
        }

        /// <summary>
        /// <para>Returns true if the specified <see cref="EnabledVertex"/> is enabled.</para>
        /// </summary>
        /// <param name="enabledVertex">Flag to analyse.</param>
        private bool IsEnabled(NodeContent enabledVertex)
        {
            return ((this._enabledContent & enabledVertex) != NodeContent.None);
        }

        /// <summary>
        /// <para>Returns true if the specified <see cref="EnabledVertex"/> is disabled.</para>
        /// </summary>
        /// <param name="disabledVertex">Flag to analyse.</param>
        private bool IsDisabled(NodeContent disabledVertex)
        {
            return ((this._enabledContent & disabledVertex) == NodeContent.None);
        }

        /// <summary>
        /// <para>Gets the <see cref="QuadNode"/> size.</para>
        /// </summary>
        /// <returns></returns>
        public float GetNodeSize()
        {
            return this.ParentTree.GetNodeSizeAtLevel(this.Depth);
        }
    
        #endregion


        #region Public methods



        private static int currentIndice = 0;
        private TerrainPrimitive terrainPrimitive;

        private void FillWithVertex(NodeVertex position)
        {
            VertexPositionNormalTexture vertexValue = this.Vertices[(int)position].Value;
            TerrainVertex vertex = this.Vertices[(int)position];

            if (this.ParentTree.ProcessIterationId != vertex.LastUsedIteration)
            {
                vertex.BufferIndice = this.ParentTree.Vertices.Count;
                this.ParentTree.Vertices.Add(vertexValue);
                vertex.LastUsedIteration = this.ParentTree.ProcessIterationId;
            }
            this.ParentTree.Indices.Add(vertex.BufferIndice);

            if (currentIndice == 0)
            {
                terrainPrimitive = new TerrainPrimitive(0, 0, 0);
                terrainPrimitive.Indice1 = vertex.BufferIndice;
                currentIndice++;
            }
            else if (currentIndice == 1)
            {
                terrainPrimitive.Indice2 = vertex.BufferIndice;
                currentIndice++;
            }
            else if (currentIndice == 2)
            {
                terrainPrimitive.Indice3 = vertex.BufferIndice;
                currentIndice = 0;
                terrainPrimitive.SetNormal(this.ParentTree.Vertices);
            }
          
        }
        
        /// <summary>
        /// <para>Build triangle view of the current node.</para>
        /// </summary>
        public void GetEnabledVertices()
        {
            if (!HaveNoEdge())
            {
                if (this.NorthWestChild == null)
                {
                    if (this.SouthWestChild != null)
                    {
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.West);
                        this.FillWithVertex(NodeVertex.NorthWest);
                    }
                    else if (this.WestVertex.Enabled)
                    {
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.West);
                        this.FillWithVertex(NodeVertex.NorthWest);
                    }

                    this.FillWithVertex(NodeVertex.Center);
                    this.FillWithVertex(NodeVertex.NorthWest);

                    if (this.NorthEastChild != null)
                    {
                        this.FillWithVertex(NodeVertex.North);
                    }
                }

                //NorthEast
                if (this.NorthEastChild == null)
                {
                    if (this.NorthWestChild != null)
                    {
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.North);
                    }
                    else if (this.NorthVertex.Enabled)
                    {
                        this.FillWithVertex(NodeVertex.North);
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.North);
                    }

                    this.FillWithVertex(NodeVertex.NorthEast);
                    this.FillWithVertex(NodeVertex.Center);
                    this.FillWithVertex(NodeVertex.NorthEast);

                    if (this.SouthEastChild != null)
                    {
                        this.FillWithVertex(NodeVertex.East);
                    }
                }

                //SouthEast
                if (this.SouthEastChild == null)
                {
                    if (this.NorthEastChild != null)
                    {
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.East);
                    }
                    else if (this.EastVertex.Enabled)
                    {
                        this.FillWithVertex(NodeVertex.East);
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.East);
                    }

                    this.FillWithVertex(NodeVertex.SouthEast);
                    this.FillWithVertex(NodeVertex.Center);
                    this.FillWithVertex(NodeVertex.SouthEast);

                    if (this.SouthWestChild != null)
                    {
                        this.FillWithVertex(NodeVertex.South);
                    }
                }

                if (this.SouthWestChild == null)
                {
                    if (this.SouthEastChild != null)
                    {
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.South);
                    }
                    else if (this.SouthVertex.Enabled)
                    {
                        this.FillWithVertex(NodeVertex.South);
                        this.FillWithVertex(NodeVertex.Center);
                        this.FillWithVertex(NodeVertex.South);
                    }

                    this.FillWithVertex(NodeVertex.SouthWest);
                    this.FillWithVertex(NodeVertex.Center);
                    this.FillWithVertex(NodeVertex.SouthWest);


                    if (this.NorthWestChild != null)
                    {
                        this.FillWithVertex(NodeVertex.West);
                    }
                    else
                    {
                        if (this.WestVertex.Enabled)
                            this.FillWithVertex(NodeVertex.West);
                        else
                            this.FillWithVertex(NodeVertex.NorthWest);
                    }
                }
            }
            else
            {
                this.FillWithVertex(NodeVertex.NorthWest);
                this.FillWithVertex(NodeVertex.NorthEast);
                this.FillWithVertex(NodeVertex.SouthEast);
                this.FillWithVertex(NodeVertex.NorthWest);
                this.FillWithVertex(NodeVertex.SouthEast);
                this.FillWithVertex(NodeVertex.SouthWest);
            }

            for (int i = 0; i < QuadTree.NodeChildsNumber; i++)
                if (this.Childs[i] != null)
                    this.Childs[i].GetEnabledVertices();
        }

        /// <summary>
        /// <para>Check the specified vertex of the current <see cref="QuadNode"/>.</para>
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="flag">Flag to enable/disable.</param>
        private void CheckVertexAt(NodeVertex position, NodeContent flag, NodeSideVertex side)
        {
            if (this.IsDisabled(flag)//if the flag is not enabled
                && VertexTest(this.Vertices[(int)position].Value.Position, side, Camera.FreeCamera.DefaultCamera.Position))//and the vertex can be enabled...
                this.EnableVertex(flag, position);
            else if (this.IsEnabled(flag)//if the flag is enabled
                && !VertexTest(this.Vertices[(int)position].Value.Position, side, Camera.FreeCamera.DefaultCamera.Position))//and the vertex have to be disabled...
                this.DisableVertex(flag, position);
        }

        /// <summary>
        /// <para>Check the specified child of the current <see cref="QuadNode"/>.</para>
        /// </summary>
        /// <param name="position">Position of the child.</param>
        /// <param name="flag">Flag to enable/disable.</param>
        /// <param name="childBox">Associated child's bounding box.</param>
        private void CheckChildAt(NodeChild position, NodeContent flag, float dotprod, BoundingBox childBox)
        {
            if (this.IsDisabled(flag)//if the flag is not enabled
                && ChildTest(dotprod, childBox, Camera.FreeCamera.DefaultCamera.Position))//and the child bounding box show that the child have to be enabled
                this.AddChild(position, flag);
            else if (this.IsEnabled(flag)//if the flag is enabled
                && this.Childs[(int)position].IsLeaf() //and the child have not childs
                && this.Childs[(int)position].HaveNoEdge() // and the child have no side edges
                && !ChildTest(dotprod, childBox, Camera.FreeCamera.DefaultCamera.Position)) ////and the child bounding box show that the child have to be disabled
                this.RemoveChild(position, flag);
        }

        public bool IsLeaf()
        {
            return (((int)this.EnabledContent >> 9) == 0);
        }

        /// <summary>
        /// <para>Returns true if the current <see cref="QuadNode"/> have no side vertex enabled.</para>
        /// </summary>
        public bool HaveNoEdge()
        {
            return (!this.NorthVertex.Enabled && !this.SouthVertex.Enabled && !this.EastVertex.Enabled && !this.WestVertex.Enabled);
        }


        /// <summary>
        /// <para>Remove a child from the specified position and disable its flag.</para>
        /// </summary>
        private void RemoveChild(NodeChild position, NodeContent flag)
        {
            this.DisableVertex(flag);
            QuadNode node = this.Childs[(int)position];
            switch (position)
            {
                case NodeChild.NorthWest:
                    this.DisableVertex(NodeContent.NorthVertex, NodeVertex.North);
                    this.DisableVertex(NodeContent.WestVertex, NodeVertex.West);
                    break;
                case NodeChild.NorthEast:
                    this.DisableVertex(NodeContent.NorthVertex, NodeVertex.North);
                    this.DisableVertex(NodeContent.EastVertex, NodeVertex.East);
                    break;
                case NodeChild.SouthWest:
                    this.DisableVertex(NodeContent.SouthVertex, NodeVertex.South);
                    this.DisableVertex(NodeContent.WestVertex, NodeVertex.West);
                    break;
                default:
                    this.DisableVertex(NodeContent.SouthVertex, NodeVertex.South);
                    this.DisableVertex(NodeContent.EastVertex, NodeVertex.East);
                    break;
            }

            this.Childs[(int)position] = null;
            this.InitializeNeighbors();
            node.InitializeNeighbors();
            node.Dispose();
        }

        /// <summary>
        /// <para>Add a child at the specified position and enable its flag.</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="flag"></param>
        private void AddChild(NodeChild position, NodeContent flag)
        {
            this.EnableVertex(flag);
            QuadNode node = new QuadNode(this, position);
            this.Childs[(int)position] = node;
            float size = node.GetNodeSize();

            switch (position)
            {
                case NodeChild.NorthWest:
                    node.Location = this.Location + new Vector2(0, size);
                    break;
                case NodeChild.NorthEast:
                    node.Location = this.Location + new Vector2(size, size);
                    break;
                case NodeChild.SouthWest:
                    node.Location = this.Location + new Vector2(0, 0);
                    break;
                default:
                    node.Location = this.Location + new Vector2(size, 0);
                    break;
            }
            node.InitializeNeighbors();
            this.InitializeNeighbors();
            node.Initialize();

            switch (position)
            {
                case NodeChild.NorthWest:
                    this.EnableVertex(NodeContent.NorthVertex, NodeVertex.North);
                    this.EnableVertex(NodeContent.WestVertex, NodeVertex.West);
                    break;
                case NodeChild.NorthEast:
                    this.EnableVertex(NodeContent.NorthVertex, NodeVertex.North);
                    this.EnableVertex(NodeContent.EastVertex, NodeVertex.East);
                    break;
                case NodeChild.SouthWest:
                    this.EnableVertex(NodeContent.SouthVertex, NodeVertex.South);
                    this.EnableVertex(NodeContent.WestVertex, NodeVertex.West);
                    break;
                default:
                    this.EnableVertex(NodeContent.SouthVertex, NodeVertex.South);
                    this.EnableVertex(NodeContent.EastVertex, NodeVertex.East);
                    break;
            }

        }

        /// <summary>
        /// <para>Check the relevance of a vertex.</para>
        /// </summary>
        public bool VertexTest(Vector3 vertexPosition, NodeSideVertex side, Vector3 cameraPosition)
        {
            //get the distance between interpolated height position and real height position
            float lengthToTest = this._realToInterpolatedVertexHeight[(int)side];
            //get the distance from the camera position to the vertex position
            float distanceCameraToPoint = Vector3.Distance(vertexPosition, cameraPosition);
            //check with the threshold
            return lengthToTest * this.ParentTree.VertexDetail > distanceCameraToPoint;
        }

        /// <summary>
        /// <para>Check the relevance of a child.</para>
        /// </summary>
        public static bool IsChildRelevant(Vector3 childNormal, Vector3 parentNormal, BoundingBox boundingBox)
        {
            //compute the dot product between parent normal and child normal
            float dotprod = 1 - Vector3.Dot(childNormal, parentNormal);

            
            //check with the threshold
            return true;// (dotprod);
        }

        /// <summary>
        /// <para>Check the relevance of a child.</para>
        /// </summary>
        private bool ChildTest(float dotprod, BoundingBox childBoundingBox, Vector3 cameraPosition)
        {
            //by default, the four childs of the root node are visible.
            if (this.Depth < this.ParentTree.MinimalDepth)
                return true;

            //get the closest point to the camera and check the distance
            float distanceCameraToPoint = Vector3.Distance(GetBoundingBoxClosestPointToPoint(childBoundingBox, cameraPosition), cameraPosition);
 
            //check with the threshold
             return ((distanceCameraToPoint - this.ParentTree.QuadTreeDetailAtFront) / this.ParentTree.QuadTreeDetailAtFar) < (dotprod);
        }

        public static Vector3 GetBoundingBoxClosestPointToPoint(BoundingBox box, Microsoft.Xna.Framework.Vector3 point)
        {
            float x, y, z;
            if (point.X > box.Max.X)
                x = box.Max.X;
            else if (point.X < box.Min.X)
                x = box.Min.X;
            else
                x = point.X;

            if (point.Y > box.Max.Y)
                y = box.Max.Y;
            else if (point.Y < box.Min.Y)
                y = box.Min.Y;
            else
                y = point.Y;

            if (point.Z > box.Max.Z)
                z = box.Max.Z;
            else if (point.Z < box.Min.Z)
                z = box.Min.Z;
            else
                z = point.Z;

            return new Vector3(x, y, z);
        }

        #endregion


        #region IDisposable Members

        public override void Dispose()
        {
            base.Dispose();

            for (int i = 0; i < this.Vertices.Length; i++)
                this.Vertices[i].RemoveReferenceFrom(this);

            for (int i = 0; i < QuadTree.NodeChildsNumber; i++)
                if (this.Childs[i] != null)
                {
                    this.Childs[i].Dispose();
                    this.Childs[i] = null;
                }

        }

        #endregion

    }
}
