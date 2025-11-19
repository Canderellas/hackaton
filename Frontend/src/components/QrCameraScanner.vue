<!-- src/views/QrCameraScannerFixed.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import jsQR from 'jsqr'
import QrResultModal from '@/components/QrResultModal.vue'

const video = ref(null)
const canvas = ref(null)
const stream = ref(null)
const errorMessage = ref('')
const showResultModal = ref(false)
const lastScannedData = ref('')
const lastFrameImage = ref('')

onMounted(async () => {
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
})

onUnmounted(() => {
  if (stream.value) stream.value.getTracks().forEach(t => t.stop())
})

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
    // Рисуем зелёную рамку
    ctx.strokeStyle = '#00ff00'
    ctx.lineWidth = 12
    ctx.beginPath()
    ctx.moveTo(code.location.topLeftCorner.x, code.location.topLeftCorner.y)
    ctx.lineTo(code.location.topRightCorner.x, code.location.topRightCorner.y)
    ctx.lineTo(code.location.bottomRightCorner.x, code.location.bottomRightCorner.y)
    ctx.lineTo(code.location.bottomLeftCorner.x, code.location.bottomLeftCorner.y)
    ctx.closePath()
    ctx.stroke()

    lastFrameImage.value = canvas.value.toDataURL('image/png')
    lastScannedData.value = code.data.trim()

    // Показываем модалку в любом случае
    showResultModal.value = true
    errorMessage.value = ''

    // Если это UUID — через 1 секунду переходим на страницу устройства
    const isUuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(lastScannedData.value)
    if (isUuid) {
      setTimeout(() => {
        window.location.href = `/device/${lastScannedData.value}`
      }, 1000)
    }

  } else {
    errorMessage.value = 'QR-код не найден. Попробуйте ещё раз.'
  }
}

const closeModal = () => {
  showResultModal.value = false
}
</script>

<template>
  <div class="app">
    <div class="camera-wrapper">
      <video ref="video" playsinline muted class="camera-video"></video>
      <canvas ref="canvas" style="display: none;"></canvas>
      <div v-if="!stream" class="placeholder">Камера недоступна</div>
    </div>

    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <button @click="scanNow" class="scan-button">
      Сканировать QR-код
    </button>

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
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
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
  background: rgba(0,0,0,0.9);
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
}
</style>