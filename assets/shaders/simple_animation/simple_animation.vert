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
    vec4 totalPosition = vec4(0.0);
    for(int i = 0 ; i < MAX_BONE_INFLUENCE ; i++)
    {
        highp int boneId = int(aBoneIDs[i]);

        if(boneId < 0)
        {
            continue;
        }
        if(boneId >= MAX_BONES) 
        {
            totalPosition = vec4(aPosition,1.0);
            break;
        }
        vec4 localPosition = finalBonesMatrices[boneId] * vec4(aPosition, 1.0);
        totalPosition += localPosition * aBoneWeights[i];
   }

   gl_Position = totalPosition * model * view * projection;
   //gl_Position =  projection * (view * model) * totalPosition;
   //gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
