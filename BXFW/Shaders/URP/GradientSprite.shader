// The default unity sprite, that can use gradients
Shader "Custom/SpriteGradient"
{
    Properties
    {
        [PerRendererData] [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [Enum(FromTo,0, Center,1, Circle,2)] _GradientType("Gradient type", Float) = 0
        _Rotation("Rotation", Range(0.0, 3.0)) = 0

        [Space]
        _GradientFromColor("Gradient From", Color) = (1,1,1,1)
        _GradientToColor("Gradient To", Color) = (0.5,0.5,0.5,1)
        _GradientCenter("Gradient Center", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed _Rotation;
            fixed4 _Color;
            fixed4 _GradientFromColor;
            fixed4 _GradientToColor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
#ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;
            float _GradientCenter;

            float _GradientType;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                if (_AlphaSplitEnabled)
                    color.a = tex2D(_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

                return color;
            }
            float2 rotateUV(float2 uv, float rotation)
            {
                if (rotation <= 0.1)
                {
                    return uv;
                }
    
                float mid = 0.5;
                return float2(
                    cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
                    cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
                );
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                c.rgb *= c.a;

                if (_GradientType == 0)
                {
                    // Apply gradient (depending on where we are)
                    // UV 'y' is reverse
                    // rotate UV 'y' by given angle
                    float2 rotFragment = rotateUV(IN.texcoord, _Rotation);
                    c *= lerp(_GradientToColor, _GradientFromColor, rotFragment.y + (_GradientCenter - 0.5));
                }
                if (_GradientType == 1)
                {
                    float2 rotFragment = rotateUV(IN.texcoord, _Rotation);
                    float2 texc = rotFragment + (_GradientCenter - 1);

                    if (rotFragment.y < _GradientCenter)
                    {
                        c *= lerp(_GradientToColor, _GradientFromColor, (rotFragment.y + (_GradientCenter - 0.5)) * 2);
                    }
                    else
                    {
                        c *= lerp(_GradientFromColor, _GradientToColor, (rotFragment.y + (_GradientCenter - 0.5) - 0.5) * 2);
                    }
                }
                if (_GradientType == 2)
                {
                    // Use uv's
                    // default offset of uv : 0.5
                    IN.texcoord -= (1 - _GradientCenter);
                    float dist = length(IN.texcoord);

                    c *= dist;
                }
                return c;
            }
            ENDCG
        }
    }
}
