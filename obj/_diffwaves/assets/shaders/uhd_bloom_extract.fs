#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;
uniform float threshold;

void main()
{
    vec3 col = texture(texture0, fragTexCoord).rgb;
    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 bloom = max(col - vec3(threshold), 0.0);
    bloom *= smoothstep(threshold - 0.08, threshold + 0.22, lum);
    finalColor = vec4(bloom, 1.0) * fragColor;
}
