import type { PerspectiveCamera, WebGLRenderer } from "three";

export function addResizeListener(renderer: WebGLRenderer, camera: PerspectiveCamera) {
  window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();

    renderer.setSize(window.innerWidth, window.innerHeight);
  });
}
