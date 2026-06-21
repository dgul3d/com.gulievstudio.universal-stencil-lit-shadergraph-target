using System;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using static Unity.Rendering.Universal.ShaderUtils;

namespace UnityEditor
{
    class ShaderGraphStencilUnlitGUI : ShaderGraphStencilMaterialGUI
    {
        MaterialProperty[] properties;

        public override void FindProperties(MaterialProperty[] properties)
        {
            this.properties = properties;

            base.FindProperties(properties);
            FindStencilProperties(properties);
        }

        public static void UpdateMaterial(Material material, MaterialUpdateType updateType)
        {
            bool automaticRenderQueue = GetAutomaticQueueControlSetting(material);
            BaseShaderGUI.UpdateMaterialSurfaceOptions(material, automaticRenderQueue);
            BaseShaderGUI.UpdateMotionVectorKeywordsAndPass(material);
#if ENABLE_VR && ENABLE_XR_MODULE
            BaseShaderGUI.UpdateXRMotionVectorKeywordsAndPass(material);
#endif
        }

        public override void ValidateMaterial(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            UpdateMaterial(material, MaterialUpdateType.ModifiedMaterial);
        }

        public override void DrawSurfaceInputs(Material material)
        {
            DrawShaderGraphProperties(material, properties);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            DrawQueueControl(material);
            base.DrawAdvancedOptions(material);
            materialEditor.DoubleSidedGIField();

            DrawStencilProperties();
        }
    }
}
