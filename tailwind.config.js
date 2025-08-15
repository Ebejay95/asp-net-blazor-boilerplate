/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{razor,html,cshtml,js}",
    "./src/CMC.Web/**/*.{razor,html,cshtml}",
    "./src/CMC.Web/Pages/**/*.razor",
    "./src/CMC.Web/Pages/Shared/*.razor",
    "./src/CMC.Web/Components/**/*.razor"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
