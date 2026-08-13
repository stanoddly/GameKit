StructuredBuffer<float4> instanceOffsets : register(t0, space0);
StructuredBuffer<float4> instanceTints : register(t0, space2);

struct VertexInput
{
    float3 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
    uint InstanceID : SV_InstanceID;
};

struct VertexToFragment
{
    float4 Position : SV_Position;
    float4 Color : TEXCOORD0;
    uint InstanceID : TEXCOORD1;
};

[shader("vertex")]
VertexToFragment vertexMain(VertexInput input)
{
    VertexToFragment output;
    output.Position = float4(input.Position.xy + instanceOffsets[input.InstanceID].xy, input.Position.z, 1.0);
    output.Color = input.Color;
    output.InstanceID = input.InstanceID;
    return output;
}

[shader("fragment")]
float4 fragmentMain(VertexToFragment input) : SV_Target0
{
    return input.Color * instanceTints[input.InstanceID];
}
