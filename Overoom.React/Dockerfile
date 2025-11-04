# Указываем node image для сборки
FROM node:25-alpine AS overoom

# Указываем рабочую область
WORKDIR /app

# Копируем настройки сборки приложения
COPY package.json ./

# Устанавливаем зависимости
RUN npm i

# Копируем исходный код
COPY . .

# Запускаем сборку приложения
RUN npm run build

# Используем nginx для отдачи собранного приложения
FROM nginx:1.29-alpine

# Копируем собранный лендинг в папку для размещения в nginx
COPY --from=overoom /app/dist /usr/share/nginx/html

# Копируем файл robots.txt в контейнер
COPY robots.txt /usr/share/nginx/html/.

# Копируем кастомный nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose порт 80
EXPOSE 80

# Expose порт 443
EXPOSE 443

# Запуск Nginx в foreground
CMD ["nginx", "-g", "daemon off;"]