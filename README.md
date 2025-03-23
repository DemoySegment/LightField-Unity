# LightField-Unity

![image-20250322221216741](E:\Computer Graphics\LightField\Assets\README-images\image-20250322221216741.png)



## Description

This unity project shows a lightfield demo based on Unity using compute shader. For better rendering result, the project is built in HDRP and uses the default scene in the HDRP template.



## Components

The demo is made to work in the editor mode. 

#### `LightField`

- A `LightField` GameObject is for generating a light field dataset. It has 2 planes for ray parametrization. The relative orientations and positions of the 2 planes are maintained. To adjust the position of the whole `LightField`, drag the `ST` plane so that the `UV` can follow.
- It will create a camera array on the `UV` plane when enabled. 
- Click `DoRender` will create the dataset to a `Texture2DArray`. `HasRenderResults` indicates the novel views can be generated.



#### `NovelViewSynthesis`

- A `NovelViewSynthesis` GameObject is a simulated camera in the scene. It uses the default plane mesh in Unity, which provides the image plane of a camera. A compute shader uses the pixel locations and a novel view camera position to sample in the light field dataset and outputs a render texture. This texture then becomes the main texture of the material of the plane to accomplish real-time novel view synthesis.
- There are two ways to sample the dataset (U, V, S, T):
  - nearest
  - quadrilinear interpolation
- It listens actions in `LightField` so that ideally, when render results are ready, the compute shader will start to run. If not, just make sure the `LightField` has render result, re-enable this script and check `DoRender` manually.
- Then the plane can be rotated/moved and the camera center can be moved so check novel views. 
- `SaveRenderTexture` will save the novel view as images.



## Versions

- Unity-2022.3.48f1
- rendering pipeline: HDRP



## Samples

![novelView4](E:\Computer Graphics\LightField\Assets\README-images\novelView4.png)

![novelView2](E:\Computer Graphics\LightField\Assets\README-images\novelView2.png)