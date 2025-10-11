import { Alert, AlertTitle, Snackbar, SnackbarCloseReason } from '@mui/material';
import { AxiosError } from 'axios';
import React, { ReactElement, ReactNode, useCallback, useState } from 'react';

import { Notification } from './notification.ts';
import { NotifyContext } from './NotifyContext.tsx';

/** Пропсы компонента NotifyContextProvider */
interface NotifyContextProviderProps {
  /** Дочерние элементы провайдера */
  children: ReactNode;
}

/**
 * Провайдер контекста уведомлений
 * @param props - Пропсы компонента
 * @param props.children - Дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера уведомлений
 */
export const NotifyContextProvider: React.FC<NotifyContextProviderProps> = ({
  children,
}): ReactElement => {
  /** Состояние текущего уведомления */
  const [notification, setNotification] = useState<Notification>();

  /** Состояние видимости уведомления */
  const [isOpened, setIsOpened] = useState(false);

  /**
   * Устанавливает уведомление при ошибке
   * @param {Error} error - Объект ошибки
   */
  const setError = useCallback(
    (error: Error) => {
      let title = 'Ошибка';
      let message = error.message ?? 'Произошла ошибка';

      if ((error as AxiosError).isAxiosError) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const axiosError = error as AxiosError<any>;
        const data = axiosError.response?.data;

        if (data && typeof data === 'object') {
          title = data.title ?? title;
          message = data.detail ?? message;
        }
      }

      setNotification({
        title,
        message,
        severity: 'error',
      });

      setIsOpened(true);
    },
    [setNotification]
  );

  /**
   * Устанавливает произвольное уведомление
   * @param {Notification} notification - Объект уведомления
   */
  const setNotify = useCallback((notification: Notification) => {
    setNotification(notification);
    setIsOpened(true);
  }, []);

  /**
   * Обработчик закрытия уведомления
   * @param _event - Событие закрытия
   * @param reason - Причина закрытия (например, 'clickaway')
   */
  const handleClose = (_event: React.SyntheticEvent | Event, reason?: SnackbarCloseReason) => {
    if (reason === 'clickaway') return;
    setIsOpened(false);
  };

  // Возвращаем провайдер контекста с текущим уведомлением и методами управления
  return (
    <NotifyContext.Provider value={{ setNotification: setNotify, setError, notification }}>
      {children}
      <Snackbar open={!!notification && isOpened} autoHideDuration={6000} onClose={handleClose}>
        <Alert onClose={handleClose} severity={notification?.severity} sx={{ width: '100%' }}>
          {notification?.title && <AlertTitle>{notification.title}</AlertTitle>}
          {notification?.message}
        </Alert>
      </Snackbar>
    </NotifyContext.Provider>
  );
};
