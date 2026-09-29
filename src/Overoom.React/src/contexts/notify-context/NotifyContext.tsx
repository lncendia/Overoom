import { createContext } from 'react';

import { Notification } from './notification.ts';

/**
 * Интерфейс контекста NotifyContext.
 * Содержит только стабильные действия, поэтому потребители не перерисовываются при показе оповещений.
 */
export interface NotifyContextType {
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

/** Контекст NotifyContext с пустыми действиями в качестве значения по умолчанию */
export const NotifyContext = createContext<NotifyContextType>({
  setNotification: () => {},
  setError: () => {},
});
