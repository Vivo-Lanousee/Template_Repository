Shader "Hidden/Draw"
{
    Properties
    {
        _SourceTex ("Texture", 2D) = "white" {}
        _Coordinate ("Coordinate", Vector) = (0, 0, 0, 0)
        _Color ("Color", Color) = (1, 1, 1, 1)
        _TextureSize ("Size", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _SourceTex;
            float4 _Coordinate; // (x, y, radius, threshold)
            float4 _Color;
            float2 _TextureSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float aspect = _TextureSize.x / _TextureSize.y;
                float2 texelPos = i.uv;
                float2 drawPos = _Coordinate.xy;

                float2 diff = texelPos - drawPos;
                diff.x *= aspect;
                float distance = length(diff);

                float mask = smoothstep(_Coordinate.z, _Coordinate.z * 0.8, distance);
                half4 color = tex2D(_SourceTex, i.uv);
                half4 destCol = lerp(color, _Color, mask);
                return destCol;
            }

            ENDCG
        }
    }
}