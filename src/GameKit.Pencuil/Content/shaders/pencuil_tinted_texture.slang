ConstantBuffer<float4x4> viewProjection : register(b0, space1);
ConstantBuffer<float4x4> world : register(b1, space1);
ConstantBuffer<float4> uvRect : register(b0, space3);
ConstantBuffer<float4> tintColor : register(b1, space3);
Texture2D<float4> texture : register(t0, space2);
SamplerState textureSampler : register(s0, space2);

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
    float4 worldPosition = mul(world, float4(input.Position, 1.0));
    output.Position = mul(viewProjection, worldPosition);
    output.TexCoord = input.TexCoord;
    return output;
}

[shader("fragment")]
float4 fragmentMain(VertexToFragment input) : SV_Target0
{
    float2 uv = uvRect.xy + input.TexCoord * (uvRect.zw - uvRect.xy);
    float4 textureColor = texture.Sample(textureSampler, uv);
    float4 color = textureColor * tintColor;
    return float4(color.rgb * color.a, color.a);
}
