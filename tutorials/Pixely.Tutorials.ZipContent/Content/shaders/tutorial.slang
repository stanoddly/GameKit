struct VertexToFragment
{
    float4 Position : SV_Position;
};

[shader("vertex")]
VertexToFragment vertexMain(uint vertexIndex : SV_VertexID)
{
    VertexToFragment output;
    output.Position = float4(0.0, 0.0, 0.0, 1.0);
    return output;
}

[shader("fragment")]
void fragmentMain(VertexToFragment input)
{
}
