Shader "Custom/IgnorePostProcessing"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_CustomColorTexture);
            TEXTURE2D(_CustomDepthTexture);
            SAMPLER(sampler_CustomColorTexture);
            SAMPLER(sampler_CustomDepthTexture);
            
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // レンダーテクスチャから深度をサンプリング.
                float2 uv = IN.positionCS.xy / _ScaledScreenParams.xy;
                float sceneDepth = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, uv).r;

                // サンプリングした深度とオブジェクトの深度を線形化して比較. 遮蔽物があれば描画しない.
                sceneDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float depth = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);
                clip(sceneDepth - depth);

                // レンダーテクスチャからオブジェクトの色をサンプリング.
                half4 color = SAMPLE_TEXTURE2D(_CustomColorTexture, sampler_CustomColorTexture, uv);
                return color;
            }
            ENDHLSL
        }
    }
}
