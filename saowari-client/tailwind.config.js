/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        'saowari-primary': '#004f98',
        'saowari-secondary': '#3372ad',
        'saowari-accent': '#1E87E4',
        'saowari-primary-dark': '#003870',
        'saowari-primary-light': '#e8f0fb',
        'saowari-surface': '#ffffff',
        'saowari-surface-alt': '#f4f8ff',
        'saowari-text-primary': '#0d1b2a',
        'saowari-text-secondary': '#4a6080',
        'saowari-success': '#22c55e',
        'saowari-warning': '#f59e0b',
        'saowari-danger': '#ef4444',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
        heading: ['Poppins', 'sans-serif'],
      }
    },
  },
  plugins: [
    require('daisyui'),
  ],
  daisyui: {
    themes: [
      {
        saowari: {
          "primary": "#004f98",
          "secondary": "#3372ad",
          "accent": "#1E87E4",
          "neutral": "#0d1b2a",
          "base-100": "#ffffff",
          "info": "#3b82f6",
          "success": "#22c55e",
          "warning": "#f59e0b",
          "error": "#ef4444",
        },
      },
    ],
  },
}
