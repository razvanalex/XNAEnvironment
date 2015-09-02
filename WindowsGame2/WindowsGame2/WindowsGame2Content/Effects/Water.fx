float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4x4 ReflectedView;
float4x4 RefractedView;
texture ReflectionMap;
texture RefractionMap;

float3 LightDirection = float3(1, 1, 1);
float3 LightColor = float3(0.0f, 5.0f, 0);
float WaterHeigh = 600;

float3 BaseColor = float3(0.2, 0.2, 0.8);
float BaseColorAmount = 0.3f;
float4  WaterColor = float4(0.5f, 0.79f, 0.75f, 1.0f);
float4  WaterColor2 = float4(0.10980f, 0.30196f, 0.49412f, 1.0f);

float WaveLength;// = 0.6;
float WaveHeight;// = 0.2;
float Time = 0;
float WaveSpeed;// = 0.04f;
//float WaveSpeed1 = WaveSpeed;

/*LOW
sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
};
*/

sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	MinFilter = ANISOTROPIC;
	MaxAnisotropy = 12;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler2D refractionSampler = sampler_state {
	texture = <RefractionMap>;
	MinFilter = ANISOTROPIC;
	MaxAnisotropy = 12;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture WaterNormalMap;
sampler2D waterNormalSampler = sampler_state {
	texture = <WaterNormalMap>;
	MinFilter = ANISOTROPIC;
	MaxAnisotropy = 12;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture WaterNormalMap1;
sampler2D waterNormalSampler1 = sampler_state {
	texture = <WaterNormalMap1>;
	MinFilter = ANISOTROPIC;
	MaxAnisotropy = 12;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

#include "PPShared.vsi"

struct VertexShaderInput
{
	float4 Position : POSITION0;
	//water
	float2 UV : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 ReflectionPosition : TEXCOORD1;
	float2 NormalMapPosition : TEXCOORD2;
	float4 WorldPosition : TEXCOORD3;
	float4 RefractedPosition : TEXCOORD4;
	float2 NormalMapPosition1 : TEXCOORD5;
	float Depth : TEXCOORD6;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.NormalMapPosition = input.UV / WaveLength;
	output.NormalMapPosition.x -= Time * WaveSpeed;

//	output.NormalMapPosition.y = output.NormalMapPosition.x;

	output.NormalMapPosition1 = input.UV / WaveLength;
	output.NormalMapPosition1.y -= Time * WaveSpeed * 2;

	output.UV = input.UV;

	output.Position = mul(input.Position, mul(World, mul(View, Projection)));

	output.ReflectionPosition = mul(input.Position, mul(World, mul(ReflectedView, Projection)));
	output.RefractedPosition = mul(input.Position, mul(World, mul(RefractedView, Projection)));

	output.WorldPosition = mul(input.Position, World);

	output.Depth = -CameraPosition.y;
//	output.Position3D = mul(input.Position, mul(World, mul(View, Projection)));
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 reflectionUV = postProjToScreen(input.ReflectionPosition) + halfPixel();
	float2 refractedUV = postProjToScreen(input.RefractedPosition) + halfPixel();

	//float4 normal = tex2D(waterNormalSampler, input.NormalMapPosition) * 2 - 1;

	float3 normal0 = tex2D(waterNormalSampler, input.NormalMapPosition);
	float3 normal1 = tex2D(waterNormalSampler1, input.NormalMapPosition1);
	normal0.yz = normal0.zy;
	normal1.yz = normal1.zy;
	normal0 = normal0 * 2 - 1;
	normal1 = normal1 * 2 - 1;
	float3 normal = normalize(0.5f*(normal0 + normal1));

	float2 UVOffset = WaveHeight * normal.z;

	float4 reflection = tex2D(reflectionSampler, reflectionUV + UVOffset);
	float4 refraction = tex2D(refractionSampler, refractedUV + UVOffset);
	/*
	float FogNear = 100;
	float FogFar = 10;
	float4 FogColor = WaterColor;

	float fog = saturate((input.Position3D - FogNear) / (FogNear - FogFar));

	reflection = lerp(FogColor, reflection, fog);
	*/

	float3 viewDirection = normalize(CameraPosition - input.WorldPosition);
	float3 reflectionVector = normalize(reflect(viewDirection, normal.bgr));

	float sunPower = 150;
	float SunFactor = 0.5;
	
	if (CameraPosition.y < WaterHeigh)
	{
		sunPower = 15.0f;
		SunFactor = 2.5f;
	}
	
	float specular = pow(saturate(dot(reflectionVector, float3(-LightDirection.x, LightDirection.y, -LightDirection.z))), sunPower);

	float3 sunLight = SunFactor * specular * float4(LightColor, 1);

	float frasnelTerm = saturate(dot(viewDirection, normal.rgb));

	float4 color;
	color.a = 1;

	float BlendDist = -500;
	float BlendWidth = 100;

	//Gradient for XY plane
	float dBlendDist = 25;
	float dBlendWidth= 100;

	float dBlendFactor = saturate((pow((pow((input.WorldPosition.z - CameraPosition.z), 2) + pow((input.WorldPosition.x - CameraPosition.x), 2)), 0.5) - dBlendDist) / (dBlendWidth - dBlendDist));

	if (CameraPosition.y < WaterHeigh)
	{
		color.rgb = WaterColor*lerp(refraction, reflection, 0) + sunLight;
		color.rgb = lerp(color.rgb, WaterColor2.rgb, clamp(((input.Depth - BlendDist) / BlendWidth), 0, 1));
		color.rgb = lerp(color.rgb, WaterColor2.rgb, dBlendFactor);
	}
	else
	{
		color.rgb = lerp(WaterColor * reflection, WaterColor * refraction, frasnelTerm) + sunLight;// *float4(1.0f, 0.8f, 0.4f, 1.0f);		
	}

	return color;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}