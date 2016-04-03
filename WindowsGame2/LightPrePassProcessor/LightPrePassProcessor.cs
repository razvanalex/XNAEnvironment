#region File Description
//-----------------------------------------------------------------------------
//Based on  
//NormalMappingModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// http://create.msdn.com/en-US/education/catalog/sample/normal_mapping
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Changed by
// Jorge Adriano Luna 2011
// http://jcoluna.wordpress.com
//-----------------------------------------------------------------------------
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace LightPrePassProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// Its based heavily on the normal mapping sample available at
    /// http://create.msdn.com/en-US/education/catalog/sample/normal_mapping
    /// </summary>
    [ContentProcessor(DisplayName = "LightPrePass Model Processor")]
    public class LightPrePassProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            //we always want to generate tangent frames, as we use tangent space normal mapping
            GenerateTangentFrames = true;

            //merge transforms
            MeshHelper.TransformScene(input, input.Transform);
            input.Transform = Matrix.Identity;
            MergeTransforms(input);

            return base.Process(input, context);
        }

        private void MergeTransforms(NodeContent input)
        {
            if (input is MeshContent)
            {
                MeshContent mc = (MeshContent) input;
                MeshHelper.TransformScene(mc, mc.Transform);
                mc.Transform = Matrix.Identity;
                MeshHelper.OptimizeForCache(mc);
            }
            foreach (NodeContent c in input.Children)
            {
                MergeTransforms(c);
            }
        }

        protected override MaterialContent ConvertMaterial(MaterialContent material,
           ContentProcessorContext context)
        {
            EffectMaterialContent lppMaterial = new EffectMaterialContent();

            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            processorParameters["ColorKeyColor"] = this.ColorKeyColor;
            processorParameters["ColorKeyEnabled"] = this.ColorKeyEnabled;
            processorParameters["TextureFormat"] = this.TextureFormat;
            processorParameters["GenerateMipmaps"] = this.GenerateMipmaps;
            processorParameters["ResizeTexturesToPowerOfTwo"] = this.ResizeTexturesToPowerOfTwo;
            processorParameters["PremultiplyTextureAlpha"] = false;
            processorParameters["ColorKeyEnabled"] = false;

            lppMaterial.Effect = new ExternalReference<EffectContent>("shaders/LPPMainEffect.fx");
            lppMaterial.CompiledEffect = context.BuildAsset<EffectContent, CompiledEffectContent>(lppMaterial.Effect, "EffectProcessor");

            //extract the extra parameters
            ExtractDefines(lppMaterial, material, context);

            // copy the textures in the original material to the new normal mapping
            // material. this way the diffuse texture is preserved. The
            // PreprocessSceneHierarchy function has already added the normal map
            // texture to the Textures collection, so that will be copied as well.
            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
                in material.Textures)
            {
                lppMaterial.Textures.Add(texture.Key, texture.Value);
            }

            try
            {
                lppMaterial.OpaqueData.Add("DiffuseColor", new Vector4((Vector3)material.OpaqueData["DiffuseColor"], (float)material.OpaqueData["Alpha"]));
                lppMaterial.OpaqueData.Add("SpecularColor", material.OpaqueData["SpecularColor"]);
                lppMaterial.OpaqueData.Add("SpecularPower", material.OpaqueData["SpecularPower"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // and convert the material using the NormalMappingMaterialProcessor,
            // who has something special in store for the normal map.
            return context.Convert<MaterialContent, MaterialContent>
                (lppMaterial, typeof(LightPrePassMaterialProcessor).Name, processorParameters);
        }

        /// <summary>
        /// Extract any defines we need from the original material, like alphaMasked, fresnel, reflection, etc, and pass it into
        /// the opaque data
        /// </summary>
        /// <param name="lppMaterial"></param>
        /// <param name="material"></param>
        /// <param name="context"></param>
        private void ExtractDefines(EffectMaterialContent lppMaterial, MaterialContent material, ContentProcessorContext context)
        {
            string defines = "";

            if (material.OpaqueData.ContainsKey("alphaMasked") && material.OpaqueData["alphaMasked"].ToString() == "True")
            {
                context.Logger.LogMessage("Alpha masked material found");
                lppMaterial.OpaqueData.Add("AlphaReference", (float)material.OpaqueData["AlphaReference"]);
                defines += "ALPHA_MASKED;";
            }
         
            if (!String.IsNullOrEmpty(defines))
                lppMaterial.OpaqueData.Add("Defines", defines);

        }
    }
}