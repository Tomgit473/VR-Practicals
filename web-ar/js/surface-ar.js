import * as THREE from 'three';

/**
 * Surface AR Experience using standard WebXR Hit-Test API
 * Supports surface detection, reticle indicator, tap-to-place,
 * single-finger drag-to-rotate, and two-finger pinch-to-scale.
 */
export class SurfaceARExperience {
  constructor(options = {}) {
    this.container = options.container || document.getElementById('ar-container');
    this.domOverlay = options.domOverlay || document.body;
    this.ui = options.ui;

    this.onPlaced = options.onPlaced || (() => {});
    this.onSessionEnded = options.onSessionEnded || (() => {});

    // WebXR / Three.js State
    this.session = null;
    this.renderer = null;
    this.scene = null;
    this.camera = null;
    this.refSpace = null;
    this.hitTestSource = null;
    this.hitTestSourceRequested = false;

    // 3D Objects
    this.reticle = null;
    this.modelInstance = null;
    this.gltfSource = null;
    this.mixer = null;
    this.clock = new THREE.Clock();

    // Interaction State
    this.isPlaced = false;
    this.initialScale = 1.0;
    this.currentScale = 1.0;
    this.currentRotationY = 0;

    // Touch tracking
    this.touchStartX = 0;
    this.touchStartY = 0;
    this.initialPinchDistance = 0;
    this.isDragging = false;
    this.isPinching = false;
  }

  static async isSupported() {
    if (!navigator.xr || !navigator.xr.isSessionSupported) {
      return false;
    }
    try {
      return await navigator.xr.isSessionSupported('immersive-ar');
    } catch (e) {
      console.warn('WebXR isSessionSupported error:', e);
      return false;
    }
  }

  async init(gltf) {
    this.gltfSource = gltf;

    // 1. Setup Three.js Scene and Camera
    this.scene = new THREE.Scene();
    this.camera = new THREE.PerspectiveCamera(70, window.innerWidth / window.innerHeight, 0.01, 20);

    // 2. Setup Renderer with WebXR enabled
    this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.xr.enabled = true;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 1.1;

    this.container.appendChild(this.renderer.domElement);

    // 3. Setup Lighting
    const hemiLight = new THREE.HemisphereLight(0xffffff, 0x444444, 1.4);
    hemiLight.position.set(0, 10, 0);
    this.scene.add(hemiLight);

    const dirLight = new THREE.DirectionalLight(0xffffff, 2.4);
    dirLight.position.set(3, 8, 4);
    this.scene.add(dirLight);

    // 4. Build Surface Reticle
    this.buildReticle();

    // 5. Build Model Container
    this.buildModelInstance();

    // 6. Bind Touch Gestures
    this.bindTouchGestures();
  }

  buildReticle() {
    this.reticle = new THREE.Group();
    this.reticle.matrixAutoUpdate = false;
    this.reticle.visible = false;

    // Outer glowing ring
    const ringGeo = new THREE.RingGeometry(0.12, 0.14, 32).rotateX(-Math.PI / 2);
    const ringMat = new THREE.MeshBasicMaterial({
      color: 0x00d2ff,
      side: THREE.DoubleSide,
      transparent: true,
      opacity: 0.85
    });
    const ringMesh = new THREE.Mesh(ringGeo, ringMat);
    this.reticle.add(ringMesh);

    // Center targeting pip
    const dotGeo = new THREE.CircleGeometry(0.02, 16).rotateX(-Math.PI / 2);
    const dotMat = new THREE.MeshBasicMaterial({
      color: 0xffffff,
      side: THREE.DoubleSide
    });
    const dotMesh = new THREE.Mesh(dotGeo, dotMat);
    this.reticle.add(dotMesh);

    this.scene.add(this.reticle);
  }

  buildModelInstance() {
    if (!this.gltfSource) return;

    const rawModel = this.gltfSource.scene.clone();

    // Measure bounding box
    const bbox = new THREE.Box3().setFromObject(rawModel);
    const size = new THREE.Vector3();
    const center = new THREE.Vector3();
    bbox.getSize(size);
    bbox.getCenter(center);

    // Center model at ground level
    rawModel.position.x = -center.x;
    rawModel.position.y = -bbox.min.y;
    rawModel.position.z = -center.z;

    // Standardize default real-world size in AR (e.g. roughly 0.6m in length)
    const maxDim = Math.max(size.x, size.z, 0.001);
    this.initialScale = 0.6 / maxDim;
    rawModel.scale.setScalar(this.initialScale);

    this.modelInstance = new THREE.Group();
    this.modelInstance.add(rawModel);
    this.modelInstance.visible = false;
    this.scene.add(this.modelInstance);

    // Setup animations
    if (this.gltfSource.animations && this.gltfSource.animations.length > 0) {
      this.mixer = new THREE.AnimationMixer(rawModel);
      this.gltfSource.animations.forEach((clip) => {
        this.mixer.clipAction(clip).play();
      });
    }
  }

  bindTouchGestures() {
    const el = window;

    el.addEventListener('touchstart', (e) => {
      if (!this.isPlaced) return;

      if (e.touches.length === 1) {
        // Single finger drag to rotate
        this.isDragging = true;
        this.isPinching = false;
        this.touchStartX = e.touches[0].clientX;
        this.touchStartY = e.touches[0].clientY;
      } else if (e.touches.length === 2) {
        // Two finger pinch to scale
        this.isPinching = true;
        this.isDragging = false;
        const dx = e.touches[0].clientX - e.touches[1].clientX;
        const dy = e.touches[0].clientY - e.touches[1].clientY;
        this.initialPinchDistance = Math.hypot(dx, dy);
      }
    }, { passive: true });

    el.addEventListener('touchmove', (e) => {
      if (!this.isPlaced || !this.modelInstance) return;

      if (this.isDragging && e.touches.length === 1) {
        const deltaX = e.touches[0].clientX - this.touchStartX;
        this.touchStartX = e.touches[0].clientX;

        // Rotate around Y axis
        this.currentRotationY += deltaX * 0.012;
        this.modelInstance.rotation.y = this.currentRotationY;
      } else if (this.isPinching && e.touches.length === 2) {
        const dx = e.touches[0].clientX - e.touches[1].clientX;
        const dy = e.touches[0].clientY - e.touches[1].clientY;
        const dist = Math.hypot(dx, dy);

        if (this.initialPinchDistance > 10) {
          const factor = dist / this.initialPinchDistance;
          this.initialPinchDistance = dist;

          // Clamp scale multiplier between 0.2x and 4.0x
          this.currentScale = Math.min(4.0, Math.max(0.2, this.currentScale * factor));
          this.modelInstance.scale.setScalar(this.currentScale);
        }
      }
    }, { passive: true });

    el.addEventListener('touchend', (e) => {
      if (e.touches.length === 0) {
        this.isDragging = false;
        this.isPinching = false;
      } else if (e.touches.length === 1) {
        this.isPinching = false;
        this.isDragging = true;
        this.touchStartX = e.touches[0].clientX;
      }
    }, { passive: true });
  }

  async start() {
    // Request WebXR session with hit-test and dom-overlay
    const sessionInit = {
      requiredFeatures: ['hit-test'],
      optionalFeatures: ['dom-overlay', 'light-estimation'],
      domOverlay: { root: this.domOverlay }
    };

    this.session = await navigator.xr.requestSession('immersive-ar', sessionInit);
    this.session.addEventListener('end', () => this.onSessionEnd());

    await this.renderer.xr.setSession(this.session);
    this.refSpace = await this.session.requestReferenceSpace('viewer');

    this.clock.start();

    // Render loop
    this.renderer.setAnimationLoop((timestamp, frame) => {
      this.onXRFrame(timestamp, frame);
    });
  }

  async requestHitTestSource() {
    const session = this.renderer.xr.getSession();
    const referenceSpace = await session.requestReferenceSpace('local');
    this.hitTestSource = await session.requestHitTestSource({ space: this.refSpace });
    this.refSpace = referenceSpace;
    this.hitTestSourceRequested = true;
  }

  onXRFrame(timestamp, frame) {
    if (!frame) return;

    const delta = this.clock.getDelta();
    if (this.mixer) this.mixer.update(delta);

    const session = this.renderer.xr.getSession();

    if (!this.hitTestSourceRequested) {
      this.requestHitTestSource();
    }

    if (this.hitTestSource) {
      const hitTestResults = frame.getHitTestResults(this.hitTestSource);
      if (hitTestResults.length > 0) {
        const hit = hitTestResults[0];
        const pose = hit.getPose(this.refSpace);

        // Position reticle on detected plane
        this.reticle.visible = true;
        this.reticle.matrix.fromArray(pose.transform.matrix);
      } else {
        this.reticle.visible = false;
      }
    }

    this.renderer.render(this.scene, this.camera);
  }

  placeOrReposition() {
    if (!this.reticle || !this.reticle.visible || !this.modelInstance) return false;

    // Apply reticle position and orientation to model
    this.modelInstance.position.setFromMatrixPosition(this.reticle.matrix);
    this.modelInstance.visible = true;
    this.isPlaced = true;

    this.onPlaced();
    return true;
  }

  onSessionEnd() {
    this.hitTestSourceRequested = false;
    this.hitTestSource = null;
    this.session = null;
    this.onSessionEnded();
  }

  async stop() {
    if (this.session) {
      try {
        await this.session.end();
      } catch (e) {
        console.warn('Session end caught:', e);
      }
    }
  }

  dispose() {
    this.stop();
    if (this.renderer) {
      this.renderer.setAnimationLoop(null);
      this.renderer.dispose();
      const dom = this.renderer.domElement;
      if (dom && dom.parentNode) dom.parentNode.removeChild(dom);
    }
    if (this.mixer) {
      this.mixer.stopAllAction();
      this.mixer = null;
    }
  }
}
