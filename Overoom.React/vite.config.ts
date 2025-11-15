import * as fs from 'fs';
import * as path from 'path';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// Определение базовой папки для хранения HTTPS-сертификатов в зависимости от операционной системы
const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

// Получение имени сертификата из переменной окружения npm (имя пакета)
const certName = process.env.npm_package_name;

// Формирование пути к файлу приватного ключа сертификата
const certKeyPath = path.join(baseFolder, `${certName}.key`);

// Формирование пути к файлу сертификата
const certCertPath = path.join(baseFolder, `${certName}.pem`);

// Проверка существования файлов сертификата и ключа, и формирование конфигурации HTTPS
const httpsConfig =
  fs.existsSync(certKeyPath) && fs.existsSync(certCertPath)
    ? { key: fs.readFileSync(certKeyPath), cert: fs.readFileSync(certCertPath) }
    : undefined;

// Конфигурация сборщика Vite
export default defineConfig({
  // базовый url
  base: '/',

  // Массив плагинов
  plugins: [react()],

  // Конфигурация production версии приложения
  preview: {
    // Рабочий порт production приложения
    port: 5173,

    // Если порт занят, приложение напишет об этом и не запустится
    strictPort: true,

    // Конфигурация сертификатов https соединения
    https: httpsConfig,
  },

  // Конфигурация develop версии приложения
  server: {
    // Запуск develop сервера для хостинга приложения
    host: true,

    // Если порт занят, приложение напишет об этом и не запустится
    strictPort: true,

    // Рабочий порт develop приложения
    port: 5173,

    // Конфигурация сертификатов https соединения
    https: httpsConfig,

    // Порт для HMR
    hmr: {
      port: 5173,
    },
  },
});
