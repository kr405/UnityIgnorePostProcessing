using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IgnorePostProcessing
{
    /// <summary>
    /// テクスチャに書き込む値のタイプ.
    /// </summary>
    public enum TextureType
    {
        Color,
        Depth
    }

    public class CustomTexture : ScriptableRendererFeature
    {
        [SerializeField] TextureType _textureType;
        [SerializeField] LayerMask _layerMask;
        [SerializeField] Shader _shader;
        [SerializeField] int _shaderPassIndex;

        public CustomTexturePass Pass { get; private set; }

        public override void Create()
        {
            Pass = new CustomTexturePass(name, _layerMask, _textureType, _shader, _shaderPassIndex);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Pass.Setup(renderingData.cameraData.cameraTargetDescriptor);
            renderer.EnqueuePass(Pass);
        }

        protected override void Dispose(bool disposing)
        {
            Pass?.Dispose();
            base.Dispose(disposing);
        }
    }
}