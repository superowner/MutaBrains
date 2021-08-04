#version 430 core

out vec4 FragColor;

in vec2 Texture;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, Texture);
}