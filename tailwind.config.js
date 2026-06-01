/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{fs,html,js}"
  ],
  theme: {
    extend: {
      fontFamily: {
        // Friendly rounded display font for headings and Body sans for everything else.
        // Nunito ships variable weights and covers both roles.
        display: ['"Fredoka"', '"Nunito"', 'system-ui', 'sans-serif'],
        sans:    ['"Nunito"', 'system-ui', 'sans-serif'],
      },
      transitionProperty: {
        'height': 'height'
      },
    },
  },
  plugins: []
}
