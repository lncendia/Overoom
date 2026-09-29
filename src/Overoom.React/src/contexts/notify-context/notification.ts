import { AlertColor } from '@mui/material';
import React from 'react';

/** Интерфейс уведомления */
export interface Notification {
  /** Заголовок уведомления */
  title?: string;
  /** Тело сообщения уведомления */
  message: string | React.ReactNode;
  /** Уровень важности уведомления */
  severity: AlertColor;
}
