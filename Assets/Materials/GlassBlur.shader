Shader "UI/GlassBlur"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Size ("Blur Size", Range(0, 20)) = 5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        // Делает снимок экрана позади объекта
        GrabPass { "_BackgroundTexture" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            sampler2D _BackgroundTexture;
            float4 _BackgroundTexture_TexelSize;
            float4 _Color;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 col = float4(0,0,0,0);
                float dist = _Size;
                
                // Простейший сбор пикселей вокруг для эффекта размытия
                col += tex2Dproj(_BackgroundTexture, i.grabPos + float4(-dist, -dist, 0, 0) * _BackgroundTexture_TexelSize.xyxy);
                col += tex2Dproj(_BackgroundTexture, i.grabPos + float4(dist, -dist, 0, 0) * _BackgroundTexture_TexelSize.xyxy);
                col += tex2Dproj(_BackgroundTexture, i.grabPos + float4(-dist, dist, 0, 0) * _BackgroundTexture_TexelSize.xyxy);
                col += tex2Dproj(_BackgroundTexture, i.grabPos + float4(dist, dist, 0, 0) * _BackgroundTexture_TexelSize.xyxy);
                col /= 4.0;

                return col * _Color;
            }
            ENDCG
        }
    }
}
