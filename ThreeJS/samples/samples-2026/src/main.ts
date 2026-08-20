import * as THREE from 'three';

import { OrbitControls } from "three/examples/jsm/Addons.js";
import { createCyberpunkAxes } from "./core/helper/AxesHelper";
import { Mesh } from "./core/mesh";
import { addResizeListener } from "./listener";

const renderer = new THREE.WebGLRenderer();
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setPixelRatio(Math.min(window.devicePixelRatio));
document.body.appendChild(renderer.domElement);

const scene = new THREE.Scene();
const geometry = new THREE.BoxGeometry(1, 1, 1);
const material = new THREE.MeshBasicMaterial({ color: 0x1ac5e0, wireframe: true });
const cube = new Mesh(geometry, material);
cube.position.y = 1;
cube.position.x = 1;
cube.rotation.x = THREE.MathUtils.degToRad(45);
cube.addAxesHelper(1);
scene.add(cube);

// const axesHelper = new THREE.AxesHelper(3);
// scene.add(axesHelper);

const cyberpunkAxes = createCyberpunkAxes(3, 0.15, 1.05);
scene.add(cyberpunkAxes.group);


const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
camera.position.z = 5;

addResizeListener(renderer, camera);

const cameraOrbitCtrl = new OrbitControls(camera, renderer.domElement);
cameraOrbitCtrl.enableDamping = true;
cameraOrbitCtrl.dampingFactor = 0.25;
cameraOrbitCtrl.screenSpacePanning = false;

const timer = new THREE.Timer();
function animate() {
  timer.update();
  const elapsedTime = timer.getElapsed();
  const delta = timer.getDelta();
  console.log(elapsedTime);


  // cube.rotation.x += 0.01;
  // cube.rotation.y += 0.01;

  // به‌روزرسانی شیدرها جهت فعال ماندن درخشش نئونی
  if (cyberpunkAxes && cyberpunkAxes.labels) {
    cyberpunkAxes.labels.forEach(label => {
      label.material.uniforms.time.value = elapsedTime;
    });
  }

  cube.rotateZ(Math.sin(delta) * 1);

  cameraOrbitCtrl.update();

  renderer.render(scene, camera);

  requestAnimationFrame(animate);
}
animate();
