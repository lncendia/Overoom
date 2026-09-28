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

    const randomIndex = Math.floor(Math.random() * 4) + 1;
    screamer.current.src = `/video/screamer${randomIndex}.mp4`;
    screamer.current.volume = 0.1;
    screamer.current.play().catch(() => {
    });

    screamer.current.style.display = 'block';

    setTimeout(() => {
      if (screamer.current) {
        screamer.current.style.display = 'none';
      }
    }, 1500);
  }, []);

  /** Эффект для подписки на события уведомлений от хаба комнаты. */
  useEffect(() => {
    if (!hub) return;

    /**
     * Обработчик событий комнаты.
     * Фильтрует события уведомлений "скример" для текущего пользователя
     * @param e - Контейнер событий комнаты
     * @returns {void}
     */
    const handler = (e: RoomEventContainer) => {
      if (!e.screamNotificationEvent) return;
      if (e.screamNotificationEvent.target !== currentViewerId) return;

      showVideo();
    };

    hub.addHandler(handler);

    return (): void => hub.removeHandler(handler);
  }, [currentViewerId, hub, showVideo]);

  return <ScreamerVideo ref={screamer} />;
};

export default ScreamModule;
