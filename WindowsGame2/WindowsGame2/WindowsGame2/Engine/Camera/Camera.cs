using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Camera
{
    public abstract class Camera
    {
        #region Fields
        private Matrix transform;
        private Matrix view;
        private Matrix projection;     
        private float aspectRatio;    
        private float fovYDegrees = 45;
        private float tanFovy = (float)Math.Tan(MathHelper.ToRadians(45 * 0.5f));
        private static Camera camera;
        #endregion
        #region Properties
        /// <summary>
        /// Transformation Matrix. Use it as World parameter for shaders.
        /// </summary>
        public Matrix Transform
        {
            get
            {
                Matrix.Invert(ref view, out transform);
                return transform;
            }
            set
            {
                transform = value;
            }
        }

        /// <summary>
        /// View Matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            protected set
            {
                view = value;
                generateFrustum();
            }
        }

        /// <summary>
        /// Projection Matrix. It converts the 3d world into a 2d one.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            protected set
            {
                projection = value;
                generateFrustum();
            }
        }

        /// <summary>
        /// Near clip plane
        /// </summary>
        public virtual float NearPlane { get; set; }
      
        /// <summary>
        /// Far clip plane
        /// </summary>
        public virtual float FarPlane { get; set; }

        /// <summary>
        /// Get/Set Target vector of camara 
        /// </summary>
        public virtual Vector3 Target { get; set; }

        /// <summary>
        /// Frustum
        /// </summary>
        public BoundingFrustum Frustum { get; private set; }

        /// <summary>
        /// GraphicsDevice
        /// </summary>
        protected GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Gets/Sets the aspect ratio of the camera
        /// </summary>
        public float AspentRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }
    
        public float TanFovy
        {
            get { return tanFovy; }
        }
      
        public float FovYDegrees
        {
            get { return fovYDegrees; }
            set
            {
                fovYDegrees = value;
                tanFovy = (float)Math.Tan(MathHelper.ToRadians(fovYDegrees));
            }
        }

        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (Frustum.Contains(sphere) != ContainmentType.Disjoint);
        }

        public static Camera DefaultCamera
        {
            get
            {
                return Camera.camera;
            }
            set
            {
                Camera.camera = value;
            }
        }

        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (Frustum.Contains(box) != ContainmentType.Disjoint);
        }
        #endregion
        #region Constructor
        public Camera(GraphicsDevice graphicsDevice, float nearPlane, float farPlane)
        {
            this.GraphicsDevice = graphicsDevice;

            generatePerspectiveProjectionMatrix(nearPlane, farPlane);
        }
        #endregion
        #region Functions
        private void generatePerspectiveProjectionMatrix(float nearPlane, float farPlane)
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            aspectRatio = (float)pp.BackBufferWidth /
                (float)pp.BackBufferHeight;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), aspectRatio, nearPlane, farPlane);
        }

        public void ProjectBoundingSphereOnScreen(BoundingSphere boundingSphere, out Vector2 topLeft, out Vector2 size)
        {
            //l is the bounding sphere's position in eye space
            Vector3 l = Vector3.Transform(boundingSphere.Center, View);

            //Store the coordinates of the scissor rectangle
            //Start by setting them to the outside of the screen
            float scissorLeft = -1.0f;
            float scissorRight = 1.0f;

            float scissorBottom = -1.0f;
            float scissorTop = 1.0f;

            //r is the radius of the bounding sphere
            float r = boundingSphere.Radius;


            //halfNearPlaneHeight is half the height of the near plane, i.e. from the centre to the top
            float halfNearPlaneHeight = NearPlane * (float)Math.Tan(MathHelper.ToRadians(fovYDegrees * 0.5f));

            float halfNearPlaneWidth = halfNearPlaneHeight * aspectRatio;

            //All calculations in eye space

            //We wish to find 2 planes parallel to the Y axis which are tangent to the bounding sphere
            //of the light and pass through the origin (camera position)

            //plane normal. Of the form (x, 0, z)
            Vector3 normal;

            //Calculate the discriminant of the quadratic we wish to solve to find nx(divided by 4)
            float d = (l.Z * l.Z) * ((l.X * l.X) + (l.Z * l.Z) - r * r);

            //If d>0, solve the quadratic to get the normal to the plane
            if (d > 0.0f)
            {
                float rootD = (float)Math.Sqrt(d);

                //Loop through the 2 solutions
                for (int i = 0; i < 2; ++i)
                {
                    //Calculate the normal
                    if (i == 0)
                        normal.X = r * l.X + rootD;
                    else
                        normal.X = r * l.X - rootD;

                    normal.X /= (l.X * l.X + l.Z * l.Z);

                    normal.Z = r - normal.X * l.X;
                    normal.Z /= l.Z;

                    //We need to divide by normal.X. If ==0, no good
                    if (normal.X == 0.0f)
                        continue;


                    //p is the point of tangency
                    Vector3 p;

                    p.Z = (l.X * l.X) + (l.Z * l.Z) - r * r;
                    p.Z /= l.Z - ((normal.Z / normal.X) * l.X);

                    //If the point of tangency is behind the camera, no good
                    if (p.Z >= 0.0f)
                        continue;

                    p.X = -p.Z * normal.Z / normal.X;

                    //Calculate where the plane meets the near plane
                    //divide by the width to give a value in [-1, 1] for values on the screen
                    float screenX = normal.Z * NearPlane / (normal.X * halfNearPlaneWidth);

                    //If this is a left bounding value (p.X<l.X) and is further right than the
                    //current value, update
                    if (p.X < l.X && screenX > scissorLeft)
                        scissorLeft = screenX;

                    //Similarly, update the right value
                    if (p.X > l.X && screenX < scissorRight)
                        scissorRight = screenX;
                }
            }


            //Repeat for planes parallel to the x axis
            //normal is now of the form(0, y, z)
            normal.X = 0.0f;

            //Calculate the discriminant of the quadratic we wish to solve to find ny(divided by 4)
            d = (l.Z * l.Z) * ((l.Y * l.Y) + (l.Z * l.Z) - r * r);

            //If d>0, solve the quadratic to get the normal to the plane
            if (d > 0.0f)
            {
                float rootD = (float)Math.Sqrt(d);

                //Loop through the 2 solutions
                for (int i = 0; i < 2; ++i)
                {
                    //Calculate the normal
                    if (i == 0)
                        normal.Y = r * l.Y + rootD;
                    else
                        normal.Y = r * l.Y - rootD;

                    normal.Y /= (l.Y * l.Y + l.Z * l.Z);

                    normal.Z = r - normal.Y * l.Y;
                    normal.Z /= l.Z;

                    //We need to divide by normal.Y. If ==0, no good
                    if (normal.Y == 0.0f)
                        continue;


                    //p is the point of tangency
                    Vector3 p;

                    p.Z = (l.Y * l.Y) + (l.Z * l.Z) - r * r;
                    p.Z /= l.Z - ((normal.Z / normal.Y) * l.Y);

                    //If the point of tangency is behind the camera, no good
                    if (p.Z >= 0.0f)
                        continue;

                    p.Y = -p.Z * normal.Z / normal.Y;

                    //Calculate where the plane meets the near plane
                    //divide by the height to give a value in [-1, 1] for values on the screen
                    float screenY = normal.Z * NearPlane / (normal.Y * halfNearPlaneHeight);

                    //If this is a bottom bounding value (p.Y<l.Y) and is further up than the
                    //current value, update
                    if (p.Y < l.Y && screenY > scissorBottom)
                        scissorBottom = screenY;

                    //Similarly, update the top value
                    if (p.Y > l.Y && screenY < scissorTop)
                        scissorTop = screenY;
                }
            }

            //compute the width & height of the rectangle
            size.X = scissorRight - scissorLeft;
            size.Y = scissorTop - scissorBottom;

            topLeft.X = scissorLeft;
            topLeft.Y = -scissorBottom - size.Y;

        }

        public virtual void Update()
        {
        }

        private void generateFrustum()
        {
            Matrix viewProjection = View * Projection;
            Frustum = new BoundingFrustum(viewProjection);
        }
        #endregion
    }
}