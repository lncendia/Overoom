import { styled } from '@mui/system';
import { ReactElement, useCallback, useEffect, useRef } from 'react';

import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';

/** Скрытый аудио элемент для воспроизведения звуковых уведомлений */
const HiddenAudio = styled('audio')({
  display: 'none',
});

/**
 * Модуль обработки звуковых уведомлений "бип"
 * Воспроизводит звуковой сигнал при получении соответствующего уведомления от сервера
 * @returns {ReactElement} JSX элемент скрытого аудио модуля
 */
const BeepModule = (): ReactElement => {
  /** Ref для доступа к аудио элементу */
  const beep = useRef<HTMLAudioElement>(null);

  /** Получение данных комнаты из контекста */
  const { hub, currentViewerId } = useRoom();

  /** Обработчик воспроизведения звука "бип" */
  const onBeep = useCallback(() => {
    if (!beep.current) return;

    beep.current.currentTime = 0;
    beep.current.volume = 0.1;
    beep.current.play().catch(() => {});
  }, []);

  /** Эффект для подписки на события уведомлений от хаба комнаты. */
  useEffect(() => {
    if (!hub) return;

    /**
     * Обработчик событий комнаты.
     * Фильтрует события уведомлений "бип" для текущего пользователя
     * @param e - Контейнер событий комнаты
     * @returns {void}
     */
    const handler = (e: RoomEventContainer) => {
      if (!e.beepNotificationEvent) return;
      if (e.beepNotificationEvent.target !== currentViewerId) return;

      onBeep();
    };

    hub.addHandler(handler);

    return (): void => hub.removeHandler(handler);
  }, [currentViewerId, hub, onBeep]);

  return <HiddenAudio ref={beep} src="/audio/beep.wav" />;
};

export default BeepModule;
