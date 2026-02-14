#ifndef DUNKSJAMS_NOISE_INCLUDED
#define DUNKSJAMS_NOISE_INCLUDED

float4 FAST32_hash_2D(float2 gridcell)
{
    const float2 OFFSET = float2(26.0, 161.0);
    const float DOMAIN = 71.0;
    const float SOMELARGEFLOAT = 951.135664;
    float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
    P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;
    P += OFFSET.xyxy;
    P *= P;
    return frac(P.xzxz * P.yyww * (1.0 / SOMELARGEFLOAT));
}

void FAST32_hash_2D(float2 gridcell, out float4 hash_0, out float4 hash_1)
{
    const float2 OFFSET = float2(26.0, 161.0);
    const float DOMAIN = 71.0;
    const float2 SOMELARGEFLOATS = float2(951.135664, 642.949883);
    float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
    P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;
    P += OFFSET.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    hash_0 = frac(P * (1.0 / SOMELARGEFLOATS.x));
    hash_1 = frac(P * (1.0 / SOMELARGEFLOATS.y));
}

void FAST32_hash_2D(float2 gridcell, out float4 hash_0, out float4 hash_1, out float4 hash_2)
{
    const float2 OFFSET = float2(26.0, 161.0);
    const float DOMAIN = 71.0;
    const float3 SOMELARGEFLOATS = float3(951.135664, 642.949883, 803.202459);
    float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
    P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;
    P += OFFSET.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    hash_0 = frac(P * (1.0 / SOMELARGEFLOATS.x));
    hash_1 = frac(P * (1.0 / SOMELARGEFLOATS.y));
    hash_2 = frac(P * (1.0 / SOMELARGEFLOATS.z));
}

void FAST32_hash_3D(float3 gridcell, out float4 lowz_hash, out float4 highz_hash)
{
    const float2 OFFSET = float2(50.0, 161.0);
    const float DOMAIN = 69.0;
    const float SOMELARGEFLOAT = 635.298681;
    const float ZINC = 48.500388;

    gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
    float3 gridcell_inc1 = step(gridcell, (float3)(DOMAIN - 1.5)) * (gridcell + 1.0);

    float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    highz_hash.xy = float2(1.0 / (SOMELARGEFLOAT + float2(gridcell.z, gridcell_inc1.z) * ZINC));
    lowz_hash = frac(P * highz_hash.xxxx);
    highz_hash = frac(P * highz_hash.yyyy);
}

void FAST32_hash_3D(float3 gridcell,
    float3 v1_mask, float3 v2_mask,
    out float4 hash_0, out float4 hash_1, out float4 hash_2)
{
    const float2 OFFSET = float2(50.0, 161.0);
    const float DOMAIN = 69.0;
    const float3 SOMELARGEFLOATS = float3(635.298681, 682.357502, 668.926525);
    const float3 ZINC = float3(48.500388, 65.294118, 63.934599);

    gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
    float3 gridcell_inc1 = step(gridcell, (float3)(DOMAIN - 1.5)) * (gridcell + 1.0);

    float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;
    P *= P;
    float4 V1xy_V2xy = lerp(P.xyxy, P.zwzw, float4(v1_mask.xy, v2_mask.xy));
    P = float4(P.x, V1xy_V2xy.xz, P.z) * float4(P.y, V1xy_V2xy.yw, P.w);

    float3 lowz_mods = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz));
    float3 highz_mods = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz));

    v1_mask = (v1_mask.z < 0.5) ? lowz_mods : highz_mods;
    v2_mask = (v2_mask.z < 0.5) ? lowz_mods : highz_mods;

    hash_0 = frac(P * float4(lowz_mods.x, v1_mask.x, v2_mask.x, highz_mods.x));
    hash_1 = frac(P * float4(lowz_mods.y, v1_mask.y, v2_mask.y, highz_mods.y));
    hash_2 = frac(P * float4(lowz_mods.z, v1_mask.z, v2_mask.z, highz_mods.z));
}

float4 FAST32_hash_3D(float3 gridcell, float3 v1_mask, float3 v2_mask)
{
    const float2 OFFSET = float2(50.0, 161.0);
    const float DOMAIN = 69.0;
    const float SOMELARGEFLOAT = 635.298681;
    const float ZINC = 48.500388;

    gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
    float3 gridcell_inc1 = step(gridcell, (float3)(DOMAIN - 1.5)) * (gridcell + 1.0);

    float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;
    P *= P;
    float4 V1xy_V2xy = lerp(P.xyxy, P.zwzw, float4(v1_mask.xy, v2_mask.xy));
    P = float4(P.x, V1xy_V2xy.xz, P.z) * float4(P.y, V1xy_V2xy.yw, P.w);

    float2 V1z_V2z = float2(v1_mask.z < 0.5 ? gridcell.z : gridcell_inc1.z, v2_mask.z < 0.5 ? gridcell.z : gridcell_inc1.z);
    float4 mod_vals = float4(1.0 / (SOMELARGEFLOAT + float4(gridcell.z, V1z_V2z, gridcell_inc1.z) * ZINC));

    return frac(P * mod_vals);
}

void FAST32_hash_3D(float3 gridcell,
    out float4 lowz_hash_0, out float4 lowz_hash_1, out float4 lowz_hash_2,
    out float4 highz_hash_0, out float4 highz_hash_1, out float4 highz_hash_2)
{
    const float2 OFFSET = float2(50.0, 161.0);
    const float DOMAIN = 69.0;
    const float3 SOMELARGEFLOATS = float3(635.298681, 682.357502, 668.926525);
    const float3 ZINC = float3(48.500388, 65.294118, 63.934599);

    gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
    float3 gridcell_inc1 = step(gridcell, (float3)(DOMAIN - 1.5)) * (gridcell + 1.0);

    float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    float3 lowz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz));
    float3 highz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz));
    lowz_hash_0 = frac(P * lowz_mod.xxxx);
    highz_hash_0 = frac(P * highz_mod.xxxx);
    lowz_hash_1 = frac(P * lowz_mod.yyyy);
    highz_hash_1 = frac(P * highz_mod.yyyy);
    lowz_hash_2 = frac(P * lowz_mod.zzzz);
    highz_hash_2 = frac(P * highz_mod.zzzz);
}

// C1 continuous: 3x^2-2x^3 (Hermite curve, same as smoothstep)
float  Interpolation_C1(float  x) { return x * x * (3.0 - 2.0 * x); }
float2 Interpolation_C1(float2 x) { return x * x * (3.0 - 2.0 * x); }
float3 Interpolation_C1(float3 x) { return x * x * (3.0 - 2.0 * x); }
float4 Interpolation_C1(float4 x) { return x * x * (3.0 - 2.0 * x); }

// C2 continuous: 6x^5-15x^4+10x^3 (Quintic curve, Perlin improved noise)
float  Interpolation_C2(float  x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
float2 Interpolation_C2(float2 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
float4 Interpolation_C2(float4 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }

// C2 interpolation + derivative combined for 2D
float4 Interpolation_C2_InterpAndDeriv(float2 x)
{
    return x.xyxy * x.xyxy * (x.xyxy * (x.xyxy * (x.xyxy * float2(6.0, 0.0).xxyy + float2(-15.0, 30.0).xxyy) + float2(10.0, -60.0).xxyy) + float2(0.0, 30.0).xxyy);
}

// C2 derivative only
float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }

float  Falloff_Xsq_C1(float  xsq) { xsq = 1.0 - xsq; return xsq * xsq; }
float  Falloff_Xsq_C2(float  xsq) { xsq = 1.0 - xsq; return xsq * xsq * xsq; }
float4 Falloff_Xsq_C2(float4 xsq) { xsq = 1.0 - xsq; return xsq * xsq * xsq; }

float4 Cellular_weight_samples(float4 samples)
{
    samples = samples * 2.0 - 1.0;
    return (samples * samples * samples) - sign(samples);
}

// Value Noise 2D -- range 0.0 -> 1.0
float Value2D(float2 P)
{
    float2 Pi = floor(P);
    float2 Pf = P - Pi;
    float4 hash = FAST32_hash_2D(Pi);
    float2 blend = Interpolation_C2(Pf);
    float4 blend2 = float4(blend, float2(1.0 - blend));
    return dot(hash, blend2.zxzx * blend2.wwyy);
}

// Value Noise 3D -- range 0.0 -> 1.0
float Value3D(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;
    float4 hash_lowz, hash_highz;
    FAST32_hash_3D(Pi, hash_lowz, hash_highz);
    float3 blend = Interpolation_C2(Pf);
    float4 res0 = lerp(hash_lowz, hash_highz, blend.z);
    float4 blend2 = float4(blend.xy, float2(1.0 - blend.xy));
    return dot(res0, blend2.zxzx * blend2.wwyy);
}

// Perlin Noise 2D -- range -1.0 -> 1.0
float Perlin2D(float2 P)
{
    float2 Pi = floor(P);
    float4 Pf_Pfmin1 = P.xyxy - float4(Pi, Pi + 1.0);

    float4 hash = FAST32_hash_2D(Pi);
    hash -= 0.5;
    float4 grad_results = Pf_Pfmin1.xzxz * sign(hash) + Pf_Pfmin1.yyww * sign(abs(hash) - 0.25);

    float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
    float4 blend2 = float4(blend, float2(1.0 - blend));
    return dot(grad_results, blend2.zxzx * blend2.wwyy);
}

// Perlin Noise 3D -- range -1.0 -> 1.0
float Perlin3D(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;
    float3 Pf_min1 = Pf - 1.0;

    float4 hash_lowz, hash_highz;
    FAST32_hash_3D(Pi, hash_lowz, hash_highz);

    hash_lowz -= 0.5;
    float4 grad_results_0_0 = float2(Pf.x, Pf_min1.x).xyxy * sign(hash_lowz);
    hash_lowz = abs(hash_lowz) - 0.25;
    float4 grad_results_0_1 = float2(Pf.y, Pf_min1.y).xxyy * sign(hash_lowz);
    float4 grad_results_0_2 = Pf.zzzz * sign(abs(hash_lowz) - 0.125);
    float4 grad_results_0 = grad_results_0_0 + grad_results_0_1 + grad_results_0_2;

    hash_highz -= 0.5;
    float4 grad_results_1_0 = float2(Pf.x, Pf_min1.x).xyxy * sign(hash_highz);
    hash_highz = abs(hash_highz) - 0.25;
    float4 grad_results_1_1 = float2(Pf.y, Pf_min1.y).xxyy * sign(hash_highz);
    float4 grad_results_1_2 = Pf_min1.zzzz * sign(abs(hash_highz) - 0.125);
    float4 grad_results_1 = grad_results_1_0 + grad_results_1_1 + grad_results_1_2;

    float3 blend = Interpolation_C2(Pf);
    float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);
    float4 blend2 = float4(blend.xy, float2(1.0 - blend.xy));
    return dot(res0, blend2.zxzx * blend2.wwyy) * (2.0 / 3.0);
}

// Calculate the 4 vectors from corners of simplex pyramid to the point
void Simplex3D_GetCornerVectors(float3 P,
    out float3 Pi, out float3 Pi_1, out float3 Pi_2,
    out float4 v1234_x, out float4 v1234_y, out float4 v1234_z)
{
    const float SKEWFACTOR = 1.0 / 3.0;
    const float UNSKEWFACTOR = 1.0 / 6.0;
    const float SIMPLEX_CORNER_POS = 0.5;
    const float SIMPLEX_PYRAMID_HEIGHT = 0.70710678118654752440084436210485;

    P *= SIMPLEX_PYRAMID_HEIGHT;
    Pi = floor(P + dot(P, (float3)SKEWFACTOR));
    float3 x0 = P - Pi + dot(Pi, (float3)UNSKEWFACTOR);
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    Pi_1 = min(g.xyz, l.zxy);
    Pi_2 = max(g.xyz, l.zxy);
    float3 x1 = x0 - Pi_1 + UNSKEWFACTOR;
    float3 x2 = x0 - Pi_2 + SKEWFACTOR;
    float3 x3 = x0 - SIMPLEX_CORNER_POS;

    v1234_x = float4(x0.x, x1.x, x2.x, x3.x);
    v1234_y = float4(x0.y, x1.y, x2.y, x3.y);
    v1234_z = float4(x0.z, x1.z, x2.z, x3.z);
}

// Surflet weights for 3D simplex
float4 Simplex3D_GetSurfletWeights(float4 v1234_x, float4 v1234_y, float4 v1234_z)
{
    float4 surflet_weights = v1234_x * v1234_x + v1234_y * v1234_y + v1234_z * v1234_z;
    surflet_weights = max(0.5 - surflet_weights, 0.0);
    return surflet_weights * surflet_weights * surflet_weights;
}

// SimplexPerlin 2D -- range -1.0 -> 1.0
float SimplexPerlin2D(float2 P)
{
    const float SKEWFACTOR = 0.36602540378443864676372317075294;
    const float UNSKEWFACTOR = 0.21132486540518711774542560974902;
    const float SIMPLEX_TRI_HEIGHT = 0.70710678118654752440084436210485;
    const float3 SIMPLEX_POINTS = float3(1.0 - UNSKEWFACTOR, -UNSKEWFACTOR, 1.0 - 2.0 * UNSKEWFACTOR);

    P *= SIMPLEX_TRI_HEIGHT;
    float2 Pi = floor(P + dot(P, (float2)SKEWFACTOR));

    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    float2 v0 = Pi - dot(Pi, (float2)UNSKEWFACTOR) - P;
    float4 v1pos_v1hash = (v0.x < v0.y) ? float4(SIMPLEX_POINTS.xy, hash_x.y, hash_y.y) : float4(SIMPLEX_POINTS.yx, hash_x.z, hash_y.z);
    float4 v12 = float4(v1pos_v1hash.xy, SIMPLEX_POINTS.zz) + v0.xyxy;

    float3 grad_x = float3(hash_x.x, v1pos_v1hash.z, hash_x.w) - 0.49999;
    float3 grad_y = float3(hash_y.x, v1pos_v1hash.w, hash_y.w) - 0.49999;
    float3 grad_results = rsqrt(grad_x * grad_x + grad_y * grad_y) * (grad_x * float3(v0.x, v12.xz) + grad_y * float3(v0.y, v12.yw));

    const float FINAL_NORMALIZATION = 99.204334582718712976990005025589;

    float3 m = float3(v0.x, v12.xz) * float3(v0.x, v12.xz) + float3(v0.y, v12.yw) * float3(v0.y, v12.yw);
    m = max(0.5 - m, 0.0);
    m = m * m;
    return dot(m * m, grad_results) * FINAL_NORMALIZATION;
}

// SimplexPerlin 3D -- range -1.0 -> 1.0
float SimplexPerlin3D(float3 P)
{
    float3 Pi, Pi_1, Pi_2;
    float4 v1234_x, v1234_y, v1234_z;
    Simplex3D_GetCornerVectors(P, Pi, Pi_1, Pi_2, v1234_x, v1234_y, v1234_z);

    float4 hash_0, hash_1, hash_2;
    FAST32_hash_3D(Pi, Pi_1, Pi_2, hash_0, hash_1, hash_2);
    hash_0 -= 0.49999;
    hash_1 -= 0.49999;
    hash_2 -= 0.49999;

    float4 grad_results = rsqrt(hash_0 * hash_0 + hash_1 * hash_1 + hash_2 * hash_2) * (hash_0 * v1234_x + hash_1 * v1234_y + hash_2 * v1234_z);

    const float FINAL_NORMALIZATION = 37.837227241611314102871574478976;
    return dot(Simplex3D_GetSurfletWeights(v1234_x, v1234_y, v1234_z), grad_results) * FINAL_NORMALIZATION;
}

// Cellular 2D -- range 0.0 -> 1.0
float Cellular2D(float2 P)
{
    float2 Pi = floor(P);
    float2 Pf = P - Pi;

    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    // Restrict random point offset to guarantee no artifacts
    const float JITTER_WINDOW = 0.25;
    hash_x = Cellular_weight_samples(hash_x) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y = Cellular_weight_samples(hash_y) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);

    float4 dx = Pf.xxxx - hash_x;
    float4 dy = Pf.yyyy - hash_y;
    float4 d = dx * dx + dy * dy;
    d.xy = min(d.xy, d.zw);
    return min(d.x, d.y) * (1.0 / 1.125);
}

// Cellular 3D -- range 0.0 -> 1.0
float Cellular3D(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;

    float4 hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1;
    FAST32_hash_3D(Pi, hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1);

    const float JITTER_WINDOW = 0.166666666;
    hash_x0 = Cellular_weight_samples(hash_x0) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y0 = Cellular_weight_samples(hash_y0) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
    hash_x1 = Cellular_weight_samples(hash_x1) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y1 = Cellular_weight_samples(hash_y1) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
    hash_z0 = Cellular_weight_samples(hash_z0) * JITTER_WINDOW + float4(0.0, 0.0, 0.0, 0.0);
    hash_z1 = Cellular_weight_samples(hash_z1) * JITTER_WINDOW + float4(1.0, 1.0, 1.0, 1.0);

    float4 dx1 = Pf.xxxx - hash_x0;
    float4 dy1 = Pf.yyyy - hash_y0;
    float4 dz1 = Pf.zzzz - hash_z0;
    float4 dx2 = Pf.xxxx - hash_x1;
    float4 dy2 = Pf.yyyy - hash_y1;
    float4 dz2 = Pf.zzzz - hash_z1;
    float4 d1 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1;
    float4 d2 = dx2 * dx2 + dy2 * dy2 + dz2 * dz2;
    d1 = min(d1, d2);
    d1.xy = min(d1.xy, d1.wz);
    return min(d1.x, d1.y) * (9.0 / 12.0);
}

// Value2D with derivatives -- returns float3(value, xderiv, yderiv)
float3 Value2D_Deriv(float2 P)
{
    float2 Pi = floor(P);
    float2 Pf = P - Pi;
    float4 hash = FAST32_hash_2D(Pi);
    float4 blend = Interpolation_C2_InterpAndDeriv(Pf);
    float4 res0 = lerp(hash.xyxz, hash.zwyw, blend.yyxx);
    return float3(res0.x, 0.0, 0.0) + (res0.yyw - res0.xxz) * blend.xzw;
}

// Value3D with derivatives -- returns float4(value, xderiv, yderiv, zderiv)
float4 Value3D_Deriv(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;
    float4 hash_lowz, hash_highz;
    FAST32_hash_3D(Pi, hash_lowz, hash_highz);
    float3 blend = Interpolation_C2(Pf);
    float4 res0 = lerp(hash_lowz, hash_highz, blend.z);
    float4 res1 = lerp(res0.xyxz, res0.zwyw, blend.yyxx);
    float4 res3 = lerp(float4(hash_lowz.xy, hash_highz.xy), float4(hash_lowz.zw, hash_highz.zw), blend.y);
    float2 res4 = lerp(res3.xz, res3.yw, blend.x);
    return float4(res1.x, 0.0, 0.0, 0.0) + (float4(res1.yyw, res4.y) - float4(res1.xxz, res4.x)) * float4(blend.x, Interpolation_C2_Deriv(Pf));
}

// Perlin2D with derivatives -- returns float3(value, xderiv, yderiv)
float3 Perlin2D_Deriv(float2 P)
{
    float2 Pi = floor(P);
    float4 Pf_Pfmin1 = P.xyxy - float4(Pi, Pi + 1.0);

    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    float4 grad_x = hash_x - 0.49999;
    float4 grad_y = hash_y - 0.49999;
    float4 norm = rsqrt(grad_x * grad_x + grad_y * grad_y);
    grad_x *= norm;
    grad_y *= norm;
    float4 dotval = (grad_x * Pf_Pfmin1.xzxz + grad_y * Pf_Pfmin1.yyww);

    float3 dotval0_grad0 = float3(dotval.x, grad_x.x, grad_y.x);
    float3 dotval1_grad1 = float3(dotval.y, grad_x.y, grad_y.y);
    float3 dotval2_grad2 = float3(dotval.z, grad_x.z, grad_y.z);
    float3 dotval3_grad3 = float3(dotval.w, grad_x.w, grad_y.w);

    float3 k0_gk0 = dotval1_grad1 - dotval0_grad0;
    float3 k1_gk1 = dotval2_grad2 - dotval0_grad0;
    float3 k2_gk2 = dotval3_grad3 - dotval2_grad2 - k0_gk0;

    float4 blend = Interpolation_C2_InterpAndDeriv(Pf_Pfmin1.xy);
    float3 results = dotval0_grad0 + blend.x * k0_gk0 + blend.y * (k1_gk1 + blend.x * k2_gk2);
    results.yz += blend.zw * (float2(k0_gk0.x, k1_gk1.x) + blend.yx * k2_gk2.xx);

    return results * 1.4142135623730950488016887242097;
}

// Perlin3D with derivatives -- returns float4(value, xderiv, yderiv, zderiv)
float4 Perlin3D_Deriv(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;
    float3 Pf_min1 = Pf - 1.0;

    float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
    FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

    float4 grad_x0 = hashx0 - 0.49999;
    float4 grad_y0 = hashy0 - 0.49999;
    float4 grad_z0 = hashz0 - 0.49999;
    float4 grad_x1 = hashx1 - 0.49999;
    float4 grad_y1 = hashy1 - 0.49999;
    float4 grad_z1 = hashz1 - 0.49999;
    float4 norm_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);
    float4 norm_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);
    grad_x0 *= norm_0; grad_y0 *= norm_0; grad_z0 *= norm_0;
    grad_x1 *= norm_1; grad_y1 *= norm_1; grad_z1 *= norm_1;

    float4 dotval_0 = float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0;
    float4 dotval_1 = float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1;

    float4 dotval0_grad0 = float4(dotval_0.x, grad_x0.x, grad_y0.x, grad_z0.x);
    float4 dotval1_grad1 = float4(dotval_0.y, grad_x0.y, grad_y0.y, grad_z0.y);
    float4 dotval2_grad2 = float4(dotval_0.z, grad_x0.z, grad_y0.z, grad_z0.z);
    float4 dotval3_grad3 = float4(dotval_0.w, grad_x0.w, grad_y0.w, grad_z0.w);
    float4 dotval4_grad4 = float4(dotval_1.x, grad_x1.x, grad_y1.x, grad_z1.x);
    float4 dotval5_grad5 = float4(dotval_1.y, grad_x1.y, grad_y1.y, grad_z1.y);
    float4 dotval6_grad6 = float4(dotval_1.z, grad_x1.z, grad_y1.z, grad_z1.z);
    float4 dotval7_grad7 = float4(dotval_1.w, grad_x1.w, grad_y1.w, grad_z1.w);

    float4 k0_gk0 = dotval1_grad1 - dotval0_grad0;
    float4 k1_gk1 = dotval2_grad2 - dotval0_grad0;
    float4 k2_gk2 = dotval4_grad4 - dotval0_grad0;
    float4 k3_gk3 = dotval3_grad3 - dotval2_grad2 - k0_gk0;
    float4 k4_gk4 = dotval5_grad5 - dotval4_grad4 - k0_gk0;
    float4 k5_gk5 = dotval6_grad6 - dotval4_grad4 - k1_gk1;
    float4 k6_gk6 = (dotval7_grad7 - dotval6_grad6) - (dotval5_grad5 - dotval4_grad4) - k3_gk3;

    float3 blend = Interpolation_C2(Pf);
    float3 blendDeriv = Interpolation_C2_Deriv(Pf);
    float u = blend.x, v = blend.y, w = blend.z;

    float4 result = dotval0_grad0
        + u * (k0_gk0 + v * k3_gk3)
        + v * (k1_gk1 + w * k5_gk5)
        + w * (k2_gk2 + u * (k4_gk4 + v * k6_gk6));

    result.y += dot(float4(k0_gk0.x, k3_gk3.x * v, float2(k4_gk4.x, k6_gk6.x * v) * w), (float4)(blendDeriv.x));
    result.z += dot(float4(k1_gk1.x, k3_gk3.x * u, float2(k5_gk5.x, k6_gk6.x * u) * w), (float4)(blendDeriv.y));
    result.w += dot(float4(k2_gk2.x, k4_gk4.x * u, float2(k5_gk5.x, k6_gk6.x * u) * v), (float4)(blendDeriv.z));

    return result * 1.1547005383792515290182975610039;
}

// SimplexPerlin2D with derivatives -- returns float3(value, xderiv, yderiv)
float3 SimplexPerlin2D_Deriv(float2 P)
{
    const float SKEWFACTOR = 0.36602540378443864676372317075294;
    const float UNSKEWFACTOR = 0.21132486540518711774542560974902;
    const float SIMPLEX_TRI_HEIGHT = 0.70710678118654752440084436210485;
    const float3 SIMPLEX_POINTS = float3(1.0 - UNSKEWFACTOR, -UNSKEWFACTOR, 1.0 - 2.0 * UNSKEWFACTOR);

    P *= SIMPLEX_TRI_HEIGHT;
    float2 Pi = floor(P + dot(P, (float2)SKEWFACTOR));

    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    float2 v0 = Pi - dot(Pi, (float2)UNSKEWFACTOR) - P;
    float4 v1pos_v1hash = (v0.x < v0.y) ? float4(SIMPLEX_POINTS.xy, hash_x.y, hash_y.y) : float4(SIMPLEX_POINTS.yx, hash_x.z, hash_y.z);
    float4 v12 = float4(v1pos_v1hash.xy, SIMPLEX_POINTS.zz) + v0.xyxy;

    float3 grad_x = float3(hash_x.x, v1pos_v1hash.z, hash_x.w) - 0.49999;
    float3 grad_y = float3(hash_y.x, v1pos_v1hash.w, hash_y.w) - 0.49999;
    float3 norm = rsqrt(grad_x * grad_x + grad_y * grad_y);
    grad_x *= norm;
    grad_y *= norm;
    float3 grad_results = grad_x * float3(v0.x, v12.xz) + grad_y * float3(v0.y, v12.yw);

    float3 m = float3(v0.x, v12.xz) * float3(v0.x, v12.xz) + float3(v0.y, v12.yw) * float3(v0.y, v12.yw);
    m = max(0.5 - m, 0.0);
    float3 m2 = m * m;
    float3 m4 = m2 * m2;

    float3 temp = 8.0 * m2 * m * grad_results;
    float xderiv = dot(temp, float3(v0.x, v12.xz)) - dot(m4, grad_x);
    float yderiv = dot(temp, float3(v0.y, v12.yw)) - dot(m4, grad_y);

    const float FINAL_NORMALIZATION = 99.204334582718712976990005025589;
    return float3(dot(m4, grad_results), xderiv, yderiv) * FINAL_NORMALIZATION;
}

// SimplexPerlin3D with derivatives -- returns float4(value, xderiv, yderiv, zderiv)
float4 SimplexPerlin3D_Deriv(float3 P)
{
    float3 Pi, Pi_1, Pi_2;
    float4 v1234_x, v1234_y, v1234_z;
    Simplex3D_GetCornerVectors(P, Pi, Pi_1, Pi_2, v1234_x, v1234_y, v1234_z);

    float4 hash_0, hash_1, hash_2;
    FAST32_hash_3D(Pi, Pi_1, Pi_2, hash_0, hash_1, hash_2);
    hash_0 -= 0.49999;
    hash_1 -= 0.49999;
    hash_2 -= 0.49999;

    float4 norm = rsqrt(hash_0 * hash_0 + hash_1 * hash_1 + hash_2 * hash_2);
    hash_0 *= norm; hash_1 *= norm; hash_2 *= norm;
    float4 grad_results = hash_0 * v1234_x + hash_1 * v1234_y + hash_2 * v1234_z;

    float4 m = v1234_x * v1234_x + v1234_y * v1234_y + v1234_z * v1234_z;
    m = max(0.5 - m, 0.0);
    float4 m2 = m * m;
    float4 m3 = m * m2;

    float4 temp = -6.0 * m2 * grad_results;
    float xderiv = dot(temp, v1234_x) + dot(m3, hash_0);
    float yderiv = dot(temp, v1234_y) + dot(m3, hash_1);
    float zderiv = dot(temp, v1234_z) + dot(m3, hash_2);

    const float FINAL_NORMALIZATION = 37.837227241611314102871574478976;
    return float4(dot(m3, grad_results), xderiv, yderiv, zderiv) * FINAL_NORMALIZATION;
}

// Cellular2D with derivatives -- returns float3(value, xderiv, yderiv)
float3 Cellular2D_Deriv(float2 P)
{
    float2 Pi = floor(P);
    float2 Pf = P - Pi;

    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    const float JITTER_WINDOW = 0.25;
    hash_x = Cellular_weight_samples(hash_x) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y = Cellular_weight_samples(hash_y) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);

    float4 dx = Pf.xxxx - hash_x;
    float4 dy = Pf.yyyy - hash_y;
    float4 d = dx * dx + dy * dy;
    float3 t1 = d.x < d.y ? float3(d.x, dx.x, dy.x) : float3(d.y, dx.y, dy.y);
    float3 t2 = d.z < d.w ? float3(d.z, dx.z, dy.z) : float3(d.w, dx.w, dy.w);
    return (t1.x < t2.x ? t1 : t2) * float3(1.0, 2.0, 2.0) * (1.0 / 1.125);
}

// Cellular3D with derivatives -- returns float4(value, xderiv, yderiv, zderiv)
float4 Cellular3D_Deriv(float3 P)
{
    float3 Pi = floor(P);
    float3 Pf = P - Pi;

    float4 hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1;
    FAST32_hash_3D(Pi, hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1);

    const float JITTER_WINDOW = 0.166666666;
    hash_x0 = Cellular_weight_samples(hash_x0) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y0 = Cellular_weight_samples(hash_y0) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
    hash_x1 = Cellular_weight_samples(hash_x1) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
    hash_y1 = Cellular_weight_samples(hash_y1) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
    hash_z0 = Cellular_weight_samples(hash_z0) * JITTER_WINDOW + float4(0.0, 0.0, 0.0, 0.0);
    hash_z1 = Cellular_weight_samples(hash_z1) * JITTER_WINDOW + float4(1.0, 1.0, 1.0, 1.0);

    float4 dx1 = Pf.xxxx - hash_x0;
    float4 dy1 = Pf.yyyy - hash_y0;
    float4 dz1 = Pf.zzzz - hash_z0;
    float4 dx2 = Pf.xxxx - hash_x1;
    float4 dy2 = Pf.yyyy - hash_y1;
    float4 dz2 = Pf.zzzz - hash_z1;
    float4 d1 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1;
    float4 d2 = dx2 * dx2 + dy2 * dy2 + dz2 * dz2;
    float4 r1 = d1.x < d1.y ? float4(d1.x, dx1.x, dy1.x, dz1.x) : float4(d1.y, dx1.y, dy1.y, dz1.y);
    float4 r2 = d1.z < d1.w ? float4(d1.z, dx1.z, dy1.z, dz1.z) : float4(d1.w, dx1.w, dy1.w, dz1.w);
    float4 r3 = d2.x < d2.y ? float4(d2.x, dx2.x, dy2.x, dz2.x) : float4(d2.y, dx2.y, dy2.y, dz2.y);
    float4 r4 = d2.z < d2.w ? float4(d2.z, dx2.z, dy2.z, dz2.z) : float4(d2.w, dx2.w, dy2.w, dz2.w);
    float4 t1 = r1.x < r2.x ? r1 : r2;
    float4 t2 = r3.x < r4.x ? r3 : r4;
    return (t1.x < t2.x ? t1 : t2) * float4(1.0, (float3)2.0) * (9.0 / 12.0);
}

// Returns perlin fractal noise value. Output normalized to ~0.0 -> 0.5 range.
float PerlinFractalNoise(float xPos, float yPos, int octaves)
{
    float value = 0.0;
    int powInt = 1;
    for (int o = 0; o < octaves; o++)
    {
        value += ((Perlin2D(float2(xPos * (float)powInt, yPos * (float)powInt)) + 1.0) * 0.5) / (float)(powInt * 2);
        powInt *= 2;
    }
    return value;
}

#endif // DUNKSJAMS_NOISE_INCLUDED
