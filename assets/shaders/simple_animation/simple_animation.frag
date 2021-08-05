#version 430 core

out vec4 FragColor;

in vec2 Texture;
in vec4 BoneColors;

uniform sampler2D texture0;

void main()
{
    FragColor = vec4(BoneColors.xyz, 1);// texture(texture0, Texture);
}