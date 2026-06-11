(function () {
  'use strict';

  var STRIPE_WIDTH = { min: 64, target: 110, max: 120 };
  var GRADIENT = { bandWidth: 0.48, minStop: 0.14, maxStop: 0.96 };
  var INTRO = { delay: 700, revealDuration: 840, idleBlendDuration: 500, stagger: 45, startCenter: 0.96, idleCenter: 0.5 };
  var WAVE_SPEED_UP = { multiplier: 2.15, rampUpDuration: 140, waveDuration: 320, rampDownDuration: 480 };
  var IDLE_WAVE = { speed: 0.42, stripePhase: 0.74, secondarySpeed: 0.26, secondaryStripePhase: 1.28, primaryAmplitude: 0.19, secondaryAmplitude: 0.055 };

  function clamp(v, min, max) { return Math.max(min, Math.min(max, v)); }
  function lerp(a, b, t) { return a + (b - a) * t; }
  function easeOutCubic(t) { return 1 - Math.pow(1 - t, 3); }
  function easeInOutCubic(t) { return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2; }
  function getRandomWavePhase() { return Math.random() * Math.PI * 2; }

  function readCssNumber(el, name, fallback) {
    var val = getComputedStyle(el).getPropertyValue(name).trim();
    return val ? parseFloat(val) : fallback;
  }

  function readCssString(el, name, fallback) {
    var val = getComputedStyle(el).getPropertyValue(name).trim();
    return val || fallback;
  }

  function createStripe(canvas, i, count, totalWidth, stripeWidth, center, bandWidth) {
    var half = totalWidth / 2;
    var pos = (i / count) * totalWidth;
    var dist = Math.abs(pos - center * totalWidth) / half;
    var band = bandWidth / 2;
    var opacity = clamp(1 - (dist - band) / (1 - band), 0, 1);
    return { pos: pos, opacity: opacity };
  }

  function getStripeCount(canvas, stripeWidth) {
    return Math.ceil(canvas.width / stripeWidth) + 2;
  }

  function getRevealDelay(i, count, stagger, centerFraction) {
    var center = centerFraction * count;
    var dist = Math.abs(i - center);
    return (dist / count) * stagger;
  }

  function getMaxRevealDelay(count, stagger) {
    return stagger;
  }

  function getStaggeredProgress(i, count, elapsed, revealDuration, stagger) {
    var delay = getRevealDelay(i, count, stagger, 0.5);
    var local = elapsed - delay;
    if (local <= 0) return 0;
    var p = local / revealDuration;
    return easeOutCubic(clamp(p, 0, 1));
  }

  function getIntroRevealProgress(elapsed, count, stagger, revealDuration) {
    var maxDelay = getMaxRevealDelay(count, stagger);
    var total = revealDuration + maxDelay;
    return clamp(elapsed / total, 0, 1);
  }

  function getIntroIdleProgress(elapsed, idleBlendDuration) {
    return clamp(elapsed / idleBlendDuration, 0, 1);
  }

  function isIntroComplete(elapsed) {
    return elapsed >= INTRO.delay + INTRO.revealDuration + INTRO.idleBlendDuration + getMaxRevealDelay(getStripeCount(null, STRIPE_WIDTH.target * 2), INTRO.stagger);
  }

  function getIdleCenter(idleProgress) {
    return INTRO.idleCenter;
  }

  function createGrainPattern(width, height, luminance, contrast, saturation) {
    var offscreen = document.createElement('canvas');
    offscreen.width = width;
    offscreen.height = height;
    var ctx = offscreen.getContext('2d');
    var imageData = ctx.createImageData(width, height);
    var data = imageData.data;
    for (var i = 0; i < data.length; i += 4) {
      var noise = (Math.random() - 0.5) * contrast;
      var lum = clamp(luminance + noise, 0, 255);
      var sat = saturation;
      data[i] = clamp(lum + sat, 0, 255);
      data[i + 1] = clamp(lum, 0, 255);
      data[i + 2] = clamp(lum - sat * 0.5, 0, 255);
      data[i + 3] = 255;
    }
    ctx.putImageData(imageData, 0, 0);
    return ctx.createPattern(offscreen, 'repeat');
  }

  function initShimmer(canvas) {
    var ctx = canvas.getContext('2d', { colorSpace: 'display-p3' });
    if (!ctx) ctx = canvas.getContext('2d');
    var width = canvas.width = window.innerWidth * devicePixelRatio;
    var height = canvas.height = window.innerHeight * devicePixelRatio;
    var startColor = readCssString(canvas, '--shimmer-start', '#181825');
    var highlightColor = readCssString(canvas, '--shimmer-highlight', '#313244');
    var alpha = readCssNumber(canvas, '--shimmer-alpha', 0.7);
    var grainAlpha = readCssNumber(canvas, '--shimmer-grain-alpha', 0.15);
    var grainLuminance = readCssNumber(canvas, '--shimmer-grain-luminance', 144);
    var grainContrast = readCssNumber(canvas, '--shimmer-grain-contrast', 64);
    var grainSaturation = readCssNumber(canvas, '--shimmer-grain-saturation', 32);
    var introAlpha = readCssNumber(canvas, '--shimmer-intro-alpha', 1);
    var speedBoost = readCssNumber(canvas, '--shimmer-speed-up-shine-boost', 0.15);

    var grainPattern = createGrainPattern(180, 180, grainLuminance, grainContrast, grainSaturation);
    var stripeWidth = STRIPE_WIDTH.target * devicePixelRatio;
    var count = getStripeCount(canvas, stripeWidth);
    var elapsed = 0;
    var introElapsed = 0;
    var introDone = false;
    var speedUpActive = false;
    var speedUpElapsed = 0;
    var phases = [];
    for (var i = 0; i < count; i++) phases.push(getRandomWavePhase());

    function parseColor(hex) {
      var r = parseInt(hex.slice(1, 3), 16);
      var g = parseInt(hex.slice(3, 5), 16);
      var b = parseInt(hex.slice(5, 7), 16);
      return { r: r, g: g, b: b };
    }

    function colorToString(c, a) {
      return 'rgba(' + Math.round(c.r) + ',' + Math.round(c.g) + ',' + Math.round(c.b) + ',' + a + ')';
    }

    var start = parseColor(startColor);
    var highlight = parseColor(highlightColor);

    function draw(timestamp) {
      var dt = 16;
      elapsed += dt;
      if (!introDone) {
        introElapsed += dt;
        if (isIntroComplete(introElapsed)) introDone = true;
      }
      if (speedUpActive) {
        speedUpElapsed += dt;
        if (speedUpElapsed >= WAVE_SPEED_UP.rampUpDuration + WAVE_SPEED_UP.waveDuration + WAVE_SPEED_UP.rampDownDuration) {
          speedUpActive = false;
          speedUpElapsed = 0;
        }
      }

      ctx.clearRect(0, 0, width, height);

      var hue = startColor;
      var speedMult = 1;
      if (speedUpActive) {
        var su = speedUpElapsed;
        if (su < WAVE_SPEED_UP.rampUpDuration) speedMult = lerp(1, WAVE_SPEED_UP.multiplier, easeInOutCubic(su / WAVE_SPEED_UP.rampUpDuration));
        else if (su < WAVE_SPEED_UP.rampUpDuration + WAVE_SPEED_UP.waveDuration) speedMult = WAVE_SPEED_UP.multiplier;
        else speedMult = lerp(WAVE_SPEED_UP.multiplier, 1, easeInOutCubic((su - WAVE_SPEED_UP.rampUpDuration - WAVE_SPEED_UP.waveDuration) / WAVE_SPEED_UP.rampDownDuration));
      }

      var idleProgress = 1;
      if (!introDone) {
        idleProgress = getIntroIdleProgress(Math.max(0, introElapsed - INTRO.delay - INTRO.revealDuration), INTRO.idleBlendDuration);
      }

      var center = getIdleCenter(idleProgress);
      var bandAlpha = alpha;
      if (!introDone && introElapsed < INTRO.revealDuration + INTRO.delay) {
        bandAlpha *= introAlpha;
      }

      var revealProgress = introDone ? 1 : getIntroRevealProgress(Math.max(0, introElapsed - INTRO.delay), count, INTRO.stagger, INTRO.revealDuration);

      for (var i = 0; i < count; i++) {
        var stripe = createStripe(canvas, i, count, width, stripeWidth, center, GRADIENT.bandWidth);
        var sp = introDone ? 1 : getStaggeredProgress(i, count, Math.max(0, introElapsed - INTRO.delay), INTRO.revealDuration, INTRO.stagger);
        var finalOpacity = stripe.opacity * sp * bandAlpha;
        if (finalOpacity <= 0) continue;

        var waveTime = elapsed * IDLE_WAVE.speed * speedMult / 1000;
        var primary = Math.sin(waveTime + i * IDLE_WAVE.stripePhase + phases[i]) * IDLE_WAVE.primaryAmplitude;
        var secondary = Math.sin(waveTime * IDLE_WAVE.secondarySpeed + i * IDLE_WAVE.secondaryStripePhase + phases[i] * 0.5) * IDLE_WAVE.secondaryAmplitude;

        if (speedUpActive) {
          var suProgress = speedUpElapsed / (WAVE_SPEED_UP.rampUpDuration + WAVE_SPEED_UP.waveDuration + WAVE_SPEED_UP.rampDownDuration);
          var boost = Math.sin(suProgress * Math.PI) * speedBoost;
          finalOpacity *= (1 + boost);
        }

        var wave = primary + secondary;
        var x = stripe.pos + wave * stripeWidth;

        var grad = ctx.createLinearGradient(x - stripeWidth * GRADIENT.minStop, 0, x + stripeWidth * GRADIENT.maxStop, 0);
        grad.addColorStop(0, colorToString(start, finalOpacity * 0));
        grad.addColorStop(0.3, colorToString(start, finalOpacity));
        grad.addColorStop(0.5, colorToString(highlight, finalOpacity * 1.2));
        grad.addColorStop(0.7, colorToString(start, finalOpacity));
        grad.addColorStop(1, colorToString(start, finalOpacity * 0));
        ctx.fillStyle = grad;
        ctx.fillRect(x - stripeWidth * GRADIENT.minStop, 0, stripeWidth * (GRADIENT.minStop + GRADIENT.maxStop), height);
      }

      if (grainPattern) {
        ctx.globalAlpha = grainAlpha;
        ctx.fillStyle = grainPattern;
        ctx.fillRect(0, 0, width, height);
        ctx.globalAlpha = 1;
      }

      requestAnimationFrame(draw);
    }

    function onResize() {
      width = canvas.width = window.innerWidth * devicePixelRatio;
      height = canvas.height = window.innerHeight * devicePixelRatio;
      stripeWidth = STRIPE_WIDTH.target * devicePixelRatio;
      count = getStripeCount(canvas, stripeWidth);
    }

    window.addEventListener('resize', onResize);
    requestAnimationFrame(draw);

    return {
      intro: function () {
        introElapsed = 0;
        introDone = false;
        elapsed = 0;
      },
      emphasize: function () {
        speedUpActive = true;
        speedUpElapsed = 0;
      },
      destroy: function () {
        window.removeEventListener('resize', onResize);
      }
    };
  }

  window.__initShimmer = initShimmer;
  window.__GradientShimmer = { init: initShimmer };
})();
