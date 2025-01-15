using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IgnorePostProcessing
{
    public class IgnorePostProcessing : ScriptableRendererFeature
    {
        [SerializeField] LayerMask _layerMask;
        [SerializeField] CustomTexture _colorTexture;

        IgnorePostProcessingPass _pass;

        public override void Create()
        {
            _pass = new IgnorePostProcessingPass(name, _layerMask, _colorTexture);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
    }
}

