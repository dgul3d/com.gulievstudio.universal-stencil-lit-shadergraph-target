#if HAS_VFX_GRAPH
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal
{
    internal class VFXShaderGraphStencilUnlitGUI : ShaderGraphStencilUnlitGUI
    {
        protected override uint materialFilter => uint.MaxValue & ~(uint)Expandable.SurfaceInputs;
    }
}
#endif
