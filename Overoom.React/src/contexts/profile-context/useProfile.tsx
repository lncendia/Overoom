import { useContext } from 'react';

import { ProfileContext, ProfileContextType } from './ProfileContext.tsx';

/**
 * Хук для доступа к контексту профиля пользователя
 * @throws {Error} Если используется вне провайдера ProfileContextProvider
 * @returns {ProfileContextType} Контекст профиля пользователя
 */
export const useProfile = (): ProfileContextType => {
  /** Используем хук useContext для получения доступа к контексту профиля */
  const context = useContext(ProfileContext);

  // Проверяем, что хук используется внутри провайдера контекста
  if (context === undefined) {
    throw new Error('useProfile must be used within a ProfileContextProvider');
  }

  return context;
};
