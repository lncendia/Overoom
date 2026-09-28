import { useEffect, useState } from 'react';

import {
  clearStaleTypingHandler,
  connectEventHandler,
  disconnectEventHandler,
  messageEventHandler,
  roomEventHandler,
  tickViewerTimeLineHandler,
  typingEventHandler,
  updateViewerEventHandler,
  updateViewerPlayerEventHandler,
} from './room-event.handlers.ts';
import RoomDto from './room.dto.ts';
import { RoomEventContainer } from '../../services/room-hub/events/room-event.container.ts';
import { RoomHub } from '../../services/room-hub/room.hub.ts';

/**
 * Кастомный хук для управления состоянием комнаты и обработкой событий
 * @param hub - Экземпляр RoomHub для подписки на события комнаты
 * @returns Массив с текущим состоянием комнаты и функцией обновления состояния
 */
const useRoom = (
  hub: RoomHub | null
): [RoomDto | null, (callback: (prev: RoomDto | null) => RoomDto | null) => void] => {
  /** Состояние текущей комнаты */
  const [room, setRoom] = useState<RoomDto | null>(null);

  /** Эффект для подписки на события комнаты через hub */
  useEffect(() => {
    if (!hub) return;

    /**
     * Основной обработчик событий комнаты
     * @param e - Событие комнаты
     */
    const handler = (e: RoomEventContainer) => {
      setRoom((prev) => {
        if (e.roomEvent) {
          return roomEventHandler(e.roomEvent);
        }
        if (!prev) {
          return prev;
        }
        if (e.joinEvent) {
          return connectEventHandler(prev, e.joinEvent);
        }
        if (e.leaveEvent) {
          return disconnectEventHandler(prev, e.leaveEvent);
        }
        if (e.updateViewerEvent) {
          return updateViewerEventHandler(prev, e.updateViewerEvent);
        }
        if (e.updateViewerPlayerEvent) {
          return updateViewerPlayerEventHandler(prev, e.updateViewerPlayerEvent);
        }
        if (e.typingEvent) {
          return typingEventHandler(prev, e.typingEvent);
        }
        if (e.messageEvent) {
          return messageEventHandler(prev, e.messageEvent);
        }

        return prev;
      });
    };

    hub.addHandler(handler);
    hub.getRoom().then();

    return () => hub.removeHandler(handler);
  }, [hub]);

  /** Эффект для регулярного обновления таймлайна зрителей и очистки устаревших typing-событий */
  useEffect(() => {
    const id = setInterval(() => {
      setRoom((prev) => {
        if (!prev) return prev;
        const tickHandled = tickViewerTimeLineHandler(prev);
        return clearStaleTypingHandler(tickHandled);
      });
    }, 1000);

    return () => clearInterval(id);
  }, []);

  /** Возврат состояния комнаты и функции для его обновления */
  return [room, setRoom];
};

export default useRoom;
