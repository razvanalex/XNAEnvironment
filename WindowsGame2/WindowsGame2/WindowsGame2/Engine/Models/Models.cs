using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Engine.Water;
namespace Engine
{
    public class Material
    {
        public virtual void SetEffectParameters(Effect effect) { }
    }

    public class CubeMapReflectMaterial : Material
    {
        public TextureCube CubeMap { get; set; }
        public CubeMapReflectMaterial(TextureCube CubeMap)
        {
            this.CubeMap = CubeMap;
        }
        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["CubeMap"] != null)
                effect.Parameters["CubeMap"].SetValue(CubeMap);
        }
    }

    public class Models : IRenderable
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 LightColor;
        public Vector3 AmbientColor;

        public Model Model { get; private set; }

        private Matrix[] modelTransforms;
        private GraphicsDevice graphicsDevice;
        private BoundingSphere boundingSphere;

        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(Scale)
                    * Matrix.CreateTranslation(Position);

                BoundingSphere transformed = boundingSphere;
                transformed = transformed.Transform(worldTransform);

                return transformed;
            }
        }
        Matrix transform = Matrix.Identity;
        public Matrix Transform
        {
            get 
            {
                Matrix _scale, _rotation, _position;
                _scale = Matrix.CreateScale(Scale);
                _rotation = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
                _position = Matrix.CreateTranslation(Position);

                Matrix.Multiply(ref _scale, ref _rotation, out transform);
                Matrix.Multiply(ref transform, ref _position, out transform);

                return transform; 
            }
            set { transform = value; }
        }

        public Models(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice)
        {
            this.Model = Model;

            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            buildBoundingSphere();
            generateTags();

            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;

            this.graphicsDevice = graphicsDevice;
        }

        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                    Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                    * baseWorld;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    if (effect is BasicEffect)
                    {
                        ((BasicEffect)effect).World = localWorld;
                        ((BasicEffect)effect).View = View;
                        ((BasicEffect)effect).Projection = Projection;
                        ((BasicEffect)effect).EnableDefaultLighting();
                    }
                    else
                    {
                        setEffectParameter(effect, "World", localWorld);
                        setEffectParameter(effect, "View", View);
                        setEffectParameter(effect, "Projection", Projection);
                        setEffectParameter(effect, "CameraPosition", CameraPosition);
                    }

                    //((MeshTag)meshPart.Tag).Material.SetEffectParameters(effect);
                }

                mesh.Draw();
            }
        }

        // Sets the specified effect parameter to the given effect, if it
        // has that parameter
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

        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                SetMeshEffect(mesh.Name, effect, CopyEffect);
        }

        public void SetModelMaterial(Material material)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                SetMeshMaterial(mesh.Name, material);
        }

        public void SetMeshEffect(string MeshName, Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                if (mesh.Name != MeshName)
                    continue;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (CopyEffect)
                        toSet = effect.Clone();

                    MeshTag tag = ((MeshTag)part.Tag);

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
            foreach (ModelMesh mesh in Model.Meshes)
            {
                if (mesh.Name != MeshName)
                    continue;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    ((MeshTag)meshPart.Tag).Material = material;
            }
        }

        private void generateTags()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                            effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        // Store references to all of the model's current effects
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;

        }

        // Restore the effects referenced by the model's cache
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }
        public void SetClipPlane(Vector4? Plane)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect.Parameters["ClipPlaneEnabled"] != null)
                        part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

                    if (Plane.HasValue)
                        if (part.Effect.Parameters["ClipPlane"] != null)
                            part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
                }
        }

        public void Draw(Camera.Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart subMesh in mesh.MeshParts)
                {
                    Effect effect = subMesh.Effect;
                    //this pass uses the light accumulation texture and reconstruct the mesh's shading
                    //our parameters were already filled in the first pass
                    effect.CurrentTechnique = effect.Techniques[1];

                    effect.Parameters["LightBuffer"].SetValue(lightBuffer);
                    effect.Parameters["LightBufferPixelSize"].SetValue(new Vector2(0.5f / lightBuffer.Width, 0.5f / lightBuffer.Height));

                    effect.Parameters["World"].SetValue(Transform);
                    effect.Parameters["WorldView"].SetValue(Transform * camera.View);
                    effect.Parameters["WorldViewProjection"].SetValue(Transform * camera.View * camera.Projection);
                    effect.Parameters["AmbientColor"].SetValue(AmbientColor);
                    effect.Parameters["LightColor"].SetValue(LightColor);

                    if (((MeshTag)subMesh.Tag) != null)
                        if (((MeshTag)subMesh.Tag).Texture != null)
                        {
                            effect.Parameters["TextureEnabled"].SetValue(true);
                            effect.Parameters["Texture"].SetValue(((MeshTag)subMesh.Tag).Texture);
                        }
                        else effect.Parameters["TextureEnabled"].SetValue(false);
                    
                    effect.CurrentTechnique.Passes[0].Apply();

                    graphicsDevice.SetVertexBuffer(subMesh.VertexBuffer, subMesh.VertexOffset);
                    graphicsDevice.Indices = subMesh.IndexBuffer;

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumVertices, subMesh.StartIndex, subMesh.PrimitiveCount);
                }

            }
        }

        public void RenderToGBuffer(Camera.Camera camera, GraphicsDevice graphicsDevice)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart subMesh in mesh.MeshParts)
                {
                    Effect effect = subMesh.Effect;
                    effect.CurrentTechnique = effect.Techniques[0];
                    //our first pass is responsible for rendering into GBuffer
                    effect.Parameters["World"].SetValue(Transform);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                    effect.Parameters["WorldView"].SetValue(Transform * camera.View);
                    effect.Parameters["WorldViewProjection"].SetValue(Transform * camera.View * camera.Projection);
                    effect.Parameters["FarClip"].SetValue(camera.FarPlane);
                    effect.CurrentTechnique.Passes[0].Apply();

                    graphicsDevice.SetVertexBuffer(subMesh.VertexBuffer, subMesh.VertexOffset);
                    graphicsDevice.Indices = subMesh.IndexBuffer;

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumVertices, subMesh.StartIndex, subMesh.PrimitiveCount);
                }
            }
        }

        public void RenderShadowMap(ref Matrix viewProj, GraphicsDevice graphicsDevice)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart subMesh in mesh.MeshParts)
                {
                    Effect effect = subMesh.Effect;
                    //render to shadow map
                    effect.CurrentTechnique = effect.Techniques[2];
                    effect.Parameters["World"].SetValue(Transform);
                    effect.Parameters["LightViewProj"].SetValue(viewProj);
                    effect.CurrentTechnique.Passes[0].Apply();
                    graphicsDevice.SetVertexBuffer(subMesh.VertexBuffer, subMesh.VertexOffset);
                    graphicsDevice.Indices = subMesh.IndexBuffer;

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumVertices, subMesh.StartIndex, subMesh.PrimitiveCount);
                }
            }
        }

    }


    public class MeshTag
    {
        public Vector3 Color;
        public Texture2D Texture;
        public float SpecularPower;
        public Effect CachedEffect = null;

        public Material Material = new Material();

        public MeshTag(Vector3 Color, Texture2D Texture, float SpecularPower)
        {
            this.Color = Color;
            this.Texture = Texture;
            this.SpecularPower = SpecularPower;
        }
    }

    public class MeshRenderer
    {
        private Model _model;

        private ModelMeshPart meshPart;

        public MeshRenderer(Model model)
        {
            _model = model;
            meshPart = _model.Meshes[0].MeshParts[0];
        }

        public void RenderMesh(GraphicsDevice graphicsDevice)
        {        
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                            meshPart.NumVertices,
                                            meshPart.StartIndex,
                                            meshPart.PrimitiveCount);
        }
        public void BindMesh(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
            graphicsDevice.Indices = meshPart.IndexBuffer;
        }
    }
    public class QuadRenderer
    {
        //buffers for rendering the quad
        private VertexPositionTexture[] _vertexBuffer = null;
        private short[] _indexBuffer = null;

        public QuadRenderer()
        {
            _vertexBuffer = new VertexPositionTexture[4];
            _vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0));
            _vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
            _vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1));
            _vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1));

            _indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(GraphicsDevice graphicsDevice, Vector2 v1, Vector2 v2)
        {
            _vertexBuffer[0].Position.X = v1.X;
            _vertexBuffer[0].Position.Y = v2.Y;

            _vertexBuffer[1].Position.X = v2.X;
            _vertexBuffer[1].Position.Y = v2.Y;

            _vertexBuffer[2].Position.X = v1.X;
            _vertexBuffer[2].Position.Y = v1.Y;

            _vertexBuffer[3].Position.X = v2.X;
            _vertexBuffer[3].Position.Y = v1.Y;

            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, _vertexBuffer, 0, 4, _indexBuffer, 0, 2);
        }

    }
}