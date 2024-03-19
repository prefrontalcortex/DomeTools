float3 _CameraOffset;
float _Debug = 0;
float _AllowedUndersampling = 2;
float _AllowedPerfectRange = 0.01;


float2 intersect(float2 p1, float2 p2, float2 p3, float2 p4) {
    float part1 = (p1.x * p2.y - p1.y * p2.x);
    float part2 = (p3.x * p4.y - p3.y * p4.x);
    float divider = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
    return float2(
        (part1 * (p3.x - p4.x) - (p1.x - p2.x) * part2) / divider,
        (part1 * (p3.y - p4.y) - (p1.y - p2.y) * part2) / divider
    );
}

half2 GetUV(half2 pos) {
    float _PI = 3.1415926535;
    float R = 1;

    // float L = _CameraOffset;
    float x = length(half2(pos.x, pos.y));
    float alpha = (_PI/2 - x * _PI / 2);
    
    float2 onCircle = float2(cos(alpha) / 2, sin(alpha) / 2);
    float2 cam = float2(0, _CameraOffset.y);

    float2 intersectionPoint = intersect(onCircle, cam, float2(-1,0), float2(1,0));
    float r = intersectionPoint.x;
    
    float phi = atan2(pos.y, pos.x) ;
    float u = r * cos(phi);
    float v = r * sin(phi);
    half2 uv = half2(u + 0.5,v + 0.5);

    return uv;
}

float invLerp(float from, float to, float value){
	return (value - from) / (to - from);
}

half4 GetDebugStretch(half2 uv, float4 _MainTex_TexelSize) {
		float2 textureCoord = uv * _MainTex_TexelSize.zw;
		float2 dx_vtc  = ddx(textureCoord);
		float2 dy_vtc  = ddy(textureCoord);
		float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
		half mipmapLevel = max( 0, 0.5 * log2(delta_max_sqr));
		float2 delta_max = max(dx_vtc, dy_vtc);

		float stretch = sqrt(delta_max_sqr);
		float undersampling = saturate(1/_AllowedUndersampling - stretch);
		float oversampling = saturate(stretch - 1);
		float perfectRange = _AllowedPerfectRange; 
		float perfect = invLerp(1 - perfectRange,1 + perfectRange, stretch) * invLerp(1 + perfectRange, 1 - perfectRange, stretch) * 4;
		return lerp(half4(0,0,0,1), half4(1,1,1,1), half4(undersampling, perfect, oversampling, 1));
}

// https://www.iquilezles.org/www/articles/spherefunctions/spherefunctions.htm
float sphIntersect( float3 ro, float3 rd, float4 sph )
{
	float3 oc = ro - sph.xyz;
	float b = dot( oc, rd );
	float c = dot( oc, oc ) - sph.w*sph.w;
	float h = b*b - c;
	if( h<0.0 ) return -1.0;
	h = sqrt( h );
	return -b - h;
}

float map(float value, float from1, float to1, float from2, float to2) {
	return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

float3 UVToDirection(half2 uv, half targetAngleRad)
{
	float2 uv2 = uv * 2 - 1;
	uv2.y *= -1;
	float phi = atan2(uv2.y, uv2.x);
	float len = length(uv2);
	float theta = len * 0.5;
	theta = map(theta, 1, 0, targetAngleRad, 0);
	// if len is outside 0..1 range, return black
	if (len > 1) return float3(0, 0, 0);
	return float3(sin(theta) * cos(phi), cos(theta), sin(theta) * sin(phi));
}


void UVToDirection_float(float2 uv, half targetAngleRad, out float3 dir)
{
	dir = UVToDirection(uv, targetAngleRad);
}

void UVToDirection_half(float2 uv, half targetAngleRad, out float3 dir)
{
	dir = UVToDirection(uv, targetAngleRad);
}
