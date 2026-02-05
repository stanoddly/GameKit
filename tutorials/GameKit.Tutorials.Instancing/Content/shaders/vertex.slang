StructuredBuffer<float4> offsets : register(t0, space0);

struct Input
{
    float4 Position : TEXCOORD0;
    uint InstanceID : SV_InstanceID;
};

struct Output
{
    float4 Position : SV_Position;
    uint InstanceID : COLOR1;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;

    float4 offset = offsets[input.InstanceID];
    float scale = 0.4;
    output.Position = float4(input.Position.xy * scale + offset.xy, 0.0, 1.0);
    output.InstanceID = input.InstanceID;

    return output;
}
