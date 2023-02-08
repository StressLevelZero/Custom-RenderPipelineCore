#if !defined(SLZ_DEFAULT_SAMPLERS)
#define SLZ_DEFAULT_SAMPLERS

// a set of default samplers that can be easily reused. Avoids defining redundant samplers in different include files.

SAMPLER(sampler_point_repeat);
SAMPLER(sampler_linear_repeat);
SAMPLER(sampler_trilinear_repeat);

SAMPLER(sampler_point_clamp);
SAMPLER(sampler_linear_clamp);
SAMPLER(sampler_trilinear_clamp);

#endif