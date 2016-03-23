using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Shaders
{
    internal class ShadowRenderer
    {
        internal class SpotShadowMapEntry
        {
            public RenderTarget2D Texture;
            public Matrix LightViewProjection;
        }

        internal class CascadeShadowMapEntry
        {
            public RenderTarget2D Texture;
            public Matrix[] LightViewProjectionMatrices = new Matrix[NUM_CSM_SPLITS];
            public Vector2[] LightClipPlanes = new Vector2[NUM_CSM_SPLITS];
        }

#if XBOX360
        private const int CASCADE_SHADOW_RESOLUTION = 640;
#else
        private const int CASCADE_SHADOW_RESOLUTION = 1024;
#endif
        private const int NUM_CSM_SPLITS = 3;

        //temporary variables to help on cascade shadow maps
        float[] splitDepthsTmp = new float[NUM_CSM_SPLITS + 1];
        Vector3[] frustumCornersWS = new Vector3[8];
        Vector3[] frustumCornersVS = new Vector3[8];
        Vector3[] splitFrustumCornersVS = new Vector3[8];

        private List<SpotShadowMapEntry> _spotShadowMaps = new List<SpotShadowMapEntry>();
        public List<CascadeShadowMapEntry> _cascadeShadowMaps = new List<CascadeShadowMapEntry>();
        private const int NUM_SPOT_SHADOWS = 4;
        private const int NUM_CSM_SHADOWS = 1;
        private const int SPOT_SHADOW_RESOLUTION = 512;

        private int _currentFreeSpotShadowMap;
        private int _currentFreeCascadeShadowMap;

        public ShadowRenderer(Renderer renderer)
        {
            //create the render targets
            for (int i = 0; i < NUM_SPOT_SHADOWS; i++)
            {
                SpotShadowMapEntry entry = new SpotShadowMapEntry();
                //we store the linear depth, in a float render target. We need also the HW zbuffer
                entry.Texture = new RenderTarget2D(renderer.GraphicsDevice, SPOT_SHADOW_RESOLUTION,
                                                   SPOT_SHADOW_RESOLUTION, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
                entry.LightViewProjection = Matrix.Identity;
                _spotShadowMaps.Add(entry);
            }
            for (int i = 0; i < NUM_CSM_SHADOWS; i++)
            {
                CascadeShadowMapEntry entry = new CascadeShadowMapEntry();
                entry.Texture = new RenderTarget2D(renderer.GraphicsDevice, CASCADE_SHADOW_RESOLUTION * NUM_CSM_SPLITS,
                                                   CASCADE_SHADOW_RESOLUTION, false, SurfaceFormat.Single,
                                                   DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
                _cascadeShadowMaps.Add(entry);
            }
        }

        public void InitFrame()
        {
            _currentFreeSpotShadowMap = 0;
            _currentFreeCascadeShadowMap = 0;
        }

        /// <summary>
        /// Returns an unused shadow map, or null if we run out of SMs
        /// </summary>
        /// <returns></returns>
        internal SpotShadowMapEntry GetFreeSpotShadowMap()
        {
            if (_currentFreeSpotShadowMap < _spotShadowMaps.Count)
            {
                return _spotShadowMaps[_currentFreeSpotShadowMap++];
            }
            return null;
        }

        /// <summary>
        /// Returns an unused cascade shadow map, or null if we run out of SMs
        /// </summary>
        /// <returns></returns>
        internal CascadeShadowMapEntry GetFreeCascadeShadowMap()
        {
            if (_currentFreeCascadeShadowMap < _cascadeShadowMaps.Count)
            {
                return _cascadeShadowMaps[_currentFreeCascadeShadowMap++];
            }
            return null;
        }
        public void GenerateShadowTextureSpotLight(Renderer renderer, object[] meshes, object[] InstancedMeshes, Light light, SpotShadowMapEntry shadowMap)
        {
            //bind the render target
            renderer.GraphicsDevice.SetRenderTarget(shadowMap.Texture);
            //clear it to white, ie, far far away
            renderer.GraphicsDevice.Clear(Color.White);
            renderer.GraphicsDevice.BlendState = BlendState.Opaque;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            Matrix viewProj = light.ViewProjection;
            shadowMap.LightViewProjection = viewProj;

            BoundingFrustum frustum = light.Frustum;


            foreach (object mesh in meshes)
            {
                if (mesh is List<Models>)
                    for (int index = 0; index < ((List<Models>)mesh).Count; index++)
                    {
                        Models m = ((List<Models>)mesh)[index];
                        //cull meshes outside the light volume
                        //   if (!frustum.Intersects(m.BoundingSphere))
                        //       continue;

                        //render it
                        m.RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                    }
                if (mesh is Models)
                {
                    //cull meshes outside the light volume
                    // if (!frustum.Intersects(((Models)mesh).BoundingSphere))
                    //    continue;

                    //render it
                    ((Models)mesh).RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                }
                if (mesh is Terrain.Terrain)
                {
                    for (int index = 0; index < ((Terrain.Terrain)mesh).QuadTrees.Count; index++)
                    {
                        //cull meshes outside the light volume
                        //  if (!frustum.Intersects(((Terrain.Terrain)mesh).QuadTrees[index].BoundingSphere))
                        //      continue;

                        //render it
                        ((Terrain.Terrain)mesh).QuadTrees[index].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                    }

                }
            }
            foreach (object mesh in InstancedMeshes)
            {
                if (mesh is Billboards.Billboard)
                {
                    ((Billboards.Billboard)mesh).TreePreDraw();

                    for (int lod = 0; lod < ((Billboards.Billboard)mesh).LOD; lod++)
                        if (((Billboards.Billboard)mesh).instanceTransforms[lod].Length != 0)
                            ((Billboards.Billboard)mesh).trunck[lod][0].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);

                    if (((Billboards.Billboard)mesh).Leaves)
                        for (int tree = 0; tree < ((Billboards.Billboard)mesh).NoTrees; tree++)
                        {
                            if (((Billboards.Billboard)mesh).LeavesAreVisible[tree])
                                for (int j = 0; j < ((Billboards.Billboard)mesh).NoLeaves; j++)
                                {
                                    ((Billboards.Billboard)mesh).leaves[tree][j].UpdateTransformationMatrix(((Billboards.Billboard)mesh).instanceTransforms1[tree]);
                                    if (j == 0)
                                        ((Billboards.Billboard)mesh).leaves[tree][j].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                                }
                        }
                }
            }
        }

        public void GenerateShadowTextureDirectionalLight(Renderer renderer, object[] meshes, object[] InstancedMeshes, Light light, CascadeShadowMapEntry cascadeShadowMap, Camera.Camera camera)
        {
            //bind the render target
            renderer.GraphicsDevice.SetRenderTarget(cascadeShadowMap.Texture);
            //clear it to white, ie, far far away
            renderer.GraphicsDevice.Clear(Color.White);
            renderer.GraphicsDevice.BlendState = BlendState.Opaque;
            renderer.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Get the corners of the frustum
            camera.Frustum.GetCorners(frustumCornersWS);
            Matrix eyeTransform = camera.View;
            Vector3.Transform(frustumCornersWS, ref eyeTransform, frustumCornersVS);

            float near = camera.NearPlane, far = MathHelper.Min(camera.FarPlane, light.ShadowDistance);

            splitDepthsTmp[0] = near;
            splitDepthsTmp[NUM_CSM_SPLITS] = far;

            //compute each distance the way you like...
            for (int i = 1; i < splitDepthsTmp.Length - 1; i++)
                splitDepthsTmp[i] = near + (far - near) * (float)Math.Pow((i / (float)NUM_CSM_SPLITS), 3);


            Viewport splitViewport = new Viewport();
            Vector3 lightDir = -Vector3.Normalize(light.Transform.Forward);

            BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

            for (int i = 0; i < NUM_CSM_SPLITS; i++)
            {
                cascadeShadowMap.LightClipPlanes[i].X = -splitDepthsTmp[i];
                cascadeShadowMap.LightClipPlanes[i].Y = -splitDepthsTmp[i + 1];

                cascadeShadowMap.LightViewProjectionMatrices[i] = CreateLightViewProjectionMatrix(lightDir, far, camera, splitDepthsTmp[i], splitDepthsTmp[i + 1], i);
                Matrix viewProj = cascadeShadowMap.LightViewProjectionMatrices[i];

                // Set the viewport for the current split     
                splitViewport.MinDepth = 0;
                splitViewport.MaxDepth = 1;
                splitViewport.Width = CASCADE_SHADOW_RESOLUTION;
                splitViewport.Height = CASCADE_SHADOW_RESOLUTION;
                splitViewport.X = i * CASCADE_SHADOW_RESOLUTION;
                splitViewport.Y = 0;
                renderer.GraphicsDevice.Viewport = splitViewport;

                frustum.Matrix = viewProj;

                foreach (object mesh in meshes)
                {
                    if (mesh is List<Models>)
                        for (int index = 0; index < ((List<Models>)mesh).Count; index++)
                        {
                            Models m = ((List<Models>)mesh)[index];
                            //cull meshes outside the light volume
                         //   if (!frustum.Intersects(m.BoundingSphere))
                         //       continue;

                            //render it
                            m.RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                        }
                    if (mesh is Models)
                    {
                        //cull meshes outside the light volume
                       // if (!frustum.Intersects(((Models)mesh).BoundingSphere))
                        //    continue;

                        //render it
                        ((Models)mesh).RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                    }
                    if (mesh is Terrain.Terrain)
                    {                      
                        for (int index = 0; index < ((Terrain.Terrain)mesh).QuadTrees.Count; index++)
                        {
                            //cull meshes outside the light volume
                          //  if (!frustum.Intersects(((Terrain.Terrain)mesh).QuadTrees[index].BoundingSphere))
                          //      continue;
                           
                            //render it
                            ((Terrain.Terrain)mesh).QuadTrees[index].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                        }
                       
                    }
                }
                foreach (object mesh in InstancedMeshes)
                {
                    if (mesh is Billboards.Billboard)
                    {
                        ((Billboards.Billboard)mesh).TreePreDraw();

                        for (int lod = 0; lod < ((Billboards.Billboard)mesh).LOD; lod++)
                            if (((Billboards.Billboard)mesh).instanceTransforms[lod].Length != 0)
                                ((Billboards.Billboard)mesh).trunck[lod][0].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                        
                        if (((Billboards.Billboard)mesh).Leaves)
                            for (int tree = 0; tree < ((Billboards.Billboard)mesh).NoTrees; tree++)
                            {
                                if (((Billboards.Billboard)mesh).LeavesAreVisible[tree])
                                    for (int j = 0; j < ((Billboards.Billboard)mesh).NoLeaves; j++)
                                    {
                                        ((Billboards.Billboard)mesh).leaves[tree][j].UpdateTransformationMatrix(((Billboards.Billboard)mesh).instanceTransforms1[tree]);
                                        if (j == 0)
                                            ((Billboards.Billboard)mesh).leaves[tree][j].RenderShadowMap(ref viewProj, renderer.GraphicsDevice);
                                    }
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the WorldViewProjection matrix from the perspective of the 
        /// light using the cameras bounding frustum to determine what is visible 
        /// in the scene.
        /// </summary>
        /// <returns>The WorldViewProjection for the light</returns>
        Matrix CreateLightViewProjectionMatrix(Vector3 lightDir, float farPlane, Camera.Camera camera, float minZ, float maxZ, int index)
        {
            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i + 4] * (minZ / camera.FarPlane);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i] * (maxZ / camera.FarPlane);

            Matrix cameraMat = camera.Transform;
            Vector3.Transform(splitFrustumCornersVS, ref cameraMat, frustumCornersWS);

            // Matrix with that will rotate in points the direction of the light
            Vector3 cameraUpVector = Vector3.Up;
            if (Math.Abs(Vector3.Dot(cameraUpVector, lightDir)) > 0.9f)
                cameraUpVector = Vector3.Forward;

            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                                                       -lightDir,
                                                       cameraUpVector);


            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCornersWS.Length; i++)
            {
                frustumCornersWS[i] = Vector3.Transform(frustumCornersWS[i], lightRotation);
            }


            // Find the smallest box around the points
            Vector3 mins = frustumCornersWS[0], maxes = frustumCornersWS[0];
            for (int i = 1; i < frustumCornersWS.Length; i++)
            {
                Vector3 p = frustumCornersWS[i];
                if (p.X < mins.X) mins.X = p.X;
                if (p.Y < mins.Y) mins.Y = p.Y;
                if (p.Z < mins.Z) mins.Z = p.Z;
                if (p.X > maxes.X) maxes.X = p.X;
                if (p.Y > maxes.Y) maxes.Y = p.Y;
                if (p.Z > maxes.Z) maxes.Z = p.Z;
            }


            // Find the smallest box around the points in view space
            Vector3 minsVS = splitFrustumCornersVS[0], maxesVS = splitFrustumCornersVS[0];
            for (int i = 1; i < splitFrustumCornersVS.Length; i++)
            {
                Vector3 p = splitFrustumCornersVS[i];
                if (p.X < minsVS.X) minsVS.X = p.X;
                if (p.Y < minsVS.Y) minsVS.Y = p.Y;
                if (p.Z < minsVS.Z) minsVS.Z = p.Z;
                if (p.X > maxesVS.X) maxesVS.X = p.X;
                if (p.Y > maxesVS.Y) maxesVS.Y = p.Y;
                if (p.Z > maxesVS.Z) maxesVS.Z = p.Z;
            }
            BoundingBox _lightBox = new BoundingBox(mins, maxes);

            bool fixShadowJittering = false;
            if (fixShadowJittering)
            {
                // I borrowed this code from some forum that I don't remember anymore =/
                // We snap the camera to 1 pixel increments so that moving the camera does not cause the shadows to jitter.
                // This is a matter of integer dividing by the world space size of a texel
                float diagonalLength = (frustumCornersWS[0] - frustumCornersWS[6]).Length();
                diagonalLength += 2; //Without this, the shadow map isn't big enough in the world.
                float worldsUnitsPerTexel = diagonalLength / (float)CASCADE_SHADOW_RESOLUTION;

                Vector3 vBorderOffset = (new Vector3(diagonalLength, diagonalLength, diagonalLength) -
                                         (_lightBox.Max - _lightBox.Min)) * 0.5f;
                _lightBox.Max += vBorderOffset;
                _lightBox.Min -= vBorderOffset;

                _lightBox.Min /= worldsUnitsPerTexel;
                _lightBox.Min.X = (float)Math.Floor(_lightBox.Min.X);
                _lightBox.Min.Y = (float)Math.Floor(_lightBox.Min.Y);
                _lightBox.Min.Z = (float)Math.Floor(_lightBox.Min.Z);
                _lightBox.Min *= worldsUnitsPerTexel;

                _lightBox.Max /= worldsUnitsPerTexel;
                _lightBox.Max.X = (float)Math.Floor(_lightBox.Max.X);
                _lightBox.Max.Y = (float)Math.Floor(_lightBox.Max.Y);
                _lightBox.Max.Z = (float)Math.Floor(_lightBox.Max.Z);
                _lightBox.Max *= worldsUnitsPerTexel;
            }

            Vector3 boxSize = _lightBox.Max - _lightBox.Min;
            if (boxSize.X == 0 || boxSize.Y == 0 || boxSize.Z == 0)
                boxSize = Vector3.One;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = _lightBox.Min + halfBoxSize;
            lightPosition.Z = _lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition,
                                              Matrix.Invert(lightRotation));

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                                                   lightPosition - lightDir,
                                                   cameraUpVector);

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                                                               -boxSize.Z, 0);

            return lightView * lightProjection;
        }
    }
}