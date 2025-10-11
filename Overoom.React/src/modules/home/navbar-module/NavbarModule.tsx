import { useTheme } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import Navbar from '../../../components/menu/navbar/Navbar.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useThemeContext } from '../../../contexts/theme-context/useThemeContext.tsx';
import { AuthApi } from '../../../services/auth/auth.api.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';

/**
 * Модуль навигационной панели приложения.
 * Отвечает за поиск фильмов, переключение темы и управление навигацией.
 * @returns {ReactElement} JSX элемент навигационной панели.
 */
const NavbarModule = (): ReactElement => {
  /** Список фильмов, найденных по запросу */
  const [films, setFilms] = useState<FilmShortResponse[]>([]);

  /** Инжектируем сервис для работы с фильмами */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Инжектируем сервис для работы с аутентификацией */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Получаем данные об авторизованном пользователе */
  const { authorizedUser } = useAuthentication();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  /**
   * Используем хук useThemeContext для получения функции setMode,
   * которая позволяет переключать тему (светлую или тёмную)
   */
  const { setMode } = useThemeContext();

  /**
   * Функция поиска фильмов по строке запроса.
   * @param value - Строка запроса для поиска фильмов.
   * @returns {Promise<void>} Обновляет состояние списка фильмов.
   */
  const onFilmSearch = useCallback(
    async (value: string) => {
      if (value === '') setFilms([]);
      else {
        const filmsResponse = await filmsApi.search({ query: value });
        setFilms(filmsResponse.list);
      }
    },
    [filmsApi]
  );

  /** Набор колбэков для навигации и управления сессией пользователя */
  const callbacks = {
    /** Обработчик выхода из аккаунта */
    onExit: useCallback(() => authApi.signOut(), [authApi]),

    /** Обработчик входа в аккаунт */
    onLogin: useCallback(() => authApi.signIn(), [authApi]),

    /** Переход в каталог фильмов */
    onCatalog: useCallback(() => navigate('/catalog'), [navigate]),

    /** Переход в список плейлистов */
    onPlaylists: useCallback(() => navigate('/playlists'), [navigate]),

    /** Переход в список комнат */
    onRooms: useCallback(() => navigate('/rooms'), [navigate]),

    /** Переход в профиль пользователя */
    onProfile: useCallback(() => navigate('/profile'), [navigate]),

    /** Переход на главную страницу */
    onHome: useCallback(() => navigate('/'), [navigate]),
  };

  /**
   * Переход на страницу фильма.
   * @param id - Идентификатор фильма.
   * @returns {void}
   */
  const onFilm = useCallback(
    (id: string) => {
      navigate('/film', { state: { id: id } });
    },
    [navigate]
  );

  /**
   * Функция переключения темы со светлой на тёмную и обратно.
   * @param enabled - Флаг, указывающий, включён ли тёмный режим.
   * @returns {void}
   */
  const toggleDarkMode = useCallback(
    (enabled: boolean) => setMode(enabled ? 'dark' : 'light'),
    [setMode]
  );

  // Рендерим компонент навигации, передавая все необходимые свойства и обработчики
  return (
    <Navbar
      onFilm={onFilm}
      films={films}
      {...callbacks}
      onFilmSearch={onFilmSearch}
      {...authorizedUser}
      photoUrl={authorizedUser?.photoUrl ?? undefined}
      toggleDarkMode={toggleDarkMode}
      darkMode={theme.palette.mode === 'dark'}
      isUserAuthorized={!!authorizedUser}
    />
  );
};

export default NavbarModule;
