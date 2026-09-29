# Указываем node image (LTS) для сборки
FROM node:24-alpine AS overoom

# Указываем рабочую область
WORKDIR /app

# Копируем настройки сборки приложения
COPY package.json package-lock.json ./

# Устанавливаем зависимости строго по lock-файлу
RUN npm ci

# Копируем исходный код
COPY . .

# Запускаем сборку приложения
RUN npm run build

# Используем nginx для отдачи собранного приложения
FROM nginx:1.29-alpine

# Копируем собранное приложение в папку для размещения в nginx
COPY --from=overoom /app/dist /usr/share/nginx/html

# Копируем файл robots.txt в контейнер
COPY robots.txt /usr/share/nginx/html/.

# Копируем кастомный nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Конфигурация по умолчанию (/usr/share/nginx/html/configuration.json) содержит значения для
# локальной разработки; для других окружений файл монтируется поверх при запуске контейнера

# Expose порт 80
EXPOSE 80

# Запуск Nginx в foreground
CMD ["nginx", "-g", "daemon off;"]
