Shader "Summit/Player/RecoveredSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
          
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_TexelSize;
            CBUFFER_END

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color *_Color * unity_SpriteColor;
                return o;
            }

            // The supplied frames retain the original RGB under zero alpha.
            // Their extraction removed dark clothing as well as the neutral backdrop.
            // Recover coloured details without changing the source image files.
            half CharacterAlpha(float4 sampleColor)
            {
                if (sampleColor.a >= 0.5) return 1;
                float3 rgb = sampleColor.rgb;
#ifndef UNITY_COLORSPACE_GAMMA
                rgb = LinearToSRGB(rgb);
#endif
                rgb = round(saturate(rgb) * 255.0);
                float brightest = max(rgb.r, max(rgb.g, rgb.b));
                float darkest = min(rgb.r, min(rgb.g, rgb.b));
                return (brightest - darkest > 9.0 || rgb.r > rgb.g + 2.0 || brightest > 45.0) ? 1 : 0;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = CharacterAlpha(texel);
                if (alpha < 0.5)
                {
                    // Close isolated one-pixel gaps inside the clothing; open spaces
                    // between the limbs and cape remain transparent.
                    float2 dx = float2(_MainTex_TexelSize.x, 0);
                    float2 dy = float2(0, _MainTex_TexelSize.y);
                    half neighbours = CharacterAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dx))
                        + CharacterAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv - dx))
                        + CharacterAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dy))
                        + CharacterAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv - dy));
                    if (neighbours >= 3) alpha = 1;
                }
                clip(alpha - 0.5);
                return half4(texel.rgb * input.color.rgb, input.color.a);
            }
            ENDHLSL
        }
    }
}
