#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float time;
uniform float sharpen;
uniform float vignette;
uniform float grain;
uniform float exposure;
uniform float warmth;
uniform float adrenaline;

vec3 acesTonemap(vec3 x)
{
    x *= exposure;
    return clamp(x / (x + vec3(1.0)), 0.0, 1.0);
}

float vignetteMask(vec2 uv)
{
    vec2 p = uv - 0.5;
    return smoothstep(0.92, 0.22, dot(p, p) * 1.35);
}

void main()
{
    vec2 uv = fragTexCoord;
    vec2 texel = 1.0 / resolution;

    vec3 col = texture(texture0, uv).rgb;

    if (sharpen > 0.001)
    {
        vec3 blur = (
            texture(texture0, uv + texel * vec2(-1.0, -1.0)).rgb +
            texture(texture0, uv + texel * vec2( 0.0, -1.0)).rgb +
            texture(texture0, uv + texel * vec2( 1.0, -1.0)).rgb +
            texture(texture0, uv + texel * vec2(-1.0,  0.0)).rgb +
            texture(texture0, uv + texel * vec2( 0.0,  0.0)).rgb +
            texture(texture0, uv + texel * vec2( 1.0,  0.0)).rgb +
            texture(texture0, uv + texel * vec2(-1.0,  1.0)).rgb +
            texture(texture0, uv + texel * vec2( 0.0,  1.0)).rgb +
            texture(texture0, uv + texel * vec2( 1.0,  1.0)).rgb
        ) / 9.0;
        col += (col - blur) * sharpen;
    }

    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 bloom = max(col - vec3(0.68), 0.0);
    col += bloom * smoothstep(0.5, 0.95, lum) * (0.28 + adrenaline * 0.18);

    col = acesTonemap(col);
    col = mix(col, col * vec3(1.06, 1.0, 0.9), warmth);

    float vig = mix(1.0 - vignette * 0.7, 1.0, vignetteMask(uv));
    col *= vig;

    float n = fract(sin(dot(uv * resolution + time * vec2(17.0, 31.0), vec2(12.9898, 78.233))) * 43758.5453);
    col += (n - 0.5) * grain * (0.65 + adrenaline * 0.35);

    finalColor = vec4(col, 1.0) * fragColor;
}
