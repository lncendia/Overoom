import { NotificationEvent } from './notification.event.ts';

/** Базовое событие уведомления, направленное на конкретного пользователя. */
export interface TargetedNotificationEvent extends NotificationEvent {
  /** Идентификатор пользователя, на которого направлено событие. */
  target: string;
}
