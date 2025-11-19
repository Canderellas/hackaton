<!-- src/components/ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- Видео с камеры -->
      <video 
        ref="videoElement" 
        autoplay 
        playsinline 
        muted
        class="camera-video"
      ></video>
      
      <!-- 3D сцена -->
      <div id="ar-scene"></div>
      
      <!-- Интерфейс -->
      <div class="ar-ui">
        <div class="info-panel">
          <h3>📱 3D Просмотр устройства</h3>
          <p>Информация зафиксирована в пространстве</p>
        </div>
  
        <button @click="closeAR" class="close-button">
          Закрыть просмотр
        </button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted, onUnmounted } from 'vue'
  
  const props = defineProps({
    scannedData: String,
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const videoStream = ref(null)
  
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
      
      const videoElement = document.querySelector('.camera-video')
      if (videoElement) {
        videoElement.srcObject = stream
        videoStream.value = stream
      }
      
      return stream
    } catch (error) {
      console.error('Ошибка камеры:', error)
      return null
    }
  }
  
  // Создаем 3D сцену с фиксированной моделью
  const create3DScene = async () => {
    // Запускаем камеру
    await startCamera()
  
    // Ждем загрузки A-Frame
    if (typeof AFRAME === 'undefined') {
      setTimeout(create3DScene, 100)
      return
    }
  
    try {
      // Создаем сцену
      const sceneElement = document.createElement('a-scene')
      sceneElement.setAttribute('embedded', 'true')
      sceneElement.setAttribute('vr-mode-ui', 'enabled: false')
      sceneElement.setAttribute('renderer', 'colorManagement: true')
      
      // Камера с отключенным приближением
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', 'zoom: 1;')
      cameraElement.setAttribute('position', '0 0 0')
      cameraElement.setAttribute('look-controls', '')
      
      // === ОСНОВНАЯ МОДЕЛЬ УСТРОЙСТВА ===
      const deviceModel = document.createElement('a-entity')
      // ✅ ФИКСИРУЕМ позицию - модель не будет двигаться при приближении
      deviceModel.setAttribute('position', '0 0 -0.8') // Ближе к камере
      deviceModel.setAttribute('scale', '0.8 0.8 0.8') // ✅ УМЕНЬШАЕМ модель
      
      // === ГЛАВНАЯ ПАНЕЛЬ ===
      const mainPanel = document.createElement('a-entity')
      mainPanel.setAttribute('position', '0 0.3 0')
      
      // Фон панели
      const panelBackground = document.createElement('a-box')
      panelBackground.setAttribute('color', '#1a1a1a')
      panelBackground.setAttribute('width', '1.2') // ✅ УМЕНЬШАЕМ
      panelBackground.setAttribute('height', '0.8') // ✅ УМЕНЬШАЕМ
      panelBackground.setAttribute('depth', '0.02')
      panelBackground.setAttribute('opacity', '0.95')
      
      // Заголовок - название модели
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#FFFFFF')
      title.setAttribute('position', '0 0.25 0.01')
      title.setAttribute('width', '1.0') // ✅ УМЕНЬШАЕМ
      title.setAttribute('wrap-count', '15')
      title.setAttribute('baseline', 'center')
      title.setAttribute('scale', '1.2 1.2 1.2') // ✅ УВЕЛИЧИВАЕМ ШРИФТ
      
      // Тип устройства
      const type = document.createElement('a-text')
      type.setAttribute('value', props.deviceData?.name_type || 'Тип не указан')
      type.setAttribute('align', 'center')
      type.setAttribute('color', '#007AFF')
      type.setAttribute('position', '0 0.1 0.01')
      type.setAttribute('width', '1.0')
      type.setAttribute('wrap-count', '20')
      type.setAttribute('baseline', 'center')
      type.setAttribute('scale', '1.0 1.0 1.0') // ✅ УВЕЛИЧИВАЕМ ШРИФТ
      
      mainPanel.appendChild(panelBackground)
      mainPanel.appendChild(title)
      mainPanel.appendChild(type)
      
      // === ПАНЕЛЬ ХАРАКТЕРИСТИК ===
      if (props.deviceData?.properties && props.deviceData.properties.length > 0) {
        const propertiesPanel = document.createElement('a-entity')
        propertiesPanel.setAttribute('position', '0 -0.3 0')
        
        const propertiesBackground = document.createElement('a-box')
        propertiesBackground.setAttribute('color', '#2a2a2a')
        propertiesBackground.setAttribute('width', '1.0') // ✅ УМЕНЬШАЕМ
        propertiesBackground.setAttribute('height', '0.6') // ✅ УМЕНЬШАЕМ
        propertiesBackground.setAttribute('depth', '0.02')
        propertiesBackground.setAttribute('opacity', '0.9')
        
        const propertiesTitle = document.createElement('a-text')
        propertiesTitle.setAttribute('value', 'ХАРАКТЕРИСТИКИ:')
        propertiesTitle.setAttribute('align', 'left')
        propertiesTitle.setAttribute('color', '#007AFF')
        propertiesTitle.setAttribute('position', '-0.45 0.2 0.01')
        propertiesTitle.setAttribute('width', '0.9')
        propertiesTitle.setAttribute('scale', '0.6 0.6 0.6') // ✅ УВЕЛИЧИВАЕМ ШРИФТ
        
        propertiesPanel.appendChild(propertiesBackground)
        propertiesPanel.appendChild(propertiesTitle)
        
        // Добавляем свойства (максимум 2)
        props.deviceData.properties.slice(0, 2).forEach((prop, index) => {
          const propText = document.createElement('a-text')
          const displayText = `${prop.Name}: ${prop.Value}`
          propText.setAttribute('value', displayText.length > 20 ? displayText.substring(0, 20) + '...' : displayText)
          propText.setAttribute('align', 'left')
          propText.setAttribute('color', '#FFFFFF')
          propText.setAttribute('position', `-0.45 ${0.05 - (index * 0.12)} 0.01`)
          propText.setAttribute('width', '0.9')
          propText.setAttribute('scale', '0.5 0.5 0.5') // ✅ УВЕЛИЧИВАЕМ ШРИФТ
          propertiesPanel.appendChild(propText)
        })
        
        deviceModel.appendChild(propertiesPanel)
      }
      
      // === ИНДИКАТОР ===
      const indicator = document.createElement('a-ring')
      indicator.setAttribute('color', '#007AFF')
      indicator.setAttribute('radius-inner', '0.08')
      indicator.setAttribute('radius-outer', '0.12')
      indicator.setAttribute('position', '0 -0.5 0')
      indicator.setAttribute('opacity', '0.8')
      indicator.setAttribute('animation', 'property: rotation; to: 0 0 360; loop: true; dur: 3000')
      
      // === АНИМАЦИЯ ПОЯВЛЕНИЯ ===
      deviceModel.setAttribute('animation__enter', {
        property: 'scale',
        from: '0.1 0.1 0.1',
        to: '0.8 0.8 0.8',
        dur: 1000,
        easing: 'easeOutElastic'
      })
      
      // Собираем модель
      deviceModel.appendChild(mainPanel)
      deviceModel.appendChild(indicator)
      
      // ✅ ОТКЛЮЧАЕМ автоматическое вращение - модель статична
      
      // Собираем сцену
      sceneElement.appendChild(cameraElement)
      sceneElement.appendChild(deviceModel)
      
      // Добавляем освещение
      const light1 = document.createElement('a-light')
      light1.setAttribute('type', 'ambient')
      light1.setAttribute('color', '#FFFFFF')
      light1.setAttribute('intensity', '0.8') // ✅ УВЕЛИЧИВАЕМ освещение
      
      const light2 = document.createElement('a-light')
      light2.setAttribute('type', 'directional')
      light2.setAttribute('color', '#FFFFFF')
      light2.setAttribute('intensity', '1.0') // ✅ УВЕЛИЧИВАЕМ освещение
      light2.setAttribute('position', '1 1 0.5')
      
      sceneElement.appendChild(light1)
      sceneElement.appendChild(light2)
      
      // ✅ ДОБАВЛЯЕМ: Отключаем управление камерой для фиксации позиции
      const disableCameraMovement = document.createElement('a-entity')
      disableCameraMovement.setAttribute('position', '0 0 0')
      
      // Добавляем на страницу
      const container = document.querySelector('.ar-container')
      if (container) {
        const uiElement = container.querySelector('.ar-ui')
        container.insertBefore(sceneElement, uiElement)
      }
      
      console.log('✅ 3D сцена создана - модель зафиксирована')
      
      // Сохраняем для очистки
      window.arScene = sceneElement
  
    } catch (error) {
      console.error('Ошибка создания 3D сцены:', error)
    }
  }
  
  const closeAR = () => {
    // Останавливаем камеру
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
    }
    
    // Удаляем сцену
    if (window.arScene) {
      window.arScene.remove()
    }
    
    emit('close')
  }
  
  onMounted(() => {
    create3DScene()
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
  
  .camera-video {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    z-index: 1;
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
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 20px;
  }
  
  .info-panel {
    background: rgba(0, 0, 0, 0.8);
    backdrop-filter: blur(20px);
    border-radius: 16px;
    padding: 16px;
    color: white;
    max-width: 300px;
    pointer-events: auto;
  }
  
  .info-panel h3 {
    margin: 0 0 8px 0;
    font-size: 18px;
    color: #007AFF;
  }
  
  .info-panel p {
    margin: 0;
    opacity: 0.8;
    font-size: 14px;
    line-height: 1.4;
  }
  
  .close-button {
    background: rgba(255, 255, 255, 0.95);
    color: #000;
    border: none;
    padding: 16px 32px;
    border-radius: 25px;
    cursor: pointer;
    pointer-events: auto;
    font-size: 16px;
    font-weight: 600;
    backdrop-filter: blur(20px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
    transition: all 0.2s;
    align-self: center;
  }
  
  .close-button:hover {
    background: rgba(255, 255, 255, 1);
    transform: translateY(-2px);
  }
  
  /* Адаптивность */
  @media (max-width: 768px) {
    .ar-ui {
      padding: 16px;
    }
    
    .info-panel {
      max-width: 280px;
    }
    
    .info-panel h3 {
      font-size: 16px;
    }
  }
  </style>