import * as THREE from 'three';
import { MindARThree } from 'mindar-image-three';

// Compatibility shim: MindAR 1.2.5 sets .outputEncoding which Three.js 0.160.0 replaced with .outputColorSpace
if (!Object.getOwnPropertyDescriptor(THREE.WebGLRenderer.prototype, 'outputEncoding')) {
  Object.defineProperty(THREE.WebGLRenderer.prototype, 'outputEncoding', {
    get() {
      return this.outputColorSpace === THREE.SRGBColorSpace ? 3001 : 3000;
    },
    set(val) {
      this.outputColorSpace = (val === 3001 || val === 3000) ? THREE.SRGBColorSpace : THREE.LinearSRGBColorSpace;
    },
    configurable: true
  });
}

/**
 * Marker-based AR Experience using MindAR.js + Three.js
 */
export class MarkerARExperience {
  constructor(options = {}) {
    this.container = options.container || document.getElementById('ar-container');
    this.modelUrl = options.modelUrl || './assets/model.glb';
    this.targetUrl = options.targetUrl || './assets/targets.mind';
    this.ui = options.ui;

    // Callbacks
    this.onTargetFound = options.onTargetFound || (() => {});
    this.onTargetLost = options.onTargetLost || (() => {});

    // State
    this.mindarThree = null;
    this.scene = null;
    this.camera = null;
    this.renderer = null;
    this.anchor = null;
    this.modelGroup = null;
    this.podiumGroup = null;
    this.mixer = null;
    this.clock = new THREE.Clock();
    this.isRunning = false;

    // Fading control
    this.isTracking = false;
    this.opacity = 1.0;
    this.targetOpacity = 0.0;
    this.fadeDuration = 0.5;
    this.materialsList = [];
  }

  async init(preloadedGltf = null) {
    console.log('[MarkerAR] Initializing MindAR engine with target:', this.targetUrl);

    // Initialize MindARThree with sensible, high-performance defaults
    // warmupTolerance: 0 triggers immediate detection on the very first recognized frame!
    this.mindarThree = new MindARThree({
      container: this.container,
      imageTargetSrc: this.targetUrl,
      warmupTolerance: 0,
      missTolerance: 5
    });

    const { renderer, scene, camera } = this.mindarThree;
    this.renderer = renderer;
    this.scene = scene;
    this.camera = camera;

    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 1.2;

    // Setup Lighting
    const hemiLight = new THREE.HemisphereLight(0xffffff, 0x334455, 1.5);
    hemiLight.position.set(0, 10, 10);
    this.scene.add(hemiLight);

    const dirLight = new THREE.DirectionalLight(0xffffff, 2.5);
    dirLight.position.set(3, 5, 8);
    this.scene.add(dirLight);

    const fillLight = new THREE.DirectionalLight(0x00d2ff, 0.8);
    fillLight.position.set(-3, -3, 4);
    this.scene.add(fillLight);

    // Setup Anchor Group
    this.anchor = this.mindarThree.addAnchor(0);

    // Build immediate visual anchor podium (guarantees visual feedback on marker scan)
    this.buildAnchorPodium();

    // Container for 3D model
    this.modelGroup = new THREE.Group();
    this.anchor.group.add(this.modelGroup);

    if (preloadedGltf) {
      this.setupModel(preloadedGltf);
    }

    // Anchor Tracking Events
    this.anchor.onTargetFound = () => {
      console.log('>>> [WebAR] MARKER DETECTED & LOCKED! <<<');
      this.isTracking = true;
      this.targetOpacity = 1.0;
      if (this.podiumGroup) this.podiumGroup.visible = true;
      if (this.modelGroup) this.modelGroup.visible = true;

      // Haptic confirmation
      if (navigator.vibrate) {
        navigator.vibrate(60);
      }

      this.onTargetFound();
    };

    this.anchor.onTargetLost = () => {
      console.log('>>> [WebAR] Marker lost <<<');
      this.isTracking = false;
      this.targetOpacity = 0.0;
      this.onTargetLost();
    };

    this.resizeHandler = () => this.handleResize();
    window.addEventListener('resize', this.resizeHandler);
  }

  buildAnchorPodium() {
    this.podiumGroup = new THREE.Group();

    // Marker dimensions: MindAR marker width is 1.0 in XY plane.
    // marker.png is 784 x 1168 (ratio 1.489)
    const markerW = 1.0;
    const markerH = 1.489;

    // Glowing border outline
    const borderGeo = new THREE.BufferGeometry();
    const halfW = markerW / 2;
    const halfH = markerH / 2;
    const vertices = new Float32Array([
      -halfW, -halfH, 0.005,
       halfW, -halfH, 0.005,
       halfW,  halfH, 0.005,
      -halfW,  halfH, 0.005,
      -halfW, -halfH, 0.005
    ]);
    borderGeo.setAttribute('position', new THREE.BufferAttribute(vertices, 3));
    const borderMat = new THREE.LineBasicMaterial({
      color: 0x00d2ff,
      linewidth: 3,
      transparent: true,
      opacity: 0.9
    });
    const borderLine = new THREE.Line(borderGeo, borderMat);
    this.podiumGroup.add(borderLine);

    // Glowing corner brackets
    const bracketSize = 0.15;
    const cornerMat = new THREE.LineBasicMaterial({ color: 0xff3344, linewidth: 4 });

    const createCorner = (x, y, sx, sy) => {
      const geo = new THREE.BufferGeometry();
      const pts = new Float32Array([
        x, y + sy * bracketSize, 0.01,
        x, y, 0.01,
        x + sx * bracketSize, y, 0.01
      ]);
      geo.setAttribute('position', new THREE.BufferAttribute(pts, 3));
      return new THREE.Line(geo, cornerMat);
    };

    this.podiumGroup.add(createCorner(-halfW, -halfH, 1, 1));
    this.podiumGroup.add(createCorner(halfW, -halfH, -1, 1));
    this.podiumGroup.add(createCorner(halfW, halfH, -1, -1));
    this.podiumGroup.add(createCorner(-halfW, halfH, 1, -1));

    // Semi-transparent tech base
    const baseGeo = new THREE.PlaneGeometry(markerW * 0.95, markerH * 0.95);
    const baseMat = new THREE.MeshBasicMaterial({
      color: 0x00d2ff,
      transparent: true,
      opacity: 0.12,
      side: THREE.DoubleSide
    });
    const baseMesh = new THREE.Mesh(baseGeo, baseMat);
    baseMesh.position.z = 0.002;
    this.podiumGroup.add(baseMesh);

    this.anchor.group.add(this.podiumGroup);
  }

  setupModel(gltf) {
    if (!gltf) return;
    while (this.modelGroup && this.modelGroup.children.length > 0) {
      this.modelGroup.remove(this.modelGroup.children[0]);
    }
    const rawModel = gltf.scene || gltf.scenes[0];

    // Compute bounding box
    const bbox = new THREE.Box3().setFromObject(rawModel);
    const size = new THREE.Vector3();
    const center = new THREE.Vector3();
    bbox.getSize(size);
    bbox.getCenter(center);

    // MindAR marker is in the XY plane, with normal pointing along +Z!
    // A standard car model has Y as UP and Z/X as horizontal.
    // To place the car wheels down on the marker and roof facing the user,
    // we rotate 90 degrees around X (+Math.PI / 2).
    const carContainer = new THREE.Group();
    carContainer.rotation.x = Math.PI / 2;

    // Center model inside carContainer
    rawModel.position.x = -center.x;
    rawModel.position.y = -bbox.min.y; // Sit wheels right on the base
    rawModel.position.z = -center.z;

    // Fit model within 0.85 of marker width
    const maxHorizontal = Math.max(size.x, size.z, 0.001);
    const targetScale = 0.85 / maxHorizontal;
    carContainer.scale.setScalar(targetScale);

    carContainer.add(rawModel);

    // Collect materials
    this.materialsList = [];
    rawModel.traverse((child) => {
      if (child.isMesh && child.material) {
        const mats = Array.isArray(child.material) ? child.material : [child.material];
        mats.forEach((m) => {
          m.transparent = true;
          m.opacity = 1.0; // Fully visible on load
          this.materialsList.push(m);
        });
      }
    });

    if (gltf.animations && gltf.animations.length > 0) {
      this.mixer = new THREE.AnimationMixer(rawModel);
      gltf.animations.forEach((clip) => {
        this.mixer.clipAction(clip).play();
      });
    }

    this.modelGroup.add(carContainer);
  }

  async start() {
    if (!this.mindarThree) return;
    this.isRunning = true;
    this.clock.start();

    console.log('[MarkerAR] Starting MindAR tracking stream...');
    await this.mindarThree.start();
    console.log('[MarkerAR] MindAR stream active');

    this.renderer.setAnimationLoop(() => {
      if (!this.isRunning) return;

      const delta = this.clock.getDelta();

      if (this.mixer) this.mixer.update(delta);

      // Smooth opacity fading on tracking transitions
      if (this.opacity !== this.targetOpacity) {
        const step = delta / this.fadeDuration;
        if (this.opacity < this.targetOpacity) {
          this.opacity = Math.min(this.targetOpacity, this.opacity + step);
        } else {
          this.opacity = Math.max(this.targetOpacity, this.opacity - step);
        }

        for (let i = 0; i < this.materialsList.length; i++) {
          this.materialsList[i].opacity = this.opacity;
        }

        if (this.podiumGroup) {
          this.podiumGroup.visible = this.opacity > 0.05;
        }
      }

      this.renderer.render(this.scene, this.camera);
    });
  }

  handleResize() {
    if (!this.renderer || !this.camera) return;
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    if (this.camera.isPerspectiveCamera) {
      this.camera.aspect = window.innerWidth / window.innerHeight;
      this.camera.updateProjectionMatrix();
    }
  }

  stop() {
    this.isRunning = false;
    if (this.renderer) this.renderer.setAnimationLoop(null);
    if (this.mindarThree) {
      try {
        this.mindarThree.stop();
      } catch (e) {
        console.warn('MindAR stop caught:', e);
      }
    }
  }

  dispose() {
    this.stop();
    window.removeEventListener('resize', this.resizeHandler);

    if (this.mixer) {
      this.mixer.stopAllAction();
      this.mixer = null;
    }

    if (this.modelGroup) {
      this.modelGroup.traverse((child) => {
        if (child.isMesh) {
          if (child.geometry) child.geometry.dispose();
          if (child.material) {
            const mats = Array.isArray(child.material) ? child.material : [child.material];
            mats.forEach((m) => m.dispose());
          }
        }
      });
      if (this.anchor && this.anchor.group) {
        this.anchor.group.remove(this.modelGroup);
      }
    }

    if (this.renderer) {
      this.renderer.dispose();
      const dom = this.renderer.domElement;
      if (dom && dom.parentNode) dom.parentNode.removeChild(dom);
    }

    const video = this.container.querySelector('video');
    if (video) {
      if (video.srcObject) {
        video.srcObject.getTracks().forEach((track) => track.stop());
      }
      video.remove();
    }
  }
}
