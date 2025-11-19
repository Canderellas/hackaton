<!-- ArDeviceInfo.vue -->
<template>
    <div class="ar-container">
      <!-- Фон - живая камера -->
      <video 
        ref="video" 
        class="camera-background"
        playsinline
        muted
        autoplay
      ></video>
      
      <!-- Затемнение для лучшей читаемости -->
      <div class="overlay"></div>
      
      <!-- Карточка с информацией об устройстве -->
      <div class="device-info-card">
        <!-- Заголовок -->
        <div class="card-header">
          <h2 class="device-title">🔧 {{ deviceData.name_model || 'Устройство' }}</h2>
          <div class="device-type">{{ deviceData.name_type || 'Тип не указан' }}</div>
        </div>
        
        <!-- Основные свойства -->
        <div class="properties-section">
          <h3 class="section-title">Характеристики</h3>
          <div class="properties-list">
            <div 
              v-for="(property, index) in visibleProperties" 
              :key="index" 
              class="property-item"
            >
              <strong class="property-name">{{ property.name }}:</strong>
              <span class="property-value">{{ property.value }}</span>
            </div>
          </div>
        </div>
        
        <!-- Последняя операция -->
        <div v-if="lastOperation" class="operation-section">
          <h3 class="section-title">Последняя операция</h3>
          <div class="operation-info">
            <div class="operation-place">📍 {{ lastOperation.place }}</div>
            <div class="operation-date">📅 {{ formatDate(lastOperation.date) }}</div>
            <div v-if="lastOperation.comment" class="operation-comment">
              💬 {{ lastOperation.comment }}
            </div>
          </div>
        </div>
        
        <!-- Описания -->
        <div v-if="deviceData.description_model || deviceData.description_type" class="descriptions-section">
          <div v-if="deviceData.description_model" class="description-item">
            <strong>Описание модели:</strong> {{ deviceData.description_model }}
          </div>
          <div v-if="deviceData.description_type" class="description-item">
            <strong>Описание типа:</strong> {{ deviceData.description_type }}
          </div>
        </div>
      </div>
      
      <!-- Кнопка закрытия -->
      <button class="close-ar-button" @click="closeAR">
        Закрыть AR просмотр
      </button>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted, computed } from 'vue'
  
  const props = defineProps({
    deviceData: Object
  })
  
  const emit = defineEmits(['close'])
  
  const video = ref(null)
  
  // Показываем только первые 5 свойств для компактности
  const visibleProperties = computed(() => {
    return props.deviceData.properties?.slice(0, 5) || []
  })
  
  // Последняя операция
  const lastOperation = computed(() => {
    return props.deviceData.operation_logs?.[0] || null
  })
  
  onMounted(async () => {
    await startCamera()
  })
  
  const startCamera = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { 
          facingMode: 'environment',
          width: { ideal: 1920 },
          height: { ideal: 1080 }
        }
      })
      if (video.value) {
        video.value.srcObject = stream
      }
    } catch (error) {
      console.error('Ошибка доступа к камере:', error)
      // Можно показать fallback - просто чёрный фон
    }
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
    // Останавливаем камеру
    if (video.value && video.value.srcObject) {
      video.value.srcObject.getTracks().forEach(track => track.stop())
    }
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
    background: #000;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: center;
    padding: 20px;
    box-sizing: border-box;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  }
  
  /* Камера как фон */
  .camera-background {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    z-index: 1;
  }
  
  /* Затемнение для читаемости текста */
  .overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(
      to bottom,
      rgba(0, 0, 0, 0.6) 0%,
      rgba(0, 0, 0, 0.4) 30%,
      rgba(0, 0, 0, 0.3) 50%,
      rgba(0, 0, 0, 0.4) 70%,
      rgba(0, 0, 0, 0.6) 100%
    );
    z-index: 2;
  }
  
  /* Карточка с информацией */
  .device-info-card {
    position: relative;
    z-index: 3;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    padding: 24px;
    max-width: 400px;
    width: 90%;
    max-height: 70vh;
    overflow-y: auto;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    border: 1px solid rgba(255, 255, 255, 0.2);
    margin-top: 10%;
  }
  
  .card-header {
    margin-bottom: 20px;
    border-bottom: 2px solid #007aff;
    padding-bottom: 16px;
  }
  
  .device-title {
    margin: 0 0 8px 0;
    font-size: 22px;
    font-weight: 700;
    color: #1d1d1f;
    line-height: 1.2;
  }
  
  .device-type {
    font-size: 16px;
    color: #8e8e93;
    font-weight: 500;
  }
  
  .section-title {
    font-size: 16px;
    font-weight: 600;
    color: #007aff;
    margin: 0 0 12px 0;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .properties-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-bottom: 20px;
  }
  
  .property-item {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
  }
  
  .property-name {
    font-size: 15px;
    font-weight: 600;
    color: #1d1d1f;
    flex-shrink: 0;
  }
  
  .property-value {
    font-size: 15px;
    color: #48484a;
    text-align: right;
    line-height: 1.3;
  }
  
  .operation-section {
    margin-bottom: 20px;
    padding: 16px;
    background: rgba(0, 122, 255, 0.1);
    border-radius: 12px;
    border-left: 4px solid #007aff;
  }
  
  .operation-info {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .operation-place,
  .operation-date,
  .operation-comment {
    font-size: 14px;
    line-height: 1.3;
  }
  
  .operation-place {
    font-weight: 600;
    color: #1d1d1f;
  }
  
  .operation-date {
    color: #8e8e93;
  }
  
  .operation-comment {
    color: #48484a;
    font-style: italic;
  }
  
  .descriptions-section {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }
  
  .description-item {
    font-size: 14px;
    line-height: 1.4;
    color: #48484a;
  }
  
  .description-item strong {
    color: #1d1d1f;
  }
  
  /* Кнопка закрытия */
  .close-ar-button {
    position: relative;
    z-index: 3;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(20px);
    color: #1d1d1f;
    border: 2px solid rgba(255, 255, 255, 0.3);
    padding: 16px 32px;
    border-radius: 50px;
    font-size: 17px;
    font-weight: 600;
    cursor: pointer;
    margin-bottom: 5%;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    transition: all 0.2s;
  }
  
  .close-ar-button:active {
    transform: scale(0.95);
    background: rgba(255, 255, 255, 0.8);
  }
  
  /* Адаптивность */
  @media (max-width: 480px) {
    .device-info-card {
      padding: 20px;
      margin-top: 5%;
    }
    
    .device-title {
      font-size: 20px;
    }
    
    .property-item {
      flex-direction: column;
      gap: 4px;
    }
    
    .property-value {
      text-align: left;
    }
  }
  </style>