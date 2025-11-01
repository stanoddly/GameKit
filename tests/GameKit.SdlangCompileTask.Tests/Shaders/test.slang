// Test shader for SdlangCompileTask
// This is a sample Slang shader file used for testing compilation

struct PSOutput
{
    float4 color : SV_Target;
};

PSOutput main() : SV_Target
{
    return PSOutput(float4(1.0, 0.0, 0.0, 1.0));
}
