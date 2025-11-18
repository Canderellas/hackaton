<!-- components/ResultPage.vue -->
<script setup>
import { computed } from 'vue'

const props = defineProps({
  data: {
    type: [Object, String],
    required: true
  }
})

const emit = defineEmits(['back'])

// Обрабатываем данные
const displayData = computed(() => {
  if (!props.data) return []
  
  if (typeof props.data === 'string') {
    return [{ key: 'Текст', value: props.data }]
  }
  
  return Object.entries(props.data).map(([key, value]) => ({
    key: formatKey(key),
    value: formatValue(value)
  }))
})

function formatKey(key) {
  const keyMap = {
    'scannedText': 'Отсканированный текст',
    'rawData': 'Исходные данные',
    'timestamp': 'Время сканирования'
  }
  return keyMap[key] || key
}

function formatValue(value) {
  if (typeof value === 'object' && value !== null) {
    return JSON.stringify(value, null, 2)
  }
  
  // Форматируем дату
  if (typeof value === 'string' && !isNaN(Date.parse(value))) {
    return new Date(value).toLocaleString('ru-RU')
  }
  
  return String(value)
}
</script>

<template>
  <div class="result-page">
    <div class="header">
      <button @click="emit('back')" class="back-btn">← Назад</button>
      <h2>Результат сканирования</h2>
    </div>

    <div class="content">
      <div class="notification">
        Данные успешно отсканированы
      </div>

      <div class="data-container">
        <div v-for="(entry, i) in displayData" :key="i" class="data-card">
          <div class="data-field">
            <label class="field-label">{{ entry.key }}</label>
            <div class="field-value">{{ entry.value }}</div>
          </div>
        </div>
      </div>

      <div class="action-panel">
        <button class="action-btn" @click="emit('back')">
          Сканировать еще раз
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.result-page {
  height: 100vh;
  background: #f5f5f7;
  display: flex;
  flex-direction: column;
}

.header {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  background: white;
  border-bottom: 1px solid #e5e5e7;
  gap: 16px;
}

.back-btn {
  background: none;
  border: none;
  font-size: 24px;
  padding: 8px;
  color: #007aff;
  cursor: pointer;
}

h2 {
  margin: 0;
  font-size: 17px;
  font-weight: 600;
  color: #000;
}

.content {
  flex: 1;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

.notification {
  background: #34c759;
  color: white;
  padding: 12px 16px;
  border-radius: 12px;
  font-size: 14px;
  text-align: center;
  margin-bottom: 20px;
  font-weight: 500;
}

.data-container {
  flex: 1;
  overflow-y: auto;
}

.data-card {
  background: white;
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.data-field {
  margin-bottom: 16px;
}

.data-field:last-child {
  margin-bottom: 0;
}

.field-label {
  display: block;
  font-size: 13px;
  color: #8e8e93;
  font-weight: 500;
  margin-bottom: 6px;
}

.field-value {
  font-size: 16px;
  color: #000;
  line-height: 1.4;
  word-break: break-word;
  white-space: pre-wrap;
  background: #f9f9f9;
  padding: 12px;
  border-radius: 8px;
  border: 1px solid #e5e5e7;
}

.action-panel {
  padding-top: 20px;
}

.action-btn {
  width: 100%;
  background: #007aff;
  color: white;
  border: none;
  border-radius: 12px;
  padding: 16px;
  font-size: 17px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s;
}

.action-btn:hover {
  background: #0056cc;
}

.action-btn:active {
  background: #004499;
  transform: scale(0.98);
}
</style>