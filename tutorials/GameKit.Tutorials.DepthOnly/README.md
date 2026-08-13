# Depth-only graphics program

This tutorial builds a pipeline with a depth target and no color targets.

SDL3's GPU API currently requires a fragment shader when creating a graphics pipeline, even though its supported backends allow depth-only pipelines without one. This limitation is tracked by [SDL issue #12311](https://github.com/libsdl-org/SDL/issues/12311).

`Content/shaders/depth.slang` keeps the workaround inside the program: `vertexMain` returns `VertexToFragment`, while a matching `fragmentMain` accepts the complete structure and returns `void`. No shared no-op shader or vertex-only runtime API is required.
