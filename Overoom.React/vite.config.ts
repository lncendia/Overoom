import * as fs from 'fs';
import * as path from 'path';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

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
