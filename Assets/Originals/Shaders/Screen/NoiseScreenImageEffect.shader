Shader "Custom/NoiseScreenImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.1
        _BlockScale("Block Scale", Range(1,50)) = 10
        _NoiseSpeed("Noise Speed", Range(1,10)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="UI" }
        //Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            sampler2D _MainTex;
            float _GlitchIntensity;
            float _BlockScale;
            float _NoiseSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 seeds)
            {
                return frac(sin(dot(seeds, float2(12.9898, 78.233))) * 43758.5453);
            }

            float blockNoise(float2 seeds)
            {
                return random(floor(seeds));
            }

            float noiserandom(float2 seeds)
            {
                return -1.0 + 2.0 * blockNoise(seeds);
            }

            //ノイズ計算
            fixed4 frag (v2f i) : SV_Target {
                float4 color;
                float2 gv = i.uv;
                float noise = blockNoise(i.uv.y * _BlockScale);
                noise += random(i.uv.x) * 0.3;
                float2 randomvalue = noiserandom(float2(i.uv.y, _Time.y * _NoiseSpeed));
                gv.x += randomvalue * sin(sin(_GlitchIntensity)*.5) * sin(-sin(noise)*.2) * frac(_Time.y);

                // R, G, Bチャンネルを個別にサンプリング (ノイズグリッチ効果)
                float r = tex2D(_MainTex, gv + float2(0.006, 0)).r;
                float g = tex2D(_MainTex, gv).g;
                float b = tex2D(_MainTex, gv - float2(0.008, 0)).b;


                // 薄い灰色
                float greyIntensity = 0.8;

                // サンプリングしたR, G, B値に、薄い灰色の値を乗算
                color.r = greyIntensity;
                color.g = greyIntensity;
                color.b = greyIntensity;

                // color.r = r * greyIntensity;
                // color.g = g * greyIntensity;
                // color.b = b * greyIntensity;

                // color.r = tex2D(_MainTex, gv + float2(0.006, 0)).r;
                // color.g = tex2D(_MainTex, gv).g;
                // color.b = tex2D(_MainTex, gv - float2(0.008, 0)).b;
                color.a = 1.0;

                return color;
            }
            ENDCG
        }
    }
}
