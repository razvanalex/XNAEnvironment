/*float4x4 World;
float4x4 View;
float4x4 Projection;

sampler2D tex[1];
float2 Offsets[15];
float Weights[15];


struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(float4 Position : POSITION0,
	float2 UV : TEXCOORD0) : COLOR0
{
	float4 output = float4(0, 0, 0, 1);
	for (int i = 0; i < 15; i++)
		output += tex2D(tex[0], UV + Offsets[i]) * Weights[i];
	return output;
}
technique Technique1
{
	pass p0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
// The texture to blur
texture ScreenTexture;

sampler2D tex = sampler_state {
	texture = <ScreenTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

// Precalculated weights and offsets
float weights[15] = { 0.1061154, 0.1028506, 0.1028506, 0.09364651, 0.09364651, 
	0.0801001, 0.0801001, 0.06436224, 0.06436224, 0.04858317, 0.04858317, 
	0.03445063, 0.03445063, 0.02294906, 0.02294906 };

float offsets[15] = { 0, 0.00125, -0.00125, 0.002916667, -0.002916667, 
	0.004583334, -0.004583334, 0.00625, -0.00625, 0.007916667, -0.007916667, 
	0.009583334, -0.009583334, 0.01125, -0.01125 };

// Blurs the input image horizontally
float4 BlurHorizontal(float4 Position : POSITION0, 
	float2 UV : TEXCOORD0) : COLOR0
{
    float4 output = float4(0, 0, 0, 1);
    
	// Sample from the surrounding pixels using the precalculated
	// pixel offsets and color weights
    for (int i = 0; i < 15; i++)
		output += tex2D(tex, UV + float2(offsets[i], 0)) * weights[i];
		
	return output;
}

// Blurs the input image vertically
float4 BlurVertical(float4 Position : POSITION0, 
	float2 UV : TEXCOORD0) : COLOR0
{
    float4 output = float4(0, 0, 0, 1);
    
    for (int i = 0; i < 15; i++)
		output += tex2D(tex, UV + float2(0, offsets[i])) * weights[i];
		
	return output;
}

technique Technique1
{
    pass Horizontal
    {
        PixelShader = compile ps_2_0 BlurHorizontal();
    }

	pass Vertical
    {
        PixelShader = compile ps_2_0 BlurVertical();
    }
}*/

// The texture to blur
texture ScreenTexture;

sampler2D tex = sampler_state {
	texture = <ScreenTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

// Precalculated weights and offsets
float weights[15];// = { 0.1061154, 0.1028506, 0.1028506, 0.09364651, 0.09364651, 0.0801001, 0.0801001, 0.06436224, 0.06436224, 0.04858317, 0.04858317, 0.03445063, 0.03445063, 0.02294906, 0.02294906 };

float offsets[15];// = { 0, 0.00125, -0.00125, 0.002916667, -0.002916667, 0.004583334, -0.004583334, 0.00625, -0.00625, 0.007916667, -0.007916667, 0.009583334, -0.009583334, 0.01125, -0.01125 };

// Blurs the input image horizontally
float4 BlurHorizontal(float4 Position : POSITION0,
	float2 UV : TEXCOORD0) : COLOR0
{
	float4 output = float4(0, 0, 0, 1);

	// Sample from the surrounding pixels using the precalculated
	// pixel offsets and color weights
	for (int i = 0; i < 15; i++)
		output += tex2D(tex, UV + float2(offsets[i], 0)) * weights[i];

	return output;
}

// Blurs the input image vertically
float4 BlurVertical(float4 Position : POSITION0,
float2 UV : TEXCOORD0) : COLOR0
{
	float4 output = float4(0, 0, 0, 1);

	for (int i = 0; i < 15; i++)
		output += tex2D(tex, UV + float2(0, offsets[i])) * weights[i];

	return output;
}

technique Technique1
{
	pass Horizontal
	{
		PixelShader = compile ps_2_0 BlurHorizontal();
	}

	pass Vertical
	{
		PixelShader = compile ps_2_0 BlurVertical();
	}
}