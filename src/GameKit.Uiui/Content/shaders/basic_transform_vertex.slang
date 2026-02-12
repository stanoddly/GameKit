ConstantBuffer<float4x4> viewProjection : register(b0, space1);
ConstantBuffer<float4x4> world : register(b1, space1);

struct Input
{
    float3 Position : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
};

struct Output
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;
    float4 worldPos = mul(world, float4(input.Position, 1.0));
    output.Position = mul(viewProjection, worldPos);
    output.TexCoord = input.TexCoord;
    return output;
}
