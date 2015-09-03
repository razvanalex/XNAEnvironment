float4x4 View;
float4x4 Projection;

float3 LightDirection = float3(1, -1, 0);
float3 LightColor = float3(1, 1, 1);
float3 AmbientColor = float3(-0.2, -0.2, -0.2);
float WaterHeigh = 600;
float4  WaterColor = float4(0.10980f, 0.30196f, 0.49412f, 1.0f);

#define NoOfTexture 4
float TextureTiling[NoOfTexture];

float4 ClipPlane;
bool ClipPlaneEnabled = false;

float3 CameraPosition;

texture RTexture;
sampler RTextureSampler = sampler_state {
	texture = <RTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture GTexture;
sampler GTextureSampler = sampler_state {
	texture = <GTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture BTexture;
sampler BTextureSampler = sampler_state {
	texture = <BTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture BaseTexture;
sampler BaseTextureSampler = sampler_state {
	texture = <BaseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

texture WeightMap;
sampler WeightMapSampler = sampler_state {
	texture = <WeightMap>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
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
	float Depth : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float Height : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, mul(View, Projection));
	output.Normal = input.Normal;
	output.UV = input.UV;
	output.Depth = output.Position.z;
	output.WorldPosition = input.Position;
	output.Height = input.Position.y;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
	clip(dot(float4(input.WorldPosition, 1), ClipPlane));

	float3 light = AmbientColor;
	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);
	light = clamp(light + 0.4f, 0, 1);
	light += saturate(dot(lightDir, normal)) * LightColor;
	
	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling[0]);
	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling[1]);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling[2]);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling[3]);

	float3 weightMap = tex2D(WeightMapSampler, input.UV);

	float3 output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1);
	output *= base;

	output += weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;
	
	float bBlendDist;
	float bBlendWidth;

	float4 waterColor = WaterColor;
	float detailDistance = DetailDistance;
	float sunFactor = 4; //4  7

	if (CameraPosition.y > WaterHeigh)
	{
		if (input.Height < WaterHeigh && input.WorldPosition.y < WaterHeigh)
		{
			detailDistance = 500;

			float BlendDist = 500;
			float BlendWidth = 600;
	
			bBlendDist = 400;
			bBlendWidth = 600;
		
			//Gradient for XY plane
			float dBlendDist = 2500;
			float dBlendWidth = 5000;

			//Gradient for Height
			float hBlendDist = 5000;
			float hBlendWidth = 10000;
			
			//Gradient for Time
			float Gtime = saturate(1-light);

			float BlendFactor = saturate((input.WorldPosition.y - BlendDist) / (BlendWidth - BlendDist));
			float bBlendFactor = saturate(((input.WorldPosition.y - bBlendDist) / (bBlendWidth - bBlendDist)));
			float dBlendFactor = saturate((pow((pow((input.WorldPosition.z - CameraPosition.z), 2) + pow((input.WorldPosition.x - CameraPosition.x), 2)), 0.5) - dBlendDist) / (dBlendWidth - dBlendDist));
			float hBlendFactor = saturate((input.Depth - hBlendDist) / (hBlendWidth - hBlendDist));
			
			waterColor = lerp(waterColor, waterColor * sunFactor, bBlendFactor);

			output = lerp(waterColor, output * saturate(light), BlendFactor);
			output = lerp(output, WaterColor, dBlendFactor);
			output = lerp(output, WaterColor, hBlendFactor);
			output = lerp(output, WaterColor, Gtime);
		}
		else output *= saturate(light);
	}
	if (CameraPosition.y < WaterHeigh)
	{
		if (input.WorldPosition.y < WaterHeigh)
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
			output = lerp(WaterColor / saturate(light), output, dBlendFactor) * saturate(light);

		}
		else output *= saturate(light);
	}

	float3 detail = tex2D(DetailSampler, input.UV * DetailTextureTiling);
	float detailAmt = input.Depth / detailDistance;
	detail = lerp(detail, 1, clamp(detailAmt, 0, 1));
	
	return float4(detail * output, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}