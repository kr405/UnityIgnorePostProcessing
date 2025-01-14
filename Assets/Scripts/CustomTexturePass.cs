using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IgnorePostProcessing
{
    public class CustomTexturePass : ScriptableRenderPass
    {
        /// <summary>
        /// �o�͐��RenderTexture�̐ݒ�.
        /// </summary>
        public class TextureSettings
        {
            public ClearFlag clearFlag;
            public Color clearColor;
            public string textureName;
            public RenderTextureFormat format;
            public int depthBufferBits;

            public TextureSettings(ClearFlag clearFlag, Color clearColor, RenderTextureFormat format, int depthBufferBits, string textureName)
            {
                this.clearFlag = clearFlag;
                this.clearColor = clearColor;
                this.format = format;
                this.depthBufferBits = depthBufferBits;
                this.textureName = $"_{textureName}";
            }
        }

        public RTHandle destination;

        List<ShaderTagId> _shaderTagIds;
        ProfilingSampler _profilingSampler;
        FilteringSettings _filteringSettings;
        TextureSettings _textureSettings;
        Shader _overrideShader;
        int _shaderPassIndex;

        public CustomTexturePass(string samplerName, LayerMask layerMask, TextureType textureType, Shader overrideShader, int shaderPassIndex)
        {
            // Transparents�̌�ɕ`�悷��. ���ׂẴ����_�[�L���[�̃I�u�W�F�N�g���Ώ�.
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            _profilingSampler = new ProfilingSampler(samplerName);
            _filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            // �ݒ肳�ꂽTextureType�ɉ�����RenderTexture�̍\�������߂�.
            _textureSettings = textureType == TextureType.Color
                ? new TextureSettings(ClearFlag.Color, Color.clear, RenderTextureFormat.ARGB32, 0, samplerName)
                : new TextureSettings(ClearFlag.Depth, Color.black, RenderTextureFormat.Depth, 32, samplerName);
            
            _overrideShader = overrideShader;
            _shaderPassIndex = shaderPassIndex;

            // �f�t�H���g�̃V�F�[�_�[�p�X��ݒ�.
            _shaderTagIds = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };
        }

        public void Setup(RenderTextureDescriptor descriptor)
        {
            // �����_�[�e�N�X�`�����m��.
            descriptor.colorFormat = _textureSettings.format;
            descriptor.depthBufferBits = _textureSettings.depthBufferBits;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref destination, descriptor);
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // �����_�[�^�[�Q�b�g��ݒ�.
            ConfigureTarget(destination);
            ConfigureClear(_textureSettings.clearFlag, _textureSettings.clearColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                // �`����@�̐ݒ�.
                var drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, SortingCriteria.BackToFront);

                if (_overrideShader != null)
                {
                    // �V�F�[�_�[���I�[�o�[���C�h.
                    drawingSettings.overrideShader = _overrideShader;
                    drawingSettings.overrideShaderPassIndex = _shaderPassIndex;
                }

                // �e�N�X�`���Ƀ����_�����O���A�V�F�[�_�[�ɋ��L����.
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
                Shader.SetGlobalTexture(_textureSettings.textureName, destination);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            destination?.Release();
        }
    }
}

