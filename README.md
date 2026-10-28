# 3D Mini Garage Workshop

A low-poly garage workshop scene built in **Unity 6** using the Universal Render Pipeline (URP). Includes an interactive car customizer, animated wall display, studio lighting, and dual support for desktop freefly and OpenXR VR.

![Mini Garage Workshop Preview](Screenshots/preview.png)

## Features

### Interactive Car Customizer
- Constructed from basic low-poly primitives (chassis, wheels, cabin, lights)
- Real-time color cycling (Cyan, Coral, Lime, Gold, Purple, White, Dark Slate)
- Texture pattern switching (Racing Stripes, Checkerboard, Cyber Grid)
- One-click reset to default state

### Animated Wall Display
- Wall-mounted screen with bezel frame
- Looping sports car video with telemetry HUD (speedometer, RPM, turbo)

### Environment
- Concrete tiled floor
- Workbench with toolbox and tool props
- Ceiling beams, baseboards, and trims
- Studio lighting setup (directional key + ambient fill)

### OpenXR VR Support
- Compatible with Meta Quest (1/2/3/3S/Pro), HTC Vive, Valve Index, and Windows Mixed Reality
- Procedural low-poly controller models with animated triggers and thumbsticks
- Laser pointer with surface reticle for UI and object interaction
- Locomotion options:
  - Smooth movement (left thumbstick, bounded to garage)
  - Snap turning (45° via right thumbstick)
  - Parabolic teleportation (grip button)

### Desktop Mode
- Freefly camera for development and testing without a headset
- Mouse + keyboard controls
- Optional simulated VR mode (press `V`) with on-screen virtual hands and laser

## Controls

### Desktop

| Action                  | Input                                      |
|-------------------------|--------------------------------------------|
| Look around             | Right Mouse Button + Mouse                 |
| Move                    | WASD or Arrow Keys                         |
| Ascend / Descend        | Space / E (up), Ctrl / Q (down)            |
| Speed boost             | Left Shift (2.5×)                          |
| Precision mode          | Left Alt (0.35×)                           |
| Adjust base speed       | Mouse Scroll Wheel                         |
| Reset camera            | Backspace or Home                          |
| Interact with UI        | Left Mouse Click                           |
| Cycle car color         | C                                          |
| Cycle car texture       | T                                          |
| Reset car               | R                                          |
| Toggle simulated VR     | V                                          |

### VR (OpenXR Controllers)

| Action                  | Input                                      |
|-------------------------|--------------------------------------------|
| Interact / Select       | Point laser + Index Trigger                |
| Smooth movement         | Left Thumbstick                            |
| Snap turn (45°)         | Right Thumbstick (left/right)              |
| Teleport                | Hold Grip → aim arc → release              |

## Getting Started

### Prerequisites
- Unity Hub
- Unity 6 (6000.0 or newer, tested on 6000.5.0f1)
- Universal Render Pipeline (URP)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Tomgit473/VR-Practicals.git
   cd VR-Practicals
   ```

2. Open the project in Unity Hub:
   - Click **Add** → **Add project from disk**
   - Select the `VR-Practicals` folder
   - Confirm the editor version is set to **Unity 6**

3. Open the scene:
   - Navigate to `Assets/TutorialInfo/Practicals.unity`
   - Double-click to open it

4. Press Play:
   - If a VR headset is connected → runs in VR
   - On desktop → use mouse + WASD to explore

## Project Structure

```
Assets/TutorialInfo/
├── Materials/              # URP materials (car, environment, VR)
├── Scripts/
│   ├── FreeFlyCamera.cs
│   ├── GarageCarCustomizer.cs
│   ├── GarageSceneBuilder.cs
│   ├── VRHandController.cs
│   ├── VRLaserPointer.cs
│   ├── VRLocomotion.cs
│   └── VRPlayerRig.cs
├── CarVideo.mp4            # Looping car video with HUD
├── Practicals.unity        # Main scene
└── ...
```

## Tech Stack

- **Engine**: Unity 6 (6000.5.0f1)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Input**: Unity Input System
- **XR**: OpenXR + XR Core Utils

## License

MIT License — see [LICENSE](LICENSE) for details.
