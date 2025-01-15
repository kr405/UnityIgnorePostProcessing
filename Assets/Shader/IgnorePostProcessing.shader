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
                // �����_�[�e�N�X�`������[�x���T���v�����O.
                float2 uv = IN.positionCS.xy / _ScaledScreenParams.xy;
                float sceneDepth = SAMPLE_TEXTURE2D(_CustomDepthTexture, sampler_CustomDepthTexture, uv).r;

                // �T���v�����O�����[�x�ƃI�u�W�F�N�g�̐[�x����`�����Ĕ�r. �Օ���������Ε`�悵�Ȃ�.
                sceneDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float depth = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);
                clip(sceneDepth - depth);

                // �����_�[�e�N�X�`������I�u�W�F�N�g�̐F���T���v�����O.
                half4 color = SAMPLE_TEXTURE2D(_CustomColorTexture, sampler_CustomColorTexture, uv);
                return color;
            }
            ENDHLSL
        }
    }
}
