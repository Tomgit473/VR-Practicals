import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { UIController } from './ui.js';
import { MarkerARExperience } from './marker-ar.js';
import { SurfaceARExperience } from './surface-ar.js';

/**
 * Main Application Orchestrator for WebAR Experience
 */
class WebARApp {
  constructor() {
    this.ui = new UIController();
    this.currentMode = 'marker'; // 'marker' | 'surface'
    this.isSurfaceSupported = false;

    // Assets
    this.modelUrl = './assets/model.glb';
    this.targetUrl = './assets/targets.mind';
    this.loadedGltf = null;

    // Active Instances
    this.markerExperience = null;
    this.surfaceExperience = null;

    // 1. Bind events synchronously so button clicks register immediately
    this.bindEvents();

    // 2. Initialize feature detection and secure context asynchronously
    this.init();

    // 3. If user already tapped Start while scripts were loading, start immediately
    if (window.startRequested) {
      console.log('[WebAR] Start requested prior to module load, starting now');
      this.startExperience();
    }
  }

  logDebug(msg) {
    console.log('[WebAR]', msg);
    const dbg = document.getElementById('debug-banner');
    if (dbg) {
      dbg.style.display = 'block';
      dbg.textContent = msg;
    }
  }

  async init() {
    this.logDebug('Engine ready. Checking device capabilities...');

    // 1. Verify Secure Context (HTTPS or localhost required for camera & WebXR)
    const isLocalhost = window.location.hostname === 'localhost' ||
                        window.location.hostname === '127.0.0.1' ||
                        window.location.hostname === '[::1]';

    if (!window.isSecureContext && !isLocalhost) {
      console.error('[WebAR] Secure context required');
      this.ui.showError('INSECURE_CONTEXT');
      return;
    }

    // 2. Feature Detection with 1.5s timeout (never hang initialization)
    try {
      const surfaceCheck = Promise.race([
        SurfaceARExperience.isSupported(),
        new Promise((resolve) => setTimeout(() => resolve(false), 1500))
      ]);
      this.isSurfaceSupported = await surfaceCheck;
    } catch (e) {
      this.isSurfaceSupported = false;
    }

    this.logDebug(`Ready • Surface AR supported: ${this.isSurfaceSupported}`);
    this.ui.showLanding();
  }

  bindEvents() {
    let lastTap = 0;
    const triggerStart = (e) => {
      const now = Date.now();
      if (now - lastTap < 600) return; // Debounce 600ms
      lastTap = now;
      if (e) e.preventDefault();
      this.logDebug('Start AR triggered. Requesting camera...');
      this.startExperience();
    };

    // Support both click and touch events
    this.ui.startBtn?.addEventListener('click', triggerStart);
    this.ui.startBtn?.addEventListener('touchend', triggerStart);

    // Mode Toggle Button ("Place in your space" / "Switch to Marker AR")
    this.ui.switchModeBtn?.addEventListener('click', () => {
      this.toggleMode();
    });

    // Exit Button
    this.ui.exitBtn?.addEventListener('click', () => {
      this.exitToLanding();
    });

    // Surface AR Placement Button
    this.ui.placeBtn?.addEventListener('click', () => {
      if (this.currentMode === 'surface' && this.surfaceExperience) {
        const placed = this.surfaceExperience.placeOrReposition();
        if (placed) {
          this.ui.setSurfacePlaced(true);
        }
      }
    });
  }

  async startExperience() {
    this.ui.showLoading('Starting AR camera...');

    try {
      // 1. Start marker mode immediately so camera feed and tracking open with zero lag
      await this.launchMarkerMode();

      // 2. Load 3D model progressively in background (or immediately if already cached)
      if (!this.loadedGltf) {
        this.loadModelWithProgress()
          .then((gltf) => {
            console.log('[WebAR] Model loaded, attaching to active AR scene');
            if (this.currentMode === 'marker' && this.markerExperience) {
              this.markerExperience.setupModel(gltf);
            } else if (this.currentMode === 'surface' && this.surfaceExperience) {
              this.surfaceExperience.buildModelInstance();
            }
          })
          .catch((err) => {
            console.warn('[WebAR] Background model load error:', err);
          });
      }
    } catch (err) {
      console.error('[WebAR] Start error:', err);
      if (err.name === 'NotAllowedError' || err.message?.includes('Permission denied')) {
        this.ui.showError('CAMERA_PERMISSION_DENIED');
      } else {
        this.ui.showError('ASSET_LOAD_FAILED', err.message);
      }
    }
  }

  loadModelWithProgress() {
    return new Promise((resolve, reject) => {
      const loader = new GLTFLoader();

      this.ui.updateProgress(10, 'Downloading 3D model...');

      loader.load(
        this.modelUrl,
        (gltf) => {
          this.loadedGltf = gltf;
          this.ui.updateProgress(100, 'Model loaded successfully');
          resolve(gltf);
        },
        (xhr) => {
          if (xhr.lengthComputable && xhr.total > 0) {
            // Scale progress from 10% to 90%
            const pct = 10 + (xhr.loaded / xhr.total) * 80;
            const mbLoaded = (xhr.loaded / 1048576).toFixed(1);
            const mbTotal = (xhr.total / 1048576).toFixed(1);
            this.ui.updateProgress(pct, `Loading model (${mbLoaded} MB / ${mbTotal} MB)...`);
          } else {
            const mbLoaded = (xhr.loaded / 1048576).toFixed(1);
            this.ui.updateProgress(50, `Loading model (${mbLoaded} MB)...`);
          }
        },
        (error) => {
          reject(error);
        }
      );
    });
  }

  async launchMarkerMode() {
    this.currentMode = 'marker';
    this.ui.updateProgress(90, 'Starting camera tracking...');

    // Clean any prior surface session
    if (this.surfaceExperience) {
      this.surfaceExperience.dispose();
      this.surfaceExperience = null;
    }

    // Initialize Marker Experience
    this.markerExperience = new MarkerARExperience({
      container: document.getElementById('ar-container'),
      modelUrl: this.modelUrl,
      targetUrl: this.targetUrl,
      ui: this.ui,
      onTargetFound: () => this.ui.setTrackingState(true),
      onTargetLost: () => this.ui.setTrackingState(false)
    });

    await this.markerExperience.init(this.loadedGltf);
    await this.markerExperience.start();

    // Show HUD
    this.ui.hideLoading();
    this.ui.showARHud('marker');

    // Show "Place in your space" button if device supports WebXR
    this.ui.showSurfaceSwitchButton(this.isSurfaceSupported, 'marker');
  }

  async launchSurfaceMode() {
    this.currentMode = 'surface';
    this.ui.showLoading('Entering Surface AR...');

    // Clean up marker tracking and camera video stream
    if (this.markerExperience) {
      this.markerExperience.dispose();
      this.markerExperience = null;
    }

    // Initialize Surface Experience
    this.surfaceExperience = new SurfaceARExperience({
      container: document.getElementById('ar-container'),
      domOverlay: document.body,
      ui: this.ui,
      onPlaced: () => this.ui.setSurfacePlaced(true),
      onSessionEnded: () => {
        console.log('[WebAR] Surface AR session ended');
        // Return back to marker mode when XR session finishes
        this.launchMarkerMode();
      }
    });

    await this.surfaceExperience.init(this.loadedGltf);
    await this.surfaceExperience.start();

    this.ui.hideLoading();
    this.ui.showARHud('surface');
    this.ui.setSurfacePlaced(false);
    this.ui.showSurfaceSwitchButton(true, 'surface');
  }

  async toggleMode() {
    if (this.currentMode === 'marker') {
      if (this.isSurfaceSupported) {
        await this.launchSurfaceMode();
      }
    } else {
      await this.launchMarkerMode();
    }
  }

  exitToLanding() {
    if (this.markerExperience) {
      this.markerExperience.dispose();
      this.markerExperience = null;
    }
    if (this.surfaceExperience) {
      this.surfaceExperience.dispose();
      this.surfaceExperience = null;
    }

    this.ui.showLanding();
  }
}

// Instantiate application on DOM ready
window.addEventListener('DOMContentLoaded', () => {
  window.webarApp = new WebARApp();
});
