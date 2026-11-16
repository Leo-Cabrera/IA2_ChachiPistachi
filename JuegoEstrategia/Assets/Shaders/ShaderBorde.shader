Shader "Custom/ShaderBorde"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _GlowColor ("Color del brillo", Color) = (1, 0.9, 0.2, 1)
        _GlowSize ("Grosor (px)", Range(0, 10)) = 2
        _GlowOn ("Activado", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _GlowColor;
            float _GlowSize, _GlowOn;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                // Mira 8 vecinos y se queda con el alpha más alto
                float2 px = _MainTex_TexelSize.xy * _GlowSize;
                float2 dirs[8] = {
                    float2(1,0), float2(-1,0), float2(0,1), float2(0,-1),
                    float2(0.7,0.7), float2(-0.7,0.7), float2(0.7,-0.7), float2(-0.7,-0.7)
                };
                float vecino = 0;
                for (int k = 0; k < 8; k++)
                    vecino = max(vecino, tex2D(_MainTex, i.uv + dirs[k] * px).a);

                // Borde = hay sprite al lado pero este píxel es transparente
                float borde = saturate(vecino - c.a) * _GlowOn;

                fixed4 o;
                o.rgb = c.rgb * c.a + _GlowColor.rgb * borde;
                o.a = c.a + borde;
                return o;
            }
            ENDCG
        }
    }
}