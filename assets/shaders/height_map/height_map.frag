#version 430

struct Material {
    sampler2D grass;
    sampler2D preground;
    sampler2D ground;
    sampler2D specular;
    float     shininess;
    float     maxheight;
};

struct DirLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

// struct PointLight {
//     vec3 position;

//     float constant;
//     float linear;
//     float quadratic;

//     vec3 ambient;
//     vec3 diffuse;
//     vec3 specular;
// };

// struct SpotLight{
//     vec3  position;
//     vec3  direction;
//     float cutOff;
//     float outerCutOff;

//     vec3 ambient;
//     vec3 diffuse;
//     vec3 specular;

//     float constant;
//     float linear;
//     float quadratic;
// };

uniform DirLight dirLight;
// uniform PointLight pointLight;
// uniform SpotLight spotLight;

uniform Material material;

uniform vec3 viewPosition;

in vec3 Normal;
in vec3 Position;
in vec3 OrigPosition;
in vec2 Texture;

out vec4 outputColor;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
// vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
// vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    //properties
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPosition - Position);

    //phase 1: Directional lighting
    vec3 result = CalcDirLight(dirLight, norm, viewDir);
    //phase 2: Point lights
    // result += CalcPointLight(pointLight, norm, Position, viewDir);
    //phase 3: Spot light
    // result += CalcSpotLight(spotLight, norm, Position, viewDir);

    outputColor = vec4(result, 1.0);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    //combine results

    vec3 grassColor = texture(material.grass, Texture).rgb;
    vec3 pregroundColor = texture(material.preground, Texture).rgb;
    vec3 groundColor = texture(material.ground, Texture).rgb;

    vec3 leadColor, upColor, downColor;
    float zerotoone_height = OrigPosition.y / material.maxheight;

    if (zerotoone_height < 0.33) {
        leadColor = groundColor;
    } else if (zerotoone_height < 0.66) {
        leadColor = pregroundColor;
    } else {
        leadColor = grassColor;
    }

    if (zerotoone_height > 0.2 && zerotoone_height < 0.4)
    {
        downColor = groundColor;
        upColor = pregroundColor;

        leadColor = mix(downColor, upColor, smoothstep(0.2, 0.4, zerotoone_height));
    }
    else if (zerotoone_height > 0.5 && zerotoone_height < 0.7)
    {
        downColor = pregroundColor;
        upColor = grassColor;

        leadColor = mix(downColor, upColor, smoothstep(0.5, 0.7, zerotoone_height));
    };

    vec3 ambient  = light.ambient  * leadColor;
    vec3 diffuse  = light.diffuse  * diff * leadColor;
    vec3 specular = light.specular * spec * vec3(texture(material.specular, Texture));
    return (ambient + diffuse + specular);
}

// vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
// {
//     vec3 lightDir = normalize(light.position - fragPos);
//     //diffuse shading
//     float diff = max(dot(normal, lightDir), 0.0);
//     //specular shading
//     vec3 reflectDir = reflect(-lightDir, normal);
//     float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
//     //attenuation
//     float distance    = length(light.position - fragPos);
//     float attenuation = 1.0 / (light.constant + light.linear * distance +
//     light.quadratic * (distance * distance));
//     //combine results
//     vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, Texture));
//     vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, Texture));
//     vec3 specular = light.specular * spec * vec3(texture(material.specular, Texture));
//     ambient  *= attenuation;
//     diffuse  *= attenuation;
//     specular *= attenuation;
//     return (ambient + diffuse + specular);
// }

// vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
// {

//     //diffuse shading
//     vec3 lightDir = normalize(light.position - Position);
//     float diff = max(dot(normal, lightDir), 0.0);

//     //specular shading
//     vec3 reflectDir = reflect(-lightDir, normal);
//     float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

//     //attenuation
//     float distance    = length(light.position - Position);
//     float attenuation = 1.0 / (light.constant + light.linear * distance +
//     light.quadratic * (distance * distance));

//     //spotlight intensity
//     float theta     = dot(lightDir, normalize(-light.direction));
//     float epsilon   = light.cutOff - light.outerCutOff;
//     float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

//     //combine results
//     vec3 ambient = light.ambient * vec3(texture(material.diffuse, Texture));
//     vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, Texture));
//     vec3 specular = light.specular * spec * vec3(texture(material.specular, Texture));
//     ambient  *= attenuation;
//     diffuse  *= attenuation * intensity;
//     specular *= attenuation * intensity;
//     return (ambient + diffuse + specular);
// }
