import React, { createContext } from 'react';

import { FilmResponse } from '../../services/films/responses/film.response.ts';

/** Интерфейс контекста фильма */
export interface FilmContextType {
  /** Данные фильма */
  film: FilmResponse | null;
  /** Функция для редактирования данных фильма */
  editFilm: (editFunc: (film: FilmResponse | null) => FilmResponse | null) => void;
  /** Флаг ошибки загрузки фильма */
  isError?: boolean;
  /** Повторная загрузка фильма */
  reload?: () => void;
}

/** Контекст фильма */
export const FilmContext: React.Context<FilmContextType | undefined> = createContext<
  FilmContextType | undefined
>(undefined);
