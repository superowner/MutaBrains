#version 430 core

out vec4 FragColor;

in vec2 Texture;

uniform sampler2D texture0;

void main()
{
    FragColor = vec4(0,1,0,1);// texture(texture0, Texture);
}