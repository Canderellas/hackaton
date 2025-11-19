<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <div class="ar-setup" v-if="!arStarted">
        <h2>🚀 AR Просмотр</h2>
        <div class="device-preview">
          <h3>{{ deviceData?.name_model }}</h3>
          <p>{{ deviceData?.name_type }}</p>
        </div>
        <p class="instruction">Наведите камеру на QR-код чтобы увидеть информацию в AR</p>
        <button @click="startAR" class="start-ar-button">
          Запустить AR камеру
        </button>
        <button @click="closeAR" class="close-button">
          Назад
        </button>
      </div>
  
      <div v-else class="ar-scene-container">
        <!-- AR сцена -->
        <a-scene 
          embedded 
          vr-mode-ui="enabled: false"
          arjs="sourceType: webcam; debugUIEnabled: false; detectionMode: mono_and_matrix; matrixCodeType: 3x3;"
        >
          <a-marker 
            type="barcode" 
            :value="barcodeValue"
            @markerFound="onMarkerFound"
            @markerLost="onMarkerLost"
          >
            <a-entity>
              <!-- Фон информации -->
              <a-plane 
                color="#FFFFFF" 
                width="1.5" 
                height="1.0"
                position="0 1.2 0"
                opacity="0.95"
              ></a-plane>
              
              <!-- Текст информации -->
              <a-text 
                :value="deviceData.name_model" 
                align="center" 
                color="#000000"
                position="0 1.5 0.01"
                width="1.4"
              ></a-text>
            </a-entity>
          </a-marker>
          
          <a-entity camera></a-entity>
        </a-scene>
  
        <div class="ar-ui">
          <div v-if="!markerVisible" class="scanning-message">
            <h3>🔍 Ищем QR-код...</h3>
          </div>
          <div v-else class="found-message">
            <h3>✅ QR-код распознан!</h3>
          </div>
          
          <button @click="closeAR" class="close-button">
            Закрыть AR
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed } from 'vue'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const arStarted = ref(false)
  const markerVisible = ref(false)
  
  const barcodeValue = computed(() => {
    if (!props.scannedData) return 0
    let hash = 0
    for (let i = 0; i < props.scannedData.length; i++) {
      hash = ((hash << 5) - hash) + props.scannedData.charCodeAt(i)
      hash |= 0
    }
    return Math.abs(hash) % 100
  })
  
  const startAR = () => {
    arStarted.value = true
    // Динамически загружаем AR.js
    import('aframe')
    import('@ar-js-org/ar.js')
  }
  
  const onMarkerFound = () => {
    console.log('Маркер найден!')
    markerVisible.value = true
  }
  
  const onMarkerLost = () => {
    console.log('Маркер потерян')
    markerVisible.value = false
  }
  
  const closeAR = () => {
    emit('close')
  }
  </script>
  
  <style scoped>
  .ar-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: black;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }
  
  .ar-setup {
    background: white;
    padding: 30px;
    border-radius: 20px;
    text-align: center;
    max-width: 400px;
    margin: 20px;
  }
  
  .device-preview {
    background: #f8f9fa;
    padding: 16px;
    border-radius: 12px;
    margin: 16px 0;
  }
  
  .device-preview h3 {
    margin: 0 0 8px 0;
    color: #1d1d1f;
  }
  
  .instruction {
    color: #666;
    margin: 16px 0;
    line-height: 1.4;
  }
  
  .start-ar-button {
    background: #007aff;
    color: white;
    border: none;
    padding: 14px 28px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 600;
    margin: 8px;
    transition: all 0.2s;
  }
  
  .start-ar-button:hover {
    background: #0056cc;
    transform: translateY(-1px);
  }
  
  .close-button {
    background: #8e8e93;
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 16px;
    margin: 8px;
  }
  
  .ar-scene-container {
    width: 100%;
    height: 100%;
    position: relative;
  }
  
  .a-scene {
    width: 100%;
    height: 100%;
  }
  
  .ar-ui {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 100;
  }
  
  .scanning-message,
  .found-message {
    position: absolute;
    top: 10%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
  }
  
  .scanning-message h3,
  .found-message h3 {
    background: rgba(0, 0, 0, 0.7);
    padding: 12px 24px;
    border-radius: 20px;
    display: inline-block;
    backdrop-filter: blur(10px);
  }
  </style>