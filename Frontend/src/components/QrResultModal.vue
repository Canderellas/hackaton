<!-- QrResultModal.vue -->
<script setup>
import { ref, onMounted, watch } from 'vue'

const props = defineProps({
  scannedData: String,
  frameImage: String
})

const emit = defineEmits(['close'])

const deviceData = ref(null)
const loading = ref(false)
const error = ref('')
const notFound = ref(false)

// Функция для извлечения ID из QR-кода
const extractDeviceId = (data) => {
  if (!data) return null
  
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(data)) {
    return data
  }
  
  try {
    const url = new URL(data)
    const pathParts = url.pathname.split('/')
    const id = pathParts.find(part => 
      /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(part)
    )
    return id || data
  } catch {
    return data
  }
}

// Функция загрузки данных устройства
const fetchDeviceData = async () => {
  if (!props.scannedData) {
    error.value = 'Нет данных для загрузки'
    return
  }

  loading.value = true
  error.value = ''
  notFound.value = false
  deviceData.value = null

  try {
    const deviceId = extractDeviceId(props.scannedData)
    
    if (!deviceId) {
      throw new Error('Не удалось извлечь ID устройства')
    }
    
    const response = await fetch(`https://comunada.store/api/device/${deviceId}`)
    
    if (response.status === 404) {
      notFound.value = true
      return
    }
    
    if (!response.ok) {
      throw new Error(`Ошибка сервера: ${response.status}`)
    }
    
    deviceData.value = await response.json()
    
  } catch (err) {
    console.error('Ошибка при загрузке данных:', err)
    error.value = err.message || 'Ошибка при загрузке данных устройства'
  } finally {
    loading.value = false
  }
}

// Автоматически загружаем данные при открытии модалки
onMounted(() => {
  if (props.scannedData) {
    fetchDeviceData()
  } else {
    error.value = 'Нет данных QR-кода'
  }
})

// Следим за изменением scannedData
watch(() => props.scannedData, (newVal) => {
  if (newVal) {
    fetchDeviceData()
  }
})

const closeModal = () => {
  emit('close')
}

// Форматирование даты
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
</script>

<template>
  <div class="modal-overlay" @click.self="closeModal">
    <div class="modal-content">
      <!-- Заголовок с градиентом -->
      <div class="modal-header">
        <div class="header-content">
          <div class="header-icon">📱</div>
          <div class="header-text">
            <h2>Информация об устройстве</h2>
            <p v-if="loading">Загрузка данных...</p>
            <p v-else-if="deviceData">Данные успешно загружены</p>
            <p v-else>Обработка запроса</p>
          </div>
        </div>
        <button class="close-button" @click="closeModal">
          <span>×</span>
        </button>
      </div>

      <!-- Загрузка -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner">
          <div class="spinner-circle"></div>
        </div>
        <h3>Загружаем данные...</h3>
        <p>Это займет всего несколько секунд</p>
      </div>

      <!-- Устройство не найдено -->
      <div v-else-if="notFound" class="state-container error">
        <div class="state-icon">🔍</div>
        <h3>Устройство не найдено</h3>
        <p>Проверьте правильность QR-кода или обратитесь к администратору</p>
        <button class="action-button primary" @click="fetchDeviceData">
          <span class="button-icon">🔄</span>
          Попробовать снова
        </button>
      </div>

      <!-- Ошибка -->
      <div v-else-if="error" class="state-container error">
        <div class="state-icon">⚠️</div>
        <h3>Произошла ошибка</h3>
        <p>{{ error }}</p>
        <button class="action-button primary" @click="fetchDeviceData">
          <span class="button-icon">🔄</span>
          Повторить попытку
        </button>
      </div>

      <!-- Данные устройства -->
      <div v-else-if="deviceData" class="device-data">
        <!-- Карточка устройства -->
        <div class="device-card">
          <div class="device-header">
            <div class="device-icon">💻</div>
            <div class="device-info">
              <h3 class="device-name">{{ deviceData.name_model || 'Не указано' }}</h3>
              <p class="device-type">{{ deviceData.name_type || 'Тип не указан' }}</p>
            </div>
          </div>
          
          <div v-if="deviceData.description_model" class="device-description">
            {{ deviceData.description_model }}
          </div>
        </div>

        <!-- Характеристики -->
        <div v-if="deviceData.properties && deviceData.properties.length > 0" class="section">
          <div class="section-header">
            <div class="section-icon">⚙️</div>
            <h4>Характеристики</h4>
          </div>
          <div class="properties-grid">
            <div 
              v-for="(property, index) in deviceData.properties" 
              :key="index" 
              class="property-card"
            >
              <div class="property-icon">
                <span>📊</span>
              </div>
              <div class="property-content">
                <strong class="property-name">{{ property.Name || 'Свойство' }}</strong>
                <span class="property-value">{{ property.Value || 'Не указано' }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- История операций -->
        <div v-if="deviceData.operation_logs && deviceData.operation_logs.length > 0" class="section">
          <div class="section-header">
            <div class="section-icon">📋</div>
            <h4>История операций</h4>
          </div>
          <div class="timeline">
            <div 
              v-for="(log, index) in deviceData.operation_logs" 
              :key="index" 
              class="timeline-item"
            >
              <div class="timeline-marker"></div>
              <div class="timeline-content">
                <div class="timeline-header">
                  <strong class="operation-place">{{ log.place || 'Место не указано' }}</strong>
                  <span class="operation-date">{{ formatDate(log.date) }}</span>
                </div>
                <p v-if="log.comment" class="operation-comment">{{ log.comment }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Скриншот QR-кода -->
        <div v-if="frameImage" class="section">
          <div class="section-header">
            <div class="section-icon">📷</div>
            <h4>Сканированный QR-код</h4>
          </div>
          <div class="qr-container">
            <img :src="frameImage" alt="Сканированный QR-код" class="qr-screenshot" />
            <div class="qr-overlay">
              <span>Распознано успешно</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Футер -->
      <div class="modal-footer">
        <button class="action-button secondary" @click="closeModal">
          <span class="button-icon">←</span>
          Закрыть
        </button>
        <button v-if="deviceData && !loading" class="action-button primary" @click="fetchDeviceData">
          <span class="button-icon">🔄</span>
          Обновить
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Стили остаются такими же как в предыдущей красивой версии */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  backdrop-filter: blur(10px);
}

.modal-content {
  background: white;
  border-radius: 24px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 
    0 25px 50px rgba(0, 0, 0, 0.3),
    0 0 0 1px rgba(255, 255, 255, 0.1);
}

/* ... остальные стили без изменений ... */

.modal-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 24px;
  border-radius: 24px 24px 0 0;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  position: relative;
}

.close-button {
  background: rgba(255, 255, 255, 0.2);
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  backdrop-filter: blur(10px);
}

.close-button:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.1);
}


.close-button span {
  font-size: 24px;
  color: white;
  font-weight: 300;
}

/* Состояния */
.loading-container,
.state-container {
  padding: 60px 40px;
  text-align: center;
}

.loading-spinner {
  margin-bottom: 24px;
}

.spinner-circle {
  width: 60px;
  height: 60px;
  border: 4px solid #f1f3f4;
  border-top: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.state-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.state-container h3 {
  margin: 0 0 12px 0;
  font-size: 20px;
  font-weight: 600;
  color: #1d1d1f;
}

.state-container p {
  margin: 0 0 24px 0;
  color: #666;
  line-height: 1.5;
}

/* Данные устройства */
.device-data {
  padding: 24px;
}

/* Карточка устройства */
.device-card {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-radius: 20px;
  padding: 24px;
  margin-bottom: 24px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.device-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.device-icon {
  font-size: 40px;
  background: white;
  width: 60px;
  height: 60px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.device-info {
  flex: 1;
}

.device-name {
  margin: 0 0 4px 0;
  font-size: 20px;
  font-weight: 700;
  color: #1d1d1f;
}

.device-type {
  margin: 0;
  color: #667eea;
  font-weight: 600;
  font-size: 16px;
}

.device-description {
  color: #666;
  line-height: 1.5;
  font-size: 15px;
  padding: 12px 0 0 0;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
}

/* Секции */
.section {
  margin-bottom: 32px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 2px solid #f1f3f4;
}

.section-icon {
  font-size: 20px;
}

.section-header h4 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
}

/* Свойства */
.properties-grid {
  display: grid;
  gap: 12px;
}

.property-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: white;
  border-radius: 16px;
  border: 1px solid #f1f3f4;
  transition: all 0.2s;
}

.property-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #667eea;
}

.property-icon {
  font-size: 20px;
  background: #f8f9fa;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.property-content {
  flex: 1;
}

.property-name {
  display: block;
  font-size: 14px;
  color: #666;
  margin-bottom: 4px;
}

.property-value {
  display: block;
  font-size: 16px;
  font-weight: 600;
  color: #1d1d1f;
}

/* Таймлайн */
.timeline {
  position: relative;
  padding-left: 20px;
}

.timeline::before {
  content: '';
  position: absolute;
  left: 9px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #e9ecef;
}

.timeline-item {
  position: relative;
  margin-bottom: 20px;
}

.timeline-marker {
  position: absolute;
  left: -20px;
  top: 6px;
  width: 12px;
  height: 12px;
  background: #667eea;
  border-radius: 50%;
  border: 3px solid white;
  box-shadow: 0 0 0 2px #667eea;
}

.timeline-content {
  background: white;
  padding: 16px;
  border-radius: 12px;
  border: 1px solid #f1f3f4;
}

.timeline-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.operation-place {
  font-size: 15px;
  color: #1d1d1f;
  font-weight: 600;
}

.operation-date {
  font-size: 13px;
  color: #8e8e93;
  white-space: nowrap;
}

.operation-comment {
  margin: 0;
  font-size: 14px;
  color: #666;
  line-height: 1.4;
}

/* QR-код */
.qr-container {
  position: relative;
  display: inline-block;
}

.qr-screenshot {
  width: 100%;
  max-width: 200px;
  height: auto;
  border-radius: 16px;
  border: 3px solid #f1f3f4;
  display: block;
  margin: 0 auto;
}

.qr-overlay {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(102, 126, 234, 0.9);
  color: white;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  backdrop-filter: blur(10px);
}

/* Футер */
.modal-footer {
  padding: 24px;
  border-top: 1px solid #f1f3f4;
  display: flex;
  gap: 12px;
  justify-content: space-between;
  background: #f8f9fa;
  border-radius: 0 0 24px 24px;
}

.action-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 20px;
  border: none;
  border-radius: 12px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  min-width: 140px;
  justify-content: center;
}

.action-button.primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.action-button.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
}

.action-button.secondary {
  background: white;
  color: #666;
  border: 2px solid #e9ecef;
}

.action-button.secondary:hover {
  border-color: #667eea;
  color: #667eea;
  transform: translateY(-2px);
}

.button-icon {
  font-size: 16px;
}

/* Адаптивность */
@media (max-width: 480px) {
  .modal-content {
    margin: 0;
    max-height: 100vh;
    border-radius: 0;
  }
  
  .modal-header {
    padding: 20px;
  }
  
  .device-data {
    padding: 20px;
  }
  
  .modal-footer {
    flex-direction: column;
  }
  
  .action-button {
    width: 100%;
  }
  
  .timeline-header {
    flex-direction: column;
    gap: 4px;
  }
}
</style>