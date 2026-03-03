## Unity URP Stencil Lit ShaderGraph Target
This package adds a **Stencil Lit** material SubTarget to the Universal Shader Graph target for URP.
It is based on Unity's built-in Lit subtarget and adds Canvas-style stencil properties and stencil render state support.

## Dependencies
* Works on Unity version 2021.2 or higher, with URP version 12 or higher

## How to install
Add this folder as a local package from your `Packages` directory, or via your git URL.

## How to use
Create a new graph from:

`Assets/Create/Shader Graph/URP/Stencil Lit Shader Graph`

Then select the **Stencil Lit** subtarget in the graph inspector. The generated material inspector includes stencil fields:
- Stencil Ref
- Stencil Comp
- Stencil Pass
- Stencil Read Mask
- Stencil Write Mask

### Notes
Stencil values map to Unity's standard stencil enums and masks.