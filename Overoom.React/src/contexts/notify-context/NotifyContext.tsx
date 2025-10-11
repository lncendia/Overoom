import { createContext } from 'react';

import { Notification } from './notification.ts';

/** Интерфейс контекста NotifyContext */
export interface NotifyContextType {
  /** Оповещение */
  notification?: Notification;

  /**
   * Установка оповещения
   * @param notification - тип оповещения
   */
  setNotification: (notification: Notification) => void;

  /**
   * Установка оповещения
   * @param error - ошибка
   */
  setError: (error: Error) => void;
}

/** Контекст NotifyContext с пустым объектом в качестве значения по умолчанию */
export const NotifyContext = createContext<NotifyContextType>({
  setNotification: () => {},
  setError: () => {},
});
