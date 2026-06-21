using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.Universal.ShaderGraph.SubShaderUtils;
using static Unity.Rendering.Universal.ShaderUtils;

namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    sealed class UniversalStencilUnlitSubTarget : UniversalSubTarget, ILegacyTarget
    {
        static readonly GUID kSourceCodeGuid = new GUID("f3a8c2d91e4b4f6a9c1d7e5b2a8043c1"); // UniversalStencilUnlitSubTarget.cs

        public override int latestVersion => 2;

        [SerializeField]
        bool m_KeepLightingVariants = false;

        [SerializeField]
        bool m_DefaultDecalBlending = true;

        [SerializeField]
        bool m_DefaultSSAO = true;

        [SerializeField]
        bool m_EnableStencil = false;

        public bool keepLightingVariants
        {
            get => m_KeepLightingVariants;
            set => m_KeepLightingVariants = value;
        }

        public bool defaultDecalBlending
        {
            get => m_DefaultDecalBlending;
            set => m_DefaultDecalBlending = value;
        }

        public bool defaultSSAO
        {
            get => m_DefaultSSAO;
            set => m_DefaultSSAO = value;
        }

        public bool enableStencil
        {
            get => m_EnableStencil;
            set => m_EnableStencil = value;
        }

        public UniversalStencilUnlitSubTarget()
        {
            displayName = "Stencil Unlit";
        }

        protected override ShaderID shaderID => ShaderID.SG_Unlit;

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);

            var universalRPType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset);
            if (!context.HasCustomEditorForRenderPipeline(universalRPType))
            {
                var gui = typeof(ShaderGraphStencilUnlitGUI);
#if HAS_VFX_GRAPH
                if (TargetsVFX())
                    gui = typeof(VFXShaderGraphStencilUnlitGUI);
#endif
                context.AddCustomEditorForRenderPipeline(gui.FullName, universalRPType);
            }

            context.AddSubShader(PostProcessSubShader(SubShaders.Unlit(target, target.renderType, target.renderQueue, target.disableBatching, enableStencil)));
        }

        public override void ProcessPreviewMaterial(Material material)
        {
            if (target.allowMaterialOverride)
            {
                material.SetFloat(Property.SurfaceType, (float)target.surfaceType);
                material.SetFloat(Property.BlendMode, (float)target.alphaMode);
                material.SetFloat(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                material.SetFloat(Property.CullMode, (int)target.renderFace);
                material.SetFloat(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                material.SetFloat(Property.ZWriteControl, (float)target.zWriteControl);
                material.SetFloat(Property.ZTest, (float)target.zTestMode);
            }

            material.SetFloat(Property.QueueOffset, 0.0f);
            material.SetFloat(Property.QueueControl, (float)BaseShaderGUI.QueueControl.Auto);
            if (enableStencil)
                ShaderGraphStencilUtility.ApplyDefaultStencilProperties(material);

            if (IsSpacewarpSupported())
                material.SetFloat(Property.XrMotionVectorsPass, 1.0f);

            ShaderGraphStencilUnlitGUI.UpdateMaterial(material, MaterialUpdateType.CreatedNewMaterial);
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            base.GetFields(ref context);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(UniversalBlockFields.VertexDescription.MotionVector, target.additionalMotionVectorMode == AdditionalMotionVectorMode.Custom);

            context.AddBlock(BlockFields.SurfaceDescription.Alpha, (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, target.alphaClip || target.allowMaterialOverride);
        }

        public override void CollectShaderProperties(PropertyCollector collector, GenerationMode generationMode)
        {
            if (target.allowMaterialOverride)
            {
                collector.AddFloatProperty(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SurfaceType, (float)target.surfaceType);
                collector.AddFloatProperty(Property.BlendMode, (float)target.alphaMode);
                collector.AddFloatProperty(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SrcBlend, 1.0f);
                collector.AddFloatProperty(Property.DstBlend, 0.0f);
                collector.AddFloatProperty(Property.SrcBlendAlpha, 1.0f);
                collector.AddFloatProperty(Property.DstBlendAlpha, 0.0f);
                collector.AddToggleProperty(Property.ZWrite, (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.ZWriteControl, (float)target.zWriteControl);
                collector.AddFloatProperty(Property.ZTest, (float)target.zTestMode);
                collector.AddFloatProperty(Property.CullMode, (float)target.renderFace);

                bool enableAlphaToMask = (target.alphaClip && (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.AlphaToMask, enableAlphaToMask ? 1.0f : 0.0f);
            }

            collector.AddFloatProperty(Property.QueueOffset, 0.0f);
            collector.AddFloatProperty(Property.QueueControl, -1.0f);
            if (enableStencil)
                ShaderGraphStencilUtility.CollectStencilProperties(collector);

            if (IsSpacewarpSupported())
                collector.AddFloatProperty(Property.XrMotionVectorsPass, 1.0f);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            var universalTarget = (target as UniversalTarget);
            universalTarget.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);
            universalTarget.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: false);

            context.AddProperty("Keep Lighting Variants", new Toggle() { value = keepLightingVariants }, (evt) =>
            {
                if (Equals(keepLightingVariants, evt.newValue))
                    return;

                registerUndo("Change Keep Lighting Variants");
                keepLightingVariants = evt.newValue;
                onChange();
            });

            context.AddProperty("Default Decal Blending", new Toggle() { value = defaultDecalBlending }, (evt) =>
            {
                if (Equals(defaultDecalBlending, evt.newValue))
                    return;

                registerUndo("Change Default Decal Blending");
                defaultDecalBlending = evt.newValue;
                onChange();
            });

            context.AddProperty("Default SSAO", new Toggle() { value = defaultSSAO }, (evt) =>
            {
                if (Equals(defaultSSAO, evt.newValue))
                    return;

                registerUndo("Change Default SSAO");
                defaultSSAO = evt.newValue;
                onChange();
            });

            context.AddProperty("Enable Stencil", new Toggle() { value = enableStencil }, (evt) =>
            {
                if (Equals(enableStencil, evt.newValue))
                    return;

                registerUndo("Change Enable Stencil");
                enableStencil = evt.newValue;
                onChange();
            });
        }

        protected override int ComputeMaterialNeedsUpdateHash()
        {
            int hash = base.ComputeMaterialNeedsUpdateHash();
            hash = hash * 23 + target.allowMaterialOverride.GetHashCode();
            hash = hash * 23 + enableStencil.GetHashCode();
            return hash;
        }

        public bool TryUpgradeFromMasterNode(IMasterNode1 masterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        {
            blockMap = null;
            if (!(masterNode is UnlitMasterNode1 unlitMasterNode))
                return false;

            blockMap = new Dictionary<BlockFieldDescriptor, int>()
            {
                { BlockFields.VertexDescription.Position, 9 },
                { BlockFields.VertexDescription.Normal, 10 },
                { BlockFields.VertexDescription.Tangent, 11 },
                { BlockFields.SurfaceDescription.BaseColor, 0 },
                { BlockFields.SurfaceDescription.Alpha, 7 },
                { BlockFields.SurfaceDescription.AlphaClipThreshold, 8 },
            };

            return true;
        }

        internal override void OnAfterParentTargetDeserialized()
        {
            Assert.IsNotNull(target);

            if (this.sgVersion < latestVersion)
            {
                if (this.sgVersion < 1)
                {
                    if (target.alphaMode == AlphaMode.Premultiply)
                        target.alphaMode = AlphaMode.Alpha;
                }
                ChangeVersion(latestVersion);
            }
        }

        #region SubShader
        static class SubShaders
        {
            public static SubShaderDescriptor Unlit(UniversalTarget target, string renderType, string renderQueue, string disableBatchingTag, bool enableStencil)
            {
                var result = new SubShaderDescriptor()
                {
                    pipelineTag = UniversalTarget.kPipelineTag,
                    customTags = UniversalTarget.kUnlitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    disableBatchingTag = disableBatchingTag,
                    generatesPreview = true,
                    passes = new PassCollection()
                };

                result.passes.Add(UnlitPasses.Forward(target, UnlitKeywords.Forward, enableStencil));

                if (target.mayWriteDepth)
                    result.passes.Add(PassVariant(CorePasses.DepthOnly(target), CorePragmas.Instanced));

                if (target.alwaysRenderMotionVectors)
                    result.customTags = string.Concat(result.customTags, " ", UniversalTarget.kAlwaysRenderMotionVectorsTag);
                result.passes.Add(PassVariant(CorePasses.MotionVectors(target), CorePragmas.MotionVectors));

                if (IsSpacewarpSupported())
                    result.passes.Add(PassVariant(CorePasses.XRMotionVectors(target), CorePragmas.XRMotionVectors));

                result.passes.Add(PassVariant(UnlitPasses.DepthNormalOnly(target, enableStencil), CorePragmas.Instanced));

                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(PassVariant(CorePasses.ShadowCaster(target), CorePragmas.Instanced));

                result.passes.Add(UnlitPasses.GBuffer(target, enableStencil));

                result.passes.Add(PassVariant(CorePasses.SceneSelection(target), CorePragmas.Default));
                result.passes.Add(PassVariant(CorePasses.ScenePicking(target), CorePragmas.Default));

                return result;
            }
        }
        #endregion

        #region Passes
        static class UnlitPasses
        {
            internal static void AddLightingVariantsControlToPass(ref PassDescriptor pass, UniversalStencilUnlitSubTarget unlitSubTarget)
            {
                if (unlitSubTarget.keepLightingVariants)
                {
                    pass.includes.Add(UnlitIncludes.LightingIncludes);
                    pass.keywords.Add(UnlitKeywords.LightingVariants);
                    pass.defines.Add(UnlitDefines.LightingDefine, 1);
                }
            }

            internal static void AddDefaultDecalBlendingControlToPass(ref PassDescriptor pass, UniversalStencilUnlitSubTarget unlitSubTarget)
            {
                if (unlitSubTarget.defaultDecalBlending)
                    pass.defines.Add(UnlitDefines.DefaultDecalBlendingDefine, 1);
            }

            internal static void AddDefaultSSAOControlToPass(ref PassDescriptor pass, UniversalStencilUnlitSubTarget unlitSubTarget)
            {
                if (unlitSubTarget.defaultSSAO)
                    pass.defines.Add(UnlitDefines.DefaultSSAODefine, 1);
            }

            public static PassDescriptor Forward(UniversalTarget target, KeywordCollection keywords, bool enableStencil)
            {
                var result = new PassDescriptor
                {
                    displayName = "Universal Forward",
                    referenceName = "SHADERPASS_UNLIT",
                    useInPreview = true,

                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentColorAlpha,

                    structs = CoreStructCollections.Default,
                    requiredFields = UnlitRequiredFields.Unlit,
                    fieldDependencies = CoreFieldDependencies.Default,

                    renderStates = ShaderGraphStencilUtility.WithStencil(CoreRenderStates.UberSwitchedRenderState(target), enableStencil),
                    pragmas = CorePragmas.Forward,
                    defines = new DefineCollection { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection { keywords },
                    includes = new IncludeCollection { UnlitIncludes.Forward },

                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                if (target.activeSubTarget is UniversalStencilUnlitSubTarget unlitSubTarget)
                {
                    AddLightingVariantsControlToPass(ref result, unlitSubTarget);
                    AddDefaultDecalBlendingControlToPass(ref result, unlitSubTarget);
                    AddDefaultSSAOControlToPass(ref result, unlitSubTarget);
                }

                return result;
            }

            public static PassDescriptor DepthNormalOnly(UniversalTarget target, bool enableStencil)
            {
                var result = new PassDescriptor
                {
                    displayName = "DepthNormalsOnly",
                    referenceName = "SHADERPASS_DEPTHNORMALSONLY",
                    lightMode = "DepthNormalsOnly",
                    useInPreview = true,

                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = UnlitBlockMasks.FragmentDepthNormals,

                    structs = CoreStructCollections.Default,
                    requiredFields = UnlitRequiredFields.DepthNormalsOnly,
                    fieldDependencies = CoreFieldDependencies.Default,

                    renderStates = ShaderGraphStencilUtility.WithStencil(CoreRenderStates.DepthNormalsOnly(target), enableStencil),
                    pragmas = CorePragmas.Forward,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection { CoreKeywordDescriptors.GBufferNormalsOct },
                    includes = new IncludeCollection { CoreIncludes.DepthNormalsOnly },

                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor GBuffer(UniversalTarget target, bool enableStencil)
            {
                var result = new PassDescriptor
                {
                    displayName = "GBuffer",
                    referenceName = "SHADERPASS_GBUFFER",
                    lightMode = "UniversalGBuffer",
                    useInPreview = true,

                    passTemplatePath = UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = UniversalTarget.kSharedTemplateDirectories,

                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentColorAlpha,

                    structs = CoreStructCollections.Default,
                    requiredFields = UnlitRequiredFields.GBuffer,
                    fieldDependencies = CoreFieldDependencies.Default,

                    renderStates = ShaderGraphStencilUtility.WithStencil(CoreRenderStates.UberSwitchedRenderState(target), enableStencil),
                    pragmas = CorePragmas.GBuffer,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection { UnlitKeywords.GBuffer },
                    includes = new IncludeCollection { UnlitIncludes.GBuffer },

                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            static class UnlitBlockMasks
            {
                public static readonly BlockFieldDescriptor[] FragmentDepthNormals = new BlockFieldDescriptor[]
                {
                    BlockFields.SurfaceDescription.NormalWS,
                    BlockFields.SurfaceDescription.Alpha,
                    BlockFields.SurfaceDescription.AlphaClipThreshold,
                };
            }

            static class UnlitRequiredFields
            {
                public static readonly FieldCollection Unlit = new FieldCollection()
                {
                    StructFields.Varyings.positionWS,
                    StructFields.Varyings.normalWS
                };

                public static readonly FieldCollection DepthNormalsOnly = new FieldCollection()
                {
                    StructFields.Varyings.normalWS,
                };

                public static readonly FieldCollection GBuffer = new FieldCollection()
                {
                    StructFields.Varyings.positionWS,
                    StructFields.Varyings.normalWS,
                    UniversalStructFields.Varyings.sh,
                    UniversalStructFields.Varyings.probeOcclusion,
                };
            }
        }
        #endregion

        #region Defines
        static class UnlitDefines
        {
            public static readonly KeywordDescriptor LightingDefine = new KeywordDescriptor()
            {
                displayName = "Keep Lighting Variants",
                referenceName = "UNLIT_REALTIME_LIGHTING",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.Predefined,
                scope = KeywordScope.Local,
                stages = KeywordShaderStage.Vertex | KeywordShaderStage.Fragment
            };

            public static readonly KeywordDescriptor DefaultDecalBlendingDefine = new KeywordDescriptor()
            {
                displayName = "Default Decal Blending",
                referenceName = "UNLIT_DEFAULT_DECAL_BLENDING",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.Predefined,
                scope = KeywordScope.Local,
                stages = KeywordShaderStage.Fragment
            };

            public static readonly KeywordDescriptor DefaultSSAODefine = new KeywordDescriptor()
            {
                displayName = "Default SSAO",
                referenceName = "UNLIT_DEFAULT_SSAO",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.Predefined,
                scope = KeywordScope.Local,
                stages = KeywordShaderStage.Fragment
            };
        }
        #endregion

        #region Keywords
        static class UnlitKeywords
        {
            public static readonly KeywordCollection Forward = new KeywordCollection()
            {
                CoreKeywordDescriptors.StaticLightmap,
                CoreKeywordDescriptors.DirectionalLightmapCombined,
                CoreKeywordDescriptors.UseLegacyLightmaps,
                CoreKeywordDescriptors.LightmapBicubicSampling,
                CoreKeywordDescriptors.DBuffer,
                CoreKeywordDescriptors.DebugDisplay,
                CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion,
            };

            public static readonly KeywordCollection GBuffer = new KeywordCollection
            {
                CoreKeywordDescriptors.DBuffer,
                CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion,
                CoreKeywordDescriptors.RenderPassEnabled,
                CoreKeywordDescriptors.GBufferNormalsOct,
                CoreKeywordDescriptors.ShadowsShadowmask
            };

            public static readonly KeywordCollection LightingVariants = new KeywordCollection()
            {
                { CoreKeywordDescriptors.MainLightShadows },
                { CoreKeywordDescriptors.AdditionalLights },
                { CoreKeywordDescriptors.AdditionalLightShadows },
                { CoreKeywordDescriptors.ReflectionProbeBlending },
                { CoreKeywordDescriptors.ReflectionProbeBoxProjection },
                { CoreKeywordDescriptors.ReflectionProbeAtlas },
                { CoreKeywordDescriptors.ReflectionProbeRotation },
                { CoreKeywordDescriptors.ShadowsSoft },
                { CoreKeywordDescriptors.LightmapShadowMixing },
                { CoreKeywordDescriptors.ShadowsShadowmask },
                { CoreKeywordDescriptors.LightLayers },
                { CoreKeywordDescriptors.LightCookies },
                { CoreKeywordDescriptors.ClusterLightLoop },
            };
        }
        #endregion

        #region Includes
        static class UnlitIncludes
        {
            const string kUnlitPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl";
            const string kUnlitGBufferPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl";
            const string kLighting = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl";

            public static IncludeCollection Forward = new IncludeCollection
            {
                { CoreIncludes.DOTSPregraph },
                { CoreIncludes.FogPregraph },
                { CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { CoreIncludes.DBufferPregraph },
                { CoreIncludes.WriteRenderLayersPregraph },

                { CoreIncludes.CorePostgraph },
                { kUnlitPass, IncludeLocation.Postgraph },
            };

            public static IncludeCollection GBuffer = new IncludeCollection
            {
                { CoreIncludes.DOTSPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { CoreIncludes.DBufferPregraph },
                { CoreIncludes.WriteRenderLayersPregraph },

                { CoreIncludes.CorePostgraph },
                { kUnlitGBufferPass, IncludeLocation.Postgraph },
            };

            public static IncludeCollection LightingIncludes = new IncludeCollection
            {
                { kLighting, IncludeLocation.Pregraph },
            };
        }
        #endregion
    }
}
