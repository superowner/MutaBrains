#version 430 core

in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexture;
in vec4 aBoneIDs;
in vec4 aBoneWeights;

const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 finalBonesMatrices[100];

out vec2 Texture;

void main(void)
{
    mat4 Bone = finalBonesMatrices[int(aBoneIDs[0])] * aBoneWeights[0];
    Bone += finalBonesMatrices[int(aBoneIDs[1])] * aBoneWeights[1];
    Bone += finalBonesMatrices[int(aBoneIDs[2])] * aBoneWeights[2];
    Bone += finalBonesMatrices[int(aBoneIDs[3])] * aBoneWeights[3];

    vec4 v = Bone * vec4(aPosition, 1);

    gl_Position = vec4(v.xyz, 1) * model * view * projection;
}
