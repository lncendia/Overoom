import { TargetedNotificationEvent } from './targeted-notification.event.ts';

/** Событие уведомления об исключении пользователя из комнаты. */
export interface KickNotificationEvent extends TargetedNotificationEvent {
  /** Имя вышедшего зрителя */
  name: string;
}
