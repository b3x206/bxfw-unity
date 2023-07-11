Shader "Custom/Vector/Circle"
{
    Properties
    {
        [MainColor] [HDR] _Color("Circle Color", Color) = (1, 1, 1, 1)
        [HDR] _StrokeColor("Stroke Color", Color) = (0.2, 0.8, 1, 1)
        _StrokeThickness("Stroke Thickness", Range(0, 4)) = 1
        [MainTexture] _MainTex("Mask Texture", 2D) = "white" {}
        _CircleSize("Circle Size (in percent)", Range(0, 100)) = 90

        // UI stencil
        [Space][Space]
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            // make particle kinda work
            #pragma multi_compile_particles
            
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;

                // Add the color in the vertex shader
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                // Define a vertex color for an particle color
                // Usable with the fragment shader
                fixed4 vertParticleColor : COLOR;
            };

            fixed4 _Color;
            fixed4 _StrokeColor;
            float _CircleSize;
            float _StrokeThickness;
            sampler2D _MainTex;

            // We don't need the vertex data in v2f (and in the fragment shader), so we get it as an out parameter.
            v2f vert(appdata_t i, out float4 vertex : POSITION)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(i);

                o.uv = i.uv;
                o.vertParticleColor = i.color;
                vertex = UnityObjectToClipPos(i.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.uv -= 0.5; // Offset UV
            
                // Distance & radius variables
                float dist = length(i.uv);
                float radius = _CircleSize / 200;
                float outlineRadius = (_CircleSize * (1 + (_StrokeThickness / 4))) / 200;

                // Main circle
                float circle = 0; 
                if (_Color.a > 0)
                {
                    circle = (dist < radius ? 1 : 0);
                }
                float4 inside = circle * _Color;
                
                // Outer ring
                float ring = 0;
                if (_StrokeColor.a > 0)
                {
                    ring = (dist < outlineRadius && dist > radius ? 1 : 0);
                }
                float4 stroke = _StrokeColor * ring;

                float4 col = ring > 0.01 ? stroke : inside;
                // UV is offseted by 0.5, so we add it in the tex2D's uv. 
                // (otherwise the texture is offseted)
                return tex2D(_MainTex, i.uv + 0.5) * clamp(col * i.vertParticleColor, float4(0,0,0,0), float4(1,1,1,1));
            }

            ENDCG
        }
    }

    FallBack "Sprites/Default"
}