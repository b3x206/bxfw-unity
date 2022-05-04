// TODO : Add instancing of material properties : See https://docs.unity3d.com/Manual/GPUInstancing.html

Shader "Custom/Sprite/2DFakeGlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        [HDR] _OutlineColor("Emission Color", Color) = (1,1,1,1)
        _GlowThickness("Glow Thickness", Range(0.01, 0.99)) = .5
        _GlowMaxAlphaThreshold("Glow Alpha Threshold (Edge Glow)", Range(0.01, 1)) = .88
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha // Standard
        ZWrite Off
        Cull Off

        // Show sprite normally
        Pass
        {
            CGPROGRAM
            // Standard lib for : TRANSFORM_TEX and such
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            // Texture
            sampler2D _MainTex;
            float4 _MainTex_ST; // bug fix for TRANSFORM_TEX because unity big dumb
            // Other
            fixed4 _Color;
            //float4 _OutlineColor; // float4 because hdr
            //float _GlowThickness;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f 
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) 
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Add standard color
                col *= _Color;
                col *= i.color;
                return col;
            }
            ENDCG
        }
        // Show fake bloom edges
        Pass
        {
            // Render smooth glow on this pass
            Blend One One // Soft additive
            
            // Basically this only renders the edge in an additive context.
            CGPROGRAM
            // Standard lib for : TRANSFORM_TEX and such
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            // Texture
            sampler2D _MainTex;
            float4 _MainTex_ST;
            // Other
            fixed4 _Color;
            float4 _OutlineColor; // float4 because hdr
            float _GlowThickness;
            float _GlowMaxAlphaThreshold;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f 
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) 
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            // Helper method to remap variable
            float Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Add step outline (this pass is additive)
                float stepAlphaTexture = step(texColor.a, 0.0); // Gets the edge of alpha (note : this edge is rough)
                float stepAlphaTextureOffset = 0.0;
                if (texColor.a < _GlowMaxAlphaThreshold)
                    stepAlphaTextureOffset = lerp(0, texColor.a, _GlowThickness); // Gets the edge of the alpha with offset
                else // Fade out smoothly (Max difference : 1.0 - _GlowMaxAlphaThreshold)
                {
                    float sFadeOut = Unity_Remap_float(texColor.a - _GlowMaxAlphaThreshold, float2(0, 1.0 - _GlowMaxAlphaThreshold), float2(0, 1));
                    stepAlphaTextureOffset = lerp(texColor.a, 0, sFadeOut);
                }

                // This is clamped as it can be negative value on certain condition
                float stepOutline = clamp(stepAlphaTextureOffset - stepAlphaTexture, 0, 1); // Gets the difference

                fixed4 stepColoredOutline = (stepOutline * _OutlineColor) * i.color.a;
                
                return stepColoredOutline;
            }
            ENDCG

        }
    }

    FallBack "Sprites/Default"
}
