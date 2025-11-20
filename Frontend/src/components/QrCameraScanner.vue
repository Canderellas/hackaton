<!-- QrCameraScanner.vue -->
<template>
  <div class="scanner-container">
    <div class="scanner-header">
      <h2>Сканирование QR-кода</h2>
      <p>Наведите камеру на QR-код устройства</p>
    </div>

    <div class="camera-wrapper">
      <video
        ref="videoElement"
        class="camera-video"
        :style="videoStyle"
        playsinline
      ></video>
      
      <!-- Область сканирования QR-кода -->
      <div class="scan-overlay">
        <div class="scan-frame">
          <div class="frame-corner top-left"></div>
          <div class="frame-corner top-right"></div>
          <div class="frame-corner bottom-left"></div>
          <div class="frame-corner bottom-right"></div>
          
          <!-- Анимированная линия сканирования -->
          <div class="scan-line"></div>
        </div>
        
        <div class="scan-instruction">
          <p>Поместите QR-код в рамку</p>
        </div>
      </div>

      <!-- Индикатор загрузки -->
      <div v-if="loading" class="camera-loading">
        <div class="loading-spinner"></div>
        <p>Загрузка камеры...</p>
      </div>

      <!-- Сообщение об ошибке -->
      <div v-if="error" class="camera-error">
        <div class="error-icon">⚠️</div>
        <h3>Ошибка доступа к камере</h3>
        <p>{{ error }}</p>
        <button @click="initCamera" class="retry-button">
          Попробовать снова
        </button>
      </div>
    </div>

    <!-- Элементы управления -->
    <div class="scanner-controls">
      <button @click="closeScanner" class="control-button close">
        <span class="button-icon">✕</span>
        Закрыть
      </button>
    </div>

    <!-- Статус сканирования -->
    <div v-if="scanStatus" class="scan-status" :class="scanStatus.type">
      <span class="status-icon">{{ scanStatus.icon }}</span>
      <span>{{ scanStatus.message }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Html5QrcodeScanner } from 'html5-qrcode'

const emit = defineEmits(['scan-success', 'close'])

const videoElement = ref(null)
const loading = ref(true)
const error = ref('')
const scanStatus = ref(null)
const html5QrcodeScanner = ref(null)

// Стиль для видео
const videoStyle = computed(() => ({
  width: '100%',
  height: '100%',
  objectFit: 'cover'
}))

// Инициализация камеры
const initCamera = async () => {
  try {
    loading.value = true
    error.value = ''
    
    const cameras = await Html5Qrcode.getCameras()
    if (cameras.length === 0) {
      throw new Error('Камеры не найдены')
    }

    // Используем первую доступную камеру
    const camera = cameras[0]

    html5QrcodeScanner.value = new Html5QrcodeScanner(
      'reader',
      {
        fps: 10,
        qrbox: { width: 250, height: 250 },
        supportedScanTypes: [
          Html5QrcodeScanType.SCAN_TYPE_QR_CODE
        ]
      },
      false
    )

    await html5QrcodeScanner.value.start(
      camera.id,
      {
        fps: 10,
        qrbox: { width: 250, height: 250 }
      },
      onScanSuccess,
      onScanFailure
    )

    loading.value = false

  } catch (err) {
    console.error('Ошибка инициализации камеры:', err)
    error.value = err.message || 'Не удалось получить доступ к камере'
    loading.value = false
  }
}

// Успешное сканирование
const onScanSuccess = (decodedText, decodedResult) => {
  scanStatus.value = {
    type: 'success',
    icon: '✅',
    message: 'QR-код успешно распознан!'
  }

  // Останавливаем сканер перед эмитом
  if (html5QrcodeScanner.value) {
    html5QrcodeScanner.value.clear()
  }

  setTimeout(() => {
    emit('scan-success', decodedText, decodedResult)
  }, 500)
}

// Ошибка сканирования
const onScanFailure = (error) => {
  if (error && !error.includes('NotFoundException')) {
    scanStatus.value = {
      type: 'error',
      icon: '❌',
      message: 'Ошибка сканирования'
    }
    
    setTimeout(() => {
      scanStatus.value = null
    }, 2000)
  }
}

// Закрытие сканера
const closeScanner = () => {
  if (html5QrcodeScanner.value) {
    html5QrcodeScanner.value.clear().catch(error => {
      console.error('Ошибка при очистке сканера:', error)
    })
  }
  emit('close')
}

onMounted(() => {
  initCamera()
})

onUnmounted(() => {
  if (html5QrcodeScanner.value) {
    html5QrcodeScanner.value.clear().catch(error => {
      console.error('Ошибка при очистке сканера:', error)
    })
  }
})
</script>

<style scoped>
.scanner-container {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: #000;
  display: flex;
  flex-direction: column;
  z-index: 1000;
}

.scanner-header {
  padding: 20px;
  text-align: center;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  z-index: 10;
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
  flex: 1;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
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
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(10px);
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

/* Анимированная линия сканирования */
.scan-line {
  position: absolute;
  top: 10%;
  left: 5%;
  right: 5%;
  height: 3px;
  background: linear-gradient(90deg, 
    transparent 0%, 
    #667eea 50%, 
    transparent 100%);
  animation: scan 2s linear infinite;
  border-radius: 3px;
  box-shadow: 0 0 10px rgba(102, 126, 234, 0.8);
}

@keyframes scan {
  0% {
    top: 10%;
  }
  50% {
    top: 90%;
  }
  100% {
    top: 10%;
  }
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

/* Индикатор загрузки */
.camera-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
  color: white;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(255, 255, 255, 0.3);
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.camera-loading p {
  margin: 0;
  font-size: 16px;
}

/* Сообщение об ошибке */
.camera-error {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
  color: white;
  background: rgba(0, 0, 0, 0.8);
  padding: 30px;
  border-radius: 20px;
  backdrop-filter: blur(10px);
}

.error-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.camera-error h3 {
  margin: 0 0 12px 0;
  font-size: 20px;
}

.camera-error p {
  margin: 0 0 20px 0;
  opacity: 0.8;
}

.retry-button {
  background: #667eea;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.retry-button:hover {
  background: #5a6fd8;
  transform: translateY(-2px);
}

/* Элементы управления */
.scanner-controls {
  padding: 20px;
  display: flex;
  justify-content: center;
  background: rgba(0, 0, 0, 0.8);
}

.control-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
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

.control-button:hover {
  background: rgba(220, 53, 69, 0.5);
  transform: translateY(-2px);
}

.button-icon {
  font-size: 16px;
}

/* Статус сканирования */
.scan-status {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  padding: 16px 24px;
  border-radius: 12px;
  font-weight: 600;
  z-index: 1001;
  display: flex;
  align-items: center;
  gap: 8px;
  backdrop-filter: blur(10px);
}

.scan-status.success {
  background: rgba(40, 167, 69, 0.9);
  color: white;
}

.scan-status.error {
  background: rgba(220, 53, 69, 0.9);
  color: white;
}

.status-icon {
  font-size: 18px;
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
  
  .control-button {
    padding: 14px 28px;
    font-size: 16px;
  }
}
</style>