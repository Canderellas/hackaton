<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- Всегда показываем интерфейс поверх AR -->
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
          <div class="device-info">
            <strong>{{ deviceData?.name_model }}</strong>
            <br>
            {{ deviceData?.name_type }}
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
  
  const markerVisible = ref(false)
  const arStatus = ref('Инициализация AR...')
  const currentGuid = ref('')
  
  // Извлекаем GUID из scannedData
  const extractedGuid = computed(() => {
    if (!props.scannedData) return null
    
    try {
      // Пытаемся найти GUID в строке
      const guidMatch = props.scannedData.match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i)
      return guidMatch ? guidMatch[0] : props.scannedData
    } catch {
      return props.scannedData
    }
  })
  
  // Конвертируем GUID в barcode value (0-1023)
  const barcodeValue = computed(() => {
    const guid = extractedGuid.value
    if (!guid) {
      arStatus.value = 'GUID не найден'
      return 100 // дефолтное значение
    }
    
    currentGuid.value = guid
    
    try {
      // Создаем стабильный хэш из GUID
      let hash = 0
      for (let i = 0; i < guid.length; i++) {
        hash = ((hash << 5) - hash) + guid.charCodeAt(i)
        hash |= 0 // Convert to 32bit integer
      }
      
      const value = Math.abs(hash) % 1024 // AR.js поддерживает 0-1023
      arStatus.value = `GUID: ${guid.slice(0, 8)}... → Barcode: ${value}`
      return value
      
    } catch (error) {
      arStatus.value = 'Ошибка конвертации GUID'
      return 100
    }
  })
  
  onMounted(() => {
    initializeAR()
  })
  
  onUnmounted(() => {
    cleanupAR()
  })
  
  const initializeAR = () => {
    // Даем время на вычисления перед созданием сцены
    setTimeout(() => {
      createARScene()
    }, 100)
  }
  
  const createARScene = () => {
    try {
      // Создаем элемент сцены
      const sceneElement = document.createElement('a-scene')
      sceneElement.setAttribute('embedded', 'true')
      sceneElement.setAttribute('vr-mode-ui', 'enabled: false')
      sceneElement.setAttribute('renderer', 'antialias: true; alpha: true')
      sceneElement.setAttribute('arjs', 'sourceType: webcam; debugUIEnabled: false; detectionMode: mono_and_matrix; matrixCodeType: 3x3;')
      
      // Создаем маркер с вычисленным barcode value
      const markerElement = document.createElement('a-marker')
      markerElement.setAttribute('type', 'barcode')
      markerElement.setAttribute('value', barcodeValue.value)
      markerElement.setAttribute('emitevents', 'true')
      markerElement.setAttribute('cursor', 'rayOrigin: mouse')
      
      // Создаем 3D контент для маркера
      const contentElement = document.createElement('a-entity')
      
      // Фон информации (белая панель)
      const background = document.createElement('a-plane')
      background.setAttribute('color', '#FFFFFF')
      background.setAttribute('width', '2.0')
      background.setAttribute('height', '1.5')
      background.setAttribute('position', '0 1.5 0')
      background.setAttribute('opacity', '0.95')
      background.setAttribute('material', 'shader: flat;')
      
      // Заголовок - название модели
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#000000')
      title.setAttribute('position', '0 2.0 0.01')
      title.setAttribute('width', '1.8')
      title.setAttribute('wrap-count', '15')
      
      // Тип устройства
      const type = document.createElement('a-text')
      type.setAttribute('value', props.deviceData?.name_type || 'Тип не указан')
      type.setAttribute('align', 'center')
      type.setAttribute('color', '#666666')
      type.setAttribute('position', '0 1.7 0.01')
      type.setAttribute('width', '1.6')
      type.setAttribute('wrap-count', '15')
      type.setAttribute('scale', '0.8 0.8 0.8')
      
      // Собираем структуру
      contentElement.appendChild(background)
      contentElement.appendChild(title)
      contentElement.appendChild(type)
      
      // Добавляем свойства устройства если есть
      if (props.deviceData?.properties && props.deviceData.properties.length > 0) {
        const propertiesTitle = document.createElement('a-text')
        propertiesTitle.setAttribute('value', 'Характеристики:')
        propertiesTitle.setAttribute('align', 'left')
        propertiesTitle.setAttribute('color', '#007AFF')
        propertiesTitle.setAttribute('position', '-0.9 1.4 0.01')
        propertiesTitle.setAttribute('width', '1.6')
        propertiesTitle.setAttribute('scale', '0.7 0.7 0.7')
        contentElement.appendChild(propertiesTitle)
        
        props.deviceData.properties.slice(0, 4).forEach((prop, index) => {
          const propElement = document.createElement('a-text')
          const displayText = `${prop.Name}: ${prop.Value}`.substring(0, 25) // Ограничиваем длину
          propElement.setAttribute('value', displayText)
          propElement.setAttribute('align', 'left')
          propElement.setAttribute('color', '#000000')
          propElement.setAttribute('position', `-0.9 ${1.2 - (index * 0.18)} 0.01`)
          propElement.setAttribute('width', '1.6')
          propElement.setAttribute('scale', '0.6 0.6 0.6')
          contentElement.appendChild(propElement)
        })
      }
      
      // Добавляем индикатор местоположения
      const indicator = document.createElement('a-ring')
      indicator.setAttribute('color', '#007AFF')
      indicator.setAttribute('radius-inner', '0.15')
      indicator.setAttribute('radius-outer', '0.25')
      indicator.setAttribute('position', '0 0.5 0')
      indicator.setAttribute('opacity', '0.8')
      contentElement.appendChild(indicator)
      
      markerElement.appendChild(contentElement)
      
      // Создаем камеру
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', '')
      
      // Собираем сцену
      sceneElement.appendChild(markerElement)
      sceneElement.appendChild(cameraElement)
      
      // Добавляем на страницу
      const container = document.querySelector('.ar-container')
      if (container) {
        container.appendChild(sceneElement)
      }
      
      // Сохраняем ссылку для очистки
      window.ARScene = sceneElement
      
      // Обработчики событий маркера
      markerElement.addEventListener('markerFound', (event) => {
        console.log('🎯 Маркер найден!', event.detail)
        markerVisible.value = true
        arStatus.value = `Устройство распознано (Barcode: ${barcodeValue.value})`
      })
      
      markerElement.addEventListener('markerLost', (event) => {
        console.log('❌ Маркер потерян', event.detail)
        markerVisible.value = false
        arStatus.value = 'Поиск QR-кода устройства...'
      })
      
      arStatus.value = `AR готов. Ищем QR-код с barcode: ${barcodeValue.value}`
      
      console.log('AR сцена создана с barcode value:', barcodeValue.value)
      
    } catch (error) {
      console.error('Ошибка создания AR сцены:', error)
      arStatus.value = 'Ошибка инициализации AR: ' + error.message
    }
  }
  
  const cleanupAR = () => {
    if (window.ARScene) {
      window.ARScene.remove()
      window.ARScene = null
    }
  }
  
  const closeAR = () => {
    cleanupAR()
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
    z-index: 1000;
  }
  
  /* AR сцена займет весь контейнер */
  .ar-container ::v-deep(a-scene) {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    left: 0;
    z-index: 1;
  }
  
  /* Интерфейс поверх AR */
  .ar-ui {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 2;
  }
  
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
    border: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .scanning-message p,
  .found-message p {
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
  
  .device-info {
    margin-top: 12px;
    background: rgba(0, 122, 255, 0.2);
    padding: 10px 16px;
    border-radius: 10px;
    display: inline-block;
    border: 1px solid rgba(0, 122, 255, 0.3);
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
    z-index: 3;
  }
  
  .close-button:hover {
    background: rgba(255, 255, 255, 1);
    transform: translateX(-50%) scale(1.05);
  }
  
  /* Адаптивность */
  @media (max-width: 768px) {
    .scanning-message h3,
    .found-message h3 {
      font-size: 18px;
      padding: 10px 20px;
    }
    
    .scanning-message p,
    .found-message p {
      font-size: 14px;
    }
    
    .debug-info {
      padding: 10px 12px;
      margin: 12px 10px;
    }
    
    .debug-info p {
      font-size: 11px;
    }
  }
  </style>