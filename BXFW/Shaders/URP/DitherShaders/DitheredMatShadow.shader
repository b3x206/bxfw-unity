// Note : Make sure you have a property named '_ShadowDissapear' 
// to use this with full function.
Shader "Hidden/Custom/DitheredTransparentUtils"
{
    Properties 
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "LightMode" = "ShadowCaster" }

        Pass
        {
            Name "SHADOW"

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "DitherFunctions.cginc"
            #pragma vertex vert
            #pragma fragment frag

            float4 _Color;
            float4 _MainTex_ST;            // For the Main Tex UV transform
            sampler2D _MainTex;            // Texture used for the line
            int _ShadowDisappear = 1;      // Shadow disappear or not.
            float _DitherResolution = 1.0; // Default value is '1.0', dither resolution.

            struct v2f
            {
                float4 pos      : POSITION;
                float2 uv       : TEXCOORD0;
                float4 spos     : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o); // Avoid the 'vert' is not initilazed.
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                float4 col = _Color * tex2D(_MainTex, i.uv);
                
                // Clips the shadow and the disappear control.
                if (_ShadowDisappear == 1)
                {
                    ditherClip(i.spos.xy / i.spos.w, col.a, _DitherResolution);
                }
                //else
                //{
                //    // We need to dither clip normally (or not?)
                //    ditherClip(i.spos.xy / i.spos.w, 1.0, _DitherResolution);
                //}
               

                return float4(0,0,0,0); 
            }

            ENDCG
        }
    }
}
