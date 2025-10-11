import { useContext } from 'react';

import { NotifyContext, NotifyContextType } from './NotifyContext.tsx';

/**
 * Хук для доступа к контексту оповещений внутри компонентов
 * @returns {NotifyContext} Объект контекста уведомлений
 */
export const useNotify = (): NotifyContextType => {
  /** Используем хук useContext для получения доступа к контексту уведомлений */
  const context = useContext(NotifyContext);

  // Проверяем, что хук используется внутри провайдера контекста
  if (context === undefined) {
    throw new Error('useNotify must be used within a NotifyContextProvider');
  }

  return context;
};
