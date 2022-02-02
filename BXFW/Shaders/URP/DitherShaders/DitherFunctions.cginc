#ifndef __DITHER_FUNCTIONS__
#define __DITHER_FUNCTIONS__
#include "UnityCG.cginc"

// Returns > 0 if not clipped, < 0 if clipped based
// on the dither
// For use with the "clip" function
// pos is the fragment position in screen space from [0,1]
// Same as : https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Dither-Node.html
// this probably is an export of an shader graph code.
float isDithered(float2 pos, float alpha, float dtResScale) 
{
    // Split the _ScreenParams.xy to get resolution down sampling.
    pos *= (_ScreenParams.xy / dtResScale);
    // TODO (maybe) : Use or allow a fixed resolution 

    // Define a dither threshold matrix which can
    // be used to define how a 4x4 set of pixels that will be dithered
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };

    // ogl2 requires us to use 'fmod()' modulus function.
    // (because in ogl2 we don't really know if a variable is a float or an int type)
    uint index = fmod(uint(pos.x), 4U) * 4U + fmod(uint(pos.y), 4U);
    return alpha - DITHER_THRESHOLDS[index];
}

// Returns whether the pixel should be discarded based
// on the dither texture
// pos is the fragment position in screen space from [0,1]
float isDithered(float2 pos, float alpha, sampler2D tex, float scale) 
{
    pos *= _ScreenParams.xy;

    // offset so we're centered
    pos.x -= _ScreenParams.x / 2;
    pos.y -= _ScreenParams.y / 2;
    
    // scale the texture
    pos.x /= scale;
    pos.y /= scale;

    // ensure that we clip if the alpha is zero by
    // subtracting a small value when alpha == 0, because
    // the clip function only clips when < 0
    return alpha - tex2D(tex, pos.xy).r - (0.0001 * (1 - ceil(alpha)));
}

// Helpers that call the above functions and clip if necessary
void ditherClip(float2 pos, float alpha, float resolution) 
{
    clip(isDithered(pos, alpha, resolution));
}

void ditherClip(float2 pos, float alpha, sampler2D tex, float scale) 
{
    clip(isDithered(pos, alpha, tex, scale));
}
#endif