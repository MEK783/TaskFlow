/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        lightbg: '#d6d3d1',
        mek: {
          green:  '#00FF66',
          yellow: '#FFD700',
          purple: '#A200FF',
          magenta:'#FF00B5',
          dark:   '#0A0A0A',
          border: '#FAFAFA',
        },
        meklight: {
          green:  '#B3FFDA',
          mgreen:  '#7DFFB7',
          yellow: '#FFEFA3',
          myellow: '#FFE872',
          purple: '#D9B3FF',
          mpurple: '#C87DFF',
          magenta:'#FFB3E3',
          mmagenta: '#FF7DD5',
          border: '#B0B0B0',
        }
      },
      fontFamily: {
        exo: ['"Exo 2"', 'sans-serif']
      }
    }
  },
  plugins: []
}
