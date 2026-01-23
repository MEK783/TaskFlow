
/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        lightbg: '#FAFAF7',
        mek: {
          green:  '#00FF66',
          yellow: '#FFD700',
          purple: '#A200FF',
          magenta:'#FF00B5',
          dark:   '#0A0A0A'
        },
        meklight: {
          green:  '#B3FFDA',
          yellow: '#FFEFA3',
          purple: '#D9B3FF',
          magenta:'#FFB3E3'
        }
      }
    }
  },
  plugins: []
}
