<!-- src/components/QrCameraScanner.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import jsQR from 'jsqr'
import ArDeviceInfo from './ArDeviceInfo.vue'

// Refs
const video = ref(null)
const canvas = ref(null)
const stream = ref(null)
const errorMessage = ref('')
const showARView = ref(false)
const showResultModal = ref(false)
const deviceData = ref(null)
const lastScannedData = ref('')
const lastScannedLocation = ref(null) // ✅ Сохраняем позицию QR-кода
const isScanning = ref(false)

// Функции
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
    }
  } catch (err) {
    errorMessage.value = 'Нет доступа к камере. Разрешите доступ к камере в настройках браузера.'
    console.error('Camera error:', err)
  }
}

const fetchDeviceData = async (deviceId) => {
  try {
    const response = await fetch(`https://comunada.store/api/device/${deviceId}`)
    
    if (response.status === 404) {
      errorMessage.value = 'Устройство не найдено'
      return null
    }
    
    if (!response.ok) {
      throw new Error(`Ошибка сервера: ${response.status}`)
    }
    
    return await response.json()
  } catch (err) {
    console.error('Ошибка при загрузке данных:', err)
    errorMessage.value = 'Ошибка при загрузке данных устройства'
    return null
  }
}

const extractDeviceId = (data) => {
  // Пытаемся найти GUID в данных
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(data)) {
    return data
  }
  
  // Пытаемся извлечь из URL
  try {
    const url = new URL(data)
    const pathParts = url.pathname.split('/')
    const id = pathParts.find(part => 
      /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(part)
    )
    return id || data
  } catch {
    return data
  }
}

const scanNow = async () => {
  if (!video.value || video.value.readyState !== video.value.HAVE_ENOUGH_DATA) {
    errorMessage.value = 'Камера ещё не готова'
    return
  }

  if (isScanning.value) return
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
    lastScannedData.value = code.data
    lastScannedLocation.value = { // ✅ Сохраняем полную информацию о позиции
      topLeft: { x: code.location.topLeftCorner.x, y: code.location.topLeftCorner.y },
      topRight: { x: code.location.topRightCorner.x, y: code.location.topRightCorner.y },
      bottomRight: { x: code.location.bottomRightCorner.x, y: code.location.bottomRightCorner.y },
      bottomLeft: { x: code.location.bottomLeftCorner.x, y: code.location.bottomLeftCorner.y },
      center: {
        x: (code.location.topLeftCorner.x + code.location.bottomRightCorner.x) / 2,
        y: (code.location.topLeftCorner.y + code.location.bottomRightCorner.y) / 2
      },
      width: Math.abs(code.location.topRightCorner.x - code.location.topLeftCorner.x),
      height: Math.abs(code.location.bottomLeftCorner.y - code.location.topLeftCorner.y)
    }
    
    errorMessage.value = ''
    
    const deviceId = extractDeviceId(code.data)
    const data = await fetchDeviceData(deviceId)
    
    if (data) {
      deviceData.value = data
      showResultModal.value = true
    }
  } else {
    errorMessage.value = 'QR-код не найден. Убедитесь, что код хорошо освещен и находится в рамке.'
    lastScannedLocation.value = null
  }

  isScanning.value = false
}

const drawGreenBorder = (ctx, location) => {
  ctx.strokeStyle = '#00ff00'
  ctx.lineWidth = 4
  ctx.beginPath()
  ctx.moveTo(location.topLeftCorner.x, location.topLeftCorner.y)
  ctx.lineTo(location.topRightCorner.x, location.topRightCorner.y)
  ctx.lineTo(location.bottomRightCorner.x, location.bottomRightCorner.y)
  ctx.lineTo(location.bottomLeftCorner.x, location.bottomLeftCorner.y)
  ctx.closePath()
  ctx.stroke()
}

const openAR = () => {
  showResultModal.value = false
  showARView.value = true
}

const closeAR = () => {
  showARView.value = false
  deviceData.value = null
  lastScannedLocation.value = null
}

const closeModal = () => {
  showResultModal.value = false
  deviceData.value = null
  lastScannedLocation.value = null
}

const retryScan = () => {
  closeModal()
  errorMessage.value = 'Наведите камеру на QR-код и нажмите "Сканировать"'
}

// Автоматическое сканирование каждые 500ms (опционально)
const autoScanInterval = ref(null)

const startAutoScan = () => {
  if (autoScanInterval.value) clearInterval(autoScanInterval.value)
  autoScanInterval.value = setInterval(() => {
    if (!showResultModal.value && !showARView.value) {
      scanNow()
    }
  }, 500)
}

const stopAutoScan = () => {
  if (autoScanInterval.value) {
    clearInterval(autoScanInterval.value)
    autoScanInterval.value = null
  }
}

// Жизненный цикл
onMounted(() => {
  startCamera().then(() => {
    // Запускаем автосканирование после успешной загрузки камеры
    startAutoScan()
  })
})

onUnmounted(() => {
  stopAutoScan()
  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())
  }
})
</script>

<template>
  <div class="app">
    <!-- Заголовок -->
    <div class="header">
      <h1>QR Сканер устройств</h1>
      <p>Наведите камеру на QR-код устройства для получения информации</p>
    </div>

    <!-- Камера -->
    <div class="camera-wrapper">
      <video ref="video" playsinline muted class="camera-video"></video>
      <canvas ref="canvas" class="camera-canvas"></canvas>
      
      <!-- Индикатор сканирования -->
      <div class="scanning-frame">
        <div class="frame-corner top-left"></div>
        <div class="frame-corner top-right"></div>
        <div class="frame-corner bottom-left"></div>
        <div class="frame-corner bottom-right"></div>
        <div class="scanning-line" :class="{ scanning: !showResultModal && !showARView }"></div>
      </div>
      
      <div v-if="!stream" class="placeholder">
        <div class="loading-spinner"></div>
        <p>Загрузка камеры...</p>
      </div>
    </div>

    <!-- Сообщение об ошибке -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <!-- Кнопки управления -->
    <div class="controls">
      <button @click="scanNow" class="scan-button" :disabled="isScanning">
        <span v-if="isScanning">🔍 Сканирую...</span>
        <span v-else>📷 Сканировать QR-код</span>
      </button>
      
      <button @click="startAutoScan" class="secondary-button" v-if="!autoScanInterval">
        🔄 Включить автосканирование
      </button>
      <button @click="stopAutoScan" class="secondary-button" v-else>
        ⏸️ Остановить автосканирование
      </button>
    </div>

    <!-- Модалка с результатом -->
    <div v-if="showResultModal && deviceData" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>✅ Устройство распознано</h2>
          <button class="close-button" @click="closeModal">×</button>
        </div>
        
        <div class="device-info">
          <div class="device-main-info">
            <h3>{{ deviceData.name_model }}</h3>
            <p class="device-type">{{ deviceData.name_type }}</p>
            <p class="device-description" v-if="deviceData.description_model">
              {{ deviceData.description_model }}
            </p>
          </div>

          <div v-if="deviceData.properties && deviceData.properties.length > 0" class="properties">
            <h4>Характеристики:</h4>
            <div class="properties-grid">
              <div v-for="(prop, idx) in deviceData.properties.slice(0, 4)" :key="idx" class="property">
                <strong>{{ prop.Name }}:</strong> 
                <span>{{ prop.Value }}</span>
              </div>
            </div>
          </div>

          <div v-if="deviceData.operation_logs && deviceData.operation_logs.length > 0" class="logs">
            <h4>Последняя операция:</h4>
            <p class="log-entry">
              {{ deviceData.operation_logs[0].action }} - 
              {{ new Date(deviceData.operation_logs[0].timestamp).toLocaleDateString() }}
            </p>
          </div>

          <div class="action-buttons">
            <button @click="openAR" class="ar-button">
              🚀 Открыть в AR режиме
            </button>
            <button @click="retryScan" class="secondary-button">
              🔄 Сканировать другой код
            </button>
            <button @click="closeModal" class="close-modal-button">
              Закрыть
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- AR режим -->
    <ArDeviceInfo
      v-if="showARView"
      :scanned-data="lastScannedData"
      :device-data="deviceData"
      :qr-location="lastScannedLocation"
      :video-element="video"
      @close="closeAR"
    />
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.header {
  text-align: center;
  margin-bottom: 30px;
  color: white;
}

.header h1 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 700;
}

.header p {
  margin: 0;
  opacity: 0.9;
  font-size: 16px;
}

.camera-wrapper {
  position: relative;
  width: 90vw;
  max-width: 400px;
  aspect-ratio: 1 / 1;
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
  background: #000;
  margin-bottom: 24px;
}

.camera-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.camera-canvas {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
}

.scanning-frame {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 70%;
  height: 70%;
  border: 2px solid rgba(255, 255, 255, 0.3);
  pointer-events: none;
}

.frame-corner {
  position: absolute;
  width: 20px;
  height: 20px;
  border: 3px solid #00ff00;
}

.frame-corner.top-left {
  top: -3px;
  left: -3px;
  border-right: none;
  border-bottom: none;
}

.frame-corner.top-right {
  top: -3px;
  right: -3px;
  border-left: none;
  border-bottom: none;
}

.frame-corner.bottom-left {
  bottom: -3px;
  left: -3px;
  border-right: none;
  border-top: none;
}

.frame-corner.bottom-right {
  bottom: -3px;
  right: -3px;
  border-left: none;
  border-top: none;
}

.scanning-line {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 3px;
  background: #00ff00;
  transform: translateY(-100%);
  opacity: 0;
}

.scanning-line.scanning {
  animation: scan 2s ease-in-out infinite;
}

@keyframes scan {
  0% {
    transform: translateY(-100%);
    opacity: 0;
  }
  10% {
    opacity: 1;
  }
  90% {
    opacity: 1;
  }
  100% {
    transform: translateY(400%);
    opacity: 0;
  }
}

.placeholder {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.8);
  color: white;
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
  padding: 16px 24px;
  border-radius: 16px;
  font-size: 16px;
  font-weight: 600;
  max-width: 90vw;
  text-align: center;
  margin-bottom: 20px;
  box-shadow: 0 8px 25px rgba(255, 59, 48, 0.3);
}

.controls {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 90vw;
  max-width: 400px;
}

.scan-button {
  background: #000;
  color: white;
  border: 3px solid white;
  padding: 18px 24px;
  border-radius: 50px;
  font-size: 18px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.2s;
  min-height: 60px;
}

.scan-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.scan-button:not(:disabled):active {
  transform: scale(0.98);
}

.secondary-button {
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 2px solid rgba(255, 255, 255, 0.3);
  padding: 14px 24px;
  border-radius: 50px;
  font-size: 16px;
  cursor: pointer;
  backdrop-filter: blur(10px);
  transition: all 0.2s;
}

.secondary-button:active {
  transform: scale(0.98);
  background: rgba(255, 255, 255, 0.3);
}

/* Модалка */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 20px;
  padding: 0;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 24px 0 24px;
  margin-bottom: 0;
}

.modal-header h2 {
  margin: 0;
  font-size: 20px;
  color: #1d1d1f;
}

.close-button {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #8e8e93;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.close-button:hover {
  background: #f0f0f0;
}

.device-info {
  padding: 24px;
}

.device-main-info {
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid #f0f0f0;
}

.device-main-info h3 {
  margin: 0 0 8px 0;
  font-size: 22px;
  color: #1d1d1f;
}

.device-type {
  margin: 0 0 12px 0;
  color: #007AFF;
  font-size: 16px;
  font-weight: 600;
}

.device-description {
  margin: 0;
  color: #666;
  line-height: 1.4;
}

.properties {
  margin-bottom: 20px;
}

.properties h4 {
  margin: 0 0 16px 0;
  font-size: 17px;
  color: #1d1d1f;
}

.properties-grid {
  display: grid;
  gap: 12px;
}

.property {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  padding: 12px;
  background: #f8f8f8;
  border-radius: 12px;
}

.property strong {
  color: #1d1d1f;
  flex-shrink: 0;
}

.property span {
  color: #666;
  text-align: right;
  word-break: break-word;
}

.logs {
  margin-bottom: 24px;
  padding: 16px;
  background: #f8f8f8;
  border-radius: 12px;
}

.logs h4 {
  margin: 0 0 8px 0;
  font-size: 16px;
  color: #1d1d1f;
}

.log-entry {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ar-button {
  background: #007AFF;
  color: white;
  border: none;
  padding: 16px 24px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 17px;
  font-weight: 600;
  transition: all 0.2s;
}

.ar-button:hover {
  background: #0056cc;
  transform: translateY(-1px);
}

.close-modal-button {
  background: #8e8e93;
  color: white;
  border: none;
  padding: 14px 24px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 16px;
  transition: all 0.2s;
}

.close-modal-button:hover {
  background: #6d6d70;
}

/* Адаптивность */
@media (max-width: 768px) {
  .app {
    padding: 16px;
  }
  
  .header h1 {
    font-size: 24px;
  }
  
  .camera-wrapper {
    width: 95vw;
  }
  
  .modal-content {
    margin: 10px;
    max-height: 85vh;
  }
}
</style>