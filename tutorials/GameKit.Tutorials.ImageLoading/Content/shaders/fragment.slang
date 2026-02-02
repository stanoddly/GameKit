Texture2D<float4> texture : register(t0, space2);
SamplerState textureSampler : register(s0, space2);

struct Input
{
    float2 TexCoord : TEXCOORD0;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    float4 color = texture.Sample(textureSampler, input.TexCoord);
    // Premultiply RGB by alpha to handle "dirty" transparent pixels
    return float4(color.rgb * color.a, color.a);
}
