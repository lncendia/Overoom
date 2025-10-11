import { styled } from '@mui/material/styles';
import { ReactElement, useCallback, useEffect, useRef } from 'react';

import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';

/** Стилизованный видео элемент для воспроизведения "скримера" */
const ScreamerVideo = styled('video')({
  position: 'fixed',
  top: 0,
  left: 0,
  width: '100%',
  height: '100%',
  objectFit: 'cover',
  zIndex: 9999,
  display: 'none',
  transition: 'display 0.3s ease-in-out',
});

/**
 * Модуль обработки видео уведомлений "скример"
 * Воспроизводит полноэкранное видео при получении соответствующего уведомления от сервера
 * @returns {ReactElement} JSX элемент скрытого видео модуля
 */
const ScreamModule = (): ReactElement => {
  /** Ref для доступа к видео элементу */
  const screamer = useRef<HTMLVideoElement>(null);

  /** Контекст комнаты */
  const { hub, currentViewerId } = useRoom();

  /** Обработчик показа и воспроизведения видео "скримера" */
  const showVideo = useCallback(() => {
    if (!screamer.current) return;

    // Случайный выбор номера скримера от 1 до 4
    const randomIndex = Math.floor(Math.random() * 4) + 1;
    screamer.current.src = `/video/screamer${randomIndex}.mp4`;

    // Устанавливаем громкость и запускаем видео
    screamer.current.volume = 0.1;
    screamer.current.play().catch(() => {
      // Игнорируем ошибки воспроизведения для избежания необработанных исключений
    });

    // Показываем видео через изменение display свойства
    screamer.current.style.display = 'block';

    // Через 2 секунды скрываем видео (длительность эффекта)
    setTimeout(() => {
      if (screamer.current) {
        screamer.current.style.display = 'none';
      }
    }, 1500);
  }, []);

  /** Эффект для подписки на события уведомлений от хаба комнаты. */
  useEffect(() => {
    // Если хаб еще не проинициализирован - ничего не делаем
    if (!hub) return;

    /**
     * Обработчик событий комнаты.
     * Фильтрует события уведомлений "скример" для текущего пользователя
     * @param e - Контейнер событий комнаты
     * @returns {void}
     */
    const handler = (e: RoomEventContainer) => {
      // Пропускаем события, не относящиеся к уведомлениям "скример"
      if (!e.screamNotificationEvent) return;

      // Проверяем, что уведомление предназначено текущему пользователю
      if (e.screamNotificationEvent.target !== currentViewerId) return;

      // Запускаем воспроизведение видео "скримера"
      showVideo();
    };

    // Добавляем обработчик событий в хаб комнаты
    hub.addHandler(handler);

    // Функция очистки эффекта
    return (): void => hub.removeHandler(handler);
  }, [currentViewerId, hub, showVideo]);

  // Возвращает скрытый видео элемент с предзагруженным видео "скример"
  return <ScreamerVideo ref={screamer} />;
};

export default ScreamModule;
