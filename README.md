![](https://img.shields.io/badge/license-MIT-green)

# URP Ignore PostProcessing
 This project provides a rendering pipeline that excludes specific objects from post-processing. This feature allows you to control which objects are affected by post-processing, even in scenes with Transparent objects, without modifying existing post-processing effects or shaders.<br>
 This has been verified to work with Unity 2022.3.17f1 and URP 14.0.9.

 # Demo
 The image below demonstrates the feature applied to capsule objects.
 
 <img name="2025-01-14 ignorepostprocessing" src="https://github.com/user-attachments/assets/4b2384b9-7297-41f4-baa5-592fa685f9ee" width="500px">

 # Features
Two RendererFeatures are provided in this project:

1. **CustomTexture**: A feature to render the color or depth of opaque and transparent objects into a texture after rendering.<br>
2. **IgnorePostProcessing**: A feature to render objects on top of post-processing effects. Requires a CustomTexture assigned in the Inspector to define the object's color.

For more complex scenes, consider layering objects that should bypass post-processing and creating corresponding CustomTextures and IgnorePostProcessing features.

# Note
 Please note that some post-processing effects, such as those involving multisampling, may produce inconsistent results.<br>
 Additionally, increasing the number of RenderFeatures can impact performance. It is recommended to minimize the number of layers to which target objects belong.
