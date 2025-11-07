# NoxSDK
Detects 3D objects from uploaded 3D models

**Note**: Requires ARCore/ARkit support for smart phones or 6DoF tracking for wearables

### Platform independent steps to add Nox SDK to existing project
- Import [NoxSDK.unitypackage](https://github.com/iboson-io/NoxSDK-Example-Unity3D/releases)
- Add ObjectDetection.prefab to the scene with ARFoundation or Snapdragon Spaces that does the 6DoF tracking
- Fill in MODEL_ID and TOKEN from [noxvision.ai](https://noxvision.ai/)
- Add Unity Analytics from package manager
- Add package by name option in package manager and enter ```com.unity.cloud.gltfast``` if you want to add glb models

### Android and iOS
- Add AR Foundation, ARKit and ARCore XR plugins from package manager
- In project settings -> XR Plug-in Management -> Enable Google ARCore and ARkit based on your platform

### Wearable (Digilens ARGO & other Snapdragon spaces supported AR devices)
- Add this line to `OnInitialized` to handle camera sensor orientation which is 90 degrees in android and ios.  `objectDetection.SetCameraSensorOrientation(0);`
- Add AR Camera Manager script to MainCamera of XR Rig if it is not attached to it.
