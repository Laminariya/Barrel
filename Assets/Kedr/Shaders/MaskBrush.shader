Shader "Kedr/MaskBrush"
{
    Properties
    {
        _MainTex ("Webcam", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,1)) = 0.35
        _Blur ("Blur (px)", Range(0,2)) = 1.0
        _NoiseTex ("Noise", 2D) = "gray" {}
        _NoiseAmount ("Noise Amount", Range(0,1)) = 0.15
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_TexelSize;
            float _Threshold, _Blur, _NoiseAmount;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // 3x3 box blur
            float BlurSample(float2 uv)
            {
                float sum = 0;
                float kernel[3] = {0.25, 0.5, 0.25};
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * _Blur * _MainTex_TexelSize.xy;
                        float3 rgb = tex2D(_MainTex, uv + offset).rgb;
                        float gray = dot(rgb, float3(0.299, 0.587, 0.114));
                        sum += gray * kernel[x + 1] * kernel[y + 1];
                    }
                return sum;
            }

            float4 frag(v2f i) : SV_Target
            {
                float mask = BlurSample(i.uv);

                // mask = (mask < _Threshold) ? 1.0 : 0.0; // 1=brush/erase, 0=nothing

                float softness = 0.2; // Try 0.1–0.3, higher = softer edge
                mask = 1.0 - smoothstep(_Threshold - softness, _Threshold + softness, mask);


                float noise = tex2D(_NoiseTex, i.uv * 4.0 + frac(_Time.y * 0.35)).r;
                mask = lerp(mask, saturate(mask + (noise - 0.5) * _NoiseAmount), 1.0);

                return float4(mask, mask, mask, 1);
            }
            ENDHLSL
        }
    }
}