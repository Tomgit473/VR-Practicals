# 🚗 3D Mini Garage Workshop (Unity 6 + OpenXR VR)

A clean, modern, low-poly 3D garage workshop scene built with **Unity 6** and the **Universal Render Pipeline (URP)**. Features an interactive customizable car, animated wall video display, realistic studio lighting, and full dual-mode support for both **Desktop Freefly** and **OpenXR Virtual Reality (VR)**.

![Mini Garage Workshop Preview](Screenshots/preview.png)

---

## ✨ Features

- **🚗 Interactive Car Customizer**:
  - Built from basic low-poly shapes (cube chassis, sphere wheels, cabin block, headlights, and taillights).
  - Real-time color palette cycling (Cyan, Coral, Lime, Gold, Purple, White, Dark Slate).
  - Texture pattern switching (Racing Stripes, Checkerboard, Cyber Grid).
  - One-click reset to factory default state.

- **📺 Animated Wall TV Display**:
  - Wall-mounted flat screen quad with sleek bezel frame playing a custom looping sports car video with dynamic telemetry HUD (Speedometer, RPM, Turbo indicator).

- **🧰 Workshop Props & Environment**:
  - Detailed light-grey concrete tiled garage floor.
  - Wooden workbench table with red toolbox cube (handle) and blue steel tool cube.
  - Overhead stylized ceiling beams, wall baseboards, and accent trims.
  - Soft studio lighting with tuned directional key, ambient fill, and anti-aliased reflections.

- **🥽 Complete OpenXR VR Support**:
  - **Hardware Support**: Meta Quest (1 / 2 / 3 / 3S / Pro via Link/AirLink or standalone), HTC Vive, Valve Index, and Windows Mixed Reality.
  - **Procedural Controller Visuals**: Stylized low-poly controller models with animated triggers and thumbsticks.
  - **Laser Pointer**: Casts laser rays with surface reticle dots to interact directly with the world-space UI buttons, the car, or the TV screen.
  - **Locomotion**:
    - **Smooth Locomotion**: Move with the Left Thumbstick (bounded to stay inside garage walls).
    - **Snap Turning**: Snap 45° with the Right Thumbstick for comfort.
    - **Parabolic Teleportation**: Hold Grip on either controller to project a green arc onto the floor and release to teleport instantly.

- **🖥️ Desktop Mode (FreeFly Camera)**:
  - No VR headset required to develop or test!
  - Fly around smoothly using Unity flythrough controls.
  - Cursor automatically unlocks when not looking around so you can click the UI buttons with your mouse.
  - Press <kbd>V</kbd> to toggle **Simulated VR Mode** with virtual hands and laser pointer on screen.

---

## 🎮 Controls

### Desktop Controls

| Action | Key / Input |
|---|---|
| **Look Around** | Hold <kbd>Right Mouse Button</kbd> + Move Mouse |
| **Move (Fly)** | <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> or Arrow Keys |
| **Ascend / Descend** | <kbd>Space</kbd> / <kbd>E</kbd> (Up) &nbsp;\|&nbsp; <kbd>Left Ctrl</kbd> / <kbd>Q</kbd> (Down) |
| **Speed Boost** | Hold <kbd>Left Shift</kbd> (2.5× speed) |
| **Precision Slow** | Hold <kbd>Left Alt</kbd> (0.35× speed) |
| **Adjust Base Speed** | <kbd>Mouse Scroll Wheel</kbd> |
| **Reset Camera** | <kbd>Backspace</kbd> or <kbd>Home</kbd> |
| **Click Buttons** | <kbd>Left Mouse Click</kbd> directly on wall buttons |
| **Car Customizer Hotkeys** | <kbd>C</kbd> (Color) &nbsp;\|&nbsp; <kbd>T</kbd> (Texture) &nbsp;\|&nbsp; <kbd>R</kbd> (Reset) |
| **Simulated VR Toggle** | <kbd>V</kbd> |

### VR Controls (OpenXR Controllers)

| Action | Controller Input |
|---|---|
| **Interact / Click** | Point laser at UI button / Car / TV and pull <kbd>Index Trigger</kbd> |
| **Smooth Movement** | <kbd>Left Thumbstick</kbd> (move forward, backward, strafe) |
| **Snap Turn (45°)** | Flick <kbd>Right Thumbstick</kbd> Left / Right |
| **Teleport** | Hold <kbd>Grip Button</kbd>, aim arc at floor, and release |

---

## 🚀 Getting Started

### Prerequisites

- **Unity Hub**
- **Unity 6** (`6000.0` or higher, e.g. `6000.5.0f1`)
- **Universal Render Pipeline (URP)**

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Tomgit473/AR-Lab-3D.git
   cd AR-Lab-3D
   ```

2. **Open in Unity Hub**:
   - Click **Add** -> **Add project from disk**.
   - Select the `AR-Lab-3D` folder.
   - Ensure the editor version is set to **Unity 6**.

3. **Open the Scene**:
   - In the Unity Project window, navigate to:
     ```text
     Assets / TutorialInfo / Practicals.unity
     ```
   - Double-click `Practicals.unity` to load the scene.

4. **Press Play**:
   - Click the ▶️ **Play** button in the Unity Editor toolbar.
   - If a VR headset is connected, it will automatically run in VR!
   - If on desktop, use the mouse and WASD keys to explore and interact.

---

## 📁 Project Structure

```text
Assets/TutorialInfo/
├── Materials/                   # URP materials for car, room, props, and VR visuals
│   ├── Mat_CarBody.mat
│   ├── Mat_CarWheel.mat
│   ├── Mat_ConcreteFloor.mat
│   ├── Mat_VRController_Dark.mat
│   ├── Mat_VRLaser.mat
│   └── ...
├── Scripts/                     # Core runtime & editor scripts
│   ├── FreeFlyCamera.cs         # Desktop flythrough camera controller
│   ├── GarageCarCustomizer.cs   # Car customization logic & button bindings
│   ├── GarageSceneBuilder.cs    # Procedural editor scene builder tool
│   ├── VRHandController.cs      # OpenXR hardware tracking & procedural hand visuals
│   ├── VRLaserPointer.cs        # UI & 3D raycast interaction laser
│   ├── VRLocomotion.cs          # Smooth movement, snap turn, and teleportation
│   └── VRPlayerRig.cs           # Master XR Origin coordinator
├── CarVideo.mp4                 # 60fps stylized car animation with telemetry HUD
├── Practicals.unity             # Main workshop scene
└── ...
```

---

## 🛠️ Tech Stack

- **Engine**: Unity 6 (`6000.5.0f1`)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Input**: New Unity Input System (`com.unity.inputsystem`)
- **XR Core**: Unity OpenXR (`com.unity.xr.openxr`) & XR Core Utils (`com.unity.xr.core-utils`)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
