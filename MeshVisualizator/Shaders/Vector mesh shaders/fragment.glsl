#version 450 core
layout (location = 0) out vec4 FragColor;

in vec3 fcolor; 

void main()
{   
   
   //vec2 res = vec2(1050, 921);
   //FragColor = vec4(gl_FragCoord.xy / res, 0.5, 1);
   FragColor = vec4(fcolor, 1.);
} 