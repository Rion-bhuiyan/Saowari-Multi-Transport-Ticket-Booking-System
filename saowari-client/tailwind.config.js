/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        'saowari-primary': 'var(--saowari-primary)',
        'saowari-secondary': 'var(--saowari-secondary)',
        'saowari-accent': 'var(--saowari-accent)',
        'saowari-primary-dark': 'var(--saowari-primary-dark)',
        'saowari-primary-light': 'var(--saowari-primary-light)',
        'saowari-surface': 'var(--saowari-surface)',
        'saowari-surface-alt': 'var(--saowari-surface-alt)',
        'saowari-text-primary': 'var(--saowari-text-primary)',
        'saowari-text-secondary': 'var(--saowari-text-secondary)',
        'saowari-border': 'var(--saowari-border)',
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
