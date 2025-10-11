import { useInjection } from 'inversify-react';
import React, { ReactElement, ReactNode, useEffect, useState } from 'react';

import { FilmContext } from './FilmContext.tsx';
import { useSafeCallback } from '../../hooks/safe-callback-hook/useSafeCallback.ts';
import { FilmsApi } from '../../services/films/films.api.ts';
import { FilmResponse } from '../../services/films/responses/film.response.ts';
import { ProfileApi } from '../../services/profile/profile.api.ts';
import { useAuthentication } from '../authentication-context/useAuthentication.tsx';

/** Пропсы компонента FilmContextProvider */
interface FilmContextProviderProps {
  /** Дочерние элементы провайдера */
  children: ReactNode;
  /** Идентификатор фильма для загрузки */
  filmId?: string;
}

/**
 * Провайдер контекста фильма
 * @param props - Пропсы компонента
 * @param props.children - Дочерние элементы
 * @param props.filmId - Идентификатор фильма
 * @returns {ReactElement} JSX элемент провайдера контекста фильма
 */
export const FilmContextProvider: React.FC<FilmContextProviderProps> = ({
  children,
  filmId,
}): ReactElement => {
  /** Хук useState для хранения данных о фильме */
  const [film, setFilm] = useState<FilmResponse | null>(null);

  /** Получаем экземпляр FilmsApi из контейнера зависимостей */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Получаем экземпляр ProfileApi из контейнера зависимостей */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Получаем текущего авторизованного пользователя из контекста аутентификации */
  const { authorizedUser } = useAuthentication();

  /** Функция добавления фильма в историю просмотра пользователя */
  const addToHistory = useSafeCallback(async () => {
    if (!filmId) return;
    await profileApi.addToHistory(filmId);
  }, [profileApi, filmId]);

  /** Функция загрузки данных фильма по его ID */
  const fetchFilm = useSafeCallback(async () => {
    if (!filmId) return;
    const response = await filmsApi.get(filmId);
    setFilm(response);
  }, [filmsApi, filmId]);

  /** useEffect для добавления фильма в историю после авторизации пользователя */
  useEffect(() => {
    if (authorizedUser) addToHistory().then();
  }, [authorizedUser, addToHistory]);

  /** useEffect для загрузки данных фильма при монтировании и очистках состояния при размонтировании */
  useEffect(() => {
    fetchFilm().then();
    return () => {
      setFilm(null);
    };
  }, [fetchFilm]);

  // Возвращаем провайдер контекста с текущим фильмом и функцией редактирования
  return (
    <FilmContext.Provider value={{ film, editFilm: setFilm }}>{children}</FilmContext.Provider>
  );
};
