// Doesn't cause trouble unlike the unity standard transparent.
// Uses a different SubShader for shadow
Shader "Custom/Diffuse/Transparent/VertexLit with Z (No Shadow Pass)" 
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    // Transparent Material
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent"}
           
        // Render into depth buffer only
        Pass 
        {
            ColorMask 0
        }
        // Render normally
        Pass 
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            Material 
            {
                Diffuse[_Color]
                Ambient[_Color]
            }
            Lighting On
            SetTexture[_MainTex] 
            {
                Combine texture * primary DOUBLE, texture * primary
            }
        }
    }
}
