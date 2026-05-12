Shader "Hidden/FastBoxBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _BlurSize;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = 0;

                col += tex2D(_MainTex, i.uv + offset * float2(-1, -1));
                col += tex2D(_MainTex, i.uv + offset * float2( 0, -1));
                col += tex2D(_MainTex, i.uv + offset * float2( 1, -1));

                col += tex2D(_MainTex, i.uv + offset * float2(-1, 0));
                col += tex2D(_MainTex, i.uv);
                col += tex2D(_MainTex, i.uv + offset * float2(1, 0));

                col += tex2D(_MainTex, i.uv + offset * float2(-1, 1));
                col += tex2D(_MainTex, i.uv + offset * float2(0, 1));
                col += tex2D(_MainTex, i.uv + offset * float2(1, 1));

                return col / 9;
            }
            ENDCG
        }
    }
}