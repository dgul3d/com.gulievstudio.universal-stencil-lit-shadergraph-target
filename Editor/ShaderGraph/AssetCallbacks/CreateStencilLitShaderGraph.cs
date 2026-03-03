using System;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    static class CreateStencilLitShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/URP/Stencil Lit Shader Graph", priority = CoreUtils.Priorities.assetsCreateShaderMenuPriority + 2)]
        public static void CreateStencilLitGraph()
        {
            var target = (UniversalTarget)Activator.CreateInstance(typeof(UniversalTarget));
            target.TrySetActiveSubTarget(typeof(UniversalStencilLitSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.Metallic,
                BlockFields.SurfaceDescription.Smoothness,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Occlusion,
            };

            GraphUtil.CreateNewGraphWithOutputs(new[] { target }, blockDescriptors);
        }
    }
}
