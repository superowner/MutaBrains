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
    vec4 totalPosition = vec4(0.0f);
    vec3 totalNormal = vec3(0);

    for(int i = 0 ; i < 4 ; i++)
    {
        if(aBoneIDs[i] == -1) 
            continue;
        if(aBoneIDs[i] >= 100) 
        {
            totalPosition = vec4(aPosition,1.0f);
            break;
        }
        vec4 localPosition = finalBonesMatrices[int(aBoneIDs[i])] * vec4(aPosition,1.0f);
        totalPosition += localPosition * aBoneWeights[i];
        vec3 localNormal = mat3(finalBonesMatrices[int(aBoneIDs[i])]) * aNormal;
        totalNormal += localNormal * aBoneWeights[i];
   }

    Position = vec3(totalPosition * model);
    Normal = totalNormal * mat3(model);
    Texture = aTexture;
    
    gl_Position = totalPosition * model * view * projection;
}
