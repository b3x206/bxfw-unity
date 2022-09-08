Shader "Custom/Diffuse/Dithered Transparent/Dithered From Texture"
{
    Properties 
    {
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        _DitherScale("Dither Scale", Float) = 10
        [NoScaleOffset] _DitherTex("Dither Texture", 2D) = "white" {}
        [MaterialToggle] _ShadowDisappear("Allow Shadow Disappear", float) = 1
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {            
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "DitherFunctions.cginc"
            #pragma vertex vert
            #pragma fragment frag
            
            uniform fixed4 _LightColor0;

            float4 _Color;
            float4 _MainTex_ST;         // For the Main Tex UV transform
            sampler2D _MainTex;         // Texture used for the line
            
            float _DitherScale;
            sampler2D _DitherTex;

            struct v2f
            {
                float4 pos      : POSITION;
                float4 col      : COLOR;
                float2 uv       : TEXCOORD0;
                float4 spos     : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                float4 norm = mul(unity_ObjectToWorld, v.normal); 
                float3 normalDirection = normalize(norm.xyz);
                float4 AmbientLight = UNITY_LIGHTMODEL_AMBIENT;
                float4 LightDirection = normalize(_WorldSpaceLightPos0);
                // Bug fix : normalDirection was negated, unnegating for correct diffuse lightning.
                float4 DiffuseLight = saturate(dot(LightDirection, normalDirection)) * _LightColor0;
                o.col = float4(AmbientLight + DiffuseLight);
                o.spos = ComputeScreenPos(o.pos);

                return o;
            }

            float4 frag(v2f i) : COLOR
            { 
                float4 col = _Color * tex2D(_MainTex, i.uv);
                ditherClip(i.spos.xy / i.spos.w, col.a, _DitherTex, _DitherScale);

                return col * i.col;
            }

            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType" = "ShadowCaster" }
        UsePass "Hidden/Custom/DitheredTransparentUtils/SHADOW"
    }
}
