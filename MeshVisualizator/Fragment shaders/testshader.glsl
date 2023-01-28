//#version 450 core
#version 330
//in vec4 vertexColor;

out vec4 color;

void main()
{
   //color = vec4(gl_FragCoord.xy / vec2(1000f, 1000f), 0.0f, 1.0f);
   gl_FragColor = vec4(gl_FragCoord.xy / vec2(1000f, 1000f), 0.0f, 1.0f);
}