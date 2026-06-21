using System;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Rendering.Universal.ShaderUtils;

namespace UnityEditor
{
    abstract class ShaderGraphStencilMaterialGUI : BaseShaderGUI
    {
        static readonly GUIContent StencilRefLabel = EditorGUIUtility.TrTextContent("Stencil Ref");
        static readonly GUIContent StencilCompLabel = EditorGUIUtility.TrTextContent("Stencil Comp");
        static readonly GUIContent StencilPassLabel = EditorGUIUtility.TrTextContent("Stencil Pass");
        static readonly GUIContent StencilFailLabel = EditorGUIUtility.TrTextContent("Stencil Fail");
        static readonly GUIContent StencilDepthFailLabel = EditorGUIUtility.TrTextContent("Stencil Depth Fail");
        static readonly GUIContent StencilReadMaskLabel = EditorGUIUtility.TrTextContent("Stencil Read Mask");
        static readonly GUIContent StencilWriteMaskLabel = EditorGUIUtility.TrTextContent("Stencil Write Mask");

        protected MaterialProperty stencilReference;
        protected MaterialProperty stencilComparison;
        protected MaterialProperty stencilPass;
        protected MaterialProperty stencilFail;
        protected MaterialProperty stencilDepthFail;
        protected MaterialProperty stencilReadMask;
        protected MaterialProperty stencilWriteMask;

        protected void FindStencilProperties(MaterialProperty[] properties)
        {
            stencilReference = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.Reference, properties, false);
            stencilComparison = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.Comparison, properties, false);
            stencilPass = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.Pass, properties, false);
            stencilFail = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.Fail, properties, false);
            stencilDepthFail = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.DepthFail, properties, false);
            stencilReadMask = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.ReadMask, properties, false);
            stencilWriteMask = BaseShaderGUI.FindProperty(ShaderGraphStencilUtility.WriteMask, properties, false);
        }

        protected void DrawQueueControl(Material material)
        {
            DoPopup(Styles.queueControl, queueControlProp, Styles.queueControlNames);
            if (material.HasProperty(Property.QueueControl) && material.GetFloat(Property.QueueControl) == (float)QueueControl.UserOverride)
                materialEditor.RenderQueueField();
        }

        protected void DrawStencilProperties()
        {
            if (stencilReference == null)
                return;

            materialEditor.ShaderProperty(stencilReference, StencilRefLabel);
            DrawEnumPopup(StencilCompLabel, stencilComparison, typeof(CompareFunction));
            DrawEnumPopup(StencilPassLabel, stencilPass, typeof(StencilOp));
            DrawEnumPopup(StencilFailLabel, stencilFail, typeof(StencilOp));
            DrawEnumPopup(StencilDepthFailLabel, stencilDepthFail, typeof(StencilOp));
            materialEditor.ShaderProperty(stencilReadMask, StencilReadMaskLabel);
            materialEditor.ShaderProperty(stencilWriteMask, StencilWriteMaskLabel);
        }

        void DrawEnumPopup(GUIContent label, MaterialProperty property, Type enumType)
        {
            if (property == null)
                return;

            DoPopup(label, property, Enum.GetNames(enumType));
        }
    }
}
