using System;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using static Unity.Rendering.Universal.ShaderUtils;

namespace UnityEditor
{
    // Used for ShaderGraph Lit shaders
    class ShaderGraphStencilLitGUI : BaseShaderGUI
    {
        const string STENCIL_REFERENCE = "_Stencil";
        const string STENCIL_COMPARISON = "_StencilComp";
        const string STENCIL_OPERATION = "_StencilOp";
        const string STENCIL_READ_MASK = "_StencilReadMask";
        const string STENCIL_WRITE_MASK = "_StencilWriteMask";

        public MaterialProperty workflowMode;
        public MaterialProperty stencilReference;
        public MaterialProperty stencilComparison;
        public MaterialProperty stencilOperation;
        public MaterialProperty stencilReadMask;
        public MaterialProperty stencilWriteMask;

        MaterialProperty[] properties;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            // save off the list of all properties for shadergraph
            this.properties = properties;

            var material = materialEditor?.target as Material;
            if (material == null)
                return;

            base.FindProperties(properties);
            workflowMode = BaseShaderGUI.FindProperty(Property.SpecularWorkflowMode, properties, false);
            stencilReference = BaseShaderGUI.FindProperty(STENCIL_REFERENCE, properties, false);
            stencilComparison = BaseShaderGUI.FindProperty(STENCIL_COMPARISON, properties, false);
            stencilOperation = BaseShaderGUI.FindProperty(STENCIL_OPERATION, properties, false);
            stencilReadMask = BaseShaderGUI.FindProperty(STENCIL_READ_MASK, properties, false);
            stencilWriteMask = BaseShaderGUI.FindProperty(STENCIL_WRITE_MASK, properties, false);
        }

        public static void UpdateMaterial(Material material, MaterialUpdateType updateType)
        {
            // newly created materials should initialize the globalIlluminationFlags (default is off)
            if (updateType == MaterialUpdateType.CreatedNewMaterial)
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

            bool automaticRenderQueue = GetAutomaticQueueControlSetting(material);
            BaseShaderGUI.UpdateMaterialSurfaceOptions(material, automaticRenderQueue);
            LitGUI.SetupSpecularWorkflowKeyword(material, out bool isSpecularWorkflow);
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

        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            if (workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, workflowMode, Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            DrawShaderGraphProperties(material, properties);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            // Always show the queue control field.  Only show the render queue field if queue control is set to user override
            DoPopup(Styles.queueControl, queueControlProp, Styles.queueControlNames);
            if (material.HasProperty(Property.QueueControl) && material.GetFloat(Property.QueueControl) == (float)QueueControl.UserOverride)
                materialEditor.RenderQueueField();

            DrawStencilProperty(stencilReference, "Stencil Ref");
            DrawStencilProperty(stencilComparison, "Stencil Comp");
            DrawStencilProperty(stencilOperation, "Stencil Pass");
            DrawStencilProperty(stencilReadMask, "Stencil Read Mask");
            DrawStencilProperty(stencilWriteMask, "Stencil Write Mask");

            base.DrawAdvancedOptions(material);

            // ignore emission color for shadergraphs, because shadergraphs don't have a hard-coded emission property, it's up to the user
            materialEditor.DoubleSidedGIField();
            materialEditor.LightmapEmissionFlagsProperty(0, enabled: true, ignoreEmissionColor: true);
        }

        private void DrawStencilProperty(MaterialProperty property, string label)
        {
            if (property != null)
                materialEditor.ShaderProperty(property, label, 1);
        }
    }
} // namespace UnityEditor
