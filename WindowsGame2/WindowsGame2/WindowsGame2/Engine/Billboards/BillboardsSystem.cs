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
using Engine.Water;

namespace Engine.Billboards
{
    public class BillboardsSystem// : IRenderable
    {
        public Vector3 LightDirection;
        public Vector3 LightColor;
        public Vector3 AmbientColor;// = new Vector3(1, 0, 0);

        public Effect effect;
        public Effect _effect;

        GraphicsDevice graphicsDevice;

        private Matrix[] modelTransforms;
        Matrix[] instancedModelBones;
        Matrix[] instanceTransforms;

        Vector3 Up;
        Vector3 Right;
        Vector3 UpLight;
        Vector3 RightLight;

        public Model model { get; private set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }
        public Effect ShadowEffect { get { return this._effect; } }

        public enum BillboardMode { Spherical, Cylindrical, None };
        BillboardMode mode;

        DynamicVertexBuffer instanceVertexBuffer;
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
      (
          new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
          new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
          new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
          new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
      );

        private BoundingSphere boundingSphere;

        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(scale)
                    * Matrix.CreateTranslation(position);

                BoundingSphere transformed = boundingSphere;
                transformed = transformed.Transform(worldTransform);

                return transformed;
            }
        }
        public Matrix Transform
        {
            get { return transform; }
        }

        Matrix transform;

        public BillboardsSystem(ContentManager content, BillboardMode mode, Model model,
            Matrix[] instancedModelBones, Matrix[] instanceTransforms,
            Vector3 position, Vector3 rotation, Vector3 scale,
            Vector3 LightDirection, Vector3 LightColor, Vector3 AmbientLight,
            GraphicsDevice graphicsDevice)
        {
            this.model = model;
            modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            buildBoundingSphere();

            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.graphicsDevice = graphicsDevice;

            this.instancedModelBones = instancedModelBones;
            this.instanceTransforms = instanceTransforms;

           // this.LightDirection = LightDirection;
           // this.LightColor = LightColor;
           // this.AmbientColor = AmbientLight;

            this.mode = mode;
            effect = content.Load<Effect>("Effects//AlphaBlending");
            _effect = content.Load<Effect>("shaders//LPPMainEffect");
            //SetModelEffect(effect, false);
        }

        public void UpdateLight(Vector3 LightDirection, Vector3 LightColor, Vector3 AmbientLight)
        {
             this.LightDirection = LightDirection;
             this.LightColor = LightColor;
             this.AmbientColor = AmbientLight;
        }
       
        public void UpdateLight(ref Vector3 LightDirection, ref Vector3 LightColor, ref Vector3 AmbientLight)
        {
            LightDirection = this.LightDirection;
            LightColor = this.LightColor;
            AmbientLight = this.AmbientColor;
        }

        public void Update(GameTime gameTime)
        {
            Matrix _scale, _rotation, _position;
            _scale = Matrix.CreateScale(scale);
            _rotation = Matrix.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            _position = Matrix.CreateTranslation(position);

            //   Matrix.Multiply(ref _scale, ref _rotation, out transform);
            Matrix.Multiply(ref _scale, ref _rotation, out transform);
            Matrix.Multiply(ref transform, ref _position, out transform);
        }

        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }

        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }

        public void UpdateCamUpRightVector(Vector3 Up, Vector3 Right)
        {
            this.Up = Up;
            this.Right = Right;
        }
        public void UpdateLightUpRightVector(Vector3 UpLight, Vector3 RightLight)
        {
            this.UpLight = UpLight;
            this.RightLight = RightLight;
        }

        public void UpdateTransformationMatrix(Matrix[] transformation)
        {
            this.instanceTransforms = transformation;
        }

        void DrawModelHardwareInstancing(Model model, Matrix[] modelBones, Matrix[] instances, Matrix view, Matrix projection, Vector3 Camera)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0), new VertexBufferBinding(instanceVertexBuffer, 0, 1));

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    effect.Parameters["CameraPosition"].SetValue(Camera);
                    effect.Parameters["LightDirection"].SetValue(LightDirection);
                    effect.Parameters["LightColor"].SetValue(LightColor);
                    //effect.Parameters["AmbientLight"].SetValue(AmbientColor);

                    if (effect.Parameters["Size"] != null)
                    {
                        effect.Parameters["Size"].SetValue(new Vector2(scale.Z, scale.Y) * 10000);
                        effect.Parameters["Up"].SetValue(mode == BillboardMode.Spherical ? Up : (mode == BillboardMode.None ? Vector3.Zero : Vector3.Up));
                        effect.Parameters["Side"].SetValue(mode != BillboardMode.None ? Right : Vector3.Zero);
                    }

                    // ((MeshTag)meshPart.Tag).Material.SetEffectParameters(effect);

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                            meshPart.NumVertices, meshPart.StartIndex,
                            meshPart.PrimitiveCount, instances.Length);
                    }
                }
            }
        }

        public void ReconstructShading(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = rs;

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(true);

            ReconstructShadingInstancing(model, instancedModelBones, instanceTransforms, camera, graphicsDevice, lightBuffer);

            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(false);

            ReconstructShadingInstancing(model, instancedModelBones, instanceTransforms, camera, graphicsDevice, lightBuffer);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void ReconstructShadingInstancing(Model model, Matrix[] modelBones, Matrix[] instances, Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0), new VertexBufferBinding(instanceVertexBuffer, 0, 1));

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = _effect;

                    effect.CurrentTechnique = effect.Techniques["ReconstructShadingInstancing"];
                    //our first pass is responsible for rendering into GBuffer
                    effect.Parameters["LightBuffer"].SetValue(lightBuffer);
                    effect.Parameters["LightBufferPixelSize"].SetValue(new Vector2(0.5f / lightBuffer.Width, 0.5f / lightBuffer.Height));

                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["WorldView"].SetValue(modelBones[mesh.ParentBone.Index] * camera.View);
                    effect.Parameters["WorldViewProjection"].SetValue(modelBones[mesh.ParentBone.Index] * camera.View * camera.Projection);
                    effect.Parameters["AmbientColor"].SetValue(AmbientColor);
                    effect.Parameters["LightColor"].SetValue(LightColor);

                    if (((MeshTag)meshPart.Tag) != null)
                    if (((MeshTag)meshPart.Tag).Texture != null)
                    {
                        effect.Parameters["TextureEnabled"].SetValue(true);
                        effect.Parameters["Texture"].SetValue(((MeshTag)meshPart.Tag).Texture);
                    }
                    else effect.Parameters["TextureEnabled"].SetValue(false);

                    effect.Parameters["Size"].SetValue(new Vector2(scale.Z, scale.Y) * 10000);
                    effect.Parameters["Up"].SetValue(mode == BillboardMode.Spherical ? Up : (mode == BillboardMode.None ? Vector3.Zero : Vector3.Up));
                    effect.Parameters["Side"].SetValue(mode != BillboardMode.None ? Right : Vector3.Zero);

                    effect.CurrentTechnique.Passes[0].Apply();

                    RasterizerState rs = new RasterizerState();
                    rs.CullMode = CullMode.None;
                    rs.FillMode = FillMode.Solid;
                    graphicsDevice.RasterizerState = rs;

                    graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instances.Length);
                }
            }
        }

        public void RenderToGBuffer(Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = rs;

            //Draw opaque pixels
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(true);

            RenderToGBufferInstanced(model, instancedModelBones, instanceTransforms, camera, graphicsDevice);

            //Draw transparent pixels
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(false);

            RenderToGBufferInstanced(model, instancedModelBones, instanceTransforms, camera, graphicsDevice);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        public void RenderToGBufferInstanced(Model model, Matrix[] modelBones, Matrix[] instances, Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0), new VertexBufferBinding(instanceVertexBuffer, 0, 1));

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = _effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
                    //our first pass is responsible for rendering into GBuffer
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                    effect.Parameters["WorldView"].SetValue(modelBones[mesh.ParentBone.Index] * camera.View);
                    effect.Parameters["WorldViewProjection"].SetValue(modelBones[mesh.ParentBone.Index] * camera.View * camera.Projection);
                    effect.Parameters["FarClip"].SetValue(camera.FarPlane);
                    
                    if (((MeshTag)meshPart.Tag) != null)
                    if (((MeshTag)meshPart.Tag).Texture != null)
                    {
                        effect.Parameters["TextureEnabled"].SetValue(true);
                        effect.Parameters["Texture"].SetValue(((MeshTag)meshPart.Tag).Texture);
                    }
                    else effect.Parameters["TextureEnabled"].SetValue(false);
                    //if (effect.Parameters["Size"] != null)
                    //{
                        effect.Parameters["Size"].SetValue(new Vector2(scale.Z, scale.Y) * 10000);
                        effect.Parameters["Up"].SetValue(mode == BillboardMode.Spherical ? Up : (mode == BillboardMode.None ? Vector3.Zero : Vector3.Up));
                        effect.Parameters["Side"].SetValue(mode != BillboardMode.None ? Right : Vector3.Zero);
                    //}
                
                    effect.CurrentTechnique.Passes[0].Apply();
                    graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instances.Length);
                }
            }
        }

        public void RenderShadowMap(ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = rs;

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(true);

            RenderShadowMapInstancing(model, instancedModelBones, instanceTransforms, ref viewProj, graphicsDevice);

            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _effect.Parameters["AlphaTest"].SetValue(true);
            _effect.Parameters["AlphaTestGreater"].SetValue(false);

            RenderShadowMapInstancing(model, instancedModelBones, instanceTransforms, ref viewProj, graphicsDevice);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        public void RenderShadowMapInstancing(Model model, Matrix[] modelBones, Matrix[] instances, ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0), new VertexBufferBinding(instanceVertexBuffer, 0, 1));

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = _effect;

                    effect.CurrentTechnique = effect.Techniques["OutputShadowInstancing"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["LightViewProj"].SetValue(viewProj);

                    if (((MeshTag)meshPart.Tag) != null)
                        if (((MeshTag)meshPart.Tag).Texture != null)
                        {
                            effect.Parameters["TextureEnabled"].SetValue(true);
                            effect.Parameters["Texture"].SetValue(((MeshTag)meshPart.Tag).Texture);
                        }
                        else effect.Parameters["TextureEnabled"].SetValue(false);

                    effect.Parameters["Size"].SetValue(new Vector2(scale.Z, scale.Y) * 10000);
                    effect.Parameters["Up"].SetValue(mode == BillboardMode.Spherical ? UpLight : (mode == BillboardMode.None ? Vector3.Zero : Vector3.Up));
                    effect.Parameters["Side"].SetValue(mode != BillboardMode.None ? RightLight : Vector3.Zero);

                    effect.CurrentTechnique.Passes[0].Apply();
                    graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instances.Length);
                }
            }
        }

        void drawOpaquePixels(Model instancedModel, Matrix[] instancedModelBones, Matrix[] instanceTransforms, Matrix View, Matrix Projection, Vector3 Camera)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(true);

            DrawModelHardwareInstancing(instancedModel, instancedModelBones, instanceTransforms, View, Projection, Camera);
        }

        void drawTransparentPixels(Model instancedModel, Matrix[] instancedModelBones, Matrix[] instanceTransforms, Matrix View, Matrix Projection, Vector3 Camera)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(false);

            DrawModelHardwareInstancing(instancedModel, instancedModelBones, instanceTransforms, View, Projection, Camera);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            drawOpaquePixels(model, instancedModelBones, instanceTransforms, View, Projection, CameraPosition);
            drawTransparentPixels(model, instancedModelBones, instanceTransforms, View, Projection, CameraPosition);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                SetMeshEffect(mesh.Name, effect, CopyEffect);
        }

        public void SetModelMaterial(Material material)
        {
            foreach (ModelMesh mesh in model.Meshes)
                SetMeshMaterial(mesh.Name, material);
        }

        public void SetMeshEffect(string MeshName, Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Name != MeshName)
                    continue;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (CopyEffect)
                        toSet = effect.Clone();

                    MeshTag tag = (MeshTag)part.Tag;

                    // If this ModelMeshPart has a texture, set it to the effect
                    if (tag.Texture != null)
                    {
                        setEffectParameter(toSet, "BasicTexture", tag.Texture);
                        setEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        setEffectParameter(toSet, "TextureEnabled", false);

                    // Set our remaining parameters to the effect
                    setEffectParameter(toSet, "DiffuseColor", tag.Color);
                    setEffectParameter(toSet, "SpecularPower", tag.SpecularPower);

                    part.Effect = toSet;
                }
            }
        }

        public void SetMeshMaterial(string MeshName, Material material)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Name != MeshName)
                    continue;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    ((MeshTag)meshPart.Tag).Material = material;
            }
        }

        public void generateTags()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor, effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
                    else
                    {
                        Effect cacheEffect = part.Effect;
                        MeshTag tag = new MeshTag(
                            cacheEffect.Parameters["LightColor"].GetValueVector3(),
                            cacheEffect.Parameters["Texture"].GetValueTexture2D(),
                            cacheEffect.Parameters["SpecularPower"].GetValueSingle());
                        part.Tag = tag;
                    }
        }

        // Store references to all of the model's current effects
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;
        }

        // Restore the effects referenced by the model's cache
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }
        public void SetClipPlane(Vector4? Plane)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect.Parameters["ClipPlaneEnabled"] != null)
                        part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

                    if (Plane.HasValue)
                        if (part.Effect.Parameters["ClipPlane"] != null)
                            part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
                }
        }
    }
}