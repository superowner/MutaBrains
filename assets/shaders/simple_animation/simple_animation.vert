#version 430 core

in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexture;
in vec4 aBoneIDs;
in vec4 aBoneWeights;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 finalBonesMatrices[100];

out vec3 Position;
out vec3 Normal;
out vec2 Texture;

void main(void)
{
    mat4 Bone = finalBonesMatrices[int(aBoneIDs[0])] * aBoneWeights[0];
    Bone += finalBonesMatrices[int(aBoneIDs[1])] * aBoneWeights[1];
    Bone += finalBonesMatrices[int(aBoneIDs[2])] * aBoneWeights[2];
    Bone += finalBonesMatrices[int(aBoneIDs[3])] * aBoneWeights[3];
    vec4 position = Bone * vec4(aPosition, 1);
    vec3 normal = mat3(Bone) * aNormal;

    Position = vec3(vec4(position.xyz, 1) * model);
    Normal = normal * mat3(model);
    Texture = aTexture;
    
    gl_Position = vec4(position.xyz, 1) * model * view * projection;
}
