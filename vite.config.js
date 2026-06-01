import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['img/icon.svg'],
      manifest: {
        name: 'Morning Routine',
        short_name: 'Morning',
        description: 'Stay on track with your morning routine.',
        start_url: '/',
        scope: '/',
        display: 'standalone',
        orientation: 'portrait',
        background_color: '#f5f3ff',
        theme_color: '#9333ea',
        icons: [
          {
            src: '/img/icon.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'any maskable',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,svg,png,ico,webmanifest}'],
        navigateFallback: '/index.html',
      },
    }),
  ],
  base: "/",
  root: "./src",
  build: {
    outDir: "../dist",
    emptyOutDir: true,
  },
  server: {
    port: 8080,
  },
})
