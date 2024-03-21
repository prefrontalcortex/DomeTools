float filteredGrid( in float2 p, in float2 dpdx, in float2 dpdy )
{
    const float N = 30.0;
    float2 w = max(abs(dpdx), abs(dpdy));
    float2 a = p + 0.5*w;                        
    float2 b = p - 0.5*w;           
    float2 i = (floor(a) + min(frac(a) * N, 1.0)-
              floor(b) - min(frac(b) * N, 1.0)) / (N * w);
    return (1.0-i.x) * (1.0-i.y);
}

void DomeCoordinates_float(float3 pos, out float2 angle)
{
    // float PI = 3.1415926535;
    
    float dist = sqrt(pos.x * pos.x + pos.z * pos.z);
    float phi = atan2(pos.y, dist);

    float a = atan2(pos.z, pos.x);
    float a_angle = a / PI * 180;
    // return frac(a_angle / 10);

    float p_angle = phi / PI * 180;
    // return frac(p_angle / 10);


    angle.x = a_angle / 360;
    angle.y = p_angle / 360;
}