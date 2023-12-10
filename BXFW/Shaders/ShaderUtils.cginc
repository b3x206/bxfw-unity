#ifndef __SHADER_UTILS__
#define __SHADER_UTILS__

#include "UnityCG.cginc"

#define HEATMAP_COLORS_COUNT 6
// Makes a heatmap from a value, interpolating the colors
// Maybe useful for troubleshooting?
float4 HeatMapColor(float value, float minValue, float maxValue)
{
    float4 colors[HEATMAP_COLORS_COUNT] =
    {
        float4(0.32, 0.00, 0.32, 1.00),
        float4(0.00, 0.00, 1.00, 1.00),
        float4(0.00, 1.00, 0.00, 1.00),
        float4(1.00, 1.00, 0.00, 1.00),
        float4(1.00, 0.60, 0.00, 1.00),
        float4(1.00, 0.00, 0.00, 1.00),
    };

    float ratio = (HEATMAP_COLORS_COUNT - 1.0) * saturate((value - minValue) / (maxValue - minValue));
    float indexMin = floor(ratio);
    float indexMax = min(indexMin + 1, HEATMAP_COLORS_COUNT - 1);

    return lerp(colors[indexMin], colors[indexMax], ratio - indexMin);
}

#endif