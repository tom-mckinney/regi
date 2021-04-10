import css from 'styled-jsx/css'

export const header = css`h1 { font-family: 'Arial'; }`

// Scoped styles
export const button = css`button { color: hotpink; }`

// Global styles
export const body = css.global`body { display: flex;, align-items: center; justify-content: center; }`

// Resolved styles
export const link = css.resolve`a { color: green; }`
// link.className -> scoped className to apply to `a` elements e.g. jsx-123
// link.styles -> styles element to render inside of your component
