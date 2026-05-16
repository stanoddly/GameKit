struct Input
{
    float4 Position : TEXCOORD0;
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
    output.Position = input.Position;
    output.Color = input.Color;

    return output;
}
