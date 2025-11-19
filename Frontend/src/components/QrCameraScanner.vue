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
const showResultModal = ref(false) // ← ДОБАВИЛИ отдельное состояние для модалки
const deviceData = ref(null)
const lastScannedData = ref('')

// Функции
const startCamera = async () => {
  try {
    stream.value = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'environment' }
    })
    if (video.value) {
      video.value.srcObject = stream.value
      video.value.play()
    }
  } catch (err) {
    errorMessage.value = 'Нет доступа к камере'
    console.error(err)
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
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(data)) {
    return data
  }
  
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

  const ctx = canvas.value.getContext('2d')
  canvas.value.width = video.value.videoWidth
  canvas.value.height = video.value.videoHeight
  ctx.drawImage(video.value, 0, 0, canvas.value.width, canvas.value.height)

  const imageData = ctx.getImageData(0, 0, canvas.value.width, canvas.value.height)
  const code = jsQR(imageData.data, imageData.width, imageData.height, {
    inversionAttempts: 'invertFirst'
  })

  if (code) {
    drawGreenBorder(ctx, code.location)
    lastScannedData.value = code.data
    errorMessage.value = ''
    
    const deviceId = extractDeviceId(code.data)
    const data = await fetchDeviceData(deviceId)
    
    if (data) {
      deviceData.value = data
      showResultModal.value = true // ← ПОКАЗЫВАЕМ МОДАЛКУ, а не AR
    }
  } else {
    errorMessage.value = 'QR-код не найден. Попробуйте ещё раз.'
  }
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
  showResultModal.value = false // ← Закрываем модалку
  showARView.value = true       // ← Открываем AR
}

const closeAR = () => {
  showARView.value = false
  deviceData.value = null
}

const closeModal = () => {
  showResultModal.value = false
  deviceData.value = null
}

// Жизненный цикл
onMounted(() => {
  startCamera()
})

onUnmounted(() => {
  if (stream.value) stream.value.getTracks().forEach(t => t.stop())
})
</script>

<template>
  <div class="app">
    <!-- Камера -->
    <div class="camera-wrapper">
      <video ref="video" playsinline muted class="camera-video"></video>
      <canvas ref="canvas" style="display: none;"></canvas>
      <div v-if="!stream" class="placeholder">Камера недоступна</div>
    </div>

    <!-- Сообщение об ошибке -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <!-- Кнопка сканирования -->
    <button @click="scanNow" class="scan-button">
      📷 Сканировать QR-код
    </button>

    <!-- Модалка с результатом -->
    <div v-if="showResultModal && deviceData" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Информация об устройстве</h2>
          <button class="close-button" @click="closeModal">×</button>
        </div>
        <div class="device-info">
          <h3>{{ deviceData.name_model }}</h3>
          <p class="device-type">{{ deviceData.name_type }}</p>
          
          <div v-if="deviceData.properties && deviceData.properties.length > 0" class="properties">
            <h4>Характеристики:</h4>
            <div v-for="(prop, idx) in deviceData.properties.slice(0, 3)" :key="idx" class="property">
              <strong>{{ prop.Name }}:</strong> {{ prop.Value }}
            </div>
          </div>

          <div class="action-buttons">
            <button @click="openAR" class="ar-button">
              🚀 Открыть в AR
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
      @close="closeAR"
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
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.85);
  color: white;
  font-size: 18px;
  text-align: center;
  padding: 20px;
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

.scan-button:active {
  transform: scale(0.95);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
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
  padding: 24px;
  max-width: 400px;
  width: 100%;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 2px solid #f0f0f0;
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

.device-info h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  color: #1d1d1f;
}

.device-type {
  margin: 0 0 20px 0;
  color: #8e8e93;
  font-size: 16px;
}

.properties {
  margin-bottom: 24px;
}

.properties h4 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: #1d1d1f;
  border-bottom: 1px solid #f0f0f0;
  padding-bottom: 8px;
}

.property {
  margin-bottom: 8px;
  line-height: 1.4;
}

.property strong {
  color: #1d1d1f;
}

.action-buttons {
  display: flex;
  gap: 12px;
  flex-direction: column;
}

.ar-button {
  background: #007aff;
  color: white;
  border: none;
  padding: 14px 20px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 16px;
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
  padding: 12px 20px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 16px;
}

.close-modal-button:hover {
  background: #6d6d70;
}
</style>