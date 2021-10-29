shader_type canvas_item;

uniform vec4 color : hint_color;
uniform float solid_color = 1.0;
uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float transparency = 1.0;
uniform float square_alpha = 1.0;
uniform float texture_alpha = 1.0;

vec4 square(vec2 uv, vec2 centre, float width){
	uv = uv * 2.0 - centre;
	vec2 abs_uv = abs(uv.xy);
	float square = step(width, max(abs_uv.x, abs_uv.y));
	return vec4(vec3(square), 1.0);
}

float rnd(float x){
    return fract(sin(dot(vec2(x+47.49,38.2467/(x+2.3)), vec2(12.9898, 78.233)))* (43758.5453));
}

float random (vec2 uv) {
    return fract(sin(dot(uv.xy,vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment(){
	float ratio = SCREEN_PIXEL_SIZE.x/SCREEN_PIXEL_SIZE.y;
	
	vec2 tex_uv = (SCREEN_UV - vec2(0.5)) / vec2(ratio, 1.0);
	vec3 colTex = texture(TEXTURE, tex_uv).rgb;
	
	vec2 uv = (SCREEN_UV - vec2(0.5)) / vec2(ratio, 1.0);
	vec3 col = color.rgb;
	col = mix(vec3(0.0), col, brightness);
	col = mix(vec3(0.5), col, contrast);
	col = mix(vec3(dot(vec3(1.0), col) * 0.33333), col, saturation);
	col = mix(vec3(1.0), col, square_alpha);
	col = mix(colTex.rgb, col, texture_alpha);
	COLOR = vec4(col, transparency);
	
	for(int i=0; i<50; i++){
		float fi = float(i);
		//float speed = rnd(fi)+rnd(cos(fi));
		//vec2 point = vec2((mod(speed * TIME, 3.5) - 1.75), 1.0 + mod(TIME * -rnd(fi), -2.0));
		float speed = random(vec2(fi)) + random(cos(vec2(fi)));
		vec2 point = vec2((mod(speed * TIME, 3.5) - 1.75), 1.0 + mod(TIME * -random(vec2(fi)), -2.0));
		COLOR += 1.0 - square(uv, point, 0.005); 
	}
}