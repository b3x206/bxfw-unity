// A fake lit shader with texture and color.
// Note that this is absolutely NOT PERFORMANT, it is only used for stuff in the GUI.
Shader "Custom/Unlit/FakeLit"
{
    Properties
    {
        [MainColor] _Color("Main Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}

        _LightColor("Light Color", Color) = (1,1,1,1)
        _LightIntensity("Light Intensity", Range(0.0, 64.0)) = 1.0
        [ShowAsVector3] _LightPos("Light Pos", Vector) = (1,1,1,1)

        _SpecularStrength("Specular Strength", Range(0.0, 1.0)) = 0.5
        _MetallicStrength("Metallic Strength", Range(0.01, 1.0)) = 0.25

       _AmbientLightIntensity("Ambient Light Intensity", Range(0.0, 0.99)) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float4 _Color;
            sampler2D _MainTex;

            float _LightIntensity;
            float4 _LightColor;
            float4 _LightPos;
            
            float _SpecularStrength;
            float _MetallicStrength;

            float _AmbientLight;
            float _AmbientLightIntensity;
            
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                // o.cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Multiply _LightPos with a big number (because it acts weird)

                // Gather vars
                float4 ambient = (_AmbientLightIntensity * _LightColor); // Or use UNITY_LIGHTMODEL_AMBIENT
               
                // Note : For non-uniformly scaled objects, use a custom calculated normal for that matrix
                // norm = mat3(transpose(inverse(model))) * aNormal;  
                float3 normDir = normalize(mul(unity_ObjectToWorld, i.normal).xyz);
                float4 lightDir = normalize((_LightPos * 720) - i.vertex);

                // Diffuse lightning
                float difference = max(dot(normDir, lightDir), 0.0);
                float3 diffuse = (_LightColor * difference) * _LightIntensity;
                
                float4 viewDir = normalize(float4(_WorldSpaceCameraPos, 1.0) - i.vertex);
                float3 reflectDir = reflect(-lightDir, normDir);

                // Specular stuff (the dot that occurs where the light goes with)
                // bad hack : the view dir's dot product also makes values higher than 1 in the back of a spherical object
                // this ternary mitigates that
                float specFrag = difference == 0.0 ? 0.0 : pow(max(dot(viewDir, reflectDir), 0.0), _MetallicStrength * 256.0);
                float4 specular = (_SpecularStrength * specFrag) * _LightColor;

                float3 result = (ambient + diffuse + specular) * _Color;
                float4 col = tex2D(_MainTex, i.texcoord) * float4(result, 1.0);

                return col;
            }

            ENDCG
        }
    }
}
