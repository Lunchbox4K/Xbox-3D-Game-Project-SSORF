//--------------------------------------------------------------------------
// Name: Tri Weighted Texture Effect
//
// Desc: Textures a Model/Mesh with a blended combonation of three Textures
//		weighted by the RGB calue of the Teaxture Weight Map Texture
//
//--------------------------------------------------------------------------
float4x4 mView;
float4x4 mProjection;
float4x4 mWorld;
float3 mLightDirection = normalize(float3(-1, -1, -1));
float3 mDiffuseLight = 1.25;
float3 mAmbientLight = 0.25;

Texture mTextureWeight;
sampler TextureSamplerW = sampler_state { texture = <mTextureWeight> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = WRAP; AddressV = WRAP;};
Texture mTextureR;
sampler TextureSamplerR = sampler_state { texture = <mTextureR> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = Mirror; AddressV = Mirror;};
Texture mTextureG;
sampler TextureSamplerG = sampler_state { texture = <mTextureG> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = WRAP; AddressV = WRAP;};
Texture mTextureB;
sampler TextureSamplerB = sampler_state { texture = <mTextureB> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = WRAP; AddressV = WRAP;};

struct PixelToFrame
{
    float4 Color        : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;    
    float3 Normal : NORMAL0;
    float2 TextureCoords : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords : TEXCOORD0)
{

    VertexShaderOutput Output = (VertexShaderOutput)0;
    float4x4 preViewProjection = mul (mView, mProjection);
    float4x4 preWorldViewProjection = mul (mWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.TextureCoords = inTexCoords;
    
	//Calculate Lighting
	float3 worldNormal =  mul((inNormal), mWorld);
	Output.Normal = worldNormal;

    return Output;
}

PixelToFrame PixelShaderFunction(VertexShaderOutput input)
{
    PixelToFrame Output = (PixelToFrame)0;    
 
	float diffuseAmount = max(-dot(input.Normal, mLightDirection), 0);
	float3 lightingResult = saturate(diffuseAmount * mDiffuseLight + mAmbientLight);

    Output.Color = tex2D(TextureSamplerR, input.TextureCoords*64)*tex2D(TextureSamplerW, input.TextureCoords).r;
    Output.Color += tex2D(TextureSamplerG, input.TextureCoords*64)*tex2D(TextureSamplerW, input.TextureCoords).g;
    Output.Color += tex2D(TextureSamplerB, input.TextureCoords*64)*tex2D(TextureSamplerW, input.TextureCoords).b;
	Output.Color *= float4(lightingResult, 1);
	return Output;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
