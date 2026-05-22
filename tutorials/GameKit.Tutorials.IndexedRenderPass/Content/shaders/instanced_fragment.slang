StructuredBuffer<float4> instanceTints : register(t0, space2);

struct Input
{
    float4 Color : TEXCOORD0;
    uint InstanceID : TEXCOORD1;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    return input.Color * instanceTints[input.InstanceID];
}
