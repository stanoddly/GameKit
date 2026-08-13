# Depth-only graphics program

This tutorial builds a pipeline with a depth target and no color targets.

SDL requires both native graphics stages, so `Content/shaders/depth.slang` keeps the workaround inside the program: `vertexMain` returns `VertexToFragment`, while a matching `fragmentMain` accepts the complete structure and returns `void`. No shared no-op shader or vertex-only runtime API is required.
