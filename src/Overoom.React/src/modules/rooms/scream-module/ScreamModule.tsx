import { styled } from '@mui/material/styles';
import { ReactElement, useCallback, useEffect, useRef, useState } from 'react';

import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';

/** Длительность показа "скримера" (мс) */
const SCREAM_DURATION = 1500;

/** Стилизованный видео элемент для воспроизведения "скримера" */
const ScreamerVideo = styled('video')({
  position: 'fixed',
  top: 0,
  left: 0,
  width: '100%',
  height: '100%',
  objectFit: 'cover',
  zIndex: 9999,
  cursor: 'pointer',
});

/**
 * Модуль обработки видео уведомлений "скример"
 * Воспроизводит полноэкранное видео при получении соответствующего уведомления от сервера.
 * Видео закрывается само, по клику или по Esc
 * @returns {ReactElement} JSX элемент скрытого видео модуля
 */
const ScreamModule = (): ReactElement => {
  /** Ref для доступа к видео элементу */
  const screamer = useRef<HTMLVideoElement>(null);

  /** Ref идентификатора таймера скрытия видео */
  const hideTimer = useRef<number | null>(null);

  /** Флаг видимости видео */
  const [visible, setVisible] = useState(false);

  /** Контекст комнаты */
  const { hub, currentViewerId } = useRoom();

  /** Снимает таймер скрытия видео */
  const clearHideTimer = useCallback(() => {
    if (hideTimer.current !== null) {
      clearTimeout(hideTimer.current);
      hideTimer.current = null;
    }
  }, []);

  /** Скрывает видео и останавливает воспроизведение */
  const hideVideo = useCallback(() => {
    clearHideTimer();
    setVisible(false);
    if (screamer.current) {
      screamer.current.pause();
      screamer.current.currentTime = 0;
    }
  }, [clearHideTimer]);

  /** Обработчик показа и воспроизведения видео "скримера" */
  const showVideo = useCallback(() => {
    if (!screamer.current) return;

    clearHideTimer();

    const randomIndex = Math.floor(Math.random() * 4) + 1;
    screamer.current.src = `/video/screamer${randomIndex}.mp4`;
    screamer.current.volume = 0.1;
    screamer.current.currentTime = 0;
    screamer.current.play().catch(() => {});

    setVisible(true);
    hideTimer.current = window.setTimeout(hideVideo, SCREAM_DURATION);
  }, [clearHideTimer, hideVideo]);

  /** Эффект очистки таймера при размонтировании */
  useEffect(() => clearHideTimer, [clearHideTimer]);

  /** Эффект закрытия видео по Esc */
  useEffect(() => {
    if (!visible) return;

    /**
     * Обработчик нажатия клавиш
     * @param e - Событие клавиатуры
     */
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') hideVideo();
    };

    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, [hideVideo, visible]);

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

  return (
    <ScreamerVideo
      ref={screamer}
      onClick={hideVideo}
      aria-hidden={!visible}
      style={{ display: visible ? 'block' : 'none' }}
    />
  );
};

export default ScreamModule;
