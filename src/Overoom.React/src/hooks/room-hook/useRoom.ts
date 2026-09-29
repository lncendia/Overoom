import { useEffect, useState } from 'react';

import {
  clearStaleTypingHandler,
  connectEventHandler,
  disconnectEventHandler,
  messageEventHandler,
  roomEventHandler,
  typingEventHandler,
  updateViewerEventHandler,
  updateViewerPlayerEventHandler,
} from './room-event.handlers.ts';
import RoomDto from './room.dto.ts';
import { RoomEventContainer } from '../../services/room-hub/events/room-event.container.ts';
import { RoomHub } from '../../services/room-hub/room.hub.ts';

/**
 * Кастомный хук для управления состоянием комнаты и обработкой событий.
 * Таймлайн зрителей здесь не тикает: текущее время вычисляется там, где отображается
 * (см. getCurrentTimeLine), чтобы не перерисовывать всю комнату каждую секунду
 * @param hub - Экземпляр RoomHub для подписки на события комнаты
 * @param reconnectCount - Счетчик переподключений, при его изменении комната загружается заново
 * @returns Массив с текущим состоянием комнаты и функцией обновления состояния
 */
const useRoom = (
  hub: RoomHub | null,
  reconnectCount: number
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

    return () => hub.removeHandler(handler);
  }, [hub]);

  /** Эффект загрузки комнаты после подключения и после каждого переподключения */
  useEffect(() => {
    if (!hub) return;
    hub.getRoom().catch((err) => console.warn('Не удалось запросить данные комнаты', err));
  }, [hub, reconnectCount]);

  /**
   * Эффект для очистки устаревших typing-событий.
   * Если ничего не изменилось, обработчик возвращает прежний объект и перерисовки не происходит
   */
  useEffect(() => {
    const id = setInterval(() => {
      setRoom((prev) => (prev ? clearStaleTypingHandler(prev) : prev));
    }, 1000);

    return () => clearInterval(id);
  }, []);

  /** Возврат состояния комнаты и функции для его обновления */
  return [room, setRoom];
};

export default useRoom;
