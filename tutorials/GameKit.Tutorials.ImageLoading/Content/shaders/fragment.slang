Texture2D<float4> texture : register(t0, space2);
SamplerState textureSampler : register(s0, space2);

struct Input
{
    float2 TexCoord : TEXCOORD0;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    return texture.Sample(textureSampler, input.TexCoord);
}
