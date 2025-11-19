<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- ✅ ДОБАВЛЕНО: Видео элемент для камеры -->
      <video 
        id="ar-video" 
        autoplay 
        playsinline 
        muted
        style="position: absolute; width: 100%; height: 100%; object-fit: cover; z-index: 1;"
      ></video>
      
      <!-- A-Frame сцена -->
      <div id="ar-scene"></div>
      
      <!-- Интерфейс поверх AR -->
      <div class="ar-ui">
        <div v-if="!markerVisible" class="scanning-message">
          <h3>🔍 Наведите камеру на QR-код устройства</h3>
          <p>Информация появится прямо над QR-кодом в пространстве</p>
          <div class="debug-info">
            <p>GUID: {{ currentGuid }}</p>
            <p>Barcode value: {{ barcodeValue }}</p>
            <p>Статус: {{ arStatus }}</p>
          </div>
        </div>
        
        <div v-else class="found-message">
          <h3>✅ Устройство распознано!</h3>
          <p>Информация закреплена в реальном пространстве</p>
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
  
  const markerVisible = ref(false)
  const arStatus = ref('Инициализация AR...')
  const currentGuid = ref('')
  const videoStream = ref(null)
  
  // Конвертируем GUID в barcode value
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
      
      const value = Math.abs(hash) % 1024
      arStatus.value = `AR ГОТОВ. Ищем QR-КОД C barcode: ${value}`
      return value
      
    } catch (error) {
      arStatus.value = 'Ошибка конвертации GUID'
      return 100
    }
  })
  
  // ✅ Запускаем камеру
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
      arStatus.value = 'Нет доступа к камере'
      return null
    }
  }
  
  // ✅ Создаем AR сцену
  const createARScene = async () => {
    // Ждем загрузки камеры
    const stream = await startCamera()
    if (!stream) return
  
    // Ждем загрузки A-Frame
    if (typeof AFRAME === 'undefined') {
      setTimeout(createARScene, 100)
      return
    }
  
    try {
      // Создаем сцену
      const sceneElement = document.createElement('a-scene')
      sceneElement.setAttribute('embedded', 'true')
      sceneElement.setAttribute('vr-mode-ui', 'enabled: false')
      sceneElement.setAttribute('renderer', 'antialias: true; alpha: true')
      sceneElement.setAttribute('arjs', 
        'sourceType: webcam; ' +
        'debugUIEnabled: false; ' +
        'detectionMode: mono_and_matrix; ' +
        'matrixCodeType: 3x3;'
      )
  
      // Создаем маркер
      const markerElement = document.createElement('a-marker')
      markerElement.setAttribute('type', 'barcode')
      markerElement.setAttribute('value', barcodeValue.value)
      markerElement.setAttribute('emitevents', 'true')
  
      // Создаем контент для маркера
      const contentElement = document.createElement('a-entity')
  
      // Белая панель с информацией
      const background = document.createElement('a-plane')
      background.setAttribute('color', '#FFFFFF')
      background.setAttribute('width', '1.8')
      background.setAttribute('height', '1.2')
      background.setAttribute('position', '0 1.2 0')
      background.setAttribute('opacity', '0.9')
  
      // Название устройства
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#000000')
      title.setAttribute('position', '0 1.5 0.01')
      title.setAttribute('width', '1.6')
  
      // Тип устройства
      const type = document.createElement('a-text')
      type.setAttribute('value', props.deviceData?.name_type || 'Тип не указан')
      type.setAttribute('align', 'center')
      type.setAttribute('color', '#666666')
      type.setAttribute('position', '0 1.3 0.01')
      type.setAttribute('width', '1.6')
      type.setAttribute('scale', '0.8 0.8 0.8')
  
      // Собираем структуру
      contentElement.appendChild(background)
      contentElement.appendChild(title)
      contentElement.appendChild(type)
      markerElement.appendChild(contentElement)
  
      // Камера
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', '')
      cameraElement.setAttribute('position', '0 0 0')
  
      // Собираем сцену
      sceneElement.appendChild(markerElement)
      sceneElement.appendChild(cameraElement)
  
      // Добавляем на страницу
      const container = document.querySelector('.ar-container')
      if (container) {
        // Вставляем перед интерфейсом
        const uiElement = container.querySelector('.ar-ui')
        container.insertBefore(sceneElement, uiElement)
      }
  
      // Обработчики событий
      markerElement.addEventListener('markerFound', () => {
        markerVisible.value = true
        arStatus.value = '✅ Устройство распознано!'
      })
  
      markerElement.addEventListener('markerLost', () => {
        markerVisible.value = false
        arStatus.value = 'Поиск QR-кода...'
      })
  
      console.log('AR сцена создана, barcode:', barcodeValue.value)
  
    } catch (error) {
      console.error('Ошибка создания AR сцены:', error)
      arStatus.value = 'Ошибка AR: ' + error.message
    }
  }
  
  const closeAR = () => {
    // Останавливаем камеру
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
    
    // Удаляем сцену
    const scene = document.querySelector('a-scene')
    if (scene) scene.remove()
    
    // Удаляем видео элемент
    const video = document.getElementById('ar-video')
    if (video) video.remove()
    
    emit('close')
  }
  
  onMounted(() => {
    createARScene()
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
  
  /* Интерфейс поверх AR */
  .ar-ui {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 3;
  }
  
  /* Остальные стили без изменений */
  .scanning-message,
  .found-message {
    position: absolute;
    top: 8%;
    left: 0;
    width: 100%;
    text-align: center;
    color: white;
    pointer-events: none;
  }
  
  .scanning-message h3,
  .found-message h3 {
    margin: 0 0 12px 0;
    font-size: 20px;
    background: rgba(0, 0, 0, 0.8);
    display: inline-block;
    padding: 12px 24px;
    border-radius: 20px;
    backdrop-filter: blur(10px);
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