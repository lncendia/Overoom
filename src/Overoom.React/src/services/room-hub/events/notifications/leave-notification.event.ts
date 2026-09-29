import { NotificationEvent } from './notification.event.ts';

/** Событие уведомления о выходе пользователя из комнаты. */
export interface LeaveNotificationEvent extends NotificationEvent {
  /** Имя вышедшего зрителя */
  name: string;
}
