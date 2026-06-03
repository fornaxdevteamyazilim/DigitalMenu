export const DEFAULT_PRODUCT_IMAGE =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" width="160" height="160" viewBox="0 0 160 160">
      <rect width="160" height="160" fill="#f3f4f6"/>
      <g fill="none" stroke="#cbd5e1" stroke-width="6" stroke-linecap="round" stroke-linejoin="round">
        <path d="M40 104 L66 74 L86 96 L104 78 L120 104 Z"/>
        <circle cx="58" cy="56" r="10"/>
      </g>
    </svg>`
  )

export const productImage = (product) =>
  product?.imageUrl ? product.imageUrl : DEFAULT_PRODUCT_IMAGE

export const onImageError = (event) => {
  if (event.target.src !== DEFAULT_PRODUCT_IMAGE) {
    event.target.src = DEFAULT_PRODUCT_IMAGE
  }
}
