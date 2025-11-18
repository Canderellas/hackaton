<script setup>
import { ref } from 'vue'
import QrCameraScanner from './components/QrCameraScanner.vue'

const currentView = ref('scanner')
const scannedData = ref(null)

function handleScan(data) {
  scannedData.value = data
  currentView.value = 'result'
}
</script>

<template>
  <div class="app">
    <QrCameraScanner />
  </div>
</template>



<style>
.app { height: 100vh; background: #f5f5f5; font-family: -apple-system, sans-serif; }
</style>