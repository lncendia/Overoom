import { useTheme } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useRef, useState } from 'react';

import Navbar from '../../../components/menu/navbar/Navbar.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useThemeContext } from '../../../contexts/theme-context/useThemeContext.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../../services/auth/auth.api.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';

/**
 * Модуль навигационной панели приложения.
 * Отвечает за поиск фильмов, переключение темы и управление сессией пользователя.
 * @returns {ReactElement} JSX элемент навигационной панели.
 */
const NavbarModule = (): ReactElement => {
  /** Список фильмов, найденных по запросу */
  const [films, setFilms] = useState<FilmShortResponse[]>([]);

  /** Последний отправленный поисковый запрос (для игнорирования устаревших ответов) */
  const lastQuery = useRef('');

  /** Инжектируем сервис для работы с фильмами */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Инжектируем сервис для работы с аутентификацией */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Получаем данные об авторизованном пользователе */
  const { authorizedUser } = useAuthentication();

  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  /**
   * Используем хук useThemeContext для получения функции setMode,
   * которая позволяет переключать тему (светлую или тёмную)
   */
  const { setMode } = useThemeContext();

  /**
   * Функция поиска фильмов по строке запроса.
   * Ответы на устаревшие запросы (если пользователь успел изменить строку) игнорируются.
   * @param value - Строка запроса для поиска фильмов.
   * @returns {Promise<void>} Обновляет состояние списка фильмов.
   */
  const onFilmSearch = useSafeCallback(
    async (value: string) => {
      lastQuery.current = value;
      if (value === '') {
        setFilms([]);
        return;
      }

      const filmsResponse = await filmsApi.search({ query: value });
      if (lastQuery.current === value) setFilms(filmsResponse.list);
    },
    [filmsApi]
  );

  /** Обработчик выхода из аккаунта */
  const onExit = useSafeCallback(() => authApi.signOut(), [authApi]);

  /** Обработчик входа в аккаунт */
  const onLogin = useSafeCallback(() => authApi.signIn(), [authApi]);

  /**
   * Функция переключения темы со светлой на тёмную и обратно.
   * @param enabled - Флаг, указывающий, включён ли тёмный режим.
   * @returns {void}
   */
  const toggleDarkMode = useCallback(
    (enabled: boolean) => setMode(enabled ? 'dark' : 'light'),
    [setMode]
  );

  return (
    <Navbar
      films={films}
      onFilmSearch={onFilmSearch}
      onExit={onExit}
      onLogin={onLogin}
      userName={authorizedUser?.userName}
      photoUrl={authorizedUser?.photoUrl ?? undefined}
      toggleDarkMode={toggleDarkMode}
      darkMode={theme.palette.mode === 'dark'}
      isUserAuthorized={!!authorizedUser}
      isAdmin={authorizedUser?.roles.includes('admin') ?? false}
    />
  );
};

export default NavbarModule;
