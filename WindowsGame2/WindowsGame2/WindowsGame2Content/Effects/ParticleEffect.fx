float4x4 World;
float4x4 View;
float4x4 Projection;

texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};

float Time;
float Lifespan;
float3 Wind;
float3 Up;
float3 Side;
float FadeInTime;
float2 Size;
float G = 9.8f;
float3 UpVector;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Direction : TEXCOORD1;
	float Speed : TEXCOORD2;
	float StartTime : TEXCOORD3;
	float2 ParticleScale : TEXCOORD4;
	float2 ParticleMass : TEXCOORD5;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float2 RelativeTime : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;
	
	// Determine how long this particle has been alive
	float relativeTime = (Time - input.StartTime);
	output.RelativeTime = relativeTime;

	float3 position = input.Position;
	float2 scale = Size;
	// Move to billboard corner

	float2 offset = scale * float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);
	offset *= input.ParticleScale * relativeTime + 1;

	position += offset.x * Side + offset.y * Up;

	float acceleration = G * input.ParticleMass;

	// Move the vertex along its movement direction and the wind direction
	position += (input.Direction * input.Speed + Wind) * relativeTime - UpVector * acceleration * relativeTime * relativeTime / 2;

	float4 worldPosition = mul(float4(position, 1), instanceTransform);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	// Transform the final position by the view and projection matrices
	//output.Position = mul(mul(float4(position, 1), instanceTransform), mul(View, Projection));

	output.UV = input.UV;

	return output;
}

VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return VertexShaderFunction(input, mul(World, transpose(instanceTransform)));
}

VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
	return VertexShaderFunction(input, float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Ignore particles that aren't active
	clip(input.RelativeTime);

	// Sample texture
	float4 color = tex2D(texSampler, input.UV);

	// Fade out towards end of life
	float d = clamp(1.0f - pow((input.RelativeTime / Lifespan), 10), 0, 1);

	// Fade in at beginning of life
	d *= clamp((input.RelativeTime / FadeInTime), 0, 1);

	// Return color * fade amount
	return float4(color * d);
}

technique HardwareInstancing
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}

technique Technique01
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 NoInstancingVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}