Shader "Kedr/MaskAccumulate"
{
    Properties
    {
        _MainTex ("Old Mask", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "black" {}
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
            sampler2D _BrushTex;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert(appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            float4 frag(v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv).r; // previous mask
                float b = tex2D(_BrushTex, i.uv).r; // new brush
                return float4(max(a, b), max(a, b), max(a, b), 1);
            }
            ENDHLSL
        }
    }
}
