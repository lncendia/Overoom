import * as fs from 'fs';
import * as path from 'path';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

const baseFolder = path.resolve(import.meta.dirname, '../../development/configurations/ssl');

const certName = process.env.npm_package_name;
const certKeyPath = path.join(baseFolder, `${certName}.key`);
const certCertPath = path.join(baseFolder, `${certName}.pem`);

const httpsConfig =
  fs.existsSync(certKeyPath) && fs.existsSync(certCertPath)
    ? { key: fs.readFileSync(certKeyPath), cert: fs.readFileSync(certCertPath) }
    : undefined;

export default defineConfig({
  base: '/',

  plugins: [react()],

  build: {
    rolldownOptions: {
      output: {
        // Выносим крупные библиотеки в отдельные чанки для лучшего кэширования
        codeSplitting: {
          groups: [
            { name: 'mui', test: /node_modules[\\/](@mui|@emotion)[\\/]/, priority: 20 },
            { name: 'signalr', test: /node_modules[\\/]@microsoft[\\/]signalr[\\/]/, priority: 20 },
            {
              name: 'react',
              test: /node_modules[\\/](react|react-dom|react-router|react-router-dom|scheduler)[\\/]/,
              priority: 10,
            },
          ],
        },
      },
    },
  },

  preview: {
    port: 5173,

    strictPort: true,

    https: httpsConfig,
  },

  server: {
    host: true,

    strictPort: true,

    port: 5173,

    https: httpsConfig,

    hmr: {
      port: 5173,
    },
  },
});
