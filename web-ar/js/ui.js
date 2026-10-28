/**
 * UI Controller for WebAR Experience
 * Manages screen transitions, loading progress, HUD states, and user feedback
 */

export class UIController {
  constructor() {
    // Screen Elements
    this.landingScreen = document.getElementById('landing-screen');
    this.loadingOverlay = document.getElementById('loading-overlay');
    this.arHud = document.getElementById('ar-hud');
    this.errorModal = document.getElementById('error-modal');

    // Controls & Status
    this.startBtn = document.getElementById('btn-start-ar');
    this.switchModeBtn = document.getElementById('btn-switch-mode');
    this.exitBtn = document.getElementById('btn-exit-ar');
    this.placeBtn = document.getElementById('btn-place-model');
    this.surfaceControls = document.getElementById('surface-placement-controls');
    this.trackingGuide = document.getElementById('tracking-guide');

    // Labels & Feedback
    this.loadingStatus = document.getElementById('loading-status');
    this.progressFill = document.getElementById('progress-fill');
    this.progressPct = document.getElementById('progress-pct');
    this.modeLabel = document.getElementById('mode-label');
    this.statusDot = document.getElementById('status-dot');
    this.gestureHint = document.getElementById('gesture-hint');

    // Error Elements
    this.errorTitle = document.getElementById('error-title');
    this.errorMessage = document.getElementById('error-message');
    this.errorTip = document.getElementById('error-tip');
    this.errorActionBtn = document.getElementById('btn-error-action');

    this.bindEvents();
  }

  bindEvents() {
    this.errorActionBtn?.addEventListener('click', () => {
      this.hideError();
      this.showLanding();
    });
  }

  showLanding() {
    this.landingScreen.classList.remove('hidden');
    this.loadingOverlay.classList.add('hidden');
    this.arHud.classList.add('hidden');
    this.errorModal.classList.add('hidden');
  }

  showLoading(statusText = 'Preparing AR Scene...') {
    this.landingScreen.classList.add('hidden');
    this.loadingOverlay.classList.remove('hidden');
    this.arHud.classList.add('hidden');
    this.updateProgress(0, statusText);
  }

  updateProgress(percent, statusText = '') {
    const clamped = Math.min(100, Math.max(0, Math.round(percent)));
    if (this.progressFill) this.progressFill.style.width = `${clamped}%`;
    if (this.progressPct) this.progressPct.textContent = `${clamped}%`;
    if (statusText && this.loadingStatus) this.loadingStatus.textContent = statusText;
  }

  hideLoading() {
    this.loadingOverlay.classList.add('hidden');
  }

  showARHud(mode = 'marker') {
    this.landingScreen.classList.add('hidden');
    this.loadingOverlay.classList.add('hidden');
    this.arHud.classList.remove('hidden');
    this.setMode(mode);
  }

  setMode(mode) {
    if (mode === 'marker') {
      this.modeLabel.textContent = 'Poster AR';
      this.trackingGuide.classList.remove('hidden');
      if (this.surfaceControls) this.surfaceControls.classList.add('hidden');
      if (this.gestureHint) this.gestureHint.classList.add('hidden');
    } else if (mode === 'surface') {
      this.modeLabel.textContent = 'Surface AR';
      this.trackingGuide.classList.add('hidden');
      if (this.surfaceControls) this.surfaceControls.classList.remove('hidden');
      if (this.gestureHint) this.gestureHint.classList.remove('hidden');
    }
  }

  setTrackingState(isTracking) {
    if (isTracking) {
      this.statusDot.classList.add('active');
      this.trackingGuide.classList.add('hidden');
    } else {
      this.statusDot.classList.remove('active');
      this.trackingGuide.classList.remove('hidden');
    }
  }

  setSurfacePlaced(isPlaced) {
    if (isPlaced) {
      if (this.placeBtn) this.placeBtn.textContent = 'Re-position Model';
      if (this.gestureHint) {
        this.gestureHint.classList.remove('hidden');
        this.gestureHint.textContent = '1-finger drag to rotate • 2-finger pinch to scale';
      }
    } else {
      if (this.placeBtn) this.placeBtn.textContent = 'Tap to Place Model';
    }
  }

  showSurfaceSwitchButton(show = true, currentMode = 'marker') {
    if (!this.switchModeBtn) return;
    if (show) {
      this.switchModeBtn.classList.remove('hidden');
      const textSpan = this.switchModeBtn.querySelector('.switch-text');
      if (textSpan) {
        textSpan.textContent = currentMode === 'marker' ? 'Place in your space' : 'Switch to Marker AR';
      }
    } else {
      this.switchModeBtn.classList.add('hidden');
    }
  }

  showError(type, customDetails = '') {
    this.hideLoading();
    this.arHud.classList.add('hidden');
    this.errorModal.classList.remove('hidden');

    switch (type) {
      case 'CAMERA_PERMISSION_DENIED':
        this.errorTitle.textContent = 'Camera Access Blocked';
        this.errorMessage.textContent = 'Augmented Reality requires camera access to track the poster marker and display 3D models.';
        this.errorTip.innerHTML = `
          <strong>How to fix on Android Chrome:</strong><br>
          1. Tap the <strong>tune/lock icon</strong> on the left of the address bar.<br>
          2. Tap <strong>Permissions</strong> &rarr; <strong>Camera</strong>.<br>
          3. Change to <strong>Allow</strong> and reload the page.
        `;
        break;

      case 'INSECURE_CONTEXT':
        this.errorTitle.textContent = 'HTTPS Required';
        this.errorMessage.textContent = 'Mobile browsers block camera and WebXR access on non-secure origins.';
        this.errorTip.innerHTML = `
          Please serve this application over <strong>HTTPS</strong> or via <strong>localhost</strong> / trusted tunnel (e.g. ngrok or Cloudflare tunnel).
        `;
        break;

      case 'UNSUPPORTED_BROWSER':
        this.errorTitle.textContent = 'Browser Unsupported';
        this.errorMessage.textContent = 'Your current browser does not support the required WebGL or WebXR features.';
        this.errorTip.innerHTML = `
          Please open this link in the latest version of <strong>Google Chrome</strong> on Android.
        `;
        break;

      case 'ASSET_LOAD_FAILED':
        this.errorTitle.textContent = 'Asset Download Failed';
        this.errorMessage.textContent = 'Could not download the 3D model or target marker files.';
        this.errorTip.innerHTML = `
          ${customDetails || 'Please check your internet connection or verify that model.glb and targets.mind are uploaded correctly.'}
        `;
        break;

      default:
        this.errorTitle.textContent = 'Experience Error';
        this.errorMessage.textContent = customDetails || 'An unexpected error occurred while starting Augmented Reality.';
        this.errorTip.innerHTML = 'Please refresh the page and try again.';
        break;
    }
  }

  hideError() {
    this.errorModal.classList.add('hidden');
  }
}
