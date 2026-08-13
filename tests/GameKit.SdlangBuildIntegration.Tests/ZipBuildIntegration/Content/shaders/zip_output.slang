struct Output
{
    float4 Position : SV_Position;
};

[shader("vertex")]
Output vertexMain(uint vertexIndex : SV_VertexID)
{
    Output output;
    output.Position = float4(0.0, 0.0, 0.0, 1.0);
    return output;
}

[shader("fragment")]
void fragmentMain(Output input)
{
}
