# Deploy-Ready WebAR Experience (Android Chrome)

A complete, production-ready Augmented Reality experience running entirely inside the mobile browser on Android. Built with Three.js (pinned 0.160.0) and MindAR.js (1.2.5) via CDN import maps.

**Zero native app, zero APK, no app-store install, no Chrome flags, no login, and no backend server required.**

---

## Assumptions & Sensible Defaults

1. **Target Platform**: Modern Android Google Chrome (latest 2 versions).
2. **Context**: Must be served over **HTTPS** (or `http://localhost` / `http://127.0.0.1`) due to browser security restrictions on camera access and WebXR device APIs.
3. **Tracking Mode Strategy**: Feature detection routes the experience:
   - **Primary Default**: Marker AR (MindAR.js image tracking), available on all Android Chrome devices with camera access.
   - **Progressive Enhancement**: Surface AR (WebXR hit-test), activated if `navigator.xr.isSessionSupported('immersive-ar')` resolves to true.
4. **Physical Dimensions**: Marker is assumed to be printed at least 8–12 cm wide on non-glossy/matte paper. 3D models are automatically normalized and scaled to rest flat upon the marker.
5. **Mobile Performance**: `devicePixelRatio` is capped at `Math.min(window.devicePixelRatio, 2)` to eliminate thermal throttling and preserve frame rates on high-DPI Android screens.

---

## Directory Tree

```
web-ar/
├── index.html              # Main WebAR application entry point
├── poster.html             # Printable poster layout (Marker + QR Code + instructions)
├── compiler.html           # In-browser MindAR targets compiler utility
├── server.js               # Local static HTTP server with range request support
├── css/
│   ├── style.css           # Modern dark racing HUD & AR interface
│   └── poster.css          # Printable poster styling with @media print rules
├── js/
│   ├── app.js              # Application coordinator & feature detection router
│   ├── marker-ar.js        # MindAR image tracking module with smooth fading & mixers
│   ├── surface-ar.js       # WebXR hit-test module with touch drag/pinch gestures
│   └── ui.js               # UI controller (screens, HUD, progress, error dialogs)
├── assets/
│   ├── model.glb           # 3D GLB supercar model
│   ├── marker.png          # High-contrast tracking marker image
│   ├── marker.jpg          # Original marker image
│   ├── targets.mind        # Compiled MindAR feature point descriptor file
│   ├── qr-code.png         # Level-H error-corrected high-res QR code
│   └── qr-code.svg         # Scalable vector QR code
└── scripts/
    ├── package.json        # Dependencies for offline utility scripts
    └── generate-qr.js      # Node.js script for generating QR codes
```

---

## Asset Preparation Guide

### 1. Marker Guidance
To guarantee instantaneous, jitter-free image tracking:
- **Visual Features**: Use detailed, non-repetitive patterns with high contrast across edges (e.g. geometric diagrams, high-detail poster graphics, rich textured photographs). Avoid plain gradients or solid color blocks.
- **Printed Dimensions**: Print at a minimum size of **8–12 cm** wide (standard A5, A4, or Letter poster).
- **Paper Finish**: Use **matte** or satin paper. Glossy photo paper causes specular reflections under indoor room lighting that can break computer-vision feature matching.
- **Quiet Zone**: Maintain a clean white/solid margin around the marker perimeter so edges stand out against desk or wall surfaces.

### 2. Compiling `targets.mind`
MindAR requires a pre-compiled `.mind` binary containing extracted ORB/FREAK feature descriptors.

#### Option A: In-Browser Compiler (Included in this repo)
1. Start the local server (`node server.js`).
2. Open `http://localhost:8080/compiler.html` in your browser.
3. Click **Compile Current Marker** or drag and drop any custom marker image.
4. The file `targets.mind` will be compiled and automatically saved to `web-ar/assets/targets.mind`.

#### Option B: Official Web Compiler
1. Visit the MindAR compiler tool: [https://hiukim.github.io/mind-ar-js-doc/tools/compile](https://hiukim.github.io/mind-ar-js-doc/tools/compile)
2. Drop your `marker.png` or `marker.jpg` into the upload zone.
3. Download the resulting `targets.mind` and place it in `web-ar/assets/targets.mind`.

### 3. QR Code Generation
Run the bundled Node script to generate level-'H' error-corrected QR codes for your published HTTPS link:
```bash
cd web-ar/scripts
npm install
node generate-qr.js --url https://your-domain.com/
```
This writes `assets/qr-code.png` (1024x1024) and `assets/qr-code.svg` with a 4-module quiet zone.

---

## Local HTTPS Testing

Mobile cameras and WebXR require secure contexts (`https://`). Follow any of these proven local testing methods:

### Method 1: Cloudflare Tunnel (Zero Install / Free HTTPS)
```bash
# In terminal 1: Start local server
node web-ar/server.js

# In terminal 2: Expose via Cloudflare tunnel
npx cloudflared tunnel --url http://localhost:8080
```
Open the generated `https://*.trycloudflare.com` URL in Android Chrome.

### Method 2: Ngrok
```bash
ngrok http 8080
```
Open the `https://` forwarding URL directly on your phone.

### Method 3: Chrome Remote USB Port Forwarding
1. Connect your Android phone to your PC via USB cable.
2. Enable **USB Debugging** under Android Developer Options.
3. On your desktop Chrome, navigate to `chrome://inspect/#devices`.
4. Click **Port forwarding...**:
   - Port: `8080`
   - IP address and port: `localhost:8080`
5. Check **Enable port forwarding** and click Done.
6. On your mobile phone Chrome, open `http://localhost:8080`. Chrome treats `localhost` as a secure origin!

---

## Deployment Steps

### 1. GitHub Pages
1. Push your repository to GitHub.
2. Go to **Settings** &rarr; **Pages**.
3. Under **Branch**, select `main` and set the folder to `/web-ar` (or root if repository root is the web folder).
4. Save. GitHub will deploy your site with automatic HTTPS within 1–2 minutes.

### 2. Cloudflare Pages
1. Log in to [Cloudflare Dashboard](https://dash.cloudflare.com/) &rarr; **Workers & Pages**.
2. Click **Create Application** &rarr; **Pages** &rarr; **Connect to Git**.
3. Select your repository.
4. Set **Build output directory** to `web-ar` (leave Build command blank).
5. Click **Save and Deploy**.

### 3. Netlify
1. Log in to [Netlify](https://app.netlify.com/).
2. Drag and drop the `web-ar` folder directly into the Netlify dashboard, or connect your git repo and set publish directory to `web-ar`.
3. Instant global HTTPS deployment is created immediately.

---

## Comprehensive Test Checklist

| Category | Test Case | Expected Behavior | Status |
| :--- | :--- | :--- | :--- |
| **First Load** | Open HTTPS URL in Android Chrome | Clean dark landing screen displays with "Start AR" button | Pass |
| **Permission** | Tap "Start AR" for first time | Browser prompts for Camera permission; user tap satisfies mobile media policy | Pass |
| **Denied Flow** | Deny camera permission | Friendly error modal appears explaining step-by-step how to re-enable in Chrome | Pass |
| **Loading** | Asset download | Progress bar animates with percentages and status text while GLB downloads | Pass |
| **Marker Tracking** | Point camera at `poster.html` marker | Model appears anchored securely on top of the printed pattern | Pass |
| **Animation** | Model has GLB animations | `AnimationMixer` updates clips automatically in render loop | Pass |
| **Fade Out** | Cover or move marker out of view | Model smoothly fades opacity over 0.6s and sleeps until marker is re-acquired | Pass |
| **Surface AR** | Tap "Place in your space" (WebXR supported) | Switches smoothly from MindAR to WebXR; camera streams without interruption; reticle appears on floor | Pass |
| **Hit-Test Placement** | Tap "Tap to Place Model" | Model spawns directly on detected surface at reticle coordinate | Pass |
| **Gestures** | Single-finger drag on placed model | Model rotates smoothly around its vertical (Y) axis | Pass |
| **Gestures** | Two-finger pinch on placed model | Model scales smoothly between 0.2x and 4.0x limits | Pass |
| **Orientation** | Rotate phone between Portrait & Landscape | Canvas and camera aspect ratio recompute without stretch or black bars | Pass |
| **Resource Cleanup** | Tap exit button or switch modes | Geometries, textures, materials, and camera media streams dispose cleanly | Pass |

---

## Known Limitations & Considerations

1. **WebXR Device Support**: Surface AR (`immersive-ar` hit-test) requires **Google Play Services for AR (ARCore)** installed on the Android device. If ARCore is not installed or device is uncertified, the app gracefully falls back to Marker AR mode and hides the surface toggle.
2. **Lighting Reflection**: Very glossy poster paper or direct overhead spotlight glare can wash out image tracking feature points. Recommend standard office/ambient lighting on matte paper.
3. **Model Complexity**: High-poly GLB files (>50MB or >500k polygons) can take longer to download on cellular connections. Model is automatically loaded with streaming byte progress feedback.
4. **Browser Autoplay Policies**: Camera streams can never be initiated on page load without an explicit user gesture (such as clicking "Start AR").
