# Pixelated Camera - Meta Quest Passthrough MR Application

## Table of Contents

1. [Project Overview](#project-overview)
2. [Features](#features)
3. [Requirements](#requirements)
4. [How It Works](#how-it-works)
5. [Setup & Installation](#setup--installation)
6. [Usage Instructions](#usage-instructions)
7. [Technical Details](#technical-details)
8. [Troubleshooting](#troubleshooting)
9. [Additional Resources](#additional-resources)
10. [License](#license)

## Project Overview

**Pixelated Camera** is a Mixed Reality (MR) application designed for Meta Quest 3/3S headsets that creates a unique pixelation effect on the live camera feed from the headset. The application leverages Unity's `WebCamTexture` API to capture the passthrough camera feed and applies a real-time pixelation effect by rendering the camera texture to a low-resolution `RenderTexture`, which is then displayed on a 3D plane in the MR environment.

This project demonstrates how to:
- Access and display Quest 3/3S camera feed in Unity
- Apply GPU-based pixelation effects using render textures
- Create interactive MR experiences with dynamic resolution control
- Handle camera permissions for Meta Quest devices

### Key Features

✨ **Real-time Camera Feed**: Captures live video from Quest 3/3S passthrough cameras  
🎨 **Dynamic Pixelation Effect**: Apply adjustable pixelation effect to the camera feed  
🎮 **Interactive Resolution Control**: Switch between multiple resolution presets (144p, 360p, 720p, Full HD)  
🔲 **Render Texture Pipeline**: Efficient GPU-based effect processing  
👓 **Immersive MR Experience**: View pixelated camera feed on a virtual plane in your environment

## Features

### 🎥 Camera Feed Display
- Real-time camera feed from Meta Quest 3/3S passthrough cameras
- Supports both left and right camera selection
- Live feed rendered on a 3D plane in MR space

### 🔲 Pixelation Effect
The pixelation effect is achieved through a **render texture pipeline**:
1. Camera feed is captured via `WebCamTexture`
2. The texture is blitted (copied) to a low-resolution `RenderTexture`
3. When the render texture is displayed at a larger size, the low resolution creates a pixelated/mosaic effect
4. This is a GPU-efficient method that doesn't require custom shaders

### 📐 Resolution Control
Four resolution presets available via UI buttons:
- **144p**: 256 x 144 pixels (Heavy pixelation)
- **360p**: 640 x 360 pixels (Medium pixelation)
- **720p**: 1280 x 720 pixels (Light pixelation)
- **Full HD**: 2064 x 2280 pixels (Minimal pixelation)

### 🎮 Interactive UI
- Button-based resolution switching
- Real-time resolution display
- Responsive control system

## Requirements

### Software

- **Unity Editor:**
  - Unity **6000.1.13f1** or compatible version
  - Tested with Unity 6000.x series
  
- **Required Unity Packages:**
  - **Meta XR SDK All** (com.meta.xr.sdk.all, v78.0.0)
  - **Meta MR Utility Kit** (com.meta.xr.mrutilitykit, v74.0.0)
  - **Unity Sentis** (com.unity.sentis, v2.2.0)
  - **XR Plugin Management** (com.unity.xr.management, v4.5.2)
  - **Oculus XR Plugin** (com.unity.xr.oculus, v4.5.2)

### Hardware

- **Meta Quest 3 or Quest 3S** headset
- Must be running **Horizon OS v74** or higher
- USB-C cable for development builds (or wireless ADB connection)

### Permissions

The application requires the following Android permissions:
- `android.permission.CAMERA` - Required by `WebCamTexture` API
- `horizonos.permission.HEADSET_CAMERA` - Meta-specific permission for passthrough camera access

These permissions are automatically requested at runtime by the `PassthroughCameraPermissions` component.

## How It Works

### Pixelation Technique

This application uses a **render texture downsampling** technique to create the pixelation effect:

1. **Camera Capture**: The `WebCamTextureManager` component initializes and manages the Quest's passthrough camera, providing a `WebCamTexture` object with the camera feed.

2. **Render Texture Pipeline**: 
   - A `RenderTexture` is created with a specific resolution (e.g., 256x144 for heavy pixelation)
   - Each frame, the camera texture is blitted (copied) to this render texture using `Graphics.Blit()`
   - This downsampling happens on the GPU, making it very efficient

3. **Display**: 
   - The render texture is applied to a material on a 3D plane in the scene
   - When the low-resolution texture is stretched to fit the plane, it creates the characteristic pixelated/blocky appearance
   - The larger the display size relative to the texture resolution, the more pronounced the pixelation effect

4. **Dynamic Resolution**: 
   - Users can change the render texture resolution at runtime via UI buttons
   - Lower resolutions = more pixelation
   - Higher resolutions = less pixelation, approaching the original camera quality

### Architecture

```
Quest Camera Feed (WebCamTexture)
         ↓
Graphics.Blit() [GPU Operation]
         ↓
Low-Resolution RenderTexture
         ↓
Material on 3D Plane
         ↓
Pixelated Visual Effect
```

### Key Components

- **`CameraDisplay.cs`**: Main controller script that:
  - Manages the WebCamTextureManager
  - Handles the blit operation from camera to render texture
  - Manages UI buttons and resolution switching
  - Updates the resolution display text

- **`WebCamTextureManager`**: Handles camera initialization, permissions, and provides access to the camera texture

- **`PassthroughCameraPermissions`**: Manages Android permission requests for camera access

- **`RenderTexture`**: GPU texture used for downsampling and creating the pixelation effect

## Setup & Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Pratik77221/PixelatedCamera_MetaPassthrough.git
cd PixelatedCamera_MetaPassthrough
```

### 2. Open in Unity

1. Launch Unity Hub
2. Click "Open" and select the cloned repository folder
3. Unity will open the project (may take a few minutes for initial import)

### 3. Configure for Quest

1. In Unity, go to **File > Build Settings**
2. Select **Android** platform and click **Switch Platform**
3. Open **Meta > Tools > Project Setup Tool**
4. Click **Fix All** to resolve any configuration issues
5. Ensure the following settings are correct:
   - **Player Settings > XR Plug-in Management > Oculus** is enabled
   - **Player Settings > Other Settings > Minimum API Level** is set to Android 10.0 (API level 29) or higher
   - **Color Space** is set to Linear

### 4. Android Manifest Configuration

Ensure your `AndroidManifest.xml` includes:

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="com.horizonos.permission.HEADSET_CAMERA" />
<uses-feature android:name="android.hardware.camera" />
```

For Unity 6, the activity must use:
```xml
android:name="com.unity3d.player.UnityPlayerGameActivity"
```

### 5. Build and Deploy

1. Connect your Quest 3/3S via USB or set up wireless ADB
2. Enable **Developer Mode** on your Quest headset
3. In Unity, go to **File > Build Settings**
4. Click **Build and Run**
5. Unity will build the APK and install it on your Quest headset

## Usage Instructions

### Starting the Application

1. Put on your Meta Quest 3/3S headset
2. Launch the "Pixelated Camera" app from your app library
3. Grant camera permissions when prompted (two permissions: Camera and Headset Camera)
4. The camera feed will appear on a plane in front of you

### Controls

#### Resolution Buttons

Use the UI buttons to change the pixelation level:

- **144p Button**: Maximum pixelation (256x144) - Creates a heavily pixelated, retro aesthetic
- **360p Button**: High pixelation (640x360) - Noticeable blocky effect
- **720p Button**: Moderate pixelation (1280x720) - Subtle pixelation
- **Full HD Button**: Minimal pixelation (2064x2280) - Near original quality

#### Using the Interface

1. Look at the UI panel in the MR space
2. Use your Quest controllers to point and click buttons
3. The resolution indicator will update to show the current setting
4. The visual effect changes immediately when you select a different resolution

### What to Expect

- **Initial Load**: The app requests camera permissions on first launch
- **Camera Feed**: Real-time video from your Quest's passthrough camera
- **Pixelation Effect**: The feed will display with the selected pixelation level
- **Performance**: The app runs smoothly as the pixelation is GPU-based and very efficient

## Technical Details

### Project Structure

```
Assets/
├── CameraDisplay.cs              # Main controller script
├── Pixelation.renderTexture      # Render texture for pixelation effect
├── PixelationMaterial.mat        # Material for displaying the render texture
├── MR.unity                      # Main scene
├── MainMenu.prefab               # UI prefab with resolution buttons
└── PassthroughCameraApiSamples/  # Meta's Passthrough Camera API samples
    └── PassthroughCamera/
        └── Scripts/
            ├── WebCamTextureManager.cs          # Camera initialization
            ├── PassthroughCameraPermissions.cs  # Permission handling
            └── PassthroughCameraUtils.cs        # Camera utilities
```

### Resolution Options

| Preset | Resolution | Aspect Ratio | Use Case |
|--------|-----------|--------------|----------|
| 144p   | 256 x 144 | 16:9 | Maximum pixelation, retro look |
| 360p   | 640 x 360 | 16:9 | High pixelation, artistic effect |
| 720p   | 1280 x 720 | 16:9 | Moderate pixelation |
| Full HD | 2064 x 2280 | ~9:10 | Minimal pixelation, camera native resolution |

### Performance Considerations

- **GPU-Based**: The `Graphics.Blit()` operation runs entirely on the GPU, ensuring minimal CPU overhead
- **Frame Rate**: The application maintains smooth performance across all resolution settings
- **Memory**: Render textures are managed efficiently and released when resolution changes
- **Camera Feed**: Supports up to 1280x960 camera resolution from Quest cameras

### Code Snippets

#### Basic Pixelation Setup

```csharp
// In Update() - Apply pixelation effect
if (WebcamManager != null && WebcamManager.WebCamTexture != null && TargetRenderTexture != null)
{
    // Copy webcam texture to low-res render texture
    Graphics.Blit(WebcamManager.WebCamTexture, TargetRenderTexture);
}
```

#### Changing Resolution

```csharp
public void SetResolution(int resIndex)
{
    var targetRes = m_resolutions[resIndex];
    
    // Update render texture resolution
    TargetRenderTexture.Release();
    TargetRenderTexture.width = targetRes.x;
    TargetRenderTexture.height = targetRes.y;
    TargetRenderTexture.Create();
}
```

## Troubleshooting

### Common Issues

#### Camera Not Initializing

**Problem**: Camera feed doesn't appear or stays black

**Solutions**:
- Verify that both camera permissions are granted in Settings
- Check Unity Console for error messages
- Ensure Quest is running Horizon OS v74 or higher
- Try restarting the app after granting permissions

#### Permission Denied

**Problem**: Permission requests are denied or not appearing

**Solutions**:
- Uninstall and reinstall the app to trigger permission requests again
- Grant permissions manually via ADB:
  ```bash
  adb shell pm grant <PACKAGE.NAME> com.horizonos.permission.HEADSET_CAMERA
  adb shell pm grant <PACKAGE.NAME> android.permission.CAMERA
  ```
- Check that AndroidManifest.xml includes the required permissions

#### Low Performance

**Problem**: App is laggy or has low frame rate

**Solutions**:
- Use lower render texture resolutions (144p or 360p)
- Ensure your Quest has adequate battery
- Close other running applications
- Check that the project is built in Release mode, not Debug

#### Build Errors

**Problem**: Unity build fails or has errors

**Solutions**:
- Run **Meta > Tools > Project Setup Tool** and fix all issues
- Verify all required packages are installed
- For Unity 6, ensure AndroidManifest uses `UnityPlayerGameActivity`
- Clear the build cache and rebuild

### Known Limitations

- **Single Camera Access**: Only one passthrough camera (left or right) can be accessed at a time
- **WebCamTexture Constraints**: Maximum supported camera resolution is 1280x960
- **Frame Delay**: Approximately 40-60ms delay between camera capture and display (Unity `WebCamTexture` limitation)
- **Permission System**: Can only handle one permission request at a time

### Getting Help

If you encounter issues not covered here:

1. Check the Unity Console for detailed error messages
2. Review device logs using `adb logcat`
3. Verify your setup matches the requirements
4. Open an issue on the GitHub repository with:
   - Unity version
   - Quest device model and OS version
   - Error messages or logs
   - Steps to reproduce the issue

## Additional Resources

### Related Projects

This project uses Meta's **Passthrough Camera API** samples. For more advanced camera features, check out:

- **[CameraViewer](./Assets/PassthroughCameraApiSamples/CameraViewer)**: Basic camera feed display
- **[CameraToWorld](./Assets/PassthroughCameraApiSamples/CameraToWorld)**: Convert 2D coordinates to 3D world space
- **[BrightnessEstimation](./Assets/PassthroughCameraApiSamples/BrightnessEstimation)**: Environmental brightness detection
- **[MultiObjectDetection](./Assets/PassthroughCameraApiSamples/MultiObjectDetection)**: ML-based object recognition with Unity Sentis
- **[ShaderSample](./Assets/PassthroughCameraApiSamples/ShaderSample)**: Custom GPU shader effects

### Documentation

- [Meta Passthrough Camera API Documentation](https://developers.meta.com/horizon/documentation/unity/unity-pca-overview)
- [Unity WebCamTexture Documentation](https://docs.unity3d.com/ScriptReference/WebCamTexture.html)
- [Unity Graphics.Blit Documentation](https://docs.unity3d.com/ScriptReference/Graphics.Blit.html)
- [Meta XR SDK Documentation](https://developers.meta.com/horizon/documentation/unity/unity-overview/)

### Learning Resources

- Unity MR Development for Quest: [Meta Developer Hub](https://developers.meta.com/horizon/)
- Unity Render Textures: [Unity Learn](https://learn.unity.com/)
- Meta Quest Development: [Getting Started Guide](https://developers.meta.com/horizon/documentation/unity/unity-gs-overview)

## License

The **Oculus License** applies to the SDK and supporting material. The **MIT License** applies to only certain, clearly marked documents. If an individual file does not indicate which license it is subject to, then the Oculus License applies.

See the [LICENSE.txt](./LICENSE.txt) file for details.

### Attribution

This project builds upon Meta's **Unity Passthrough Camera API Samples**:
- Repository: [Unity-PassthroughCameraApiSamples](https://github.com/oculus-samples/Unity-PassthroughCameraApiSamples)
- Components used: WebCamTextureManager, PassthroughCameraPermissions, PassthroughCameraUtils

---

## Contributing

Contributions are welcome! If you have improvements or bug fixes:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

Please ensure your code follows the existing style and includes appropriate comments.

---

**Made with ❤️ for the Meta Quest Community**

For questions or feedback, please open an issue on GitHub.
