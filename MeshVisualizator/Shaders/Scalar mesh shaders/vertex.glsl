#version 450 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in float aTex1D;

out float fTex1D;

uniform mat4 projection;
uniform mat4 transform;

void main()
{
    fTex1D = aTex1D;
    gl_Position = projection * transform * vec4(aPos.xy, 0.0, 1.0);
}