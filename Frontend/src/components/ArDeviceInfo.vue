<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- AR сцена с встроенным AR.js -->
      <a-scene 
        embedded 
        arjs='sourceType: webcam; debugUIEnabled: false; detectionMode: mono_and_matrix; matrixCodeType: 3x3;'
        vr-mode-ui="enabled: false"
        renderer="logarithmicDepthBuffer: true;"
      >
        <!-- Маркер для QR-кода -->
        <a-marker 
          type="barcode" 
          :value="barcodeValue"
          @markerfound="onMarkerFound"
          @markerlost="onMarkerLost"
          emitevents="true"
          cursor="rayOrigin: mouse"
        >
          <!-- 3D панель с информацией -->
          <a-entity v-if="deviceData">
            <!-- Фон панели -->
            <a-plane 
              color="#FFFFFF" 
              width="1.5" 
              height="1.0"
              position="0 1.2 0"
              opacity="0.95"
            ></a-plane>
            
            <!-- Заголовок -->
            <a-text 
              :value="deviceData.name_model" 
              align="center" 
              color="#000000"
              position="0 1.5 0.01"
              width="1.4"
            ></a-text>
            
            <a-text 
              :value="deviceData.name_type" 
              align="center" 
              color="#666666"
              position="0 1.35 0.01"
              width="1.2"
              scale="0.8 0.8 0.8"
            ></a-text>
  
            <!-- Свойства -->
            <a-entity 
              v-for="(property, index) in visibleProperties" 
              :key="index"
              :position="`-0.6 ${1.2 - (index * 0.15)} 0.01`"
            >
              <a-text 
                :value="`${property.Name}: ${property.Value}`"
                align="left"
                color="#000000"
                width="1.2"
                scale="0.7 0.7 0.7"
              ></a-text>
            </a-entity>
          </a-entity>
        </a-marker>
  
        <a-entity camera></a-entity>
      </a-scene>
  
      <!-- Интерфейс -->
      <div class="ar-ui">
        <div class="scanning-message">
          <h3>Наведите камеру на QR-код</h3>
          <p>Информация появится над QR-кодом</p>
        </div>
  
        <button class="close-button" @click="closeAR">
          Закрыть AR
        </button>
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
  
  // Генерируем barcode value из scannedData
  const barcodeValue = computed(() => {
    if (!props.scannedData) return 0
    
    let hash = 0
    for (let i = 0; i < props.scannedData.length; i++) {
      hash = ((hash << 5) - hash) + props.scannedData.charCodeAt(i)
      hash |= 0
    }
    return Math.abs(hash) % 1000
  })
  
  // Ограничиваем свойства для отображения
  const visibleProperties = computed(() => {
    return props.deviceData?.properties?.slice(0, 4) || []
  })
  
  const onMarkerFound = () => {
    console.log('Маркер найден!')
  }
  
  const onMarkerLost = () => {
    console.log('Маркер потерян')
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
    overflow: hidden;
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
  
  .scanning-message {
    position: absolute;
    top: 15%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .scanning-message h3 {
    margin: 0 0 10px 0;
    font-size: 18px;
    background: rgba(0, 0, 0, 0.7);
    display: inline-block;
    padding: 10px 20px;
    border-radius: 20px;
    backdrop-filter: blur(10px);
  }
  
  .scanning-message p {
    margin: 0;
    font-size: 14px;
    color: #cccccc;
    background: rgba(0, 0, 0, 0.5);
    display: inline-block;
    padding: 8px 16px;
    border-radius: 15px;
    backdrop-filter: blur(10px);
  }
  
  .close-button {
    position: absolute;
    bottom: 30px;
    left: 50%;
    transform: translateX(-50%);
    background: rgba(255, 255, 255, 0.9);
    color: #000;
    border: none;
    padding: 12px 24px;
    border-radius: 25px;
    cursor: pointer;
    pointer-events: auto;
    font-size: 16px;
    font-weight: 600;
    backdrop-filter: blur(10px);
  }
  </style>