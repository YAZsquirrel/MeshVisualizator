#version 420 core
out vec4 vertexColor; // Передаем цвет во фрагментный шейдер

void main()
{
    vertexColor = vec4(0.5f, 0.5f, 0.5f, 1.0f); // Устанавливаем значение выходной переменной в темно-красный цвет.
}