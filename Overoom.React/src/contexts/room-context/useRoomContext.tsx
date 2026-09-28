import { useContext } from 'react';

import { RoomContext, RoomContextType } from './RoomContext.tsx';

/**
 * Хук для доступа к контексту комнаты
 * @throws {Error} Если используется вне провайдера RoomContextProvider
 * @returns {RoomContextType} Контекст комнаты
 */
export const useRoom = (): RoomContextType => {
  /** Используем хук useContext для получения доступа к контексту комнаты */
  const context = useContext(RoomContext);

  if (context === undefined) {
    throw new Error('useRoom must be used within a RoomContextProvider');
  }

  return context;
};
