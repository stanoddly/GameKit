struct Input
{
    float4 Position : TEXCOORD0;
};

struct Output
{
    float4 Position : SV_Position;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;
    output.Position = input.Position;
    return output;
}
