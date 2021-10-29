shader_type canvas_item;

uniform vec4 color : hint_color;
uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

uniform int _SnowflakeAmount = 20;
uniform float _BlizardFactor = 0.2;

uniform int type : hint_range(1,3);

vec2 rotate2D(vec2 p, float theta){
	return p * mat2(vec2(cos(theta), -sin(theta)), vec2(sin(theta), cos(theta)));
}

float rnd(float x){
    return fract(sin(dot(vec2(x+47.49,38.2467/(x+2.3)), vec2(12.9898, 78.233)))* (43758.5453));
}

float drawCircle(vec2 uv, vec2 center, float radius){
    return 1.0 - smoothstep(0.0, radius, length(uv - center));
}

void fragment(){
	float ratio = SCREEN_PIXEL_SIZE.x/SCREEN_PIXEL_SIZE.y;
	vec2 p = (SCREEN_UV - vec2(0.5)) / vec2(ratio, 1.0);
	p.y *= -1.0;
	
	p = rotate2D(p, 0.05 * TIME);
	float a = atan(p.y,p.x);
	float r = length(p);
	vec2 uv = vec2(0.3/r + 0.2*TIME, a/3.1415927);
	vec2 uv2 = vec2( uv.x, atan(p.y,abs(p.x))/3.1415927 );
	vec3 col = textureGrad(TEXTURE, uv, dFdx(uv2), dFdy(uv2) ).xyz; 
	col = col * r; // darken at the centre
	
	//col *= color.rgb;
	col = mix(vec3(0.0), col, brightness);
	col = mix(vec3(0.5), col, contrast);
	col = mix(vec3(dot(vec3(1.0), col) * 0.33333), col, saturation);
	COLOR = vec4(col * color.rgb, 1.0);
}