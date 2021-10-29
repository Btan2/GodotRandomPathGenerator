shader_type canvas_item;
render_mode unshaded, blend_disabled;

uniform float speed = 1.0;

vec2 rotate(vec2 uv, vec2 pivot, float angle, float dir){
	float sina = sin(angle * dir);
	float cosa = cos(angle * dir);
	
	//translate point back to origin:
	uv.x -= pivot.x;
	uv.y -= pivot.y;
	
	//rotate point
	float xnew = uv.x * cosa - uv.y * sina;
	float ynew = uv.x * sina + uv.y * cosa;
	
	//translate point back:
	uv.x = xnew + pivot.x;
	uv.y = ynew + pivot.y;
	return uv;
}

//vec2 rotate(vec2 v, vec2 p, float a)
//{
//	float cosa = cos(a);
//	float sina = sin(a);
//	mat2 m = mat2(vec2(sina, -cosa),vec2(cosa, sina));
//	v -= p;
//	return m * v;
//}


float rand(float co){ 
	return fract(sin(co*(91.3458)) * 47453.5453); 
	}

varying float sine_t;

void vertex() {
	float intervalTime = floor(TIME / 2.0) * 2.5;
	float rtime = rand(1.0) * intervalTime;
	//float d = sign(sin(intervalTime));
	float d = sign(sin(TIME));
	
	VERTEX = rotate(VERTEX, vec2(186), speed * TIME, d);
	//VERTEX = rotate(VERTEX, vec2(186), TIME);
}