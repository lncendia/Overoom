import { useCallback, useEffect } from 'react';

import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';

/**
 * NotificationModule — компонент для обработки уведомлений комнаты.
 * Подписывается на события hub и отображает уведомления через useNotify.
 * @returns {null} Не рендерит JSX, работает только на уровне эффектов
 */
const NotificationModule = (): null => {
  const { hub, currentViewerId, room } = useRoom();
  const { setNotification } = useNotify();

  /**
   * Получает отображаемое имя пользователя.
   * Если viewerId совпадает с текущим зрителем — возвращает "Вы" или "Вас".
   * @param {string} viewerId — ID зрителя
   * @param {boolean} [accusative=false] — использовать форму "Вас" вместо "Вы"
   * @returns {string} — имя пользователя для уведомления
   */
  const getViewerUserName = useCallback(
    (viewerId: string, accusative = false): string | null => {
      if (!room?.viewers) return null;
      if (viewerId === currentViewerId) return accusative ? 'Вас' : 'Вы';
      return room.viewers.get(viewerId)?.userName ?? 'Неизвестный';
    },
    [currentViewerId, room?.viewers]
  );

  /**
   * Получает отображаемое имя пользователя.
   * Если viewerId совпадает с текущим зрителем — возвращает "Вы" или "Вас".
   * @param {string} viewerId — ID зрителя
   * @param {string} username — Имя зрителя
   * @param {boolean} [accusative=false] — использовать форму "Вас" вместо "Вы"
   * @returns {string} — имя пользователя для уведомления
   */
  const getUserName = useCallback(
    (viewerId: string, username: string, accusative = false): string => {
      if (viewerId === currentViewerId) return accusative ? 'Вас' : 'Вы';
      return username;
    },
    [currentViewerId]
  );

  /**
   * Выбирает правильную форму глагола в зависимости от того,
   * кто совершает действие: "Вы" → множественное, остальные → единственное.
   * @param {string} actorId — ID субъекта действия
   * @param {string} verb — глагол в 3 лице ед. числа ("удалил", "разбудил")
   * @returns {string} — правильная форма глагола
   */
  const verbForm = useCallback(
    (actorId: string | undefined, verb: string): string => {
      if (actorId === currentViewerId) {
        if (verb.endsWith('л')) return verb + 'и';
        if (verb.endsWith('ся')) return verb.slice(0, -2) + 'ись';
        return verb;
      }
      return verb;
    },
    [currentViewerId]
  );

  /** Добавления обработчика событий комнаты*/
  useEffect(() => {
    // Если хаб еще не инициализирован или нет владельца комнаты — не выполняем обработку
    if (!hub || !room?.ownerId) return;

    /**
     * Обработчик событий комнаты
     * @param {RoomEventContainer} event — объект события комнаты
     * @returns {void}
     */
    const handler = (event: RoomEventContainer): void => {
      // Событие удаления комнаты
      if (event.deleteNotificationEvent) {
        const initiatorName = getViewerUserName(room.ownerId);
        setNotification({
          message: `${initiatorName} ${verbForm(room.ownerId, 'удалил')} комнату`,
          severity: 'info',
        });
      }
      // Событие "beep" — разбудил пользователя
      else if (event.beepNotificationEvent) {
        const initiator = event.beepNotificationEvent.initiator;
        const target = event.beepNotificationEvent.target;
        const initiatorName = getViewerUserName(initiator);
        const targetName = getViewerUserName(target, true);
        setNotification({
          message: `${initiatorName} ${verbForm(initiator, 'разбудил')} ${targetName}`,
          severity: 'info',
        });
      }
      // Событие "scream" — напугал пользователя
      else if (event.screamNotificationEvent) {
        const initiator = event.screamNotificationEvent.initiator;
        const target = event.screamNotificationEvent.target;
        const initiatorName = getViewerUserName(initiator);
        const targetName = getViewerUserName(target, true);
        setNotification({
          message: `${initiatorName} ${verbForm(initiator, 'напугал')} ${targetName}`,
          severity: 'warning',
        });
      }
      // Событие кика пользователя из комнаты
      else if (event.kickNotificationEvent) {
        const initiator = event.kickNotificationEvent.initiator;
        const target = event.kickNotificationEvent.target;
        const initiatorName = getViewerUserName(initiator);
        const targetName = getUserName(target, event.kickNotificationEvent.name, true);
        setNotification({
          message: `${initiatorName} ${verbForm(initiator, 'выгнал')} ${targetName}`,
          severity: 'error',
        });
      }
      // Событие выхода пользователя из комнаты
      else if (event.leaveNotificationEvent) {
        const initiator = event.leaveNotificationEvent.initiator;
        const viewerName = getUserName(initiator, event.leaveNotificationEvent.name);
        setNotification({
          message: `${viewerName} ${verbForm(initiator, 'покинул')} комнату`,
          severity: 'info',
        });
      }
      // Событие присоединения пользователя к комнате
      else if (event.joinNotificationEvent) {
        const viewer = event.joinNotificationEvent.initiator;
        const viewerName = getUserName(viewer, event.joinNotificationEvent.name);
        setNotification({
          message: `${viewerName} ${verbForm(viewer, 'присоединился')} к комнате`,
          severity: 'info',
        });
      }
      // Событие ошибки
      else if (event.errorNotificationEvent) {
        setNotification({
          message: event.errorNotificationEvent.message,
          severity: 'error',
        });
      }
    };

    // Подписка на события хаба
    hub.addHandler(handler);

    // Очистка подписки при размонтировании компонента
    return () => hub.removeHandler(handler);
  }, [hub, setNotification, getUserName, getViewerUserName, verbForm, room?.ownerId]);

  return null;
};

export default NotificationModule;
