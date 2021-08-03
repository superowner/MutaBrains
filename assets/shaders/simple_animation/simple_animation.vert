#version 430 core

in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexture;
in ivec4 aBoneIDs;
in vec4 aBoneWeights;

const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 finalBonesMatrices[100];

out vec3 Position;
out vec3 Normal;
out vec2 Texture;

void main(void)
{
    vec4 totalPosition = vec4(0.0f);
    for(int i = 0 ; i < MAX_BONE_INFLUENCE ; i++)
    {
        if(aBoneIDs[i] == -1) 
            continue;
        if(aBoneIDs[i] >= MAX_BONES) 
        {
            totalPosition = vec4(aPosition,1.0f);
            break;
        }
        vec4 localPosition = finalBonesMatrices[aBoneIDs[i]] * vec4(aPosition,1.0f);
        totalPosition += localPosition * aBoneWeights[i];
        vec3 localNormal = mat3(finalBonesMatrices[aBoneIDs[i]]) * aNormal;
   }
	
    gl_Position =  totalPosition * model * view * projection;
    //Position = vec3(totalPosition * model * view * projection);
    //Normal = aNormal * mat3(transpose(inverse(model)));
	Texture = aTexture;
}
