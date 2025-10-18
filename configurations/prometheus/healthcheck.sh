#!/bin/sh

# Проверяем статус базы данных Prometheus
# Используем wget для выполнения GET-запроса к эндпоинту prometheus_target_interval_length_seconds
# Аутентификация выполняется с использованием пользователя pradmin и пароля prpassword
# Если статус равен success, скрипт завершается успешно (код возврата 0)
# В противном случае скрипт завершается с ошибкой (код возврата 1)

# Получаем статус кластера Prometheus
status=$(wget -q -O - --header="Authorization: Basic $(echo -n "${PROMETHEUS_USER}:${PROMETHEUS_PASSWORD}" | base64)" http://localhost:9090/api/v1/query?query=prometheus_target_interval_length_seconds)

# Проверяем, успешно ли выполнен запрос
if [ $? -ne 0 ]; then
    echo "Failed to retrieve status from Prometheus"
    exit 1
fi

# Извлекаем значение параметра status из вывода команды
success=$(echo "$status" | awk -F'"status":"' '{print $2}' | awk -F'"' '{print $1}')

# Проверяем условия:
# Если success равно 'success', то скрипт завершается успешно (код возврата 0)
# В противном случае скрипт завершается с ошибкой (код возврата 1)
if [ "$success" = "success" ]; then
  # Если условия выполнены, завершаем скрипт успешно
  exit 0
else
  # Если условия не выполнены, завершаем скрипт с ошибкой
  exit 1
fi