<template>
  <div v-if="loading" class="flex h-screen items-center justify-center bg-gray-50">
    <div class="h-10 w-10 animate-spin rounded-full border-4 border-red-500 border-t-transparent"></div>
  </div>

  <div v-else-if="error" class="flex h-screen flex-col items-center justify-center p-6 bg-gray-50 text-center">
    <span class="text-4xl">⚠️</span>
    <p class="mt-4 text-gray-800 font-medium">{{ error }}</p>
  </div>

  <div v-else class="min-h-screen bg-gray-50 pb-24 text-gray-800 font-sans">
    <div class="sticky top-0 z-50 bg-white px-4 py-3 shadow-sm border-b border-gray-100 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <img :src="tenantInfo.logoUrl" class="h-10 w-10 rounded-full object-cover border border-gray-200" alt="Logo" />
        <div>
          <h1 class="font-bold text-base leading-tight">{{ tenantInfo.name }}</h1>
          <span class="text-xs text-green-600 font-medium bg-green-50 px-2 py-0.5 rounded-full border border-green-100">
            {{ currentTable }}
          </span>
        </div>
      </div>

      <div class="flex gap-2">
        <button @click="triggerNotification('waiter-call')"
                :class="['px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95',
                          isWaiterCalled ? 'bg-red-500 text-white border-red-500 animate-pulse' : 'bg-white text-red-500 border-red-200 hover:bg-red-50']">
          {{ isWaiterCalled ? 'Garson Geliyor' : 'Garson Çağır' }}
        </button>
        <button @click="triggerNotification('bill-request')"
                :class="['px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95',
                          isBillRequested ? 'bg-amber-500 text-white border-amber-500' : 'bg-white text-amber-500 border-amber-200 hover:bg-amber-50']">
          {{ isBillRequested ? 'Hesap İstendi' : 'Hesap İste' }}
        </button>
      </div>
    </div>

    <div class="sticky top-[65px] z-40 bg-white/90 backdrop-blur-md border-b border-gray-100 px-4 py-2.5 flex gap-2 overflow-x-auto no-scrollbar shadow-sm">
      <button v-for="category in categories" :key="category.id"
              @click="activeCategory = category.id"
              :class="['px-4 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-all duration-200',
                        activeCategory === category.id ? 'bg-red-500 text-white shadow-sm' : 'bg-gray-100 text-gray-600 hover:bg-gray-200']">
        {{ category.name }}
      </button>
    </div>

    <div class="p-4 space-y-4">
      <div v-for="product in filteredProducts" :key="product.id"
           role="button"
           tabindex="0"
           @click="openProduct(product)"
           @keydown.enter="openProduct(product)"
           :class="[
             'bg-white rounded-xl p-3 flex gap-3 shadow-sm border border-gray-100/80 transition-all',
             product.isAvailable ? 'cursor-pointer active:bg-gray-50 active:scale-[0.99]' : 'opacity-60 cursor-not-allowed'
           ]">
        <img :src="productImage(product)" @error="onImageError" class="h-20 w-20 rounded-lg object-cover bg-gray-100 flex-shrink-0" :alt="product.name" />

        <div class="flex flex-col justify-between flex-grow">
          <div>
            <h3 class="font-bold text-sm text-gray-900">{{ product.name }}</h3>
            <p class="text-xs text-gray-500 line-clamp-2 mt-0.5 leading-relaxed">{{ product.description }}</p>
          </div>
          <div class="flex justify-between items-center mt-2">
            <span class="font-extrabold text-sm text-gray-900">{{ formatPrice(product.displayPrice) }}</span>
            <span v-if="!product.isAvailable" class="text-[10px] bg-gray-100 text-gray-400 px-2 py-0.5 rounded-md font-medium">Tükendi</span>
            <span v-else class="text-[10px] bg-red-50 text-red-500 px-2 py-0.5 rounded-md font-semibold border border-red-100">Detay</span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="cartCount > 0"
         class="fixed bottom-4 left-4 right-4 z-40 flex items-center justify-between rounded-xl bg-gray-900 px-4 py-3 text-white shadow-xl">
      <span class="text-sm font-medium">{{ cartCount }} ürün · {{ formatPrice(cartTotal) }}</span>
      <span class="text-xs text-gray-300">Sepet (yakında)</span>
    </div>

    <ProductDetailModal
      v-model="showProductModal"
      :product="selectedProduct"
      @add-to-cart="onAddToCart"
    />

    <Transition name="toast">
      <div v-if="cartToast"
           class="fixed left-1/2 top-20 z-[110] -translate-x-1/2 rounded-full bg-gray-900 px-4 py-2 text-xs font-medium text-white shadow-lg">
        {{ cartToast }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'
import ProductDetailModal from './components/ProductDetailModal.vue'
import { productImage, onImageError } from './utils/productImage.js'

// Dev: Vite proxy ile aynı origin (/api, /hubs). Prod: VITE_API_BASE_URL (Railway build arg)
const API_BASE = import.meta.env.DEV
  ? ''
  : (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')

const formatPrice = (amount) =>
  Number(amount).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })

// State Yönetimi
const loading = ref(true)
const error = ref(null)
const categories = ref([])
const activeCategory = ref(null)
const tenantInfo = ref({ name: '', logoUrl: '' })

const currentTenant = ref('')
const currentTable = ref('')
const tableId = ref(null)

const isWaiterCalled = ref(false)
const isBillRequested = ref(false)

const selectedProduct = ref(null)
const showProductModal = ref(false)
const cart = ref([])
const cartToast = ref('')
let hubConnection = null

const cartCount = computed(() =>
  cart.value.reduce((sum, line) => sum + line.quantity, 0)
)
const cartTotal = computed(() =>
  cart.value.reduce((sum, line) => sum + line.totalPrice, 0)
)

const openProduct = (product) => {
  if (!product?.isAvailable) return
  selectedProduct.value = product
  showProductModal.value = true
}

const onAddToCart = (line) => {
  cart.value.push(line)
  cartToast.value = `${line.name} sepete eklendi`
  setTimeout(() => {
    cartToast.value = ''
  }, 2200)
}

// URL'den Parametreleri Oku (Örn: /r/lezzet-duragi?table=Masa%201)
const parseUrlParams = () => {
  const pathParts = window.location.pathname.split('/')
  // Basit routing: URL yapısı /r/tenant-id şeklinde ise
  currentTenant.value = pathParts[pathParts.indexOf('r') + 1] || 'lezzet-duragi'

  const urlParams = new URLSearchParams(window.location.search)
  currentTable.value = urlParams.get('table') || 'Masa Tanımsız'
}

// Aktif kategoriye ait ürünleri filtrele
const filteredProducts = computed(() => {
  const cat = categories.value.find(c => c.id === activeCategory.value)
  return cat ? cat.products : []
})

// Verileri API'den Çek
const loadMenu = async () => {
  try {
    // Axios ile header'da hangi restoran olduğumuzu belirtiyoruz
    const response = await axios.get(`${API_BASE}/api/products-by-categories`, {
      headers: { 'X-Tenant-Id': currentTenant.value }
    })

    categories.value = response.data
    if (categories.value.length > 0) {
      activeCategory.value = categories.value[0].id
    }

    // Masaları çekip mevcut masa ID'sini bulalım (SignalR tetiklemeleri için lazım olacak)
    const tableResponse = await axios.get(`${API_BASE}/api/tables`, {
      headers: { 'X-Tenant-Id': currentTenant.value }
    })
    const matchedTable = tableResponse.data.find(t => t.tableNumber === currentTable.value)
    if (matchedTable) {
      tableId.value = matchedTable.id
      isWaiterCalled.value = matchedTable.isWaiterCalled
      isBillRequested.value = matchedTable.isBillRequested
    }

    // Demo amaçlı tenant ismi atıyoruz, ileride dinamik tenant detail API'sinden çekilebilir
    tenantInfo.value = {
      name: currentTenant.value === 'lezzet-duragi' ? 'Lezzet Durağı' : 'Restoran',
      logoUrl: 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=200'
    }
  } catch (err) {
    error.value = 'Menü şu an yüklenemiyor. Lütfen QR kodu tekrar okutun.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

// Garson / Hesap Bildirimlerini Tetikle
const triggerNotification = async (type) => {
  if (!tableId.value) return

  const isActionCall = type === 'waiter-call' ? !isWaiterCalled.value : !isBillRequested.value
  const endpoint = `${API_BASE}/api/notifications/${type}?tableId=${tableId.value}&is${type === 'waiter-call' ? 'Called' : 'Requested'}=${isActionCall}`

  try {
    await axios.post(endpoint, {}, {
      headers: { 'X-Tenant-Id': currentTenant.value }
    })

    if (type === 'waiter-call') isWaiterCalled.value = isActionCall
    if (type === 'bill-request') isBillRequested.value = isActionCall
  } catch (err) {
    alert('İstek iletilemedi, lütfen garsona sözlü bildiriniz.')
  }
}

const showStatusToast = (message) => {
  cartToast.value = message
  setTimeout(() => {
    cartToast.value = ''
  }, 3500)
}

const isSameTable = (incomingId) =>
  incomingId != null &&
  tableId.value != null &&
  String(incomingId).toLowerCase() === String(tableId.value).toLowerCase()

const connectToHub = async () => {
  if (!tableId.value || !currentTenant.value) return

  try {
    const hubUrl = API_BASE ? `${API_BASE}/hubs/menu` : '/hubs/menu'
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    hubConnection.on('ReceiveWaiterCall', (payload) => {
      if (!payload || !isSameTable(payload.tableId)) return

      const wasCalled = isWaiterCalled.value
      isWaiterCalled.value = payload.isWaiterCalled

      if (wasCalled && !payload.isWaiterCalled) {
        showStatusToast('Garson çağrınız karşılandı.')
      }
    })

    hubConnection.on('ReceiveBillRequest', (payload) => {
      if (!payload || !isSameTable(payload.tableId)) return

      const wasRequested = isBillRequested.value
      isBillRequested.value = payload.isBillRequested

      if (wasRequested && !payload.isBillRequested) {
        showStatusToast('Hesap isteğiniz alındı.')
      }
    })

    await hubConnection.start()
    await hubConnection.invoke('JoinRestoranGroup', currentTenant.value)
  } catch (err) {
    console.warn('SignalR bağlantısı kurulamadı:', err)
  }
}

const disconnectHub = async () => {
  if (hubConnection) {
    try {
      if (hubConnection.state === signalR.HubConnectionState.Connected) {
        await hubConnection.invoke('LeaveRestoranGroup', currentTenant.value)
      }
      await hubConnection.stop()
    } catch {
      // ignore dispose errors
    }
    hubConnection = null
  }
}

onMounted(async () => {
  parseUrlParams()
  await loadMenu()
  await connectToHub()
})

onUnmounted(() => {
  disconnectHub()
})
</script>

<style>
/* Yatay kategori menüsünün scroll barını gizleme */
.no-scrollbar::-webkit-scrollbar {
  display: none;
}
.no-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.25s ease, transform 0.25s ease;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translate(-50%, -8px);
}
</style>
