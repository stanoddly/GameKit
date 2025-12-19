Texture2DArray<float4> textureArray : register(t0, space2);
SamplerState textureSampler : register(s0, space2);

ConstantBuffer<float> layerIndex : register(b0, space3);

struct Input
{
    float2 TexCoord : TEXCOORD0;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    return textureArray.Sample(textureSampler, float3(input.TexCoord, layerIndex));
}
