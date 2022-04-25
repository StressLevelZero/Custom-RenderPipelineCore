
float3 SafeNormalizeHighP(float3 inVec)
{
    float3 dp3 = max(REAL_MIN, dot(inVec, inVec));
    return inVec * rsqrt(dp3);
}