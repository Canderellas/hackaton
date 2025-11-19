<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- Видео с камеры -->
      <video 
        ref="videoElement" 
        autoplay 
        playsinline 
        muted
        style="position: absolute; width: 100%; height: 100%; object-fit: cover; z-index: 1;"
      ></video>
      
      <!-- 3D сцена -->
      <div id="ar-scene"></div>
      
      <!-- Интерфейс -->
      <div class="ar-ui">
        <div class="tracking-message">
          <h3>🎯 Режим отслеживания QR-кода</h3>
          <p>3D модель следует за QR-кодом в реальном времени</p>
          <div class="debug-info">
            <p>GUID: {{ currentGuid }}</p>
            <p>Статус: {{ arStatus }}</p>
            <p v-if="trackingPosition">Позиция: {{ trackingPosition.x }}, {{ trackingPosition.y }}</p>
          </div>
        </div>
  
        <button @click="closeAR" class="close-button">
          Закрыть AR
        </button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
  import jsQR from 'jsqr'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object,
    qrLocation: Object,
    videoElement: HTMLVideoElement
  })
  
  const emit = defineEmits(['close'])
  
  const arStatus = ref('Инициализация отслеживания...')
  const currentGuid = ref('')
  const videoStream = ref(null)
  const trackingPosition = ref(null)
  const model3D = ref(null)
  const trackingActive = ref(false)
  
  // Canvas для анализа видео
  const canvas = document.createElement('canvas')
  const ctx = canvas.getContext('2d')
  
  // Извлекаем GUID
  const extractedGuid = computed(() => {
    if (!props.scannedData) return null
    try {
      const guidMatch = props.scannedData.match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i)
      return guidMatch ? guidMatch[0] : props.scannedData
    } catch {
      return props.scannedData
    }
  })
  
  // Запускаем отслеживание QR-кода
  const startQRTracking = async () => {
    if (!props.videoElement) {
      arStatus.value = '❌ Видео элемент не доступен'
      return
    }
  
    // Настраиваем canvas под размер видео
    canvas.width = props.videoElement.videoWidth
    canvas.height = props.videoElement.videoHeight
  
    trackingActive.value = true
    arStatus.value = '🎯 Начинаю отслеживание QR-кода...'
    
    trackQRCode()
  }
  
  // Функция отслеживания QR-кода
  const trackQRCode = () => {
    if (!trackingActive.value) return
  
    requestAnimationFrame(() => {
      try {
        // Рисуем текущий кадр видео на canvas
        ctx.drawImage(props.videoElement, 0, 0, canvas.width, canvas.height)
        const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height)
        
        // Ищем QR-код
        const code = jsQR(imageData.data, imageData.width, imageData.height, {
          inversionAttempts: 'invertFirst'
        })
  
        if (code && code.data === props.scannedData) {
          // ✅ Нашли наш QR-код!
          const centerX = (code.location.topLeftCorner.x + code.location.bottomRightCorner.x) / 2
          const centerY = (code.location.topLeftCorner.y + code.location.bottomRightCorner.y) / 2
          
          trackingPosition.value = { x: centerX, y: centerY }
          updateModelPosition(centerX, centerY)
          arStatus.value = '✅ QR-код отслеживается'
          
        } else {
          // QR-код не найден
          trackingPosition.value = null
          hideModel()
          arStatus.value = '🔍 Поиск QR-кода...'
        }
      } catch (error) {
        console.error('Ошибка отслеживания:', error)
      }
  
      // Продолжаем отслеживание
      if (trackingActive.value) {
        setTimeout(trackQRCode, 100) // 10 FPS для производительности
      }
    })
  }
  
  // Создаем 3D сцену
  const create3DScene = () => {
    if (typeof AFRAME === 'undefined') {
      setTimeout(create3DScene, 100)
      return
    }
  
    try {
      const sceneElement = document.createElement('a-scene')
      sceneElement.setAttribute('embedded', 'true')
      sceneElement.setAttribute('vr-mode-ui', 'enabled: false')
      
      // Камера
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', '')
      cameraElement.setAttribute('position', '0 0 0')
      
      // 3D модель устройства (изначально скрыта)
      model3D.value = document.createElement('a-entity')
      model3D.value.setAttribute('id', 'device-model')
      model3D.value.setAttribute('visible', 'false')
      
      // Содержимое модели
      const panel = document.createElement('a-box')
      panel.setAttribute('color', '#007AFF')
      panel.setAttribute('width', '0.8')
      panel.setAttribute('height', '0.6')
      panel.setAttribute('depth', '0.05')
      panel.setAttribute('position', '0 0.3 0')
      
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#FFFFFF')
      title.setAttribute('position', '0 0.3 0.03')
      title.setAttribute('width', '0.7')
      title.setAttribute('scale', '0.8 0.8 0.8')
      
      model3D.value.appendChild(panel)
      model3D.value.appendChild(title)
      
      sceneElement.appendChild(cameraElement)
      sceneElement.appendChild(model3D.value)
      
      const container = document.querySelector('.ar-container')
      if (container) {
        const uiElement = container.querySelector('.ar-ui')
        container.insertBefore(sceneElement, uiElement)
      }
      
      window.arScene = sceneElement
      arStatus.value = '✅ 3D сцена готова. Начинаю отслеживание...'
      
    } catch (error) {
      console.error('Ошибка создания 3D сцены:', error)
      arStatus.value = '❌ Ошибка 3D сцены'
    }
  }
  
  // Обновляем позицию 3D модели относительно QR-кода
  const updateModelPosition = (qrX, qrY) => {
    if (!model3D.value) return
    
    // Конвертируем координаты экрана в 3D пространство
    const normalizedX = (qrX / canvas.width) * 2 - 1
    const normalizedY = -(qrY / canvas.height) * 2 + 1
    
    // Позиционируем модель перед камерой
    const distance = 1.5 // 1.5 метра от камеры
    
    // Простая проекция - модель следует за QR-кодом
    model3D.value.setAttribute('position', `${normalizedX * 2} ${normalizedY * 1.5} -${distance}`)
    model3D.value.setAttribute('visible', 'true')
  }
  
  // Скрываем модель когда QR-код не виден
  const hideModel = () => {
    if (model3D.value) {
      model3D.value.setAttribute('visible', 'false')
    }
  }
  
  const closeAR = () => {
    trackingActive.value = false
    
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
    
    if (window.arScene) {
      window.arScene.remove()
    }
    
    emit('close')
  }
  
  onMounted(() => {
    currentGuid.value = extractedGuid.value
    create3DScene()
    startQRTracking()
  })
  
  onUnmounted(() => {
    trackingActive.value = false
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
  })
  </script>
  
  <style scoped>
  /* Стили остаются такими же как в предыдущей версии */
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
  
  .ar-container ::v-deep(a-scene) {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 2;
  }
  
  .ar-ui {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 3;
  }
  
  .tracking-message {
    position: absolute;
    top: 8%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .tracking-message h3 {
    margin: 0 0 12px 0;
    font-size: 20px;
    background: rgba(0, 122, 255, 0.9);
    display: inline-block;
    padding: 12px 24px;
    border-radius: 20px;
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