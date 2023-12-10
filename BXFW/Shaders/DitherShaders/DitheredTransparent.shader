// TODO : Dithering presets.
Shader "Custom/Diffuse/Dithered Transparent/Dithered"
{
    Properties 
    {
        [MainColor] _Color("Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [Enum(Full,1, Half,2, OneThird,3, Quarter,4, OneEighth,8)] _DitherResolution("Dither Resolution", float) = 1.0 // Use 'Enum' keyword for custom values.
        [MaterialToggle] _ShadowDisappear("Allow Shadow Disappear", int) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "DitherFunctions.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            uniform fixed4 _LightColor0;
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            float4 _MainTex_ST;         // For the Main Tex UV transform
            sampler2D _MainTex;         // Texture used for the line
            float _DitherResolution;

            struct v2f
            {
                float4 pos      : POSITION;
                float4 col      : COLOR;
                float2 uv       : TEXCOORD0;
                float4 spos     : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                //UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    
                // This crap vertexlit bruhh lmaooo
                // This would have looked okay, if it was 1998 or something
                // TODO : Do the lit in fragment.
                // norm = mul(unity_ObjectToWorld, v.normal);
                float3 norm = UnityObjectToWorldNormal(v.normal);
                float3 normalDirection = normalize(norm.xyz);
                float4 AmbientLight = UNITY_LIGHTMODEL_AMBIENT;
                float4 LightDirection = normalize(_WorldSpaceLightPos0);
                // Bug fix : normalDirection was negated, unnegating for correct diffuse lightning.
                float4 DiffuseLight = saturate(dot(LightDirection, normalDirection)) * _LightColor0;
                o.col = float4(AmbientLight + DiffuseLight);
                o.spos = ComputeScreenPos(o.pos);
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                //UNITY_SETUP_INSTANCE_ID(i);
                float4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * tex2D(_MainTex, i.uv);
                ditherClip(i.spos.xy / i.spos.w, col.a, _DitherResolution);

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
