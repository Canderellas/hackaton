<!-- QrCameraScanner.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import jsQR from 'jsqr'
import QrResultModal from './QrResultModal.vue'

const video = ref(null)
const canvas = ref(null)
const stream = ref(null)
const errorMessage = ref('')
const showResultModal = ref(false) // ✅ Начинаем с false
const lastScannedData = ref('')
const lastFrameImage = ref('')
const cameraReady = ref(false)

const startCamera = async () => {
  try {
    stream.value = await navigator.mediaDevices.getUserMedia({
      video: { 
        facingMode: 'environment',
        width: { ideal: 1280 },
        height: { ideal: 720 }
      }
    })
    
    if (video.value) {
      video.value.srcObject = stream.value
      await video.value.play()
      cameraReady.value = true
      errorMessage.value = ''
    }
  } catch (err) {
    errorMessage.value = 'Нет доступа к камере. Разрешите доступ к камере в настройках браузера.'
    console.error('Camera error:', err)
  }
}

const scanNow = () => {
  if (!cameraReady.value) {
    errorMessage.value = 'Камера ещё не готова'
    return
  }

  if (!video.value || video.value.readyState !== video.value.HAVE_ENOUGH_DATA) {
    errorMessage.value = 'Видео не готово'
    return
  }

  const ctx = canvas.value.getContext('2d')
  canvas.value.width = video.value.videoWidth
  canvas.value.height = video.value.videoHeight
  ctx.drawImage(video.value, 0, 0, canvas.value.width, canvas.value.height)

  const imageData = ctx.getImageData(0, 0, canvas.value.width, canvas.value.height)
  const code = jsQR(imageData.data, imageData.width, imageData.height, {
    inversionAttempts: 'dontInvert'
  })

  if (code) {
    drawGreenBorder(ctx, code.location)
    lastFrameImage.value = canvas.value.toDataURL('image/png')
    lastScannedData.value = code.data
    showResultModal.value = true
    errorMessage.value = ''
  } else {
    errorMessage.value = 'QR-код не найден. Попробуйте ещё раз.'
  }
}

const drawGreenBorder = (ctx, location) => {
  ctx.strokeStyle = '#00ff00'
  ctx.lineWidth = 8
  ctx.beginPath()
  ctx.moveTo(location.topLeftCorner.x, location.topLeftCorner.y)
  ctx.lineTo(location.topRightCorner.x, location.topRightCorner.y)
  ctx.lineTo(location.bottomRightCorner.x, location.bottomRightCorner.y)
  ctx.lineTo(location.bottomLeftCorner.x, location.bottomLeftCorner.y)
  ctx.closePath()
  ctx.stroke()
}

const closeModal = () => {
  showResultModal.value = false
  lastScannedData.value = ''
  lastFrameImage.value = ''
}

onMounted(() => {
  startCamera()
})

onUnmounted(() => {
  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())
  }
})
</script>

<template>
  <div class="app">
    <!-- Камера -->
    <div class="camera-wrapper">
      <video 
        ref="video" 
        playsinline 
        muted 
        class="camera-video"
        @loadeddata="cameraReady = true"
      ></video>
      <canvas ref="canvas" style="display: none;"></canvas>
      
      <div v-if="!stream" class="placeholder">
        <div class="loading-spinner"></div>
        <p>Загрузка камеры...</p>
      </div>
      
      <div v-else-if="!cameraReady" class="placeholder">
        <div class="loading-spinner"></div>
        <p>Инициализация камеры...</p>
      </div>
    </div>

    <!-- Сообщение об ошибке -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <!-- Кнопка сканирования -->
    <button 
      @click="scanNow" 
      class="scan-button"
      :disabled="!cameraReady"
    >
      <span v-if="!cameraReady">⏳ Подготовка...</span>
      <span v-else>📷 Сканировать QR-код</span>
    </button>

    <!-- Модалка с результатом -->
    <QrResultModal
      v-if="showResultModal"
      :scanned-data="lastScannedData"
      :frame-image="lastFrameImage"
      @close="closeModal"
    />
  </div>
</template>

<style scoped>
.app {
  position: fixed;
  inset: 0;
  background: #000;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 32px;
  padding: 20px;
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.camera-wrapper {
  position: relative;
  width: 90vw;
  max-width: 420px;
  aspect-ratio: 1 / 1;
  border-radius: 32px;
  overflow: hidden;
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
  background: #000;
}

.camera-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.placeholder {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.85);
  color: white;
  font-size: 18px;
  text-align: center;
  padding: 20px;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(255, 255, 255, 0.3);
  border-top: 4px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  background: #ff3b30;
  color: white;
  padding: 16px 32px;
  border-radius: 30px;
  font-size: 17px;
  font-weight: 600;
  max-width: 90vw;
  text-align: center;
  box-shadow: 0 8px 25px rgba(255, 59, 48, 0.3);
}

.scan-button {
  background: #000;
  color: white;
  border: 3px solid white;
  padding: 18px 40px;
  border-radius: 50px;
  font-size: 19px;
  font-weight: bold;
  cursor: pointer;
  min-width: 280px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.4);
  transition: all 0.2s;
}

.scan-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  border-color: #666;
}

.scan-button:not(:disabled):active {
  transform: scale(0.95);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}
</style>