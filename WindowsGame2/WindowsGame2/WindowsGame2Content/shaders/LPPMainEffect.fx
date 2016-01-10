//-----------------------------------------------------------------------------
// LPPMainEffect.fx
//
// Jorge Adriano Luna 2011
// http://jcoluna.wordpress.com
//
// It uses some code from Nomal Mapping Sample found at
// http://create.msdn.com/en-US/education/catalog/sample/normal_mapping
// and also code from here
// http://aras-p.info/texts/CompactNormalStorage.html
//-----------------------------------------------------------------------------


//-----------------------------------------
// Parameters
//-----------------------------------------
float4x4 World;
float4x4 WorldView;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;
float4x4 LightViewProj; //used when rendering to shadow map

float FarClip;
float2 LightBufferPixelSize;

//as we used a 0.1f scale when rendering to light buffer,
//revert it back here.
const static float LightBufferScaleInv = 10.0f;

//Light Color
float4 DiffuseColor = float4(1, 1, 1, 1);
float4 DefaultSpecular = float4(1, 1, 1, 1);
float4 EmissiveColor = float4(0, 0, 0, 1);
float4 NormalColor = float4(0.50196, 0.50196, 1, 0.05882);
float3 AmbientColor = float3(0.5, 0.5, 0.5);
float3 LightColor;

#ifdef ALPHA_MASKED
float AlphaReference;
#endif

//-----------------------------------------
// Textures
//-----------------------------------------
texture LightBuffer;
sampler2D lightSampler = sampler_state
{
	Texture = <LightBuffer>;
	MipFilter = POINT;
	MagFilter = POINT;
	MinFilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};
//-------------------------------
// Helper functions
//-------------------------------
half2 EncodeNormal (half3 n)
{
	float kScale = 1.7777;
	float2 enc;
	enc = n.xy / (n.z+1);
	enc /= kScale;
	enc = enc*0.5+0.5;
	return enc;
}

float2 PostProjectionSpaceToScreenSpace(float4 pos)
{
	float2 screenPos = pos.xy / pos.w;
	return (0.5f * (float2(screenPos.x, -screenPos.y) + 1));
}

half3 NormalMapToSpaceNormal(half3 normalMap, float3 normal, float3 binormal, float3 tangent)
{
	normalMap = normalMap * 2 - 1;
	normalMap = half3(normal * normalMap.z + normalMap.x * tangent - normalMap.y * binormal);
	return normalMap;
}	


//-------------------------------
// Textures and Alpha Test
//-------------------------------
bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

bool TextureEnabled = false;
texture Texture;
sampler2D Sampler = sampler_state
{
	Texture = (Texture);
	MaxAnisotropy = 12;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Anisotropic; // Magnification Filter
	MipFilter = Point; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

//-------------------------------
// Shaders
//-------------------------------

struct VertexShaderInput
{
    float4 Position  : POSITION0;
    float2 TexCoord  : TEXCOORD0;
    float3 Normal    : NORMAL0;    
    //float3 Binormal  : BINORMAL0;
    float4 Tangent   : TANGENT;
};


struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
    float Depth				: TEXCOORD1;
	
    float3 Normal	: TEXCOORD2;
    float3 Tangent	: TEXCOORD3;
	float3 Binormal : TEXCOORD4;
};

struct PixelShaderInput
{
    float4 Position			: POSITION0;
    float3 TexCoord			: TEXCOORD0;
    float Depth				: TEXCOORD1;

    float3 Normal	: TEXCOORD2;
    float3 Tangent	: TEXCOORD3;
    float3 Binormal : TEXCOORD4; 	

	//we need this to detect back bacing triangles
#ifdef ALPHA_MASKED	
	float Face : VFACE;
#endif
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float3 viewSpacePos = mul(input.Position, WorldView);
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord; //pass the texture coordinates further

	//we output our normals/tangents/binormals in viewspace
	output.Normal = normalize(mul(input.Normal,WorldView)); 
	output.Tangent =  normalize(mul(input.Tangent.xyz,WorldView)); 
	
	output.Binormal =  normalize(cross(output.Normal, output.Tangent)*input.Tangent.w);
		
	output.Depth = viewSpacePos.z; //pass depth
    return output;
}

VertexShaderOutput InstancedVertexShaderFunction(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

	float4x4 worldView = mul(instanceTransform, View);
	float4x4 worldViewProjection = mul(worldView, Projection);

	float3 viewSpacePos = mul(input.Position, worldView);
	output.Position = mul(input.Position, worldViewProjection);
	output.TexCoord = input.TexCoord; //pass the texture coordinates further

	//we output our normals/tangents/binormals in viewspace
	output.Normal = normalize(mul(input.Normal, worldView));
	output.Tangent = normalize(mul(input.Tangent.xyz, worldView));

	output.Binormal = normalize(cross(output.Normal, output.Tangent)*input.Tangent.w);

	output.Depth = viewSpacePos.z; //pass depth
	return output;
}

// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
	float4x4 instanceTransform : BLENDWEIGHT)
{
	return InstancedVertexShaderFunction(input, mul(World, transpose(instanceTransform)));
}


//render to our 2 render targets, normal and depth 
struct PixelShaderOutput
{
    float4 Normal : COLOR0;
    float4 Depth : COLOR1;
};

PixelShaderOutput PixelShaderFunction(PixelShaderInput input)
{
	PixelShaderOutput output = (PixelShaderOutput)1;   

	//if we are using alpha mask, we need to read the diffuse map	
#ifdef ALPHA_MASKED
	half4 diffuseMap = tex2D(diffuseMapSampler, input.TexCoord);
	clip(diffuseMap.a - AlphaReference);	
#endif

	//read from our normal map
	half4 NormalMap = (half4)(NormalColor);
	half3 normalViewSpace = NormalMapToSpaceNormal(NormalMap.xyz, input.Normal, input.Binormal, input.Tangent);
    
	//if we are using alpha mask, we need to invert the normal if its a back face
#ifdef ALPHA_MASKED	
	normalViewSpace = normalViewSpace * sign(input.Face);
#endif

	output.Normal.rg =  EncodeNormal (normalize(normalViewSpace));	//our encoder output in RG channels
	output.Normal.b = NormalMap.a;			//our specular power goes into B channel
	output.Normal.a = 1;					//not used
	output.Depth.x = -input.Depth/ FarClip;		//output Depth in linear space, [0..1]
	
	return output;
} 

struct ReconstructVertexShaderInput
{
    float4 Position  : POSITION0;
    float2 TexCoord  : TEXCOORD0;
};


struct ReconstructVertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
	float4 TexCoordScreenSpace : TEXCOORD1;
};

ReconstructVertexShaderOutput ReconstructVertexShaderFunction(ReconstructVertexShaderInput input)
{
    ReconstructVertexShaderOutput output;
	
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord; //pass the texture coordinates further
	output.TexCoordScreenSpace = output.Position;

    return output;
}

ReconstructVertexShaderOutput HardwareReconstructVertexShaderFunction(ReconstructVertexShaderInput input, float4x4 instanceTransform)
{
	ReconstructVertexShaderOutput output;

	float4x4 worldView = mul(instanceTransform, View);
	float4x4 worldViewProjection = mul(worldView, Projection);

	output.Position = mul(input.Position, worldViewProjection);
	output.TexCoord = input.TexCoord; //pass the texture coordinates further
	output.TexCoordScreenSpace = output.Position;
	
	return output;
}

ReconstructVertexShaderOutput HardwareInstancingReconstructVertexShaderFunction(ReconstructVertexShaderInput input,
	float4x4 instanceTransform : BLENDWEIGHT)
{
	return HardwareReconstructVertexShaderFunction(input, mul(World, transpose(instanceTransform)));
}


float4 ReconstructPixelShaderFunction(ReconstructVertexShaderOutput input):COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)1;   
	// Find the screen space texture coordinate and offset it
	float2 screenPos = PostProjectionSpaceToScreenSpace(input.TexCoordScreenSpace) + LightBufferPixelSize;

	//read our light buffer texture. Remember to multiply by our magic constant explained on the blog
	float4 lightColor =  tex2D(lightSampler, screenPos) * LightBufferScaleInv;

	//our specular intensity is stored in alpha. We reconstruct the specular here, using a cheap and NOT accurate trick
	float3 specular = lightColor.rgb*lightColor.a;
	//return float4(lightColor.aaa,1);
	float4 finalColor = float4(DiffuseColor*lightColor.rgb + specular*DefaultSpecular + EmissiveColor, 1);
	//add a small constant to avoid dark areas
	finalColor.rgb += DiffuseColor * 0.2f;
	return finalColor;
}
float4 ReconstructPixelShaderFunctionInstance(ReconstructVertexShaderOutput input) :COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)1;
	// Find the screen space texture coordinate and offset it
	float2 screenPos = PostProjectionSpaceToScreenSpace(input.TexCoordScreenSpace) + LightBufferPixelSize;

		//read our light buffer texture. Remember to multiply by our magic constant explained on the blog
		float4 lightColor = tex2D(lightSampler, screenPos) * LightBufferScaleInv;

		//our specular intensity is stored in alpha. We reconstruct the specular here, using a cheap and NOT accurate trick
		float3 specular = lightColor.rgb*lightColor.a;
		//return float4(lightColor.aaa,1);
		float3 AmbientLight = AmbientColor;
		float3 finalColor = AmbientLight * 2;
		finalColor += (DiffuseColor * lightColor.rgb + specular*DefaultSpecular + EmissiveColor) * 2;
		//add a small constant to avoid dark areas
		//finalColor += AmbientLight;

	// Texture if necessary
#if (TextureEnabled == true)
	float4 alpha = tex2D(Sampler, input.TexCoord);

	if (TextureEnabled)
		finalColor *= alpha;

	if (AlphaTest == true)
		clip((alpha.a - AlphaTestValue));
#endif
		
	return float4(finalColor, 1);
}
struct ShadowMapVertexShaderInput
{
    float4 Position : POSITION0;	
	//if we have alpha mask, we need to use the tex coord
#ifdef ALPHA_MASKED	
    float2 TexCoord  : TEXCOORD0;
#endif
};

struct ShadowMapVertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
#ifdef ALPHA_MASKED	
    float2 TexCoord  : TEXCOORD1;
#endif
};

struct ShadowMapVertexShaderInputInstance
{
	float4 Position : POSITION0;
	//if we have alpha mask, we need to use the tex coord
#if (TextureEnabled == true)	
	float2 TexCoord  : TEXCOORD0;
#endif
};

struct ShadowMapVertexShaderOutputInstance
{
	float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
#if (TextureEnabled == true)	
	float2 TexCoord  : TEXCOORD1;
#endif
};


ShadowMapVertexShaderOutput OutputShadowVertexShaderFunction(ShadowMapVertexShaderInput input)
{
    ShadowMapVertexShaderOutput output = (ShadowMapVertexShaderOutput)0;
	
    float4 clipPos = mul(input.Position, mul(World, LightViewProj));
	//clamp to the near plane
	clipPos.z = max(clipPos.z,0);
	
	output.Position = clipPos;
	output.Depth = output.Position.zw;
	
#ifdef ALPHA_MASKED	
    output.TexCoord = input.TexCoord; //pass the texture coordinates further	
#endif
    return output;
}


ShadowMapVertexShaderOutputInstance HardwareOutputShadowVertexShaderFunction(ShadowMapVertexShaderInputInstance input, float4x4 instanceTransform)
{
	ShadowMapVertexShaderOutputInstance output = (ShadowMapVertexShaderOutputInstance)0;
	
	float4 clipPos = mul(input.Position, mul(instanceTransform, LightViewProj));
	//clamp to the near plane
	clipPos.z = max(clipPos.z, 0);

	output.Position = clipPos;
	output.Depth = output.Position.zw;

#if (TextureEnabled == true)
	output.TexCoord = input.TexCoord;
#endif
	return output;
}

ShadowMapVertexShaderOutputInstance HardwareInstancingOutputShadowVertexShaderFunction(ShadowMapVertexShaderInputInstance input,
	float4x4 instanceTransform : BLENDWEIGHT)
{
	return HardwareOutputShadowVertexShaderFunction(input, mul(World, transpose(instanceTransform)));
}


float4 OutputShadowPixelShaderFunction(ShadowMapVertexShaderOutput input) : COLOR0
{
	float depth = input.Depth.x / input.Depth.y;	
    return float4(depth, 1, 1, 1) ; 
}

float4 OutputShadowPixelShaderFunctionInstance(ShadowMapVertexShaderOutputInstance input) : COLOR0
{
#ifdef ALPHA_MASKED	
	//read our diffuse
	//half4 diffuseMap = tex2D(diffuseMapSampler, input.TexCoord);
	//clip(diffuseMap.a - AlphaReference);
#endif

	// Texture if necessary
#if (TextureEnabled == true)
	float4 alpha = tex2D(Sampler, input.TexCoord);
	if (AlphaTest == true)
		clip((alpha.a - AlphaTestValue));
#endif

	float depth = input.Depth.x / input.Depth.y;
	return float4(depth, 1, 1, 1);
}

PixelShaderOutput PixelShaderFunctionInstance(PixelShaderInput input)
{
	PixelShaderOutput output = (PixelShaderOutput)1;

	//if we are using alpha mask, we need to read the diffuse map	
#ifdef ALPHA_MASKED
	half4 diffuseMap = tex2D(diffuseMapSampler, input.TexCoord);
		clip(diffuseMap.a - AlphaReference);
#endif

	// Texture if necessary
#if (TextureEnabled == true)
	float4 alpha = tex2D(Sampler, input.TexCoord);
		if (AlphaTest == true)
			clip((alpha.a - AlphaTestValue));
#endif

	//read from our normal map
	half4 NormalMap = (half4)(NormalColor);
		half3 normalViewSpace = NormalMapToSpaceNormal(NormalMap.xyz, input.Normal, input.Binormal, input.Tangent);

		//if we are using alpha mask, we need to invert the normal if its a back face
#ifdef ALPHA_MASKED	
		normalViewSpace = normalViewSpace * sign(input.Face);
#endif

	output.Normal.rg = EncodeNormal(normalize(normalViewSpace));	//our encoder output in RG channels
	output.Normal.b = NormalMap.a;			//our specular power goes into B channel
	output.Normal.a = 1;					//not used
	output.Depth.x = -input.Depth / FarClip;		//output Depth in linear space, [0..1]

	return output;
}
//-----------------------------------------
// Terrain Effect
//-----------------------------------------

float WaterHeight = 600;
float3 WaterColor = float3(0.10980f, 0.30196f, 0.49412f);

const static int NoOfTextures = 6;
float TextureTiling[NoOfTextures];

float4 ClipPlane;
bool ClipPlaneEnabled = false;

float3 CameraPosition;

texture Texture1, Texture2, Texture3, Texture4, Texture5, Texture6;

sampler TextureSampler[NoOfTextures] = {
	sampler_state {
		texture = <Texture1>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	},
		sampler_state {
		texture = <Texture2>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	},
		sampler_state {
		texture = <Texture3>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	},
		sampler_state {
		texture = <Texture4>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	},
		sampler_state {
		texture = <Texture5>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	},
		sampler_state {
		texture = <Texture6>;
		AddressU = Wrap;
		AddressV = Wrap;
		MinFilter = Anisotropic;
		MagFilter = Anisotropic;
		MipFilter = LINEAR;
	}
};

texture TexturesMaps1, TexturesMaps2, TexturesMaps3, TexturesMaps4, TexturesMaps5, TexturesMaps6;
sampler TexturesMapSampler[NoOfTextures] = {
	sampler_state {
		texture = <TexturesMaps1>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	},
		sampler_state {
		texture = <TexturesMaps2>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	},
		sampler_state {
		texture = <TexturesMaps3>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	},
		sampler_state {
		texture = <TexturesMaps4>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	},
		sampler_state {
		texture = <TexturesMaps5>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	},
		sampler_state {
		texture = <TexturesMaps6>;
		AddressU = Clamp;
		AddressV = Clamp;
		MinFilter = Linear;
		MagFilter = Linear;
	}
};

float DetailTextureTiling;
float DetailDistance = 2500;

texture DetailTexture;
sampler DetailSampler = sampler_state {
	texture = <DetailTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
};
struct TerrainVertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct TerrainVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float Height : TEXCOORD4;
	float4 TexCoordScreenSpace : TEXCOORD5;
};

TerrainVertexShaderOutput TerrainVertexShaderFunction(TerrainVertexShaderInput input)
{
	TerrainVertexShaderOutput output;

	output.Position = mul(input.Position, mul(World, mul(View, Projection)));
	output.Normal = input.Normal;
	output.UV = input.UV;
	output.Depth = output.Position.z;
	output.WorldPosition = mul(input.Position, World);
	output.Height = input.Position.y;	
	output.TexCoordScreenSpace = output.Position;
	return output;
}
float4 TerrainPixelShaderFunction(TerrainVertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float3 Tex[NoOfTextures], TexMaps[NoOfTextures];
		
	for (int i = 0; i < NoOfTextures; i++)
	{
		Tex[i] = tex2D(TextureSampler[i], input.UV * TextureTiling[i]);
		TexMaps[i] = tex2D(TexturesMapSampler[i], input.UV);
	}
	float dif = 1.0f;
	for (int i = 0; i < NoOfTextures; i++)
		dif -= TexMaps[i].r;
	float3 output = saturate(dif);
	for (int i = 0; i < NoOfTextures; i++)
		output += TexMaps[i].r * Tex[i];

	float3 light = AmbientColor;
	light += LightColor;
	output *= light;
	
	// Find the screen space texture coordinate and offset it
	float2 screenPos = PostProjectionSpaceToScreenSpace(input.TexCoordScreenSpace) + LightBufferPixelSize;

	//read our light buffer texture. Remember to multiply by our magic constant explained on the blog
	float4 lightColor = tex2D(lightSampler, screenPos) * LightBufferScaleInv;

	//our specular intensity is stored in alpha. We reconstruct the specular here, using a cheap and NOT accurate trick
	float3 specular = lightColor.rgb*lightColor.a;
	//return float4(lightColor.aaa,1);
	float4 finalColor = float4(DiffuseColor*lightColor.rgb + specular*DefaultSpecular + EmissiveColor, 1);
	//add a small constant to avoid dark areas
	finalColor.rgb += DiffuseColor*0.1f;
	
	float bBlendDist;
	float bBlendWidth;

	float3 waterColor = WaterColor;
	float detailDistance = DetailDistance;
	float sunFactor = 4; //4  7

	if (CameraPosition.y > WaterHeight)
	{
		if (input.Height < WaterHeight && input.WorldPosition.y < WaterHeight)
		{
			detailDistance = 500;

			float BlendDist = WaterHeight - 100;
			float BlendWidth = WaterHeight;

			bBlendDist = WaterHeight - 200;
			bBlendWidth = WaterHeight;

			//Gradient for XY plane
			float dBlendDist = 2500;
			float dBlendWidth = 5000;

			//Gradient for Height
			float hBlendDist = 5000;
			float hBlendWidth = 10000;

			//Gradient for Time
			//float Gtime = normalize(1 - light.r);

			float BlendFactor = saturate((input.WorldPosition.y - BlendDist) / (BlendWidth - BlendDist));
			float bBlendFactor = saturate(((input.WorldPosition.y - bBlendDist) / (bBlendWidth - bBlendDist)));
			float dBlendFactor = saturate((pow((pow((input.WorldPosition.z - CameraPosition.z), 2) + pow((input.WorldPosition.x - CameraPosition.x), 2)), 0.5) - dBlendDist) / (dBlendWidth - dBlendDist));
			float hBlendFactor = saturate((input.Depth - hBlendDist) / (hBlendWidth - hBlendDist));

			waterColor = lerp(waterColor, waterColor * sunFactor, bBlendFactor);

			output = lerp(waterColor, output * light, BlendFactor);
			output = lerp(output, WaterColor, dBlendFactor);
			output = lerp(output, WaterColor, hBlendFactor);
		//	output = lerp(output, WaterColor, Gtime);	
		}
	}
	else if (CameraPosition.y < WaterHeight)
	{
		if (input.WorldPosition.y < WaterHeight)
		{
			bBlendDist = 400;
			bBlendWidth = 200;
			float dBlendDist = 100;
			float dBlendWidth = 50;
			detailDistance = 200;
			float bBlendFactor = saturate((input.WorldPosition.y - bBlendDist) / bBlendWidth);
			float dBlendFactor = saturate((input.Depth - dBlendDist) / (dBlendWidth - dBlendDist));

			waterColor = lerp(waterColor, saturate(waterColor * sunFactor), bBlendFactor);
			light = WaterColor;
			output = lerp(WaterColor / normalize(light), output, dBlendFactor) * normalize(light);
		}
	}
	
	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
		float detailAmt = input.Depth / detailDistance;
	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));
	
	return float4(output, 1) *finalColor;
}



technique RenderToGBuffer
{
    pass RenderToGBufferPass
    {
	#ifdef ALPHA_MASKED	
		CullMode = None;
	#else
		CullMode = CCW;
	#endif

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique ReconstructShading
{
	pass ReconstructShadingPass
    {
	#ifdef ALPHA_MASKED	
		CullMode = None;
	#else
		CullMode = CCW;
	#endif

        VertexShader = compile vs_3_0 ReconstructVertexShaderFunction();
        PixelShader = compile ps_3_0 ReconstructPixelShaderFunction();
    }
}

technique OutputShadow
{
	pass OutputShadowPass
	{		
	#ifdef ALPHA_MASKED	
		CullMode = None;
	#else
		CullMode = CCW;
	#endif

        VertexShader = compile vs_3_0 OutputShadowVertexShaderFunction();
        PixelShader = compile ps_3_0 OutputShadowPixelShaderFunction();
	}
}

technique RenderTerrain
{
	pass TerrainPass
	{
		VertexShader = compile vs_3_0 TerrainVertexShaderFunction();
		PixelShader = compile ps_3_0 TerrainPixelShaderFunction();
	}
}

technique HardwareInstancing
{
	pass RenderToGBufferPass
	{
		CullMode = None;
		VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunctionInstance();
	}
}


technique ReconstructShadingInstancing
{
	pass ReconstructShadingPass
	{
		CullMode = None;
		VertexShader = compile vs_3_0 HardwareInstancingReconstructVertexShaderFunction();
		PixelShader = compile ps_3_0 ReconstructPixelShaderFunctionInstance();
	}
}

technique OutputShadowInstancing
{
	pass OutputShadowPass
	{
		CullMode = None;
		VertexShader = compile vs_3_0 HardwareInstancingOutputShadowVertexShaderFunction();
		PixelShader = compile ps_3_0 OutputShadowPixelShaderFunctionInstance();
	}
}