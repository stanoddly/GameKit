StructuredBuffer<float4> offsets : register(t0, space0);
StructuredBuffer<float4> colors : register(t0, space2);

struct VertexInput
{
    float4 Position : TEXCOORD0;
    uint InstanceID : SV_InstanceID;
};

struct VertexToFragment
{
    float4 Position : SV_Position;
    uint InstanceID : COLOR1;
};

[shader("vertex")]
VertexToFragment vertexMain(VertexInput input)
{
    VertexToFragment output;
    float4 offset = offsets[input.InstanceID];
    float scale = 0.4;
    output.Position = float4(input.Position.xy * scale + offset.xy, 0.0, 1.0);
    output.InstanceID = input.InstanceID;
    return output;
}

[shader("fragment")]
float4 fragmentMain(VertexToFragment input) : SV_Target0
{
    return colors[input.InstanceID];
}
