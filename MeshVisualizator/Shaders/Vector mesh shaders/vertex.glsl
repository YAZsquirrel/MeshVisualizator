#version 450 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 3) in mat4 instanceMat; 

out vec3 fcolor;

uniform mat4 projection;
uniform mat4 transform;

void main()
{
    gl_Position = projection * transform * instanceMat * vec4(aPos, 0.0, 1.0);
    fcolor = aColor;//vec3(aPos, 0);//(normalize(vec3(aPos, 0)) + 1) / 2; //vec3(0.5);
}  