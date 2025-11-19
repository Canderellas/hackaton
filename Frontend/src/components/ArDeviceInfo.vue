<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- AR сцена -->
      <a-scene 
        embedded 
        vr-mode-ui="enabled: false"
        renderer="antialias: true; alpha: true"
        arjs="sourceType: webcam; videoTexture: true; debugUIEnabled: false; detectionMode: mono_and_matrix; matrixCodeType: 3x3;"
      >
        <!-- Маркер для QR-кода -->
        <a-marker 
          type="barcode" 
          :value="barcodeValue"
          @markerFound="onMarkerFound"
          @markerLost="onMarkerLost"
          emitevents="true"
          cursor="rayOrigin: mouse"
        >
          <!-- 3D панель с информацией об устройстве -->
          <a-entity class="info-panel">
            <!-- Фон панели -->
            <a-plane 
              class="panel-background"
              color="#FFFFFF" 
              width="1.8" 
              height="1.2"
              position="0 1.5 0"
              opacity="0.95"
            ></a-plane>
            
            <!-- Заголовок - название модели -->
            <a-text 
              class="model-name"
              :value="deviceData.name_model" 
              align="center" 
              color="#000000"
              position="0 2.0 0.01"
              width="1.6"
              wrap-count="20"
            ></a-text>
            
            <!-- Тип устройства -->
            <a-text 
              class="type-name"
              :value="deviceData.name_type" 
              align="center" 
              color="#666666"
              position="0 1.8 0.01"
              width="1.4"
              wrap-count="18"
              scale="0.8 0.8 0.8"
            ></a-text>
  
            <!-- Свойства устройства -->
            <a-entity 
              v-for="(property, index) in visibleProperties" 
              :key="index"
              class="property-item"
              :position="getPropertyPosition(index)"
            >
              <a-text 
                :value="getPropertyText(property)"
                align="left"
                color="#000000"
                width="1.5"
                wrap-count="25"
                scale="0.7 0.7 0.7"
              ></a-text>
            </a-entity>
  
            <!-- Последняя операция -->
            <a-entity 
              v-if="lastOperation" 
              class="last-operation"
              position="0 0.6 0.01"
            >
              <a-text 
                :value="`📍 ${lastOperation.Place}`"
                align="center"
                color="#007AFF"
                width="1.4"
                wrap-count="20"
                scale="0.6 0.6 0.6"
              ></a-text>
              <a-text 
                :value="formatDate(lastOperation.DateOperation)"
                align="center"
                color="#8E8E93"
                position="0 -0.1 0"
                width="1.2"
                wrap-count="15"
                scale="0.5 0.5 0.5"
              ></a-text>
            </a-entity>
  
            <!-- Индикатор что это AR контент -->
            <a-ring
              color="#007AFF"
              radius-inner="0.2"
              radius-outer="0.3"
              position="0 -0.5 0"
              opacity="0.8"
            ></a-ring>
          </a-entity>
        </a-marker>
  
        <!-- Статическая камера -->
        <a-entity camera></a-entity>
      </a-scene>
  
      <!-- Интерфейс управления -->
      <div class="ar-ui">
        <div v-if="!markerFound" class="scanning-message">
          <div class="message-content">
            <h3>🎯 Наведите камеру на QR-код</h3>
            <p>Информация об устройстве появится прямо над QR-кодом в пространстве</p>
            <div class="hint">
              ⚡ Двигайте камеру - информация будет следовать за QR-кодом
            </div>
          </div>
        </div>
        
        <div v-else class="found-message">
          <div class="message-content">
            <h3>✅ Устройство распознано!</h3>
            <p>Информация закреплена в реальном пространстве</p>
          </div>
        </div>
  
        <button class="close-button" @click="closeAR">
          🔙 Закрыть AR
        </button>
      </div>
  
      <!-- Индикатор загрузки -->
      <div v-if="loading" class="loading-overlay">
        <div class="spinner"></div>
        <p>Загрузка AR...</p>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue'
  
  // Импорты AR.js
  import 'aframe'
  import '@ar-js-org/ar.js'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const markerFound = ref(false)
  const loading = ref(true)
  
  // Генерируем barcode value из scannedData
  const barcodeValue = computed(() => {
    if (!props.scannedData) return 0
    
    let hash = 0
    for (let i = 0; i < props.scannedData.length; i++) {
      hash = ((hash << 5) - hash) + props.scannedData.charCodeAt(i)
      hash |= 0
    }
    return Math.abs(hash) % 100
  })
  
  // Ограничиваем свойства для отображения
  const visibleProperties = computed(() => {
    return props.deviceData?.properties?.slice(0, 5) || []
  })
  
  // Последняя операция
  const lastOperation = computed(() => {
    return props.deviceData?.operation_logs?.[0] || null
  })
  
  // Позиционирование свойств
  const getPropertyPosition = (index) => {
    return `-0.8 ${1.4 - (index * 0.2)} 0.01`
  }
  
  // Форматирование текста свойства
  const getPropertyText = (property) => {
    const name = property.Name || 'Свойство'
    const value = property.Value || 'Не указано'
    return `${name}: ${value}`
  }
  
  const onMarkerFound = (event) => {
    console.log('🎯 Маркер найден!', event.detail)
    markerFound.value = true
  }
  
  const onMarkerLost = (event) => {
    console.log('❌ Маркер потерян', event.detail)
    markerFound.value = false
  }
  
  const formatDate = (dateString) => {
    if (!dateString) return ''
    try {
      const date = new Date(dateString)
      return date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    } catch {
      return dateString
    }
  }
  
  const closeAR = () => {
    emit('close')
  }
  
  onMounted(() => {
    // Даем время на инициализацию AR
    setTimeout(() => {
      loading.value = false
    }, 2000)
    
    console.log('AR.js инициализирован с barcode value:', barcodeValue.value)
  })
  
  onUnmounted(() => {
    // Очистка при необходимости
  })
  </script>
  
  <style scoped>
  .ar-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: #000;
    overflow: hidden;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
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
    top: 10%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .message-content {
    background: rgba(0, 0, 0, 0.8);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    padding: 20px;
    margin: 0 20px;
    border: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .scanning-message h3,
  .found-message h3 {
    margin: 0 0 12px 0;
    font-size: 20px;
    font-weight: 600;
  }
  
  .scanning-message p,
  .found-message p {
    margin: 0 0 8px 0;
    font-size: 16px;
    color: #cccccc;
    line-height: 1.4;
  }
  
  .hint {
    font-size: 14px;
    color: #007AFF;
    font-style: italic;
    margin-top: 8px;
  }
  
  .close-button {
    position: absolute;
    bottom: 30px;
    left: 50%;
    transform: translateX(-50%);
    background: rgba(255, 255, 255, 0.95);
    color: #000;
    border: none;
    padding: 14px 28px;
    border-radius: 25px;
    cursor: pointer;
    pointer-events: auto;
    font-size: 16px;
    font-weight: 600;
    backdrop-filter: blur(10px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
    transition: all 0.2s;
  }
  
  .close-button:hover {
    background: rgba(255, 255, 255, 1);
    transform: translateX(-50%) scale(1.05);
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
    background: rgba(0, 0, 0, 0.9);
    color: white;
    z-index: 1000;
  }
  
  .spinner {
    width: 50px;
    height: 50px;
    border: 4px solid rgba(255, 255, 255, 0.3);
    border-top: 4px solid #007AFF;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  /* Адаптивность */
  @media (max-width: 768px) {
    .message-content {
      margin: 0 15px;
      padding: 16px;
    }
    
    .scanning-message h3,
    .found-message h3 {
      font-size: 18px;
    }
    
    .scanning-message p,
    .found-message p {
      font-size: 14px;
    }
  }
  </style>