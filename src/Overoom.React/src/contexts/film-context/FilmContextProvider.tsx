import { useInjection } from 'inversify-react';
import React, { ReactElement, ReactNode, useCallback, useEffect, useState } from 'react';

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

  /** Флаг ошибки загрузки фильма */
  const [isError, setIsError] = useState(false);

  /** Счетчик для принудительной повторной загрузки фильма */
  const [reloadKey, setReloadKey] = useState(0);

  /** Получаем экземпляр FilmsApi из контейнера зависимостей */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Получаем экземпляр ProfileApi из контейнера зависимостей */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Получаем текущего авторизованного пользователя из контекста аутентификации */
  const { authorizedUser } = useAuthentication();

  /** Идентификатор авторизованного пользователя (чтобы не зависеть от пересоздания объекта) */
  const userId = authorizedUser?.id;

  /** Функция добавления фильма в историю просмотра пользователя */
  const addToHistory = useSafeCallback(async () => {
    if (!filmId) return;
    await profileApi.addToHistory(filmId);
  }, [profileApi, filmId]);

  /**
   * Функция загрузки данных фильма по его ID
   * @param isActual - функция, возвращающая false, если ответ устарел (сменился filmId)
   */
  const fetchFilm = useSafeCallback(
    async (isActual: () => boolean) => {
      if (!filmId) return;
      setIsError(false);
      try {
        const response = await filmsApi.get(filmId);
        if (isActual()) setFilm(response);
      } catch (e) {
        if (!isActual()) return;
        setIsError(true);
        throw e;
      }
    },
    [filmsApi, filmId]
  );

  /** useEffect для добавления фильма в историю после авторизации пользователя */
  useEffect(() => {
    if (userId) addToHistory().then();
  }, [userId, addToHistory]);

  /** useEffect для загрузки данных фильма при монтировании и очистках состояния при размонтировании */
  useEffect(() => {
    let actual = true;
    fetchFilm(() => actual).then();
    return () => {
      actual = false;
      setFilm(null);
      setIsError(false);
    };
  }, [fetchFilm, reloadKey]);

  /** Функция повторной загрузки фильма (например, после ошибки) */
  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return (
    <FilmContext.Provider value={{ film, editFilm: setFilm, isError, reload }}>
      {children}
    </FilmContext.Provider>
  );
};
