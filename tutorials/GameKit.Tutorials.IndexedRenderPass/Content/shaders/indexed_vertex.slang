struct Input
{
    float3 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
};

struct Output
{
    float4 Position : SV_Position;
    float4 Color : TEXCOORD0;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;

    output.Position = float4(input.Position, 1.0);
    output.Color = input.Color;

    return output;
}
