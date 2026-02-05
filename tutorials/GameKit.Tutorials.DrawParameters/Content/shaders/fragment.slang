StructuredBuffer<float4> colors : register(t0, space2);

struct Input
{
    uint DrawIndex : COLOR1;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    return colors[input.DrawIndex];
}
