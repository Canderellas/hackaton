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
          <h3>📱 Режим просмотра</h3>
          <p>Информация об устройстве отображается в 3D пространстве</p>
          
          <div class="device-card">
            <h4>{{ deviceData?.name_model || 'Устройство' }}</h4>
            <p class="device-type">{{ deviceData?.name_type || 'Тип не указан' }}</p>
            
            <div v-if="deviceData?.properties && deviceData.properties.length > 0" class="properties">
              <div v-for="(prop, idx) in deviceData.properties.slice(0, 3)" :key="idx" class="property">
                <strong>{{ prop.Name }}:</strong> {{ prop.Value }}
              </div>
            </div>
          </div>
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
  
  // Создаем 3D сцену с информацией об устройстве
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
      
      // Камера
      const cameraElement = document.createElement('a-entity')
      cameraElement.setAttribute('camera', '')
      cameraElement.setAttribute('position', '0 1.6 0')
      cameraElement.setAttribute('look-controls', '')
      cameraElement.setAttribute('cursor', 'rayOrigin: mouse')
      
      // Основная модель устройства
      const deviceModel = document.createElement('a-entity')
      deviceModel.setAttribute('position', '0 0.5 -1.5')
      
      // === ОСНОВНАЯ ПАНЕЛЬ УСТРОЙСТВА ===
      const mainPanel = document.createElement('a-entity')
      mainPanel.setAttribute('position', '0 0.8 0')
      
      // Фон панели
      const panelBackground = document.createElement('a-box')
      panelBackground.setAttribute('color', '#1a1a1a')
      panelBackground.setAttribute('width', '1.8')
      panelBackground.setAttribute('height', '1.2')
      panelBackground.setAttribute('depth', '0.02')
      panelBackground.setAttribute('opacity', '0.95')
      
      // Заголовок - название модели
      const title = document.createElement('a-text')
      title.setAttribute('value', props.deviceData?.name_model || 'Устройство')
      title.setAttribute('align', 'center')
      title.setAttribute('color', '#FFFFFF')
      title.setAttribute('position', '0 0.4 0.01')
      title.setAttribute('width', '1.6')
      title.setAttribute('wrap-count', '20')
      title.setAttribute('baseline', 'center')
      
      // Тип устройства
      const type = document.createElement('a-text')
      type.setAttribute('value', props.deviceData?.name_type || 'Тип не указан')
      type.setAttribute('align', 'center')
      type.setAttribute('color', '#007AFF')
      type.setAttribute('position', '0 0.2 0.01')
      type.setAttribute('width', '1.6')
      type.setAttribute('wrap-count', '25')
      type.setAttribute('baseline', 'center')
      type.setAttribute('scale', '0.8 0.8 0.8')
      
      // Описание (если есть)
      if (props.deviceData?.description_model) {
        const description = document.createElement('a-text')
        description.setAttribute('value', props.deviceData.description_model)
        description.setAttribute('align', 'center')
        description.setAttribute('color', '#CCCCCC')
        description.setAttribute('position', '0 0 0.01')
        description.setAttribute('width', '1.6')
        description.setAttribute('wrap-count', '30')
        description.setAttribute('baseline', 'center')
        description.setAttribute('scale', '0.6 0.6 0.6')
        mainPanel.appendChild(description)
      }
      
      mainPanel.appendChild(panelBackground)
      mainPanel.appendChild(title)
      mainPanel.appendChild(type)
      
      // === ПАНЕЛЬ ХАРАКТЕРИСТИК ===
      if (props.deviceData?.properties && props.deviceData.properties.length > 0) {
        const propertiesPanel = document.createElement('a-entity')
        propertiesPanel.setAttribute('position', '0 -0.4 0')
        
        const propertiesBackground = document.createElement('a-box')
        propertiesBackground.setAttribute('color', '#2a2a2a')
        propertiesBackground.setAttribute('width', '1.6')
        propertiesBackground.setAttribute('height', '0.8')
        propertiesBackground.setAttribute('depth', '0.02')
        propertiesBackground.setAttribute('opacity', '0.9')
        
        const propertiesTitle = document.createElement('a-text')
        propertiesTitle.setAttribute('value', 'ХАРАКТЕРИСТИКИ:')
        propertiesTitle.setAttribute('align', 'left')
        propertiesTitle.setAttribute('color', '#007AFF')
        propertiesTitle.setAttribute('position', '-0.7 0.25 0.01')
        propertiesTitle.setAttribute('width', '1.4')
        propertiesTitle.setAttribute('scale', '0.5 0.5 0.5')
        
        propertiesPanel.appendChild(propertiesBackground)
        propertiesPanel.appendChild(propertiesTitle)
        
        // Добавляем свойства (максимум 3)
        props.deviceData.properties.slice(0, 3).forEach((prop, index) => {
          const propText = document.createElement('a-text')
          const displayText = `${prop.Name}: ${prop.Value}`
          propText.setAttribute('value', displayText.length > 25 ? displayText.substring(0, 25) + '...' : displayText)
          propText.setAttribute('align', 'left')
          propText.setAttribute('color', '#FFFFFF')
          propText.setAttribute('position', `-0.7 ${0.1 - (index * 0.15)} 0.01`)
          propText.setAttribute('width', '1.4')
          propText.setAttribute('scale', '0.4 0.4 0.4')
          propertiesPanel.appendChild(propText)
        })
        
        deviceModel.appendChild(propertiesPanel)
      }
      
      // === ИНДИКАТОР ЦЕНТРА ===
      const centerIndicator = document.createElement('a-ring')
      centerIndicator.setAttribute('color', '#007AFF')
      centerIndicator.setAttribute('radius-inner', '0.1')
      centerIndicator.setAttribute('radius-outer', '0.15')
      centerIndicator.setAttribute('position', '0 0 0')
      centerIndicator.setAttribute('opacity', '0.8')
      centerIndicator.setAttribute('animation', 'property: rotation; to: 0 0 360; loop: true; dur: 4000')
      
      // === АНИМАЦИЯ ПОЯВЛЕНИЯ ===
      deviceModel.setAttribute('animation__enter', {
        property: 'scale',
        from: '0.1 0.1 0.1',
        to: '1 1 1',
        dur: 800,
        easing: 'easeOutElastic'
      })
      
      // Собираем модель
      deviceModel.appendChild(mainPanel)
      deviceModel.appendChild(centerIndicator)
      
      // Добавляем легкое вращение для привлекательности
      deviceModel.setAttribute('animation__rotate', {
        property: 'rotation',
        to: '0 10 0',
        dur: 20000,
        loop: true,
        easing: 'easeInOutSine'
      })
      
      // Собираем сцену
      sceneElement.appendChild(cameraElement)
      sceneElement.appendChild(deviceModel)
      
      // Добавляем освещение
      const light1 = document.createElement('a-light')
      light1.setAttribute('type', 'ambient')
      light1.setAttribute('color', '#FFFFFF')
      light1.setAttribute('intensity', '0.6')
      
      const light2 = document.createElement('a-light')
      light2.setAttribute('type', 'directional')
      light2.setAttribute('color', '#FFFFFF')
      light2.setAttribute('intensity', '0.8')
      light2.setAttribute('position', '1 1 1')
      
      sceneElement.appendChild(light1)
      sceneElement.appendChild(light2)
      
      // Добавляем на страницу
      const container = document.querySelector('.ar-container')
      if (container) {
        const uiElement = container.querySelector('.ar-ui')
        container.insertBefore(sceneElement, uiElement)
      }
      
      console.log('✅ 3D сцена создана с информацией об устройстве')
      
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
    border-radius: 20px;
    padding: 20px;
    color: white;
    max-width: 400px;
    pointer-events: auto;
  }
  
  .info-panel h3 {
    margin: 0 0 8px 0;
    font-size: 20px;
    color: #007AFF;
  }
  
  .info-panel p {
    margin: 0 0 16px 0;
    opacity: 0.8;
    font-size: 14px;
    line-height: 1.4;
  }
  
  .device-card {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 12px;
    padding: 16px;
    border: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .device-card h4 {
    margin: 0 0 4px 0;
    font-size: 18px;
    color: white;
  }
  
  .device-type {
    margin: 0 0 12px 0 !important;
    color: #007AFF !important;
    font-size: 14px !important;
    font-weight: 600;
  }
  
  .properties {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .property {
    display: flex;
    justify-content: space-between;
    font-size: 12px;
    line-height: 1.3;
  }
  
  .property strong {
    color: #CCCCCC;
  }
  
  .property span {
    color: white;
    text-align: right;
    max-width: 60%;
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
      max-width: none;
    }
    
    .device-card h4 {
      font-size: 16px;
    }
  }
  </style>