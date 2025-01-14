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
            // ポストエフェクト後に描画する. すべてのレンダーキューのオブジェクトが対象.
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilingSampler = new ProfilingSampler(samplerName);
            _filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            // デフォルトのシェーダーパスを設定.
            _shaderTagIds = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };

            // シェーダープロパティにレンダーテクスチャを渡す.
            _overrideMaterial.SetTexture(_colorTextureId, colorTexturePass?.destination);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // 描画方法の設定. マテリアルをオーバーライド.
                var drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, SortingCriteria.BackToFront);
                drawingSettings.overrideMaterial = _overrideMaterial;

                // 画面にレンダリング.
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}

