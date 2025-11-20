<!-- QrCameraScanner.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import jsQR from 'jsqr'
import QrResultModal from './QrResultModal.vue'

const video = ref(null)
const canvas = ref(null)
const stream = ref(null)
const errorMessage = ref('')
const showResultModal = ref(false)
const lastScannedData = ref('')
const lastFrameImage = ref('')
const cameraReady = ref(false)
const isScanning = ref(false)

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

  isScanning.value = true

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

  isScanning.value = false
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

const closeScanner = () => {
  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())
  }
}

onMounted(() => {
  startCamera()
})

onUnmounted(() => {
  closeScanner()
})
</script>

<template>
  <div class="app">
    <!-- Заголовок -->
    <div class="scanner-header">
      <h2>Сканирование QR-кода</h2>
      <p>Наведите камеру на QR-код устройства</p>
    </div>

    <!-- Камера с областью сканирования -->
    <div class="camera-wrapper">
      <video 
        ref="video" 
        playsinline 
        muted 
        class="camera-video"
        @loadeddata="cameraReady = true"
      ></video>
      <canvas ref="canvas" style="display: none;"></canvas>
      
      <!-- Область сканирования -->
      <div class="scan-overlay">
        <div class="scan-frame">
          <div class="frame-corner top-left"></div>
          <div class="frame-corner top-right"></div>
          <div class="frame-corner bottom-left"></div>
          <div class="frame-corner bottom-right"></div>
        </div>
        
        <div class="scan-instruction">
          <p>Поместите QR-код в рамку</p>
        </div>
      </div>

      <!-- Индикатор загрузки -->
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
      :disabled="!cameraReady || isScanning"
    >
      <span v-if="isScanning">🔍 Сканирование...</span>
      <span v-else-if="!cameraReady">⏳ Подготовка...</span>
      <span v-else>📷 Сканировать QR-код</span>
    </button>

    <!-- Кнопка закрытия -->
    <button @click="closeScanner" class="close-button">
      <span class="button-icon">✕</span>
      Закрыть сканер
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
  gap: 24px;
  padding: 20px;
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.scanner-header {
  text-align: center;
  color: white;
  margin-bottom: 10px;
}

.scanner-header h2 {
  margin: 0 0 8px 0;
  font-size: 24px;
  font-weight: 600;
}

.scanner-header p {
  margin: 0;
  opacity: 0.8;
  font-size: 16px;
}

.camera-wrapper {
  position: relative;
  width: 90vw;
  max-width: 400px;
  aspect-ratio: 1 / 1;
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
  background: #000;
}

.camera-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* Область сканирования */
.scan-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.scan-frame {
  width: 280px;
  height: 280px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 20px;
  position: relative;
}

/* Уголки рамки */
.frame-corner {
  position: absolute;
  width: 30px;
  height: 30px;
  border: 3px solid #667eea;
}

.frame-corner.top-left {
  top: -3px;
  left: -3px;
  border-right: none;
  border-bottom: none;
  border-radius: 12px 0 0 0;
}

.frame-corner.top-right {
  top: -3px;
  right: -3px;
  border-left: none;
  border-bottom: none;
  border-radius: 0 12px 0 0;
}

.frame-corner.bottom-left {
  bottom: -3px;
  left: -3px;
  border-right: none;
  border-top: none;
  border-radius: 0 0 0 12px;
}

.frame-corner.bottom-right {
  bottom: -3px;
  right: -3px;
  border-left: none;
  border-top: none;
  border-radius: 0 0 12px 0;
}

.scan-instruction {
  margin-top: 30px;
  text-align: center;
  color: white;
  background: rgba(0, 0, 0, 0.7);
  padding: 12px 20px;
  border-radius: 25px;
  backdrop-filter: blur(10px);
}

.scan-instruction p {
  margin: 0;
  font-size: 16px;
  font-weight: 500;
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
  background: #667eea;
  color: white;
  border: none;
  padding: 18px 40px;
  border-radius: 50px;
  font-size: 19px;
  font-weight: bold;
  cursor: pointer;
  min-width: 280px;
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
  transition: all 0.2s;
}

.scan-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background: #666;
}

.scan-button:not(:disabled):hover {
  background: #5a6fd8;
  transform: translateY(-2px);
  box-shadow: 0 15px 35px rgba(102, 126, 234, 0.5);
}

.scan-button:not(:disabled):active {
  transform: scale(0.95);
  box-shadow: 0 5px 15px rgba(102, 126, 234, 0.3);
}

.close-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  background: rgba(220, 53, 69, 0.3);
  color: white;
  border: 2px solid rgba(220, 53, 69, 0.5);
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  backdrop-filter: blur(10px);
}

.close-button:hover {
  background: rgba(220, 53, 69, 0.5);
  transform: translateY(-2px);
}

.button-icon {
  font-size: 16px;
}

/* Адаптивность */
@media (max-width: 480px) {
  .scan-frame {
    width: 250px;
    height: 250px;
  }
  
  .scanner-header h2 {
    font-size: 20px;
  }
  
  .scanner-header p {
    font-size: 14px;
  }
  
  .camera-wrapper {
    max-width: 320px;
  }
  
  .scan-button {
    min-width: 250px;
    padding: 16px 32px;
    font-size: 17px;
  }
}
</style>