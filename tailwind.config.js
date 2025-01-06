/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{fs,html,js}"
  ],
  theme: {
    extend: {
      transitionProperty: {
        'height': 'height'
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}