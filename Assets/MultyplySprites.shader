Shader "Sprites/SpriteMultiply"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlendFactor ("Blend Factor", Range(0, 1)) = 1.0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert1
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            sampler2D _SecondTex;
            float4 _SecondTex_ST;
            float _BlendFactor;

            struct v2f_ext
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f_ext SpriteVert1(appdata_t IN)
            {
                v2f_ext OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.texcoord2 = TRANSFORM_TEX(IN.texcoord, _SecondTex);
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f_ext IN) : SV_Target
            {
                //fixed4 mainColor = SampleSpriteTexture(IN.texcoord) * IN.color;
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord2);
                fixed4 secondColor = tex2D(_SecondTex, IN.texcoord2);
                
                // Перемножение цветов с сохранением прозрачности
                fixed4 result =  mainColor;//* secondColor;
                //if(secondColor.a<0.7f) result.a = mainColor.a * 0;
                //else result.a = mainColor.a ;//* (secondColor.a);
                //result.a = mainColor.a * (1.5 -secondColor.a);
                result.a = mainColor.a * (1.0 - secondColor.a);
                if(result.a<0.5)
                {
                    result.r = 0.0;
                    result.g = 0.0;
                    result.b = 0.0;
                }
                
                
                // Плавное смешивание между оригиналом и результатом
                return result;
                //return lerp(mainColor, result, _BlendFactor);
            }
            ENDCG
        }
    }
}