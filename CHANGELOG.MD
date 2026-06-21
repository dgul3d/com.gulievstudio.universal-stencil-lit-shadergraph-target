# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


## [1.2.0] - 2026-06-21
### Added
- `Stencil Unlit` Shader Graph subtarget based on URP built-in Unlit subtarget.
- Stencil render state support in unlit passes (Forward, GBuffer, DepthNormalsOnly).
- Shader Graph creation menu item: `Assets/Create/Shader Graph/URP/Stencil Unlit Shader Graph`.
- Custom material GUI and VFX GUI variant for Stencil Unlit materials.
- Shared `ShaderGraphStencilUtility` for stencil properties and render state.
- Stencil Fail and Stencil Depth Fail material properties and shader render states.
- Shared `ShaderGraphStencilMaterialGUI` base with enum popups for Comp, Pass, Fail, and Depth Fail.

### Changed
- Stencil material inspector fields are shown at the bottom of Advanced options.
- Stencil Lit material GUI now uses the shared stencil material GUI base class.

## [1.1.0] - 2026-06-21
### Changed
- Updated package metadata and documentation.

## [1.0.0] - 2026-03-03
### Added
- Initial package setup at `Packages/com.gulievstudio.universal-stencil-lit-shadergraph-target`.
- `Stencil Lit` Shader Graph subtarget based on URP built-in Lit subtarget.
- Stencil render state support in lit passes (Forward, ForwardOnly, GBuffer, Meta, 2D, DepthNormals, DepthNormalsOnly).
- Shader Graph creation menu item: `Assets/Create/Shader Graph/URP/Stencil Lit Shader Graph`.
- Custom material GUI with stencil controls (`Stencil Ref`, `Stencil Comp`, `Stencil Pass`, `Stencil Read Mask`, `Stencil Write Mask`).
- VFX GUI variant for Stencil Lit materials.

### Changed
- Added graph inspector toggle `Enable Stencil` (Fullscreen-style behavior).
- Stencil properties and stencil render states are now conditionally included only when `Enable Stencil` is enabled.
