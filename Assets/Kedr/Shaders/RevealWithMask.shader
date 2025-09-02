Shader "Kedr/RevealWithMask"
{
    Properties
    {
        [MainTexture] _MainTex ("Foreground", 2D) = "white" {}
        _BgTex ("Background", 2D) = "black" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "gray" {}
        _NoiseAmount ("Noise Amount", Range(0,1)) = 0.2
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _EdgeFeather ("Edge Feather", Range(0.01, 0.5)) = 0.15
        _FadeT ("Fade Out Foreground", Range(0,1)) = 0
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
            sampler2D _BgTex;
            sampler2D _MaskTex;
            sampler2D _NoiseTex;
            float _NoiseAmount;
            float _NoiseScale;
            float _EdgeFeather;
            float _FadeT;

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

            float4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;

                // Make mask a soft gradient (if it's not already)
                // If your accumulate mask is 0/1, blur it in MaskBrush or here
                float smoothMask = smoothstep(0.0, 1.0, mask); // just in case

                // Noise - bigger scale = larger "flakes"
                float noise = tex2D(_NoiseTex, i.uv * _NoiseScale + frac(_Time.y * 0.35)).r;

                // Feather region: band around the cutout, e.g. mask in [0.3, 0.7]
                float feather = _EdgeFeather; // e.g. 0.25 for smoky effect
                float edgeStart = 0.5 - feather * 0.5;
                float edgeEnd = 0.5 + feather * 0.5;

                // Edge factor: 1 at center of feather, 0 outside
                float edgeFactor = smoothstep(edgeStart, 0.5, mask) * (1.0 - smoothstep(0.5, edgeEnd, smoothMask));

                // Mix in noise only on the edge band
                float noisyMask = smoothMask + (noise - 0.5) * _NoiseAmount * edgeFactor;
                noisyMask = saturate(noisyMask);

                float t = lerp(noisyMask, 1.0, _FadeT); // fade wipes out any leftovers

                float4 fg = tex2D(_MainTex, i.uv);
                float4 bg = tex2D(_BgTex, i.uv);
                return lerp(fg, bg, t);
                return lerp(fg, bg, noisyMask);
            }
            ENDHLSL
        }
    }
}