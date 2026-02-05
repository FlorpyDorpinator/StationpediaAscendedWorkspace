/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./src/renderer/index.html",
    "./src/renderer/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Game-accurate colors extracted from Stationeers
        'stationpedia': {
          'bg': '#0F1F38',         // Game background
          'surface': '#0A1520',   // Secondary background
          'border': '#264D73',    // Border color
          'text': '#E6EDF3',      // Primary text
          'text-muted': '#8B949E', // Secondary text
          'text-dim': '#6E7681',  // Dim text
          'accent': '#FF7A18',    // Orange accent
          'accent-hover': '#FF9533', // Orange hover
          'link': '#008AE6',      // Link color (NOT cyan)
          'link-hover': '#00A8FF', // Link hover
        }
      },
      fontFamily: {
        'mono': ['Courier New', 'monospace'],
      }
    },
  },
  plugins: [],
}
