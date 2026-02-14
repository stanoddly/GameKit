ConstantBuffer<float4> uvRect : register(b0, space3);
ConstantBuffer<float4> tintColor : register(b1, space3);
Texture2D<float4> texture : register(t0, space2);
SamplerState textureSampler : register(s0, space2);

struct Input
{
    float2 TexCoord : TEXCOORD0;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    float2 uv = uvRect.xy + input.TexCoord * (uvRect.zw - uvRect.xy);
    float4 texColor = texture.Sample(textureSampler, uv);
    float4 color = texColor * tintColor;
    return float4(color.rgb * color.a, color.a);
}
