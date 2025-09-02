Shader "Custom/TextureMultiply"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _MultiplyFactor ("Multiply Factor", Range(0, 1)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SecondTex;
        float _MultiplyFactor;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_SecondTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Чтение цветов из обеих текстур
            fixed4 tex1 = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 tex2 = tex2D(_SecondTex, IN.uv_SecondTex);
            
            // Перемножение цветов
            fixed4 result = tex1 * tex2;
            
            // Смешивание с оригинальным цветом
            o.Albedo = tex1.rgba; //lerp(tex1.rgb, result.rgb, _MultiplyFactor);
            //o.Albedo = result;
            o.Alpha = tex1.a * (1-tex2.a);
        }

        
        
        ENDCG
    }
    FallBack "Diffuse"
}