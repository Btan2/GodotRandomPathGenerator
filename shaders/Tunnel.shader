shader_type canvas_item;

uniform vec4 color : hint_color;
uniform vec2 pos;

vec2 rotate2D(vec2 p, float theta){
	return p * mat2(vec2(cos(theta), -sin(theta)), vec2(sin(theta), cos(theta)));
}

vec2 polar_coordinates(vec2 uv, vec2 center, float zoom, float repeat){
	vec2 dir = uv - center;
	float radius = length(dir) * 2.0;
	float angle = atan(dir.y, dir.x) * 1.0/(3.1416 * 2.0);
	return mod(vec2(radius * zoom, angle * repeat), 1.0);
}

void fragment(){
	vec2 polar_uv = polar_coordinates(UV.xy, pos, 0.5 * TIME, 1.0);
	vec2 p = polar_uv + pos;
	//polar_uv = rotate2D(polar_uv, 0.5 * TIME);
	vec3 col = textureGrad(TEXTURE, polar_uv, dFdx(polar_uv), dFdy(polar_uv)).xyz;
	col *= length(p);
	//COLOR = texture(TEXTURE, polar_uv);
	COLOR = vec4(col, 1.0);
}

//void fragment(){
//	vec2 p = UV + pos;
//	p = rotate2D(p, 0.05 * TIME);
//	float a = atan(p.y,p.x);
//	float r = length(p);
//	vec2 uv = vec2(0.3/r + 0.2*TIME, a/3.1415927);
//	vec2 uv2 = vec2( uv.x, atan(p.y,abs(p.x))/3.1415927 );
//	vec3 col = textureGrad(TEXTURE, uv, dFdx(uv2), dFdy(uv2) ).xyz; 
//	col = col * r; // darken at the centre
//	COLOR = vec4(col * color.rgb, 1.0);
//}