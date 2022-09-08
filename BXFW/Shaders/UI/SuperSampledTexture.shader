Shader "Custom/SuperSampledTexture"
{
    Properties {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _Bias ("Mip Bias", Range(-4, 4)) = -0.75
        [KeywordEnum(Off, 2x2 RGSS, 8x Halton, 16x16 OGSS)] _SuperSampling ("Super Sampling Technique", Float) = 1
        _AAScale ("AA Pixel Width", Range(0.75, 10.0)) = 1.25
    }
     
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
     
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
     
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
     
            #pragma shader_feature _ _SUPERSAMPLING_2X2_RGSS _SUPERSAMPLING_8X_HALTON _SUPERSAMPLING_16X16_OGSS
     
            half4 tex2DSS(sampler2D tex, float2 uv, float bias, float aascale)
            {
                // get uv derivatives, optionally scaled to reduce aliasing at the cost of clarity
                // used by all 3 super sampling options
                float2 dx = ddx(uv * aascale);
                float2 dy = ddy(uv * aascale);
                 
                half4 col = 0;
     
            #if defined(_SUPERSAMPLING_2X2_RGSS)
                // MSAA style "four rooks" rotated grid super sampling
                // samples the texture 4 times
     
                float2 uvOffsets = float2(0.125, 0.375);
     
                col += tex2Dbias(tex, float4(uv + uvOffsets.x * dx + uvOffsets.y * dy, 0, bias));
                col += tex2Dbias(tex, float4(uv - uvOffsets.x * dx - uvOffsets.y * dy, 0, bias));
                col += tex2Dbias(tex, float4(uv + uvOffsets.y * dx - uvOffsets.x * dy, 0, bias));
                col += tex2Dbias(tex, float4(uv - uvOffsets.y * dx + uvOffsets.x * dy, 0, bias));
     
                col *= 0.25;
            #elif defined(_SUPERSAMPLING_8X_HALTON)
                // 8 points from a 2, 3 Halton sequence
                // similar to what TAA uses, though they usually use more points
                // samples the texture 8 times
                // better quality for really fine details
     
                float2 halton[8] = {
                    float2(1,-3) / 16.0,
                    float2(-1,3) / 16.0,
                    float2(5,1) / 16.0,
                    float2(-3,-5) / 16.0,
                    float2(-5,5) / 16.0,
                    float2(-7,-1) / 16.0,
                    float2(3,7) / 16.0,
                    float2(7,-7) / 16.0
                };
     
                for (int i=0; i<8; i++)
                    col += tex2Dbias(tex, float4(uv + halton[i].x * dx + halton[i].y * dy, 0, bias));
     
                col *= 0.125;
            #elif defined(_SUPERSAMPLING_16X16_OGSS)
                // brute force ground truth 16x16 ordered grid super sampling
                // samples the texture 256 times! you should not use this!
                // does not use tex2Dbias, but instead always samples the top mip
     
                float gridDim = 16;
                float halfGridDim = gridDim / 2;
     
                for (float u=0; u<gridDim; u++)
                {
                    float uOffset = (u - halfGridDim + 0.5) / gridDim;
                    for (float v=0; v<gridDim; v++)
                    {
                        float vOffset = (v - halfGridDim + 0.5) / gridDim;
                        col += tex2Dlod(tex, float4(uv + uOffset * dx + vOffset * dy, 0, 0));
                    }
                }
     
                col /= (gridDim * gridDim);
            #else
                // no super sampling, just bias
     
                col = tex2Dbias(tex, float4(uv, 0, bias));
            #endif
                return col;
            }
     
            sampler2D _MainTex;
            float _Bias;
            float _AAScale;
     
            struct v2f {
                float4 pos : SV_Position;
                float2 uv : TEXCOORD0;
            };
     
            void vert(appdata_base v, out v2f o)
            {
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
            }
     
            half4 frag(v2f i) : SV_Target
            {
                return tex2DSS(_MainTex, i.uv, _Bias, _AAScale);
            }
     
            ENDCG
        }
    }
}
