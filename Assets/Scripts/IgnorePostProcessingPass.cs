using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IgnorePostProcessing
{
    public class IgnorePostProcessingPass : ScriptableRenderPass
    {
        static readonly int _colorTextureId = Shader.PropertyToID("_CustomColorTexture");

        ProfilingSampler _profilingSampler;
        List<ShaderTagId> _shaderTagIds;
        FilteringSettings _filteringSettings;
        Material _overrideMaterial = new Material(Shader.Find("Custom/IgnorePostProcessing"));

        public IgnorePostProcessingPass(string samplerName, LayerMask layerMask, CustomTexturePass colorTexturePass)
        {
            // �|�X�g�G�t�F�N�g��ɕ`�悷��. ���ׂẴ����_�[�L���[�̃I�u�W�F�N�g���Ώ�.
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilingSampler = new ProfilingSampler(samplerName);
            _filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            // �f�t�H���g�̃V�F�[�_�[�p�X��ݒ�.
            _shaderTagIds = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };

            // �V�F�[�_�[�v���p�e�B�Ƀ����_�[�e�N�X�`����n��.
            _overrideMaterial.SetTexture(_colorTextureId, colorTexturePass?.destination);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // �`����@�̐ݒ�. �}�e���A�����I�[�o�[���C�h.
                var drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, SortingCriteria.BackToFront);
                drawingSettings.overrideMaterial = _overrideMaterial;

                // ��ʂɃ����_�����O.
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}

