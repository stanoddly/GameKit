StructuredBuffer<float4> offsets : register(t0, space0);

struct Input
{
    float4 Position : TEXCOORD0;
    uint DrawIndex : SV_StartInstanceLocation;
};

struct Output
{
    float4 Position : SV_Position;
    uint DrawIndex : COLOR1;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;

    float4 offset = offsets[input.DrawIndex];
    float scale = 0.4;
    output.Position = float4(input.Position.xy * scale + offset.xy, 0.0, 1.0);
    output.DrawIndex = input.DrawIndex;

    return output;
}
