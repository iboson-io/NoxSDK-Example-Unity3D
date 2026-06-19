# NoxSDK — Unity3D Example
Detects 3D objects from uploaded 3D models using the NoxVision SDK.

**Note**: Requires ARCore/ARKit support for smartphones or 6DoF tracking for wearables.

### Platform independent steps to add NoxSDK to existing project
- Import [NoxSDK.unitypackage](https://github.com/iboson-io/NoxSDK-Example-Unity3D/releases)
- Add ObjectDetection.prefab to the scene with ARFoundation or Snapdragon Spaces that does the 6DoF tracking
- Fill in MODEL_ID and TOKEN from [noxvision.ai](https://noxvision.ai/)
- Add package by name option in package manager and enter `com.unity.cloud.gltfast` if you want to add glb models
- Add package by name `com.unity.nuget.newtonsoft-json` or install Unity Analytics from package manager which has this package

### Android and iOS
- Add AR Foundation, ARKit and ARCore XR plugins from package manager
- In project settings → XR Plug-in Management → enable Google ARCore and ARKit based on your platform

### Wearable (Digilens ARGO & other Snapdragon Spaces supported AR devices)
- Add this line to `OnInitialized` to handle camera sensor orientation which is 90 degrees in Android and iOS: `objectDetection.SetCameraSensorOrientation(0);`
- Add AR Camera Manager script to MainCamera of XR Rig if it is not attached to it

### Video tutorial
https://www.youtube.com/watch?v=3fzKWOMkOqc

---

### Other NoxSDK examples
- [NoxSDK-Example-Android](https://github.com/iboson-io/NoxSDK-Example-Android) — ARCore (Java)
- [NoxSDK-Example-iOS](https://github.com/iboson-io/NoxSDK-Example-iOS) — ARKit (SwiftUI)

Get your MODEL_ID and TOKEN at [noxvision.ai](https://noxvision.ai/)
