import { useContext } from 'react';

import { FilmContext, FilmContextType } from './FilmContext.tsx';

/**
 * Хук для доступа к контексту фильма внутри компонентов
 * @returns {FilmContextType} Объект контекста фильма
 */
export const useFilm = (): FilmContextType => {
  /** Используем хук useContext для получения доступа к контексту фильма */
  const context = useContext(FilmContext);

  // Проверяем, что хук используется внутри провайдера контекста
  if (context === undefined) {
    throw new Error('useFilm must be used within a FilmContextProvider');
  }

  return context;
};
