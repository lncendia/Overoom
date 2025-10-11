import { useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';

/**
 * Модуль для обработки отключения пользователя из комнаты.
 * Перенаправляет пользователя на страницу комнат при выходе, кике или удалении комнаты.
 * @returns {null} Не рендерит JSX, работает только на уровне эффектов
 */
const DisconnectModule = (): null => {
  /** Хук для получения данных комнаты и хаба */
  const { hub, currentViewerId } = useRoom();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Обработчик перенаправления пользователя при отключении
   */
  const onDisconnect = useCallback(() => {
    navigate('/rooms');
  }, [navigate]);

  /** useEffect для добавления обработчика событий хаба при монтировании */
  useEffect(() => {
    // Если хаб еще не инициализирован, пропускаем добавление обработчика
    if (!hub) return;

    /**
     * Функция-обработчик событий комнаты
     * @param e - объект события комнаты
     */
    const handler = (e: RoomEventContainer) => {
      if (
        e.kickNotificationEvent?.target === currentViewerId ||
        e.leaveNotificationEvent?.initiator === currentViewerId ||
        e.deleteNotificationEvent
      ) {
        onDisconnect();
      }
    };

    // Добавляем обработчик событий в хаб
    hub.addHandler(handler);

    // Возвращаем функцию для удаления обработчика при размонтировании
    return () => hub.removeHandler(handler);
  }, [currentViewerId, hub, onDisconnect]);

  // Не рендерим JSX, компонент работает только через эффекты
  return null;
};

export default DisconnectModule;
