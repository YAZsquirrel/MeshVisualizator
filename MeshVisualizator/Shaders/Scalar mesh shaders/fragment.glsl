#version 450 core
layout (location = 0) out vec4 FragColor;

in float fTex1D; 
uniform sampler1D texture0;

void main()
{   
   FragColor = texture(texture0, fTex1D);
} 