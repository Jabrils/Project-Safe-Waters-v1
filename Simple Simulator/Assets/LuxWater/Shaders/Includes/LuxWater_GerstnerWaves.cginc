#if defined(_GERSTNERDISPLACEMENT) || defined (GERSTNERENABLED)

float _Lux_Time;




half3 GerstnerOffset4 (half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) {
    
    half3 offsets;
    
    half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
    half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
    
    half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
    #if defined(USINGWATERVOLUME)
        half4 TIME = _Lux_Time.xxxx * speed;
    #else
        half4 TIME = _Time.yyyy * speed;
    #endif
    
    half4 COS = cos (dotABCD + TIME);
    half4 SIN = sin (dotABCD + TIME);
    
    offsets.x = dot(COS, half4(AB.xz, CD.xz));
    offsets.z = dot(COS, half4(AB.yw, CD.yw));
    offsets.y = dot(SIN, amp);

    offsets.xyz *= _GerstnerVertexIntensity;

    return offsets;         
}

half3 GerstnerNormal4 (half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) {
    
    half3 nrml = half3(0, 2.0, 0);
    
    half4 AB = freq.xxyy  * amp.xxyy * dirAB.xyzw;
    half4 CD = freq.zzww  * amp.zzww * dirCD.xyzw;
    
    half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
    #if defined(USINGWATERVOLUME)
        half4 TIME = _Lux_Time.xxxx * speed;
    #else
        half4 TIME = _Time.yyyy * speed;
    #endif
    
    half4 COS = cos (dotABCD + TIME);
 
    nrml.x -= dot(COS, half4(AB.xz, CD.xz)) ;
    nrml.z -= dot(COS, half4(AB.yw, CD.yw)) ;
    
    nrml.xz *= _GerstnerNormalIntensity * _GerstnerVertexIntensity.y; 
//  We skip the normalize here and do it in the vertex shader after having combined the normals.
    //nrml = normalize (nrml);

    return nrml;            
} 


// https://gist.github.com/yorung/5f72b5bff2082cd15f1722cd2f679dfa

float3 CalcGerstnerWaveOffset(half3 P, half4 steepness, half4 amplitude, half4 frequency, half4 speed, half4 directionAB, half4 directionCD)
{
    float3 sum = float3(0, 0, 0);
    int numWaves = 4;

    float4 dirx = float4(directionAB.x, directionAB.z, directionCD.x, directionCD.z);
    float4 dirz = float4(directionAB.y, directionAB.w, directionCD.y, directionCD.w);


    //[unroll]
    for (int i = 0; i < numWaves; i++)
    {
        float wi = frequency[i];
        float Qi = steepness[i] / (amplitude[i] * wi * numWaves);
        float phi = speed[i] * wi;
        
        float2 waveDir = float2(dirx[i], dirz[i]);
        
        float rad = wi * dot(waveDir, P.xz) + phi * _Time.y;
        sum.y  += sin(rad) * amplitude[i];
        sum.xz += cos(rad) * amplitude[i] * Qi * waveDir;
    }
    return sum;
}

// makes more sense but needs more testing
half3 CalcGerstnerWaveNormal(half3 P, half4 steepness, half4 amplitude, half4 frequency, half4 speed, half4 directionAB, half4 directionCD)
{
    float3 normal = float3(0, 1, 0);
    int numWaves = 4;

    float4 dirx = float4(directionAB.x, directionAB.z, directionCD.x, directionCD.z);
    float4 dirz = float4(directionAB.y, directionAB.w, directionCD.y, directionCD.w);

    //[unroll]
    for (int i = 0; i < numWaves; i++)
    {
        //Wave wave = waves[i];
        float wi = /*2 / */frequency[i];
        float Qi = steepness[i] / (amplitude[i] * wi * numWaves);
        float WA = wi * amplitude[i];
        float phi = speed[i] * wi;

        float2 waveDir = float2(dirx[i], dirz[i]);

        float rad = wi * dot(waveDir, P.xz) + phi * _Time.y;
        normal.xz -= waveDir * WA * cos(rad);
        normal.y -= Qi * WA * sin(rad);
    }
    normal.xz *= _GerstnerNormalIntensity;
    return normalize(normal);
}

void GerstnerOffsetOnly ( out half3 offs,
             half3 vtx, half3 tileableVtx, 
             half4 amplitude, half4 frequency, half4 steepness, 
             half4 speed, half4 directionAB, half4 directionCD ) {
    offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
}

void Gerstner ( out half3 offs, out half3 nrml,
             half3 vtx, half3 tileableVtx, 
             half4 amplitude, half4 frequency, half4 steepness, 
             half4 speed, half4 directionAB, half4 directionCD ) {
    offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
    nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);

    //float3 dx = float3(0.01, 0, 0) + GerstnerOffset4(tileableVtx.xz + float2(0.01, 0), steepness, amplitude, frequency, speed, directionAB, directionCD);
    //float3 dz = float3(0, 0, 0.01) + GerstnerOffset4(tileableVtx.xz + float2(0, 0.01), steepness, amplitude, frequency, speed, directionAB, directionCD);
    //nrml = normalize(cross(dz - offs, dx - offs));
 
    //offs = CalcGerstnerWaveOffset(vtx, steepness, amplitude, frequency, speed, directionAB, directionCD); 
    //vtx = (vtx + offs);
    //nrml = CalcGerstnerWaveNormal(vtx, steepness, amplitude, frequency, speed, directionAB, directionCD);                                 
}

#endif