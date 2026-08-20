import * as THREE from 'three';
import { createCyberpunkAxes } from "./helper/AxesHelper";


export class Mesh<
  TGeometry extends THREE.BufferGeometry = THREE.BufferGeometry,
  TMaterial extends THREE.Material | THREE.Material[] = THREE.Material | THREE.Material[],
  TEventMap extends THREE.Object3DEventMap = THREE.Object3DEventMap,
> extends THREE.Mesh<TGeometry, TMaterial, TEventMap> {

  addAxesHelper(lineSize: number, labelSize = 0.1) {
    // const axesHelper = new THREE.AxesHelper(size);
    // this.add(axesHelper);

    const customAxes = createCyberpunkAxes(lineSize, labelSize);
    this.add(customAxes.group);
  }
}