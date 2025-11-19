<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- Видео элемент для камеры -->
      <video 
        id="ar-video" 
        autoplay 
        playsinline 
        muted
        style="position: absolute; width: 100%; height: 100%; object-fit: cover; z-index: 1;"
      ></video>
      
      <!-- A-Frame сцена -->
      <div id="ar-scene"></div>
      
      <!-- Интерфейс -->
      <div class="ar-ui">
        <div class="test-message">
          <h3>🧪 ТЕСТОВЫЙ РЕЖИМ</h3>
          <p>3D модель должна быть видна в пространстве</p>
          <div class="debug-info">
            <p>GUID: {{ currentGuid }}</p>
            <p>Barcode: {{ barcodeValue }}</p>
            <p>Статус: {{ arStatus }}</p>
          </div>
        </div>
  
        <button @click="closeAR" class="close-button">
          Закрыть AR
        </button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted, onUnmounted } from 'vue'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const arStatus = ref('Запуск теста...')
  const currentGuid = ref('')
  const videoStream = ref(null)
  
  // Вычисляем barcode (оставляем для отладки)
  const barcodeValue = computed(() => {
    const guid = props.scannedData
    if (!guid) return 100
    
    try {
      const guidMatch = guid.match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i)
      const cleanGuid = guidMatch ? guidMatch[0] : guid
      currentGuid.value = cleanGuid
      
      let hash = 0
      for (let i = 0; i < cleanGuid.length; i++) {
        hash = ((hash << 5) - hash) + cleanGuid.charCodeAt(i)
        hash |= 0
      }
      
      return Math.abs(hash) % 1024
    } catch {
      return 100
    }
  })
  
  // Запускаем камеру
  const startCamera = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { 
          facingMode: 'environment',
          width: { ideal: 1280 },
          height: { ideal: 720 }
        }
      })
      
      const videoElement = document.getElementById('ar-video')
      if (videoElement) {
        videoElement.srcObject = stream
        videoStream.value = stream
      }
      
      return stream
    } catch (error) {
      console.error('Ошибка камеры:', error)
      arStatus.value = '❌ Нет доступа к камере'
      return null
    }
  }
  
  // Создаем тестовую 3D сцену
  const createTestScene = async () => {
    // Запускаем камеру
    const stream = await startCamera()
    if (!stream) return
  
    // Ждем загрузки A-Frame
    if (typeof AFRAME === 'undefined') {
      arStatus.value = '⏳ Загрузка AR...'
      setTimeout(createTestScene, 500)
      return
    }
  
    try {
      // Создаем простую сцену БЕЗ AR.js
      const sceneElement = document.createElement('a-scene')
      sceneElement.setAttribute('embedded', 'true')
      sceneElement.setAttribute('vr-mode-ui', 'enabled: false')
      
      // ✅ ПРОСТАЯ КАМЕРА (не AR)
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', '')
      cameraElement.setAttribute('position', '0 1.6 0')
      cameraElement.setAttribute('look-controls', '')
      
      // ✅ ТЕСТОВАЯ 3D МОДЕЛЬ - появляется сразу!
      const testModel = document.createElement('a-entity')
      testModel.setAttribute('position', '0 0.5 -2') // Перед камерой
      
      // Белая панель с информацией
      const panel = document.createElement('a-box')
      panel.setAttribute('color', '#007AFF')
      panel.setAttribute('width', '1.5')
      panel.setAttribute('height', '1.0')
      panel.setAttribute('depth', '0.1')
      panel.setAttribute('position', '0 0.8 0')
      
      // Текст с названием устройства
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Тестовое устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#FFFFFF')
      title.setAttribute('position', '0 0.8 0.06')
      title.setAttribute('width', '1.4')
      
      // Тип устройства
      const type = document.createElement('a-text')
      type.setAttribute('value', props.deviceData?.name_type || 'Тестовый тип')
      type.setAttribute('align', 'center')
      type.setAttribute('color', '#CCCCCC')
      type.setAttribute('position', '0 0.6 0.06')
      type.setAttribute('width', '1.4')
      type.setAttribute('scale', '0.8 0.8 0.8')
      
      // Вращающаяся сфера для наглядности
      const sphere = document.createElement('a-sphere')
      sphere.setAttribute('color', '#FF3B30')
      sphere.setAttribute('radius', '0.3')
      sphere.setAttribute('position', '0 0 0')
      sphere.setAttribute('animation', 'property: rotation; to: 0 360 0; loop: true; dur: 5000')
      
      // Собираем модель
      testModel.appendChild(panel)
      testModel.appendChild(title)
      testModel.appendChild(type)
      testModel.appendChild(sphere)
      
      // Добавляем вращение всей модели
      testModel.setAttribute('animation', 'property: rotation; to: 0 360 0; loop: true; dur: 10000')
      
      // Собираем сцену
      sceneElement.appendChild(cameraElement)
      sceneElement.appendChild(testModel)
      
      // Добавляем на страницу
      const container = document.querySelector('.ar-container')
      if (container) {
        const uiElement = container.querySelector('.ar-ui')
        container.insertBefore(sceneElement, uiElement)
      }
      
      arStatus.value = '✅ 3D модель загружена! Должна быть видна синяя панель'
      console.log('✅ Тестовая сцена создана')
      
      // Сохраняем для очистки
      window.testScene = sceneElement
  
    } catch (error) {
      console.error('Ошибка создания тестовой сцены:', error)
      arStatus.value = '❌ Ошибка: ' + error.message
    }
  }
  
  const closeAR = () => {
    // Останавливаем камеру
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
    
    // Удаляем сцену
    if (window.testScene) {
      window.testScene.remove()
    }
    
    // Удаляем видео элемент
    const video = document.getElementById('ar-video')
    if (video) video.remove()
    
    emit('close')
  }
  
  onMounted(() => {
    arStatus.value = '🚀 Запуск теста...'
    createTestScene()
  })
  
  onUnmounted(() => {
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
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
    overflow: hidden;
    z-index: 1000;
  }
  
  /* A-Frame сцена */
  .ar-container ::v-deep(a-scene) {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 2;
  }
  
  /* Интерфейс */
  .ar-ui {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 3;
  }
  
  .test-message {
    position: absolute;
    top: 8%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .test-message h3 {
    margin: 0 0 12px 0;
    font-size: 20px;
    background: rgba(255, 59, 48, 0.9);
    display: inline-block;
    padding: 12px 24px;
    border-radius: 20px;
    backdrop-filter: blur(10px);
  }
  
  .test-message p {
    margin: 0 0 8px 0;
    font-size: 16px;
    color: #cccccc;
    background: rgba(0, 0, 0, 0.6);
    display: inline-block;
    padding: 8px 16px;
    border-radius: 12px;
    backdrop-filter: blur(10px);
  }
  
  .debug-info {
    margin-top: 16px;
    background: rgba(0, 0, 0, 0.7);
    padding: 12px 16px;
    border-radius: 12px;
    display: inline-block;
    text-align: left;
  }
  
  .debug-info p {
    margin: 4px 0;
    font-size: 12px;
    color: #00ff00;
    font-family: 'Courier New', monospace;
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
    z-index: 4;
  }
  </style>