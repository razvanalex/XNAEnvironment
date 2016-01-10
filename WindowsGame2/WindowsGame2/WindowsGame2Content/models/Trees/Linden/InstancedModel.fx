// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

// This sample uses a simple Lambert lighting model.

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

bool TextureEnabled = false;
float3 DiffuseLight = float3(1, 1, 1);
float3 AmbientLight = float3(1, 1, 1);
float3 LightDirection = float3(0, -1, 0);
float3 LightColor = float3(1, 1, 1);
float SpecularPower = 1;
float3 SpecularColor = float3(1, 1, 1);

float2 Size;
float3 Up;
float3 Side;

float4 ClipPlane;
bool ClipPlaneEnabled = false;

texture Texture;
sampler Sampler = sampler_state
{
	Texture = (Texture);
	MaxAnisotropy = 12;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Anisotropic; // Magnification Filter
	MipFilter = Point; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

	// Apply the world and camera matrices to compute the output position.
	float4 position = input.Position;
	float2 offset = float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);
	float3 rotation = Size.x * offset.x * Side + Size.y * offset.y * Up;

	float4 worldPosition = mul(position, instanceTransform);
	output.WorldPosition = worldPosition;
	worldPosition.xyz += rotation;	
	float4 viewPosition = mul(worldPosition.xyzw, View);
	output.Position = mul(viewPosition, Projection);

	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);
	output.ViewDirection = worldPosition - CameraPosition;

	return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
	float4x4 instanceTransform : BLENDWEIGHT)
{
	return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}

// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Start with diffuse color
	float3 color = DiffuseLight;
	float4 alpha = tex2D(Sampler, input.UV);

	if (ClipPlaneEnabled)
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	// Texture if necessary
	if (!TextureEnabled)
		color *= tex2D(Sampler, input.UV);
	// Start with ambient lighting
	float3 lighting = AmbientLight;
		float3 lightDir = normalize(float3(LightDirection.x, -LightDirection.y, LightDirection.z));
		float3 normal = normalize(input.Normal);
		// Add lambertian lighting
		lighting = clamp(lighting + 0.4f, 0, 1);
	lighting += saturate(dot(lightDir, normal)) * LightColor;

	float3 refl = reflect(lightDir, normal);
		float3 view = normalize(input.ViewDirection);

		// Add specular highlights
		lighting += pow(saturate(dot(refl, view)), SpecularPower) * LightColor;
	if (AlphaTest)
		clip((alpha.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	// Calculate final color
	float3 output = saturate(lighting) * color;
		return float4(output, 1);
}


// Hardware instancing technique.
technique HardwareInstancing
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
