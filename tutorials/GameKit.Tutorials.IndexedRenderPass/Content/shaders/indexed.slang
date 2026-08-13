struct VertexInput
{
    float3 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
};

struct VertexToFragment
{
    float4 Position : SV_Position;
    float4 Color : TEXCOORD0;
};

[shader("vertex")]
VertexToFragment vertexMain(VertexInput input)
{
    VertexToFragment output;
    output.Position = float4(input.Position, 1.0);
    output.Color = input.Color;
    return output;
}

[shader("fragment")]
float4 fragmentMain(VertexToFragment input) : SV_Target0
{
    return input.Color;
}
