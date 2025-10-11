import { useContext } from 'react';

import { ThemeContext, ThemeContextType } from './ThemeContext.tsx';

/**
 * Хук для доступа к контексту темы внутри компонентов
 * @throws {Error} Если используется вне провайдера ThemeContextProvider
 * @returns {ThemeContextType} Объект контекста темы
 */
export const useThemeContext = (): ThemeContextType => {
  /** Используем хук useContext для получения доступа к контексту темы */
  const context = useContext(ThemeContext);

  // Проверяем, что хук используется внутри провайдера контекста
  if (context === undefined) {
    throw new Error('useThemeContext must be used within a ThemeContextProvider');
  }

  return context;
};
