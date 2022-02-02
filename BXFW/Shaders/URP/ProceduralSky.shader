// Modified version of the 'BuiltIn Shaders'

Shader "Custom/Skybox/Procedural Sky"
{
    Properties
    {
        [Header(Sun Disc)]
        _SunDiscColor ("Color", Color) = (1, 1, 1, 1)
        _SunDiscMultiplier ("Multiplier", float) = 25
        _SunDiscExponent ("Exponent", float) = 100000

        [Header(Sun Halo)]
        _SunHaloColor ("Color", Color) = (0.9, 0.776, 0.66, 1)
        _SunHaloExponent ("Exponent", float) = 125
        _SunHaloContribution ("Contribution", Range(0, 1)) = 0.75

        [Header(Horizon Line)]
        _HorizonLineColor ("Color", Color) = (0.904, 0.887, 0.791, 1)
        _HorizonLineExponent ("Exponent", float) = 4
        _HorizonLineContribution ("Contribution", Range(0, 1)) = 0.25
        
        [Header(Sky Gradient)]
        _SkyGradientTop ("Top", Color) = (0.172, 0.564, 0.694, 1)
        _SkyGradientBottom ("Bottom", Color) = (0.764, 0.815, 0.85)
        _SkyGradientExponent ("Exponent", float) = 2.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            float3 _SunDiscColor;
            float _SunDiscExponent;
            float _SunDiscMultiplier;

            float3 _SunHaloColor;
            float _SunHaloExponent;
            float _SunHaloContribution;

            float3 _HorizonLineColor;
            float _HorizonLineExponent;
            float _HorizonLineContribution;

            float3 _SkyGradientTop;
            float3 _SkyGradientBottom;
            float _SkyGradientExponent;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Masks.
                float maskHorizon = dot(normalize(i.worldPosition), float3(0, 1, 0));
                float maskSunDir = dot(normalize(i.worldPosition), _WorldSpaceLightPos0.xyz);

                // Sun disc.
                float maskSun = pow(saturate(maskSunDir), _SunDiscExponent);
                maskSun = saturate(maskSun * _SunDiscMultiplier);

                // Sun halo.
                float3 sunHaloColor = _SunHaloColor * _SunHaloContribution;
                float bellCurve = pow(saturate(maskSunDir), _SunHaloExponent * saturate(abs(maskHorizon)));
                float horizonSoften = 1 - pow(1 - saturate(maskHorizon), 50);
                sunHaloColor *= saturate(bellCurve * horizonSoften);

                // Horizon line.
                float3 horizonLineColor = _HorizonLineColor * saturate(pow(1 - abs(maskHorizon), _HorizonLineExponent));
                horizonLineColor = lerp(0, horizonLineColor, _HorizonLineContribution);

                // Sky gradient.
                float3 skyGradientColor = lerp(_SkyGradientTop, _SkyGradientBottom, pow(1 - saturate(maskHorizon), _SkyGradientExponent));

                // Interpolate all.
                float3 finalColor = lerp(saturate(sunHaloColor + horizonLineColor + skyGradientColor), _SunDiscColor, maskSun);

                // Apply color frag.
                return float4(finalColor, 1);
            }
            ENDCG
        }
    }
}
