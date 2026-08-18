Texture2DArray<float4> textureArray : register(t0, space2);
SamplerState textureSampler : register(s0, space2);
ConstantBuffer<float> layerIndex : register(b0, space3);

struct VertexInput
{
    float3 Position : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
};

struct VertexToFragment
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

[shader("vertex")]
VertexToFragment vertexMain(VertexInput input)
{
    VertexToFragment output;
    output.Position = float4(input.Position, 1.0);
    output.TexCoord = input.TexCoord;
    return output;
}

[shader("fragment")]
float4 fragmentMain(VertexToFragment input) : SV_Target0
{
    return textureArray.Sample(textureSampler, float3(input.TexCoord, layerIndex));
}
