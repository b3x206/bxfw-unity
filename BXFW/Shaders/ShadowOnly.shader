// Only casts the shadow of the mesh.
Shader "Custom/Unlit/ShadowOnly"  
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
        // Pass to render object as a shadow caster
        Pass 
        {
            Name "SHADOW"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
               
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct v2f 
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            uniform float4 _MainTex_ST;
            v2f vert(appdata_base v) 
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                // _MainTex_ST is used here.
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                return o;
            }

            uniform sampler2D _MainTex;
            uniform fixed4 _Color;
            int _ShadowDisappear = 1;

            float4 frag(v2f i) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, i.uv);
                if (_ShadowDisappear == 1)
                {
                    // Clip shadow if disappear (with .1 offset)
                    clip(texcol.a * _Color.a - 0.1);
                }

                SHADOW_CASTER_FRAGMENT(i)
            }
                
            ENDCG
        }
    }
}