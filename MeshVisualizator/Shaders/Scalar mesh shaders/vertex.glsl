#version 450 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

out vec3 fcolor;

uniform mat4 projection;
uniform mat4 transform;

void main()
{
    fcolor = aColor;
    gl_Position = projection * transform *  vec4(aPos.xy, 0.0, 1.0);
}