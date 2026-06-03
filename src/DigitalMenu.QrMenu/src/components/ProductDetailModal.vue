<template>
  <Teleport to="body">
    <Transition name="modal-backdrop">
      <div
        v-if="modelValue && product"
        class="fixed inset-0 z-[100] flex flex-col justify-end"
        role="dialog"
        aria-modal="true"
        :aria-label="product.name"
      >
        <button
          type="button"
          class="absolute inset-0 bg-black/40 backdrop-blur-[2px]"
          aria-label="Kapat"
          @click="close"
        />

        <Transition name="modal-sheet" appear>
          <div
            v-if="modelValue"
            class="relative z-10 flex max-h-[92vh] w-full flex-col rounded-t-2xl bg-white shadow-2xl"
          >
            <div class="flex shrink-0 justify-center pt-2 pb-1">
              <span class="h-1 w-10 rounded-full bg-gray-200" aria-hidden="true" />
            </div>

            <button
              type="button"
              class="absolute right-3 top-3 z-20 flex h-8 w-8 items-center justify-center rounded-full bg-white/90 text-gray-500 shadow-sm ring-1 ring-gray-100 transition active:scale-95"
              aria-label="Kapat"
              @click="close"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <div class="overflow-y-auto overscroll-contain px-4 pb-28 no-scrollbar">
              <div class="relative -mx-4 mb-4 overflow-hidden bg-gray-100">
                <img
                  :src="imageSrc"
                  :alt="product.name"
                  class="h-52 w-full object-cover sm:h-60"
                  @error="onImageError"
                />
              </div>

              <div class="mb-5">
                <h2 class="text-lg font-bold text-gray-900">{{ product.name }}</h2>
                <p v-if="product.description" class="mt-1 text-sm leading-relaxed text-gray-500">
                  {{ product.description }}
                </p>
                <p class="mt-2 text-base font-extrabold text-gray-900">
                  {{ formatPrice(product.displayPrice) }}
                </p>
              </div>

              <section v-if="removableIngredients.length" class="mb-5">
                <h3 class="mb-2 text-xs font-bold uppercase tracking-wide text-gray-400">
                  Malzeme Çıkar
                </h3>
                <div class="flex flex-wrap gap-2">
                  <button
                    v-for="item in removableIngredients"
                    :key="item.id"
                    type="button"
                    :class="[
                      'rounded-full border px-3 py-1.5 text-xs font-medium transition-all duration-200 active:scale-95',
                      removedIds.has(item.id)
                        ? 'border-red-500 bg-red-50 text-red-600 shadow-sm'
                        : 'border-gray-200 bg-white text-gray-600 hover:border-gray-300',
                    ]"
                    @click="toggleRemoved(item.id)"
                  >
                    {{ item.label }} Olmasın
                  </button>
                </div>
              </section>

              <template v-for="group in extraSauceGroups" :key="group.name">
                <section v-if="group.items.length" class="mb-5">
                  <h3 class="mb-2 text-xs font-bold uppercase tracking-wide text-gray-400">
                    {{ group.name }}
                  </h3>
                  <div class="space-y-2">
                    <button
                      v-for="sauce in group.items"
                      :key="sauce.id"
                      type="button"
                      :class="[
                        'flex w-full items-center justify-between rounded-xl border px-3 py-2.5 text-left text-sm transition-all duration-200 active:scale-[0.99]',
                        selectedSauceIds.has(sauce.id)
                          ? 'border-red-500 bg-red-50 text-gray-900'
                          : 'border-gray-100 bg-gray-50/80 text-gray-700 hover:border-gray-200',
                      ]"
                      @click="toggleSauce(sauce.id)"
                    >
                      <span class="font-medium">{{ sauce.label }}</span>
                      <span class="text-xs font-semibold text-gray-500">
                        {{ sauce.price > 0 ? `+${formatPrice(sauce.price)}` : 'Ücretsiz' }}
                      </span>
                    </button>
                  </div>
                </section>
              </template>

              <section class="mb-2">
                <h3 class="mb-2 text-xs font-bold uppercase tracking-wide text-gray-400">
                  Adet
                </h3>
                <div class="inline-flex items-center gap-3 rounded-xl border border-gray-100 bg-gray-50 px-2 py-1">
                  <button
                    type="button"
                    class="flex h-9 w-9 items-center justify-center rounded-lg bg-white text-lg font-bold text-gray-700 shadow-sm ring-1 ring-gray-100 transition active:scale-95 disabled:opacity-40"
                    :disabled="quantity <= 1"
                    aria-label="Azalt"
                    @click="quantity--"
                  >
                    −
                  </button>
                  <span class="min-w-[2rem] text-center text-base font-bold text-gray-900">
                    {{ quantity }}
                  </span>
                  <button
                    type="button"
                    class="flex h-9 w-9 items-center justify-center rounded-lg bg-red-500 text-lg font-bold text-white shadow-sm transition active:scale-95"
                    aria-label="Artır"
                    @click="quantity++"
                  >
                    +
                  </button>
                </div>
              </section>
            </div>

            <div class="absolute bottom-0 left-0 right-0 border-t border-gray-100 bg-white/95 px-4 py-3 backdrop-blur-md safe-area-pb">
              <button
                type="button"
                class="flex w-full items-center justify-between rounded-xl bg-red-500 px-4 py-3.5 text-white shadow-lg shadow-red-500/25 transition active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50"
                :disabled="!product.isAvailable"
                @click="addToCart"
              >
                <span class="text-sm font-bold">Sepete Ekle</span>
                <span class="text-sm font-extrabold">{{ formatPrice(totalPrice) }}</span>
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { getProductCustomization } from '../composables/useProductCustomization.js'
import { productImage, onImageError } from '../utils/productImage.js'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  product: { type: Object, default: null },
})

const emit = defineEmits(['update:modelValue', 'add-to-cart'])

const quantity = ref(1)
const removedIds = ref(new Set())
const selectedSauceIds = ref(new Set())

const customization = computed(() => getProductCustomization(props.product))
const removableIngredients = computed(() => customization.value.removableIngredients)
const extraSauces = computed(() => customization.value.extraSauces)

const extraSauceGroups = computed(() => {
  const map = new Map()
  for (const sauce of extraSauces.value) {
    const name = sauce.groupName || 'Ekstra Seçenekler'
    if (!map.has(name)) map.set(name, [])
    map.get(name).push(sauce)
  }
  return [...map.entries()].map(([name, items]) => ({ name, items }))
})

const imageSrc = computed(() => productImage(props.product))

const extrasUnitTotal = computed(() =>
  extraSauces.value
    .filter((s) => selectedSauceIds.value.has(s.id))
    .reduce((sum, s) => sum + Number(s.price || 0), 0)
)

const unitPrice = computed(() => {
  if (!props.product) return 0
  return Number(props.product.displayPrice) + extrasUnitTotal.value
})

const totalPrice = computed(() => unitPrice.value * quantity.value)

const formatPrice = (amount) =>
  Number(amount).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })

const resetState = () => {
  quantity.value = 1
  removedIds.value = new Set()
  selectedSauceIds.value = new Set()
}

watch(
  () => props.product?.id,
  () => resetState()
)

watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      resetState()
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  }
)

const toggleRemoved = (id) => {
  const next = new Set(removedIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  removedIds.value = next
}

const toggleSauce = (id) => {
  const next = new Set(selectedSauceIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  selectedSauceIds.value = next
}

const close = () => {
  emit('update:modelValue', false)
}

const addToCart = () => {
  if (!props.product?.isAvailable) return

  const removed = removableIngredients.value
    .filter((i) => removedIds.value.has(i.id))
    .map((i) => i.label)

  const sauces = extraSauces.value
    .filter((s) => selectedSauceIds.value.has(s.id))
    .map((s) => ({ id: s.id, label: s.label, price: s.price, groupName: s.groupName }))

  emit('add-to-cart', {
    productId: props.product.id,
    name: props.product.name,
    imageUrl: props.product.imageUrl,
    quantity: quantity.value,
    unitPrice: unitPrice.value,
    totalPrice: totalPrice.value,
    removedIngredients: removed,
    extraSauces: sauces,
  })
  close()
}
</script>

<style scoped>
.modal-backdrop-enter-active,
.modal-backdrop-leave-active {
  transition: opacity 0.28s ease;
}
.modal-backdrop-enter-from,
.modal-backdrop-leave-to {
  opacity: 0;
}

.modal-sheet-enter-active {
  transition: transform 0.35s cubic-bezier(0.32, 0.72, 0, 1);
}
.modal-sheet-leave-active {
  transition: transform 0.26s cubic-bezier(0.32, 0.72, 0, 1);
}
.modal-sheet-enter-from,
.modal-sheet-leave-to {
  transform: translateY(100%);
}

.safe-area-pb {
  padding-bottom: max(0.75rem, env(safe-area-inset-bottom));
}
</style>
