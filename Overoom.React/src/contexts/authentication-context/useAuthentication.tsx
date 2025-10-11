import { useContext } from 'react';

import { AuthenticationContext, AuthenticationContextType } from './AuthenticationContext.tsx';

/**
 * Хук для доступа к контексту аутентификации
 * @returns {AuthenticationContextType} Объект контекста аутентификации
 */
export const useAuthentication = (): AuthenticationContextType => {
  /** Используем хук useContext для получения доступа к контексту аутентификации */
  const context = useContext(AuthenticationContext);

  // Проверяем, что хук используется внутри провайдера контекста
  if (context === undefined) {
    throw new Error('useAuthentication must be used within a AuthenticationContextProvider');
  }

  return context;
};
