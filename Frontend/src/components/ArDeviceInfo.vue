<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <div v-if="!arInitialized" class="ar-setup">
        <h2>AR Просмотр</h2>
        <p>Данные устройства: {{ deviceData?.name_model }}</p>
        <button @click="initializeAR" class="start-ar-button">
          🚀 Запустить AR
        </button>
        <button @click="closeAR" class="close-button">
          Закрыть
        </button>
      </div>
  
      <div v-else class="ar-scene-container">
        <!-- AR сцена будет здесь -->
        <a-scene 
          embedded 
          vr-mode-ui="enabled: false"
          arjs="sourceType: webcam; debugUIEnabled: false;"
        >
          <a-marker type="barcode" :value="barcodeValue">
            <a-box position="0 0.5 0" material="color: blue;"></a-box>
          </a-marker>
          <a-entity camera></a-entity>
        </a-scene>
        
        <button @click="closeAR" class="close-button">
          Закрыть AR
        </button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted } from 'vue'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const arInitialized = ref(false)
  
  const barcodeValue = computed(() => {
    if (!props.scannedData) return 0
    let hash = 0
    for (let i = 0; i < props.scannedData.length; i++) {
      hash = ((hash << 5) - hash) + props.scannedData.charCodeAt(i)
      hash |= 0
    }
    return Math.abs(hash) % 100
  })
  
  const initializeAR = () => {
    arInitialized.value = true
    // Динамически импортируем AR.js чтобы не блокировать загрузку
    import('aframe')
    import('@ar-js-org/ar.js')
  }
  
  const closeAR = () => {
    emit('close')
  }
  
  onMounted(() => {
    console.log('AR компонент загружен')
  })
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
  }
  
  .ar-setup {
    background: white;
    padding: 30px;
    border-radius: 20px;
    text-align: center;
    max-width: 400px;
  }
  
  .start-ar-button {
    background: #007aff;
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 10px;
    cursor: pointer;
    font-size: 16px;
    margin: 10px;
  }
  
  .close-button {
    background: #8e8e93;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 10px;
    cursor: pointer;
    margin: 10px;
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
  </style>