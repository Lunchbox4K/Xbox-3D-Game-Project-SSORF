// Camera settings.
float4x4 mWorld;
float4x4 mView;
float4x4 mProjection;


// Simple Lambert Light
float3 mLightDirection = normalize(float3(-1, -1, -1));
float3 mDiffuseLight = 1.25;
float3 mAmbientLight = 0.25;

Texture mTexture;
sampler Sampler = sampler_state { texture = <mTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = WRAP; AddressV = WRAP;};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};


struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, mView);
    output.Position = mul(viewPosition, mProjection);

    //Calculate Lighting
	float3 worldNormal = mul(input.Normal, instanceTransform);
	float diffuseAmount = max(-dot(worldNormal, mLightDirection), 0);
	float3 lightingResult = saturate(diffuseAmount * mDiffuseLight + mAmbientLight);
	output.Color = float4(lightingResult, 1);
	output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, 
											      float4x4 instanceTransform : BLENDWEIGHT)
{
	return VertexShaderFunction(input, mul(mWorld, transpose(instanceTransform)));
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
	//return input.Color;
}

// Hardware instancing technique.
technique HwInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
