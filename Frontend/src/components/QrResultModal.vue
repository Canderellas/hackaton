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
  // Если это прямой GUID
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(data)) {
    return data
  }
  
  // Если это URL, пытаемся извлечь ID из пути
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
  loading.value = true
  error.value = ''
  notFound.value = false
  deviceData.value = null

  try {
    const deviceId = extractDeviceId(props.scannedData)
    
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
    error.value = 'Ошибка при загрузке данных устройства'
  } finally {
    loading.value = false
  }
}

// Автоматически загружаем данные при открытии модалки
onMounted(() => {
  fetchDeviceData()
})

// Следим за изменением scannedData
watch(() => props.scannedData, () => {
  fetchDeviceData()
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
      <!-- Заголовок -->
      <div class="modal-header">
        <h2>Информация об устройстве</h2>
        <button class="close-button" @click="closeModal">×</button>
      </div>

      <!-- Загрузка -->
      <div v-if="loading" class="loading-container">
        <div class="spinner"></div>
        <p>Загрузка данных...</p>
      </div>

      <!-- Устройство не найдено -->
      <div v-else-if="notFound" class="not-found-container">
        <div class="error-icon">⚠️</div>
        <h3>Устройство не найдено</h3>
        <p>Проверьте правильность QR-кода</p>
        <button class="retry-button" @click="fetchDeviceData">Повторить</button>
      </div>

      <!-- Ошибка -->
      <div v-else-if="error" class="error-container">
        <div class="error-icon">❌</div>
        <h3>Ошибка</h3>
        <p>{{ error }}</p>
        <button class="retry-button" @click="fetchDeviceData">Повторить</button>
      </div>

      <!-- Данные устройства -->
      <div v-else-if="deviceData" class="device-data">
        <!-- Основная информация -->
        <div class="section">
          <h3 class="section-title">Основная информация</h3>
          <div class="info-grid">
            <div class="info-item">
              <strong class="info-label">Модель</strong>
              <span class="info-value">{{ deviceData.name_model || 'Не указано' }}</span>
            </div>
            <div class="info-item">
              <strong class="info-label">Тип</strong>
              <span class="info-value">{{ deviceData.name_type || 'Не указано' }}</span>
            </div>
            <div v-if="deviceData.description_model" class="info-item full-width">
              <strong class="info-label">Описание модели</strong>
              <span class="info-value">{{ deviceData.description_model }}</span>
            </div>
            <div v-if="deviceData.description_type" class="info-item full-width">
              <strong class="info-label">Описание типа</strong>
              <span class="info-value">{{ deviceData.description_type }}</span>
            </div>
          </div>
        </div>

        <!-- Свойства -->
        <div v-if="deviceData.properties && deviceData.properties.length > 0" class="section">
          <h3 class="section-title">Характеристики</h3>
          <div class="info-grid">
            <div 
              v-for="(property, index) in deviceData.properties" 
              :key="index" 
              class="info-item"
            >
              <strong class="info-label">{{ property.Name || 'Свойство' }}</strong>
              <span class="info-value">{{ property.Value || 'Не указано' }}</span>
            </div>
          </div>
        </div>

        <!-- История операций -->
        <div v-if="deviceData.operation_logs && deviceData.operation_logs.length > 0" class="section">
          <h3 class="section-title">История операций</h3>
          <div class="operations-list">
            <div 
              v-for="(log, index) in deviceData.operation_logs" 
              :key="index" 
              class="operation-item"
            >
              <div class="operation-header">
                <strong class="operation-place">{{ log.Place || 'Место не указано' }}</strong>
                <span class="operation-date">{{ formatDate(log.DateOperation) }}</span>
              </div>
              <p v-if="log.Comment" class="operation-comment">{{ log.Comment }}</p>
            </div>
          </div>
        </div>

        <!-- Скриншот QR-кода -->
        <div v-if="frameImage" class="section">
          <h3 class="section-title">Сканированный QR-код</h3>
          <img :src="frameImage" alt="Сканированный QR-код" class="qr-screenshot" />
        </div>
      </div>

      <!-- Кнопки -->
      <div class="modal-footer">
        <button class="close-modal-button" @click="closeModal">Закрыть</button>
        <button v-if="!loading && (notFound || error)" class="retry-button" @click="fetchDeviceData">
          Попробовать снова
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
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
}

.modal-content {
  background: white;
  border-radius: 20px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 1px solid #e5e5e7;
  background: #f8f9fa;
  border-radius: 20px 20px 0 0;
}

.modal-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1d1d1f;
}

.close-button {
  background: none;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: #8e8e93;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s;
}

.close-button:hover {
  background: rgba(0, 0, 0, 0.1);
}

/* Содержимое */
.loading-container,
.not-found-container,
.error-container {
  padding: 40px 24px;
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007aff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.not-found-container h3,
.error-container h3 {
  margin: 0 0 8px 0;
  color: #1d1d1f;
  font-size: 18px;
}

.not-found-container p,
.error-container p {
  margin: 0 0 20px 0;
  color: #8e8e93;
}

/* Данные устройства */
.device-data {
  padding: 0 24px;
}

.section {
  margin-bottom: 24px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0 0 16px 0;
  padding-bottom: 8px;
  border-bottom: 2px solid #007aff;
}

.info-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-item.full-width {
  grid-column: 1 / -1;
}

.info-label {
  font-size: 16px;
  font-weight: 600;
  color: #1d1d1f;
  line-height: 1.2;
}

.info-value {
  font-size: 16px;
  color: #48484a;
  line-height: 1.4;
  padding-left: 8px;
  border-left: 3px solid #007aff;
}

/* История операций */
.operations-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.operation-item {
  background: #f8f9fa;
  padding: 16px;
  border-radius: 12px;
  border-left: 4px solid #007aff;
}

.operation-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.operation-place {
  font-size: 16px;
  color: #1d1d1f;
  font-weight: 600;
}

.operation-date {
  font-size: 14px;
  color: #8e8e93;
  white-space: nowrap;
}

.operation-comment {
  margin: 0;
  font-size: 14px;
  color: #48484a;
  line-height: 1.4;
}

/* Скриншот QR-кода */
.qr-screenshot {
  width: 100%;
  max-width: 200px;
  height: auto;
  border-radius: 12px;
  border: 2px solid #e5e5e7;
  display: block;
  margin: 0 auto;
}

/* Футер модалки */
.modal-footer {
  padding: 20px 24px;
  border-top: 1px solid #e5e5e7;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  background: #f8f9fa;
  border-radius: 0 0 20px 20px;
}

.close-modal-button,
.retry-button {
  padding: 12px 24px;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.close-modal-button {
  background: #8e8e93;
  color: white;
}

.close-modal-button:hover {
  background: #6d6d70;
}

.retry-button {
  background: #007aff;
  color: white;
}

.retry-button:hover {
  background: #0056cc;
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
    padding: 0 20px;
  }
  
  .operation-header {
    flex-direction: column;
    gap: 4px;
  }
}
</style>