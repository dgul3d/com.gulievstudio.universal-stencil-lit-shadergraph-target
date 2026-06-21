using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    static class ShaderGraphStencilUtility
    {
        public const string Reference = "_Stencil";
        public const string Comparison = "_StencilComp";
        public const string Pass = "_StencilOp";
        public const string Fail = "_StencilFail";
        public const string DepthFail = "_StencilZFail";
        public const string ReadMask = "_StencilReadMask";
        public const string WriteMask = "_StencilWriteMask";

        public const float DefaultReference = 0.0f;
        public const float DefaultComparison = (float)CompareFunction.Always;
        public const float DefaultPass = (float)StencilOp.Keep;
        public const float DefaultFail = (float)StencilOp.Keep;
        public const float DefaultDepthFail = (float)StencilOp.Keep;
        public const float DefaultReadMask = 255.0f;
        public const float DefaultWriteMask = 255.0f;

        public static void ApplyDefaultStencilProperties(Material material)
        {
            material.SetFloat(Reference, DefaultReference);
            material.SetFloat(Comparison, DefaultComparison);
            material.SetFloat(Pass, DefaultPass);
            material.SetFloat(Fail, DefaultFail);
            material.SetFloat(DepthFail, DefaultDepthFail);
            material.SetFloat(ReadMask, DefaultReadMask);
            material.SetFloat(WriteMask, DefaultWriteMask);
        }

        public static void CollectStencilProperties(PropertyCollector collector)
        {
            collector.AddFloatProperty(Reference, DefaultReference);
            collector.AddFloatProperty(Comparison, DefaultComparison);
            collector.AddFloatProperty(Pass, DefaultPass);
            collector.AddFloatProperty(Fail, DefaultFail);
            collector.AddFloatProperty(DepthFail, DefaultDepthFail);
            collector.AddFloatProperty(ReadMask, DefaultReadMask);
            collector.AddFloatProperty(WriteMask, DefaultWriteMask);
        }

        public static RenderStateCollection WithStencil(RenderStateCollection renderStates, bool enableStencil)
        {
            var result = new RenderStateCollection();
            result.Add(renderStates);

            if (!enableStencil)
                return result;

            result.Add(RenderState.Stencil(new StencilDescriptor()
            {
                Ref = kReference,
                Comp = kComparison,
                Pass = kPass,
                Fail = kFail,
                ZFail = kDepthFail,
                ReadMask = kReadMask,
                WriteMask = kWriteMask,
            }));

            return result;
        }

        const string kReference = "[_Stencil]";
        const string kComparison = "[_StencilComp]";
        const string kPass = "[_StencilOp]";
        const string kFail = "[_StencilFail]";
        const string kDepthFail = "[_StencilZFail]";
        const string kReadMask = "[_StencilReadMask]";
        const string kWriteMask = "[_StencilWriteMask]";
    }
}
