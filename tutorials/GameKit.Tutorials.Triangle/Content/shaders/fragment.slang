ConstantBuffer<float4> color: register(b0, space3);

[shader("fragment")]
float4 main() : SV_Target0
{   
    return color;
}
