StructuredBuffer<float4> instanceOffsets : register(t0, space0);

struct Input
{
    float3 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
    uint InstanceID : SV_InstanceID;
};

struct Output
{
    float4 Position : SV_Position;
    float4 Color : TEXCOORD0;
    uint InstanceID : TEXCOORD1;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;

    output.Position = float4(input.Position.xy + instanceOffsets[input.InstanceID].xy, input.Position.z, 1.0);
    output.Color = input.Color;
    output.InstanceID = input.InstanceID;

    return output;
}
