<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const uuid = route.params.id

const device = ref(null)
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  try {
    const res = await fetch(`/api/devices/${uuid}`)
    if (!res.ok) throw new Error('Устройство не найдено')
    device.value = await res.json()
  } catch (err) {
    error.value = 'Не удалось загрузить данные устройства'
    console.error(err)
  } finally {
    loading.value = false
  }
})
</script>