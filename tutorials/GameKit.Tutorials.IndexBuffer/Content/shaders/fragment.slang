struct Input
{
    float4 Color : TEXCOORD0;
};

[shader("fragment")]
float4 main(Input input) : SV_Target0
{
    return input.Color;
}
