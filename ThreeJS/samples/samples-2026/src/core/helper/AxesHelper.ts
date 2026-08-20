import * as THREE from 'three';

const glsl = v => v[0];

const vertexShader = glsl`
    varying vec2 vUv;
    void main() {
        vUv = uv;
        // محاسبه هوشمند Billboard: باعث می‌شود لبل همیشه رو به دوربین بچرخد
        vec4 mvPosition = modelViewMatrix * vec4(0.0, 0.0, 0.0, 1.0);
        mvPosition.xy += position.xy;
        gl_Position = projectionMatrix * mvPosition;
    }
`;

const fragmentShader = glsl`
    uniform float stringChar; // 0: X, 1: Y, 2: Z
    uniform vec3 color;
    uniform float time;
    varying vec2 vUv;

    float dfLine(vec2 p, vec2 a, vec2 b) {
        vec2 pa = p - a, ba = b - a;
        float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
        return length(pa - ba * h);
    }

    float getCharSDF(vec2 p, int charType) {
        float d = 1.0;
        if (charType == 0) { // X
            d = min(dfLine(p, vec2(-0.3, -0.3), vec2(0.3, 0.3)), 
                    dfLine(p, vec2(-0.3, 0.3), vec2(0.3, -0.3)));
        } else if (charType == 1) { // Y
            d = min(dfLine(p, vec2(-0.3, 0.3), vec2(0.0, 0.0)), 
                    dfLine(p, vec2(0.3, 0.3), vec2(0.0, 0.0)));
            d = min(d, dfLine(p, vec2(0.0, 0.0), vec2(0.0, -0.3)));
        } else if (charType == 2) { // Z
            d = min(dfLine(p, vec2(-0.3, 0.3), vec2(0.3, 0.3)), 
                    dfLine(p, vec2(0.3, 0.3), vec2(-0.3, -0.3)));
            d = min(d, dfLine(p, vec2(-0.3, -0.3), vec2(0.3, -0.3)));
        }
        return d;
    }

    void main() {
        vec2 p = vUv - 0.5;
        int charIdx = int(stringChar);
        float d = getCharSDF(p, charIdx);

        // ۱. مرز هسته مرکزی (Core) برای آنتی‌الیاسینگ تمیز
        float edge = fwidth(d);
        float core = smoothstep(0.025 + edge, 0.01, d); // هسته باریک سفید

        // ۲. محاسبه هاله رنگی نئون (Glow) با شدت بالا
        float glowFactor = 0.025 / (d + 0.002);
        glowFactor = pow(glowFactor, 1.2); // متراکم کردن رنگ حول خط
        glowFactor *= (0.85 + 0.15 * sin(time * 3.5)); // انیمیشن درخشش پویای ملایم

        // ۳. ترکیب لایه‌ای: هاله خالص رنگی + هسته درخشان
        vec3 neonGlow = color * glowFactor * 1.8; 
        vec3 whiteCore = vec3(1.0) * core;

        vec3 finalColor = neonGlow + whiteCore;
        float alpha = clamp(glowFactor + core, 0.0, 1.0);

        if (alpha < 0.03) discard;
        gl_FragColor = vec4(finalColor, alpha);
    }
`;

function createGradientAxisLine(endPoint, startColorHex, endColorHex) {
  const points = [
    new THREE.Vector3(0, 0, 0), // نقطه شروع (مبدأ)
    endPoint                    // نقطه پایان (انتهای محور)
  ];

  const geometry = new THREE.BufferGeometry().setFromPoints(points);

  // تعریف رنگ‌های متناظر با هر نقطه برای ایجاد Linear Gradient
  const startColor = new THREE.Color(startColorHex);
  const endColor = new THREE.Color(endColorHex);

  // آرایه رنگ‌ها: [R, G, B برای نقطه اول,  R, G, B برای نقطه دوم]
  const colors = new Float32Array([
    startColor.r, startColor.g, startColor.b,
    endColor.r, endColor.g, endColor.b
  ]);

  // نگاشت رنگ‌ها به هندسه خط
  geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

  // استفاده از همان LineBasicMaterial با فعال‌سازی vertexColors
  const mat = new THREE.LineBasicMaterial({
    vertexColors: true, // فعال‌سازی طیف رنگی خطی (Linear Gradient)

    transparent: false,
    // opacity: 0.95,

    // توجه: linewidth روی اکثر کارت‌های گرافیک مدرن به دلیل محدودیت WebGL روی 1 یا 2 ثابت است
    linewidth: 2
  });

  return new THREE.Line(geometry, mat);
}

function createGradientAxes(length = 4) {
  const group = new THREE.Group();

  // محور X: از قرمز تیره/کم‌رنگ در مرکز به قرمز نئونی در انتها
  const lineX = createGradientAxisLine(
    new THREE.Vector3(length, 0, 0),
    // رنگ مبدأ
    0xff0066,
    // رنگ انتهای خط
    0xe8cfef,
  );

  // محور Y: از سبز تیره به سبز نئونی
  const lineY = createGradientAxisLine(
    new THREE.Vector3(0, length, 0),
    0x00ff66,
    0xc7ffe9,
  );

  // محور Z: از آبی تیره به آبی نئونی
  const lineZ = createGradientAxisLine(
    new THREE.Vector3(0, 0, length),
    0x0099ff,
    0xc7e9ff,
  );

  group.add(lineX, lineY, lineZ);
  return group;
}

// تابع ساخت لبل‌های شیدری خلاقانه
function createCreativeLabel(charIndex, colorHex, position, labelSize = 0.4) {
  const material = new THREE.ShaderMaterial({
    vertexShader: vertexShader,
    fragmentShader: fragmentShader,
    uniforms: {
      stringChar: { value: charIndex },
      color: { value: new THREE.Color(colorHex) },
      time: { value: 0 }
    },
    transparent: true,
    depthTest: false,
    depthWrite: false,
    side: THREE.DoubleSide
  });

  // استفاده از پارامتر labelSize برای هندسه
  const geometry = new THREE.PlaneGeometry(labelSize, labelSize);
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.copy(position);
  mesh.renderOrder = 999;
  return mesh;
}

// ساخت محورها و برچسب‌های سایبرپانکی
export function createCyberpunkAxes(length = 4, labelSize = 0.4, offsetRatio = 1.1) {
  const group = new THREE.Group();

  const gradientAxes = createGradientAxes(length);
  group.add(gradientAxes);

  // محاسبه فاصله برچسب‌ها بر اساس طول محور
  const offset = length * offsetRatio;

  // ساخت برچسب‌ها با اندازه دلخواه
  const labelX = createCreativeLabel(0, 0xff0055, new THREE.Vector3(offset, 0, 0), labelSize);
  const labelY = createCreativeLabel(1, 0x00ff66, new THREE.Vector3(0, offset, 0), labelSize);
  const labelZ = createCreativeLabel(2, 0x0099ff, new THREE.Vector3(0, 0, offset), labelSize);

  group.add(labelX, labelY, labelZ);

  return { group, labels: [labelX, labelY, labelZ] };
}