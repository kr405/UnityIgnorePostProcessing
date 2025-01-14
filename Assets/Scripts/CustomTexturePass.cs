using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IgnorePostProcessing
{
    public class CustomTexturePass : ScriptableRenderPass
    {
        /// <summary>
        /// 出力先のRenderTextureの設定.
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
            // Transparentsの後に描画する. すべてのレンダーキューのオブジェクトが対象.
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            _profilingSampler = new ProfilingSampler(samplerName);
            _filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            // 設定されたTextureTypeに応じてRenderTextureの構成を決める.
            _textureSettings = textureType == TextureType.Color
                ? new TextureSettings(ClearFlag.Color, Color.clear, RenderTextureFormat.ARGB32, 0, samplerName)
                : new TextureSettings(ClearFlag.Depth, Color.black, RenderTextureFormat.Depth, 32, samplerName);
            
            _overrideShader = overrideShader;
            _shaderPassIndex = shaderPassIndex;

            // デフォルトのシェーダーパスを設定.
            _shaderTagIds = new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };
        }

        public void Setup(RenderTextureDescriptor descriptor)
        {
            // レンダーテクスチャを確保.
            descriptor.colorFormat = _textureSettings.format;
            descriptor.depthBufferBits = _textureSettings.depthBufferBits;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref destination, descriptor);
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // レンダーターゲットを設定.
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
                
                // 描画方法の設定.
                var drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, SortingCriteria.BackToFront);

                if (_overrideShader != null)
                {
                    // シェーダーをオーバーライド.
                    drawingSettings.overrideShader = _overrideShader;
                    drawingSettings.overrideShaderPassIndex = _shaderPassIndex;
                }

                // テクスチャにレンダリングし、シェーダーに共有する.
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

