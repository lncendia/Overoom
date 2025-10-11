import { NotificationEvent } from './notification.event.ts';

/** Событие уведомления о подключении пользователя к комнате. */
export interface JoinNotificationEvent extends NotificationEvent {
  /** Имя подключившегося зрителя */
  name: string;
}
