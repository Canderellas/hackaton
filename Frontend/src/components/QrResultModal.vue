<!-- QrResultModal.vue -->
<script setup>
defineProps({
  scannedData: String,
  frameImage: String // base64 с кадром и зелёной рамкой
})

defineEmits(['close'])
</script>

<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="modal-content" @click.stop>
      <button class="close-btn" @click="$emit('close')">✕</button>

      <!-- Кадр с зелёной рамкой -->
      <div class="frame-preview">
        <img :src="frameImage" alt="Захваченный QR-код" />
      </div>

      <div class="result-info">
        <h3>QR-код успешно отсканирован!</h3>
        <p class="result-text">{{ scannedData }}</p>

        <a
          v-if="scannedData.startsWith('http')"
          :href="scannedData"
          target="_blank"
          class="open-link-btn"
        >
          Открыть ссылку
        </a>

        <button @click="$emit('close')" class="done-btn">
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
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 24px;
  overflow: hidden;
  max-width: 90vw;
  width: 400px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.4);
  position: relative;
}

.close-btn {
  position: absolute;
  top: 12px;
  right: 16px;
  width: 36px;
  height: 36px;
  border: none;
  background: rgba(0,0,0,0.1);
  border-radius: 50%;
  font-size: 20px;
  cursor: pointer;
  z-index: 10;
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
  padding: 24px;
  text-align: center;
}

.result-info h3 {
  margin: 0 0 16px;
  font-size: 20px;
  color: #333;
}

.result-text {
  background: #f5f5f5;
  padding: 12px;
  border-radius: 8px;
  font-size: 14px;
  word-break: break-all;
  margin: 16px 0;
  color: #333;
}

.open-link-btn,
.done-btn {
  display: block;
  width: 100%;
  padding: 14px;
  margin-top: 12px;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
}

.open-link-btn {
  background: #0066ff;
  color: white;
}

.done-btn {
  background: #333;
  color: white;
}
</style>