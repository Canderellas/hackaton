<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- AR сцена -->
      <a-scene 
        embedded 
        arjs="sourceType: webcam; debugUIEnabled: false; detectionMode: mono_and_matrix; matrixCodeType: 3x3;"
        vr-mode-ui="enabled: false"
        renderer="logarithmicDepthBuffer: true;"
      >
        <!-- Маркер для QR-кода -->
        <a-marker 
          type="barcode" 
          :value="barcodeValue"
          @markerFound="onMarkerFound"
          @markerLost="onMarkerLost"
        >
          <!-- 3D панель с информацией -->
          <a-entity v-if="deviceData" :visible="markerVisible">
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
  
            <!-- Последняя операция -->
            <a-entity v-if="lastOperation" position="0 0.8 0.01">
              <a-text 
                :value="`📍 ${lastOperation.Place}`"
                align="center"
                color="#007AFF"
                width="1.2"
                scale="0.6 0.6 0.6"
              ></a-text>
              <a-text 
                :value="formatDate(lastOperation.DateOperation)"
                align="center"
                color="#8E8E93"
                position="0 -0.08 0"
                width="1.0"
                scale="0.5 0.5 0.5"
              ></a-text>
            </a-entity>
          </a-entity>
        </a-marker>
  
        <a-entity camera></a-entity>
      </a-scene>
  
      <!-- Интерфейс -->
      <div class="ar-ui">
        <div v-if="!markerVisible" class="scanning-message">
          <h3>Наведите камеру на QR-код</h3>
          <p>Информация появится над QR-кодом</p>
        </div>
        
        <div v-else class="found-message">
          <h3>✅ Устройство распознано</h3>
          <p>Двигайте камеру - информация следует за QR-кодом</p>
        </div>
  
        <button class="close-button" @click="closeAR">
          Закрыть AR
        </button>
      </div>
  
      <!-- Загрузка -->
      <div v-if="loading" class="loading-overlay">
        <div class="spinner"></div>
        <p>Загрузка AR...</p>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted, onUnmounted, computed } from 'vue'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const loading = ref(true)
  const markerVisible = ref(false)
  
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
  
  // Последняя операция
  const lastOperation = computed(() => {
    return props.deviceData?.operation_logs?.[0] || null
  })
  
  onMounted(() => {
    // Даем время на загрузку AR
    setTimeout(() => {
      loading.value = false
    }, 2000)
  })
  
  const onMarkerFound = () => {
    console.log('Маркер найден!')
    markerVisible.value = true
  }
  
  const onMarkerLost = () => {
    console.log('Маркер потерян')
    markerVisible.value = false
  }
  
  const formatDate = (dateString) => {
    if (!dateString) return ''
    try {
      const date = new Date(dateString)
      return date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      })
    } catch {
      return dateString
    }
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
  
  /* Интерфейс поверх AR */
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
    top: 15%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .scanning-message h3,
  .found-message h3 {
    margin: 0 0 10px 0;
    font-size: 18px;
    background: rgba(0, 0, 0, 0.7);
    display: inline-block;
    padding: 10px 20px;
    border-radius: 20px;
    backdrop-filter: blur(10px);
  }
  
  .scanning-message p,
  .found-message p {
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
  
  .loading-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    z-index: 1000;
  }
  
  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #007aff;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 16px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  </style>