<!-- src/components/QrResultModal.vue -->
<script setup>
defineProps({
  scannedData: {
    type: String,
    required: true
  },
  frameImage: {
    type: String,
    required: true
  }
})

const emit = defineEmits(['close'])

// Опционально: если хочешь кнопку "Перейти к устройству" прямо в модалке
const goToDevice = () => {
  const uuid = scannedData.trim()
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
  if (uuidRegex.test(uuid)) {
    window.location.href = `/device/${uuid}`
  }
}
</script>

<template>
  <div class="modal-overlay" @click="emit('close')">
    <div class="modal-content" @click.stop>
      <button class="close-btn" @click="emit('close')">✕</button>

      <!-- Кадр с зелёной рамкой -->
      <div class="frame-preview">
        <img :src="frameImage" alt="Отсканированный QR-код" />
      </div>

      <div class="result-info">
        <h3>QR-код успешно отсканирован!</h3>
        <p class="result-text">{{ scannedData }}</p>

        <!-- Если это ссылка — открываем -->
        <a
          v-if="scannedData.startsWith('http')"
          :href="scannedData"
          target="_blank"
          rel="noopener"
          class="open-link-btn"
        >
          Открыть ссылку
        </a>

        <!-- Если это UUID — предлагаем перейти к устройству -->
        <button
          v-else-if="/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(scannedData.trim())"
          @click="goToDevice"
          class="open-link-btn"
        >
          Перейти к устройству
        </button>

        <!-- Кнопка закрытия -->
        <button @click="emit('close')" class="done-btn">
          Готово
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.85);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  backdrop-filter: blur(8px);
}

.modal-content {
  background: white;
  border-radius: 28px;
  overflow: hidden;
  max-width: 90vw;
  width: 400px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  position: relative;
}

.close-btn {
  position: absolute;
  top: 12px;
  right: 16px;
  width: 40px;
  height: 40px;
  border: none;
  background: rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  font-size: 22px;
  cursor: pointer;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame-preview {
  width: 100%;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  background: #000;
}

.frame-preview img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.result-info {
  padding: 28px;
  text-align: center;
}

.result-info h3 {
  margin: 0 0 16px;
  font-size: 21px;
  color: #222;
  font-weight: 600;
}

.result-text {
  background: #f5f5f5;
  padding: 14px;
  border-radius: 12px;
  font-size: 15px;
  word-break: break-all;
  margin: 16px 0;
  color: #333;
  font-family: monospace;
}

.open-link-btn,
.done-btn {
  display: block;
  width: 100%;
  padding: 16px;
  margin-top: 14px;
  border: none;
  border-radius: 16px;
  font-size: 17px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.2s;
}

.open-link-btn {
  background: #007aff;
  color: white;
}

.open-link-btn:active {
  background: #0066cc;
}

.done-btn {
  background: #333;
  color: white;
  margin-top: 20px;
}

.done-btn:active {
  background: #111;
}
</style>