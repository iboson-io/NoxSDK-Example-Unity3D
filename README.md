# NoxSDK
Detects 3D objects from uploaded 3D models

**Note**: Requires ARCore/ARkit support for smart phones or 6DoF tracking for wearables

## Steps to add Nox SDK to existing project
- Import [NoxSDK.unitypackage](https://github.com/iboson-io/NoxSDK-Example-Unity3D/releases)
- Add ObjectDetection.prefab to the scene with ARFoundation or ARCameraManager that does the 6Dof tracking
- Fill in MODEL_ID and TOKEN from [noxvision.ai](https://noxvision.ai/)
- Add AR Foundation, ARKit and ARCore XR plugins from package manager
- In project settings -> XR Plug-in Management -> Enable Google ARCore and ARkit based on your platform

- Wearable users add this line to `OnInitialized` to handle camera sensor orientation which is 90 degrees in android and ios.  `objectDetection.SetCameraSensorOrientation(0);`
