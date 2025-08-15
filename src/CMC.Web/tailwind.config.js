/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/CMC.Web/**/*.{razor,html,cshtml}',
    './src/CMC.Web/Pages/**/*.razor',
    './src/CMC.Web/Pages/Shared/*.razor',
    './src/CMC.Web/*.razor'
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
