import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/renderer/editor/__tests__/setup.ts'],
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@renderer': path.resolve(__dirname, './src/renderer'),
      '@components': path.resolve(__dirname, './src/renderer/components'),
      '@services': path.resolve(__dirname, './src/renderer/services'),
      '@store': path.resolve(__dirname, './src/renderer/store'),
      '@models': path.resolve(__dirname, './src/renderer/models'),
      '@styles': path.resolve(__dirname, './src/renderer/styles'),
      '@simulator': path.resolve(__dirname, './src/renderer/simulator'),
    },
  },
});
