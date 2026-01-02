StructuredBuffer<float4> colors : register(t0, space2);

ConstantBuffer<int> colorIndex : register(b0, space3);

[shader("fragment")]
float4 main() : SV_Target0
{
    return colors[colorIndex];
}
