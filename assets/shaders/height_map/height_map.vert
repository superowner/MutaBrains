#version 330 core

in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexture;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Position;
out vec3 OrigPosition;
out vec3 Normal;
out vec2 Texture;

void main(void)
{
    OrigPosition = aPosition;
    Position = vec3(vec4(aPosition, 1.0) * model);
    Normal = aNormal * mat3(transpose(inverse(model)));
    Texture = aTexture;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
