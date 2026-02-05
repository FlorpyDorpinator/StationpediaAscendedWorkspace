import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import electron from 'vite-plugin-electron'
import renderer from 'vite-plugin-electron-renderer'
import path from 'path'

const projectRoot = __dirname

export default defineConfig({
  plugins: [
    react(),
    electron([
      {
        entry: path.resolve(projectRoot, 'src/main/index.ts'),
        vite: {
          build: {
            outDir: path.resolve(projectRoot, 'dist-electron/main'),
            rollupOptions: {
              external: ['electron'],
              output: {
                inlineDynamicImports: false,
              },
            },
          },
        },
      },
      {
        entry: [
          path.resolve(projectRoot, 'src/preload/index.ts'),
          path.resolve(projectRoot, 'src/preload/simulatorPreload.ts'),
        ],
        vite: {
          build: {
            outDir: path.resolve(projectRoot, 'dist-electron/preload'),
            rollupOptions: {
              external: ['electron'],
            },
          },
        },
      },
    ]),
    renderer(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(projectRoot, './src'),
      '@renderer': path.resolve(projectRoot, './src/renderer'),
      '@components': path.resolve(projectRoot, './src/renderer/components'),
      '@services': path.resolve(projectRoot, './src/renderer/services'),
      '@models': path.resolve(projectRoot, './src/renderer/models'),
      '@styles': path.resolve(projectRoot, './src/renderer/styles'),
      '@store': path.resolve(projectRoot, './src/renderer/store'),
    },
  },
  root: path.resolve(projectRoot, './src/renderer'),
  base: './',
  server: {
    port: 5173,
  },
  build: {
    outDir: path.resolve(projectRoot, './dist'),
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: path.resolve(projectRoot, './src/renderer/index.html'),
        simulator: path.resolve(projectRoot, './src/renderer/simulator/simulator.html'),
      },
    },
  },
})
