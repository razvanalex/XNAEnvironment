float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

texture BasicTexture;

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

float4 ClipPlane;
bool ClipPlaneEnabled = false;

sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;
	MaxAnisotropy = 12;
	MinFilter = POINT; // Minification Filter
	MagFilter = POINT; // Magnification Filter
	MipFilter = POINT; // Mip-mapping
	AddressU = CLAMP; // Address Mode for U Coordinates
	AddressV = CLAMP; // Address Mode for V Coordinates
};

bool TextureEnabled = false;
float3 DiffuseColor = float3(1, 1, 1);
float3 AmbientColor = float3(0.1, 0.1, 0.1);
float3 LightDirection = float3(1, 1, 1);
float3 LightColor = float3(0.9, 0.9, 0.9);
float SpecularPower = 200;
float3 SpecularColor = float3(1, 1, 1);

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	output.WorldPosition = worldPosition;
	float4x4 viewProjection = mul(View, Projection);
	
	output.Position = mul(worldPosition, viewProjection);
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);
	output.ViewDirection = worldPosition - CameraPosition;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
	clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	// Start with diffuse color
	float3 color = DiffuseColor;

	float4 alpha = tex2D(BasicTextureSampler, input.UV);

	// Texture if necessary
	if (TextureEnabled)
		color *= tex2D(BasicTextureSampler, input.UV);
	// Start with ambient lighting
	float3 lighting = AmbientColor;
		float3 lightDir = normalize(LightDirection);
		float3 normal = normalize(input.Normal);
		// Add lambertian lighting
		lighting += saturate(dot(lightDir, normal)) * LightColor;
	float3 refl = reflect(lightDir, normal);
		float3 view = normalize(input.ViewDirection);
		// Add specular highlights
		lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;
	if (AlphaTest)
		clip((alpha.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	// Calculate final color
	float3 output = saturate(lighting) * color;
		return float4(output, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}



/*float4x4 World;
float4x4 View;
float4x4 Projection;

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

float3 LightDirection = float3(30, 200, 50);
float3 LightColor = float3(1, 1, 1);
float3 DiffuseColor = float3(1, 1, 1);
float3 AmbientColor = float3(1, 1, 1);

float SpecularPower = 32;
float3 SpecularColor = float3(1, 1, 1);

texture BasicTexture;
sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Anisotropic; // Magnification Filter
	MipFilter = None; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

bool TextureEnabled = false;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4x4 viewProjection = mul(View, Projection);
	output.Position = mul(worldPosition, viewProjection);
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 color = DiffuseColor;

	if (TextureEnabled)
		color *= tex2D(BasicTextureSampler, input.UV);
	float4 alpha = tex2D(BasicTextureSampler, input.UV);
		/*
	float3 color = DiffuseColor;
	color *= tex2D(BasicTextureSampler, input.UV);
	float3 lighting = AmbientColor;
	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);
	lighting += saturate(dot(lightDir, normal)) * LightColor;
	float3 output = saturate(lighting) * color;
	

	// Start with ambient lighting
	float3 lighting = AmbientColor;
	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);
	// Add lambertian lighting
	lighting += saturate(dot(lightDir, normal)) * LightColor;// +SpecularColor;
	//float3 refl = reflect(lightDir, normal);
	//float3 view = normalize(input.ViewDirection);
	// Add specular highlights
	//lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;
	
	// Calculate final color
	float3 output = saturate(lighting) * color;

	if (AlphaTest)
		clip((alpha.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	return float4(output, 1) +alpha / 20;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
*/