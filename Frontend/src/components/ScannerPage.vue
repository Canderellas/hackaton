<!-- components/ScannerPage.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { BrowserQRCodeReader } from '@zxing/browser'

const emit = defineEmits(['scan'])
const codeReader = new BrowserQRCodeReader()
const videoElement = ref(null)
const scanning = ref(false)
const stream = ref(null)

// Запуск камеры (только предпросмотр)
async function startCamera() {
  if (!videoElement.value) return

  try {
    stream.value = await navigator.mediaDevices.getUserMedia({ 
      video: { 
        facingMode: 'environment',
        width: { ideal: 1920 },
        height: { ideal: 1080 }
      } 
    })
    
    videoElement.value.srcObject = stream.value
    await videoElement.value.play()
  } catch (err) {
    console.error('Ошибка доступа к камере:', err)
    alert('Не удалось получить доступ к камере. Проверьте разрешения.')
  }
}

// Сканирование по нажатию кнопки
async function scanQRCode() {
  if (!videoElement.value || scanning.value) return

  scanning.value = true

  try {
    const result = await codeReader.decodeFromVideoElement(videoElement.value)
    
    if (result) {
      console.log('Найден QR-код:', result.getText())
      
      try {
        // Пробуем распарсить JSON
        const parsedData = JSON.parse(result.getText())
        emit('scan', parsedData)
      } catch (jsonError) {
        // Если не JSON, создаем объект с текстом
        emit('scan', { 
          scannedText: result.getText(),
          rawData: result.getText(),
          timestamp: new Date().toISOString()
        })
      }
    } else {
      alert('QR-код не найден. Убедитесь, что код в пределах рамки и хорошо освещен.')
    }
  } catch (error) {
    console.error('Ошибка сканирования:', error)
    // Более конкретные сообщения об ошибках
    if (error.name === 'NotFoundException') {
      alert('QR-код не распознан. Попробуйте еще раз.')
    } else if (error.name === 'ChecksumException') {
      alert('Ошибка чтения QR-кода. Попробуйте другой код или улучшите освещение.')
    } else {
      alert('Ошибка при сканировании: ' + error.message)
    }
  } finally {
    scanning.value = false
  }
}

// Альтернативный метод сканирования через canvas (более надежный)
async function scanQRCodeAlternative() {
  if (!videoElement.value || scanning.value) return

  scanning.value = true

  try {
    // Создаем canvas для захвата кадра
    const canvas = document.createElement('canvas')
    const context = canvas.getContext('2d')
    canvas.width = videoElement.value.videoWidth
    canvas.height = videoElement.value.videoHeight
    
    context.drawImage(videoElement.value, 0, 0, canvas.width, canvas.height)
    
    const imageData = context.getImageData(0, 0, canvas.width, canvas.height)
    const result = await codeReader.decodeFromImageData(imageData)
    
    if (result) {
      console.log('Найден QR-код:', result.getText())
      
      try {
        const parsedData = JSON.parse(result.getText())
        emit('scan', parsedData)
      } catch {
        emit('scan', { 
          scannedText: result.getText(),
          rawData: result.getText(),
          timestamp: new Date().toISOString()
        })
      }
    } else {
      alert('QR-код не найден. Попробуйте еще раз.')
    }
  } catch (error) {
    console.error('Ошибка сканирования (альтернативный метод):', error)
    alert(' ')
  } finally {
    scanning.value = false
  }
}

// Остановка камеры
function stopCamera() {
  if (stream.value) {
    stream.value.getTracks().forEach(track => {
      track.stop()
    })
    stream.value = null
  }
  if (codeReader) {
    codeReader.reset()
  }
}

onMounted(() => {
  startCamera()
})

onUnmounted(() => {
  stopCamera()
})
</script>

<template>
  <div class="scanner-page">
    <!-- Убрал заголовок iPhone -->

    <div class="video-container">
      <video ref="videoElement" playsinline autoplay muted></video>
      <div class="scan-overlay">
        <div class="scan-frame">
          <div class="corner top-left"></div>
          <div class="corner top-right"></div>
          <div class="corner bottom-left"></div>
          <div class="corner bottom-right"></div>
          <p class="scan-text">Наведите на QR-код</p>
        </div>
      </div>
    </div>

    <button 
      class="scan-button" 
      @click="scanQRCode" 
      :disabled="scanning"
      :style="{ 
        backgroundColor: scanning ? '#999' : '#007aff',
        color: 'white'
      }"
    >
      {{ scanning ? 'Сканирую...' : 'Сканировать' }}
    </button>
  </div>
</template>

<style scoped>
.scanner-page {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: #000;
}

.video-container {
  position: relative;
  flex: 1;
  background: #000;
  overflow: hidden;
}

video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.scan-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.scan-frame {
  position: relative;
  width: 280px;
  height: 280px;
  border: 2px solid rgba(255, 255, 255, 0.7);
  border-radius: 20px;
  background: rgba(0, 0, 0, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.corner {
  position: absolute;
  width: 30px;
  height: 30px;
  border-color: #007aff;
  border-width: 3px;
}

.top-left {
  top: -3px;
  left: -3px;
  border-top-style: solid;
  border-left-style: solid;
  border-top-left-radius: 15px;
}

.top-right {
  top: -3px;
  right: -3px;
  border-top-style: solid;
  border-right-style: solid;
  border-top-right-radius: 15px;
}

.bottom-left {
  bottom: -3px;
  left: -3px;
  border-bottom-style: solid;
  border-left-style: solid;
  border-bottom-left-radius: 15px;
}

.bottom-right {
  bottom: -3px;
  right: -3px;
  border-bottom-style: solid;
  border-right-style: solid;
  border-bottom-right-radius: 15px;
}

.scan-text {
  color: white;
  font-size: 16px;
  text-align: center;
  margin: 0;
  background: rgba(0, 0, 0, 0.7);
  padding: 8px 16px;
  border-radius: 20px;
}

.scan-button {
  position: absolute;
  bottom: 40px;
  left: 50%;
  transform: translateX(-50%);
  border: none;
  border-radius: 50px;
  padding: 18px 40px;
  font-size: 18px;
  font-weight: 600;
  min-width: 200px;
  box-shadow: 0 4px 20px rgba(0, 122, 255, 0.3);
  z-index: 10;
  transition: all 0.2s;
}

.scan-button:disabled {
  opacity: 0.7;
  transform: translateX(-50%) scale(0.98);
}

.scan-button:active {
  transform: translateX(-50%) scale(0.95);
}
</style>