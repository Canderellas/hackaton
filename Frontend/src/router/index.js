// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import QrCameraScannerFixed from '@/views/QrCameraScannerFixed.vue'  // ← твой сканер
import DeviceInfo from '@/views/DeviceInfo.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: QrCameraScannerFixed
  },
  {
    path: '/device/:id',
    name: 'DeviceInfo',
    component: DeviceInfo
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router